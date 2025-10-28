import os
import json
from typing import List, Optional, Dict, Any
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field
from llama_cpp import Llama
import uvicorn

# ----------------------
# Config from env
# ----------------------
MODEL_PATH   = os.environ.get("MODEL_PATH", "models/mistral-7b-instruct.Q4_K_M.gguf")
CTX_SIZE     = int(os.environ.get("CTX_SIZE", "4096"))
N_GPU_LAYERS = int(os.environ.get("N_GPU_LAYERS", "0"))      # 0 = CPU only
THREADS      = int(os.environ.get("THREADS", str(os.cpu_count() or 4)))
HOST         = os.environ.get("HOST", "0.0.0.0")
PORT         = int(os.environ.get("PORT", "1234"))

if not os.path.isfile(MODEL_PATH):
    raise RuntimeError(f"MODEL_PATH does not exist: {MODEL_PATH}")

# ----------------------
# Load model
# ----------------------
# chat_format="mistral" активира вградените chat templates
def try_load_model():
    print("="*70)
    print("🧠  Loading local Mistral GGUF model...")
    print(f"MODEL_PATH   : {MODEL_PATH}")
    print(f"CTX_SIZE     : {CTX_SIZE}")
    print(f"THREADS      : {THREADS}")
    print(f"N_GPU_LAYERS : {N_GPU_LAYERS}")
    print(f"chat_format  : mistral")
    print("="*70)

    try:
        llm_obj = Llama(
            model_path=MODEL_PATH,
            n_ctx=CTX_SIZE,
            n_threads=THREADS,
            n_gpu_layers=N_GPU_LAYERS,
            # chat_format="mistral",
            use_mmap=False,   # <-- key change
            use_mlock=False,  # keep off on Windows
            verbose=True   # enable verbose loading log
        )
        print("✅ Model loaded successfully!\n")
        return llm_obj

    except Exception as e:
        import traceback, sys
        print("❌ Failed to load model.")
        print("Python Exception Type :", type(e).__name__)
        print("Python Exception Value:", e)
        print("Full traceback:")
        traceback.print_exc(file=sys.stdout)
        print("\nPossible causes:")
        print("  • MODEL_PATH is incorrect or file not found")
        print("  • File corrupted (check .gguf file integrity)")
        print("  • You have a non-compatible quantization (wrong model type)")
        print("  • You compiled llama-cpp-python without GPU/CPU backend support")
        print("  • Your environment lacks OpenBLAS / AVX2 / CUDA runtime libs")
        print("="*70)
        raise   # re-raise so FastAPI will 500, but you get full trace in console

llm = try_load_model()

# ----------------------
# OpenAI-like schemas
# ----------------------
class ChatMessage(BaseModel):
    role: str
    content: str

class ChatCompletionsRequest(BaseModel):
    model: Optional[str] = "mistral-gguf"
    messages: List[ChatMessage]
    temperature: Optional[float] = 0.2
    max_tokens: Optional[int] = 512
    top_p: Optional[float] = 0.95
    stop: Optional[List[str]] = None

class ChoiceMessage(BaseModel):
    role: str = "assistant"
    content: str

class ChatChoice(BaseModel):
    index: int
    message: ChoiceMessage
    finish_reason: Optional[str] = "stop"

class ChatCompletionsResponse(BaseModel):
    id: str = Field(default_factory=lambda: "chatcmpl-local")
    object: str = "chat.completion"
    created: int = 0
    model: str = "mistral-gguf"
    choices: List[ChatChoice] = []
    usage: Optional[Dict[str, int]] = None

# ----------------------
# FastAPI app
# ----------------------
app = FastAPI(
    title="Local GGUF Chat Completions",
    version="1.0.0"
)

SYSTEM_FALLBACK = (
    'You are a strict query planner. '
    'Return ONLY a single compact JSON object (no prose, no markdown). '
    'Table: Contacts; Allowed columns: Id, Name, Email, PhoneNumber, AddressLine1, AddressLine2; '
    'Allowed operators: equals, contains, starts_with, ends_with; '
    'Combine with AND; Optional order_by (asc|desc), limit<=200, offset. '
    'Example: {"filters":[{"column":"Email","op":"ends_with","value":"@abv.bg"}],'
    '"order_by":[{"column":"Name","direction":"asc"}],"limit":50,"offset":0}'
)

def ensure_system_in_messages(messages: List[ChatMessage]) -> List[Dict[str, str]]:
    """
    Преобразува към llama.cpp chat формат и добавя стриктен system prompt,
    ако липсва.
    """
    sys_present = any(m.role == "system" for m in messages)
    chat = []
    if not sys_present:
        chat.append({"role": "system", "content": SYSTEM_FALLBACK})
    for m in messages:
        chat.append({"role": m.role, "content": m.content})
    return chat

def extract_json(text: str) -> str:
    """Вади вътрешния JSON обект, ако моделът добави обяснения или ```json```."""
    s = text.strip()
    start = s.find("{")
    end = s.rfind("}")
    if start != -1 and end > start:
        return s[start:end+1]
    return s  # последна инстанция – върни каквото е

@app.post("/v1/chat/completions", response_model=ChatCompletionsResponse)
def chat_completions(req: ChatCompletionsRequest):
    if not req.messages:
        raise HTTPException(status_code=400, detail="messages is required")

    chat = ensure_system_in_messages(req.messages)

    try:
        # llama.cpp  API за чат
        # return_dict=True -> получаваме структуриран отговор
        out = llm.create_chat_completion(
            messages=chat,
            temperature=req.temperature or 0.2,
            max_tokens=req.max_tokens or 512,
            top_p=req.top_p or 0.95,
            stop=req.stop or None,
        )
    except Exception as e:
        print(f"Exception: {e}")
        raise HTTPException(status_code=500, detail=f"Generation failed: {e}")

    # Очакван формат:
    # out["choices"][0]["message"]["content"]
    try:
        content = out["choices"][0]["message"]["content"]
    except Exception:
        content = json.dumps({"error": "invalid_model_output"})

    # Подсигури чист JSON (за твоя .NET парсер)
    content = extract_json(content)

    resp = ChatCompletionsResponse(
        choices=[
            ChatChoice(
                index=0,
                message=ChoiceMessage(content=content),
                finish_reason="stop"
            )
        ],
        model=req.model or "mistral-gguf",
        usage=out.get("usage")
    )
    return resp

@app.get("/")
def root():
    return {"status": "ok", "message": "Use POST /v1/chat/completions"}

if __name__ == "__main__":
    uvicorn.run("server:app", host=HOST, port=PORT, reload=False)
