# 🏗️ Arquitetura

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Backend | ASP.NET Core 10 — Web API (controllers) |
| ORM | Entity Framework Core (PostgreSQL provider — Npgsql) |
| Banco | PostgreSQL local (`isekai`) |
| Frontend | Angular 19 — standalone components, signals |
| HTTP | `HttpClient` (frontend) ↔ REST/JSON (backend) |
| Docs API | Swagger / Swagger UI (em Development) |

> **.NET 10:** o projeto foi pensado para .NET 9, mas só havia .NET 10 instalado na máquina — usado sem problemas.

---

## Estrutura de pastas

```
Isekai-Desnecessario/
├── IsekaiDesnecessario.slnx          # solução .NET
├── CLAUDE.md                         # contexto curto p/ assistentes
├── docs/                             # 📖 esta documentação
│
├── backend/
│   └── IsekaiDesnecessario.API/
│       ├── Program.cs                # bootstrap, DI, CORS, Swagger, static files
│       ├── appsettings.json          # connection string
│       ├── Controllers/              # endpoints REST (12 controllers)
│       ├── Models/                   # entidades EF (15 modelos)
│       ├── Services/                 # XpService, GachaService
│       ├── Data/AppDbContext.cs      # DbContext + seed (atributos, classes)
│       ├── Migrations/               # 27 migrations EF
│       └── wwwroot/uploads/          # fotos de perfil enviadas
│
└── frontend/
    └── isekai-desnecessario-app/
        ├── src/app/
        │   ├── app.routes.ts         # rotas
        │   ├── models/models.ts      # interfaces TypeScript (espelham os Models C#)
        │   ├── services/             # api.service, profile.service
        │   ├── components/           # navbar, profile-selector
        │   └── pages/                # 10 páginas (dashboard, missoes, loja, ...)
        ├── public/                   # favicon.ico, icon-256.png
        └── scripts/gen-favicon.mjs   # gerador procedural do favicon
```

---

## Portas e endereços

| Serviço | URL |
|---------|-----|
| Backend (HTTP) | `http://localhost:5008` |
| Backend — base da API | `http://localhost:5008/api` |
| Swagger UI | `http://localhost:5008/swagger` |
| Frontend (dev) | `http://localhost:4200` |
| Uploads (fotos) | `http://localhost:5008/uploads/...` |

A base da API está **hardcoded** no frontend em `src/app/services/api.service.ts`:

```ts
const BASE = 'http://localhost:5008/api';
```

---

## Configuração relevante (Program.cs)

- **CORS:** política default liberada (`AllowAnyOrigin/Method/Header`) — ok para dev local.
- **Upload:** limite de **5 MB** por arquivo (`MultipartBodyLengthLimit`).
- **Static files:** `app.UseStaticFiles()` serve `wwwroot/uploads/` para as fotos de perfil.
- **DI:** `XpService` e `GachaService` registrados como `Scoped`.

## Connection string (appsettings.json)

```
Host=localhost;Database=isekai;Username=postgres;Password=postgres
```

Autenticação por usuário/senha do PostgreSQL. As credenciais de dev ficam no `appsettings.json`; em produção use variáveis de ambiente ou user-secrets.

---

## Como rodar

### Pré-requisitos
- .NET SDK 10
- PostgreSQL (instância local `localhost:5432`)
- Node.js + npm

### Backend
```bash
cd backend/IsekaiDesnecessario.API
dotnet ef database update     # cria/atualiza o banco com todas as migrations
dotnet run                    # sobe em http://localhost:5008
```

> ⚠️ O processo `IsekaiDesnecessario.API` **trava o .exe** enquanto roda. Pare o `dotnet run` antes de `dotnet build` ou `dotnet ef`.

### Frontend
```bash
cd frontend/isekai-desnecessario-app
npm install
npm start                     # ng serve em http://localhost:4200
```

### Após dar `git pull` com novas migrations
```bash
cd backend/IsekaiDesnecessario.API
dotnet ef database update
```

---

## Fluxo de uma requisição

```
[Angular page] → ApiService (HttpClient)
      │  GET/POST http://localhost:5008/api/...
      ▼
[Controller] ── valida ──→ [Service: XpService/GachaService]
      │                          │
      ▼                          ▼
[AppDbContext] ←──── EF Core ────┘
      │
      ▼
[PostgreSQL: isekai]
```

O frontend guarda apenas o **id do perfil ativo** em `localStorage` (`solo_perfil_id`); todo o resto do estado vem da API a cada navegação.
