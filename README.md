# ST2-2025-Team-Pashev-Kontakti

## Author
гл. ас. д-р Георги Пашев

## Overview
This project demonstrates a Contacts MVC application enhanced with an Intelligent Search system powered by a local LLM (Mistral / llama.cpp). Natural language queries are transformed into a JSON Query Plan by the LLM and then safely translated into SQL with whitelisting and parameterization.

### Structure
| Directory | Purpose |
|----------|----------|
| `Kontakti/Kontakti/` | ASP.NET Core MVC Application |
| `pythonServer/` | FastAPI LLM Server (OpenAI compatible) |

---

## Requirements
- .NET 8 SDK
- Python 3.10 – 3.12
- GGUF model placed under `pythonServer/models/` (example: `mistral-7b-instruct.Q4_K_M.gguf`)

---

## Start Instructions

### 1) Start LLM server
Inside `/pythonServer/` run (Windows):

```
start.bat
```

or PowerShell:

```
.\start.ps1
```

Before starting, define model path:
```
$env:MODEL_PATH="models/mistral-7b-instruct.Q4_K_M.gguf"
```

### 2) Start MVC App
```
cd Kontakti/Kontakti
dotnet restore
dotnet run
```

Check `appsettings.json` → LLM section must point to:
```
http://localhost:1234/v1
```

---

## Testing

1) Create several Contacts using Create form.
2) Use Intelligent Search form.
3) Example prompts:

```
покажи контактите с имейл abv.bg и сортирай по Name
people with phone starting with 0888 order by Name asc
```

Result: UI shows filtered contacts + Debug JSON Plan below it.

---

## Used Design Patterns

| Pattern | Where | Purpose |
|--------|--------|---------|
| MVC | ASP.NET MVC portion | Separation of UI / Logic / Data |
| Repository Pattern | `Database.cs` | Wraps DB access besides Controllers |
| Singleton | DB instance + llama model | Exactly one instance in process |
| Dependency Injection | LlmClient + HttpClientFactory | Easy replacement & testablity |
| Options Pattern | LlmOptions/appsettings.json | Configured endpoint/model & timeout |
| Adapter/API Gateway | `LlmClient` | Converts local LLM to OpenAI API format |
| DTO | `LlmDto` / `QueryPlan` | Structured message formats |
| Facade | LlmClient & python server wrapper | Hides complexity of transport |
| Validation Pattern | DataAnnotations Contact.cs | Safe data entry |

---

## Release Checklist

### Environment
- [ ] .NET 8 SDK installed
- [ ] Python installed
- [ ] MODEL_PATH set properly

### LLM Server
- [ ] `start.bat` or `start.ps1` successfully starts server
- [ ] curl test works

### MVC
- [ ] appsettings.json correct LLM URL
- [ ] dotnet run successful
- [ ] Create/Edit/Delete OK

### Intelligent Search
- [ ] JSON Plan visible and valid
- [ ] no crashes with invalid or unsupported fields

### Security
- [ ] SQL fully parameterized
- [ ] operator whitelist enforced

---

## Troubleshooting

| Problem | Cause | Fix |
|--------|-------|-----|
| PowerShell blocks ps1 | Execution policy | `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass` |
| Timeout from LLM | too heavy model | reduce ctx, smaller quant |
| LLM prints comments not only JSON | prompt not strict | ensure exact system prompt |
| SQLite provider collisions | 2 libs referenced | keep Microsoft.Data.Sqlite only |
| Bulgarian phrasing odd | model not BG tuned | neutral english works best |

---

## Demo Script (Defence)

1) start.bat  
2) dotnet run  
3) add 3 sample contacts  
4) Intelligent Search natural language query  
5) show generated JSON PLAN  
6) show it prevented SQL injection

