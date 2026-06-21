# Isekai Desnecessário — Contexto do Projeto

## O que é
RPG **habit tracker**: transforma a rotina real numa aventura. Ganha XP, sobe de nível/rank,
completa missões, junta moedas e troca por recompensas reais.

> 📖 **Documentação completa em [`docs/`](docs/README.md)** — arquitetura, banco, API, mecânicas e frontend.

## Stack
- **Backend:** ASP.NET Core 10 (Web API) + EF Core + PostgreSQL (Npgsql) — porta `5008`
- **Frontend:** Angular 19 (standalone) — porta `4200`
- **Banco:** `isekai` (PostgreSQL local, usuário/senha)

## Estrutura
- `backend/IsekaiDesnecessario.API/` — API (Controllers, Models, Services, Data, Migrations)
- `frontend/isekai-desnecessario-app/` — Angular
- `IsekaiDesnecessario.slnx` — solução
- `docs/` — documentação completa

## Git e versionamento (regras de subida)
- **Branch:** toda alteração sobe **sempre** na branch `temp-luan-casa` — nunca commitar/pushar direto na `main`.
- **Antes de subir:** ao chegar num ponto de subida, **perguntar** se quer (a) commitar/pushar agora ou (b) continuar com mais alterações. Só commitar/pushar após o "ok".
- **Documentação antes do commit:** ao receber pedido de commit/push, **antes de commitar**, varrer todos os arquivos de `docs/` e o `CLAUDE.md` e atualizar qualquer informação que diverge do código alterado na sessão. A documentação deve refletir o estado real — só então commitar tudo junto.
- **Versionamento (SemVer):** toda subida **muda a versão** nos quatro lugares, mantendo-os iguais:
  - `backend/IsekaiDesnecessario.API/IsekaiDesnecessario.API.csproj` → `<Version>`
  - `frontend/isekai-desnecessario-app/package.json` → `"version"`
  - `frontend/isekai-desnecessario-app/src/environments/environment.ts` → `version` (exibida na tela de cadastro)
  - `frontend/isekai-desnecessario-app/src/environments/environment.prod.ts` → `version`
  - Regra do incremento: `patch` (fix/refactor/docs) · `minor` (feature nova compatível) · `major` (quebra compatibilidade).

## Progressão (código: `Services/XpService.cs`)
- Nível 1 começa com `ProximoNivelXp = 100`
- **Fórmula:** `XP_próximo = round(XP_atual × 1.036)`
- **Rank** = índice `Nivel / 10`: 1–9 **H**, 10–19 **G**, 20–29 **F**, 30–39 **E**, 40–49 **D**,
  50–59 **C**, 60–69 **B**, 70–79 **A**, 80–89 **S**, 90–99 **SS**, 100+ **SSS**
- Maus hábitos deduzem XP e podem regredir nível

## Mecânicas-chave
- **Catálogo (v0.7):** hábitos/missões/recompensas são **definições** (catálogo global ou conteúdo próprio), separadas da **ativação** no perfil. Cada perfil ativa itens (tabelas `Perfil*`); o estado por-perfil (streak, conclusão, última execução, trava) vive na ativação. As APIs antigas devolvem o item **achatado** (definição + ativação) no formato de sempre. Ver [`docs/BANCO-DE-DADOS.md`](docs/BANCO-DE-DADOS.md) e [`docs/API.md`](docs/API.md).
- **Atributos (6):** Inteligência🧠, Sabedoria📚, Físico💪, Disciplina⚙️, Foco🎯, Vitalidade❤️
- **Classes (6):** Mago, Bardo, Guerreiro, Escudeiro, Executor, Clérigo (com nome feminino; cada uma ligada a um atributo)
- **Missões:** Principal / Secundária / Desafio — dão XP **e moedas** (única fonte de moedas).
  Concluir uma principal auto-conclui as secundárias vinculadas (`MissaoPrincipalId`).
- **Lootbox:** 1× por dia, exige **1000 XP acumulados no dia**; sorteio ponderado (mais barato = mais chance).
- **Recompensas:** compradas com moedas, vão para o inventário.
- **Experimentos:** hábitos em teste (21 dias) que podem virar bom hábito.

## Autenticação (🚧 em construção)
Login Google. `Usuario` (conta) 1→N `Perfil`. `Perfil.UsuarioId` null = convidado.
Limite de 3 perfis por conta. Fases: banco ✅ · OAuth Client ID (externo) · backend JWT · frontend · vincular convidados.

## Papéis de acesso (Role) — na conta `Usuario`
- Campo `Role` em `Usuario` (não no `Perfil`): `Usuario` (default) · `VIP` · `Moderador` · `Admin`. **Não confundir com Classe RPG** (Mago, Bardo… que é do `Perfil`).
- Gravado como texto (`varchar(20)`) — ajuste manual no banco é fácil. `UsuarioId = 1` é forçado a `Admin` no login (`AuthService.GarantirAdminInicialAsync`, hardcoded por enquanto).
- **Criação de conteúdo por papel (backend, v0.7):** Admin → global aprovado · Moderador → global pendente · VIP → próprio aprovado · Usuário comum → **403**. Editar/excluir definição: só o autor ou Admin. Front expõe `auth.role()`/`isAdmin()`/`podeCatalogoGlobal()` etc.
- Falta a **UI de catálogo** (navegar/ativar/desativar) por papel → **Parte 7**. Fluxo de aprovação Moderador→Admin → **Parte 5**. Notificações → **Parte 6**. Multi-classe nos itens → **Parte 3**.

## Notas de ambiente (Windows)
- O processo `IsekaiDesnecessario.API` trava o `.exe` — pare o `dotnet run` antes de `dotnet build`/`dotnet ef`.
- Após `git pull` com migrations novas: `dotnet ef database update` em `backend/IsekaiDesnecessario.API`.
- PostgreSQL guarda texto em UTF-8 nativamente — emoji em INSERT manual **não** precisa do prefixo `N` (isso era do SQL Server).
- `TiposMissao` ainda **não tem seed** — popular `Principal`/`Secundária`/`Desafio` antes de criar missões.
