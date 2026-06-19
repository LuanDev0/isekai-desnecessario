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
│       ├── Controllers/              # endpoints REST (12 + ApiControllerBase)
│       ├── Models/                   # entidades EF
│       ├── Services/                 # XpService, LootboxService, MissaoService, AuthService
│       ├── Data/AppDbContext.cs      # DbContext + seed (atributos, classes)
│       ├── Migrations/               # migrations EF (histórico squashado em InitialCreate)
│       └── wwwroot/uploads/          # fotos de perfil enviadas
│
└── frontend/
    └── isekai-desnecessario-app/
        ├── src/app/
        │   ├── app.routes.ts         # rotas
        │   ├── models/models.ts      # interfaces TypeScript (espelham os Models C#)
        │   ├── services/             # api, profile, auth, auth.interceptor, auth.guard, language
        │   ├── components/           # navbar, profile-selector
        │   └── pages/                # 10 páginas (dashboard, missoes, loja, ...)
        ├── public/                   # favicon.ico, icon-256.png
        └── scripts/gen-favicon.mjs   # gerador procedural do favicon
```

---

## Portas e endereços

| Serviço | URL (dev) |
|---------|-----|
| Backend (HTTP) | `http://localhost:5008` |
| Backend — base da API | `http://localhost:5008/api` |
| Swagger UI | `http://localhost:5008/swagger` |
| Frontend (dev) | `http://localhost:4200` |
| Uploads (fotos) | `http://localhost:5008/uploads/...` |

**Produção:** API hospedada no Railway — `https://isekai-desnecessario-production.up.railway.app`.

A base da API vem do **environment** (não é mais hardcoded). Em `src/app/services/api.service.ts`:

```ts
import { environment } from '../../environments/environment';
const BASE = environment.apiUrl;   // dev: localhost:5008/api · prod: Railway
```

Os valores ficam em `environment.ts` (dev) e `environment.prod.ts` (produção).

---

## Configuração relevante (Program.cs)

- **Autenticação:** JWT Bearer (`AddAuthentication`/`AddJwtBearer`) validando issuer, audience, lifetime e assinatura (`Jwt:Secret`). `app.UseAuthentication()`/`UseAuthorization()` no pipeline.
- **CORS:** origens vêm de `Cors:AllowedOrigins` (config). Se vazio, cai para permissivo (`AllowAnyOrigin`) — **defina as origens em produção**.
- **Migrations automáticas:** `Database.Migrate()` roda na inicialização — aplica migrations pendentes sem comando manual (vale para produção também).
- **HTTPS:** `app.UseHttpsRedirection()`.
- **Upload:** limite de **5 MB** por arquivo (`MultipartBodyLengthLimit`).
- **Static files:** `app.UseStaticFiles()` serve `wwwroot/uploads/` para as fotos de perfil.
- **DI:** `XpService`, `LootboxService`, `MissaoService` e `AuthService` registrados como `Scoped`.
- **Health check:** `GET /health` retorna `healthy`.

## Connection string (appsettings.json)

```
Host=localhost;Database=isekai;Username=postgres;Password=postgres
```

Autenticação por usuário/senha do PostgreSQL. As credenciais de dev ficam no `appsettings.json`; em produção, o Railway injeta a connection string (e o `Jwt:Secret`) via **variáveis de ambiente** — nada de credenciais reais no repositório.

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
O `dotnet run` já aplica migrations pendentes no startup (`Database.Migrate()`). Para aplicar sem subir a API:
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
[Controller] ─ valida + checa dono ─→ [Service: Xp/Lootbox/Missao/Auth]
      │                          │
      ▼                          ▼
[AppDbContext] ←──── EF Core ────┘
      │
      ▼
[PostgreSQL: isekai]
```

O frontend guarda apenas o **id do perfil ativo** em `localStorage` (`solo_perfil_id`); todo o resto do estado vem da API a cada navegação.
