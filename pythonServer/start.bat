@echo off
REM ============================================================
REM  Local Mistral GGUF OpenAI-Compatible Server (Windows)
REM ============================================================
title Mistral GGUF Server
echo Starting local LLM server using llama-cpp-python...
echo.

REM --- Virtual environment setup ---
if not exist .venv (
    echo Creating virtual environment...
    python -m venv .venv
)

call .venv\Scripts\activate

REM --- Install dependencies if not already installed ---
echo Installing dependencies (this may take a while the first time)...
pip install --upgrade pip >nul
pip install fastapi uvicorn "llama-cpp-python==0.2.90" pydantic >nul

REM --- Environment configuration ---
set MODEL_PATH=models\mistral-7b-instruct.Q4_K_M.gguf
set CTX_SIZE=1024
set THREADS=1
set N_GPU_LAYERS=0
set HOST=0.0.0.0
set PORT=1234

REM --- Check that model exists ---
if not exist "%MODEL_PATH%" (
    echo [ERROR] Model file not found at %MODEL_PATH%
    echo Please download a GGUF model and place it in the models\ folder.
    echo Example: mistral-7b-instruct-v0.2.Q4_K_M.gguf
    pause
    exit /b 1
)

echo.
echo ============================================================
echo   MODEL: %MODEL_PATH%
echo   THREADS: %THREADS%
echo   GPU Layers: %N_GPU_LAYERS%
echo   Context Size: %CTX_SIZE%
echo   Host: %HOST%:%PORT%
echo ============================================================
echo.

REM --- Run the FastAPI server ---
python server.py

REM --- Keep window open on exit ---
pause
