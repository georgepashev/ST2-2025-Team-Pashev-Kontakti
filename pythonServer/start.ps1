$env:MODEL_PATH="models\mistral-7b-instruct.Q4_K_M.gguf"
$env:CTX_SIZE="4096"
$env:N_GPU_LAYERS="0"
$env:THREADS="12"

$env:PORT="1234"
python server.py