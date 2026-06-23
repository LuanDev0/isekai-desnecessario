# Isekai Desnecessário — Instruções para o Claude

> Documentação completa em [`docs/`](docs/README.md) — arquitetura, banco, API, mecânicas e frontend.

## Stack (referência rápida)
- **Backend:** ASP.NET Core 10 + EF Core + PostgreSQL — porta `5008` (`backend/IsekaiDesnecessario.API/`)
- **Frontend:** Angular 19 (standalone) — porta `4200` (`frontend/isekai-desnecessario-app/`)
- **Banco:** `isekai` (PostgreSQL local)

## Git e versionamento
- **Branch:** toda alteração sobe **sempre** na branch `temp-luan-casa` — nunca commitar/pushar direto na `main`.
- **Antes de subir:** perguntar se quer (a) commitar/pushar agora ou (b) continuar. Só commitar/pushar após o "ok".
- **Documentação antes do commit:** varrer todos os arquivos de `docs/` e o `CLAUDE.md` e atualizar qualquer divergência. Commitar tudo junto.
- **Versionamento (SemVer):** toda subida muda a versão nos quatro lugares:
  - `backend/IsekaiDesnecessario.API/IsekaiDesnecessario.API.csproj` → `<Version>`
  - `frontend/isekai-desnecessario-app/package.json` → `"version"`
  - `frontend/isekai-desnecessario-app/src/environments/environment.ts` → `version` (exibida na tela de cadastro)
  - `frontend/isekai-desnecessario-app/src/environments/environment.prod.ts` → `version`
  - `patch` (fix/refactor/docs) · `minor` (feature nova compatível) · `major` (quebra compatibilidade)

## Código backend
- **Antes de gerar ou alterar código backend**, consultar [`docs/SKILLS.md`](docs/SKILLS.md) — checklist, padrões EF Core, tratamento de erros e convenções de nomenclatura.
- **Não aplicar Clean Architecture nem CQRS** sem pedido explícito — o projeto ainda usa Controllers direto com `AppDbContext`. Evoluir incrementalmente, uma feature por vez.
- **Idioma:** nomes de domínio em **português** (`Perfil`, `Habito`, `Missao`, `Recompensa`); termos técnicos em **inglês** (`Repository`, `Handler`, `Service`, `Command`, `Query`).

## Notas de ambiente (Windows)
- Pare o `dotnet run` antes de `dotnet build`/`dotnet ef` — o processo trava o `.exe`.
- Após `git pull` com migrations novas: `dotnet ef database update` em `backend/IsekaiDesnecessario.API`.
- PostgreSQL é UTF-8 nativo — emoji em INSERT manual não precisa do prefixo `N` (isso era do SQL Server).
