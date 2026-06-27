# 🎮 Mecânicas do jogo

Toda a lógica de progressão vive em `Services/XpService.cs` e nos controllers. Esta página descreve **o comportamento real do código**.

---

## XP, níveis, ranks e títulos

### Subir de nível
Implementado em `XpService.AdicionarXp`:

- O perfil começa em **Nível 1** com `ProximoNivelXp = 100`.
- Ao ganhar XP, enquanto `Xp >= ProximoNivelXp`:
  - `Xp -= ProximoNivelXp`
  - `Nivel++`
  - `ProximoNivelXp = round(ProximoNivelXp × 1.036)`
  - recalcula rank/título e registra "🎉 Subiu para Nível N" no diário.

> **Fórmula de progressão:** `XP_próximo = round(XP_atual × 1.036)`.
> Crescimento suave (~3,6% por nível). *Nota: o CLAUDE.md antigo citava 1.35 — o valor real do código é **1.036**.*

### Perder XP (maus hábitos)
`XpService.DeduzerXp`:
- Subtrai a penalidade do XP.
- Se ficar negativo e `Nivel > 1`, **regride** de nível (inverte a fórmula: `xpAnterior = round(ProximoNivelXp / 1.036)`).
- No Nível 1, o XP nunca fica abaixo de 0.

### Rank e título
Derivados do nível: **índice = `Nivel / 10`** (divisão inteira).

| Faixa de nível | Rank | Título |
|----------------|------|--------|
| 1–9 | **H** | Iniciante |
| 10–19 | **G** | Aprendiz |
| 20–29 | **F** | Aventureiro |
| 30–39 | **E** | Veterano |
| 40–49 | **D** | Renomado |
| 50–59 | **C** | Eminente |
| 60–69 | **B** | Especialista |
| 70–79 | **A** | Campeão |
| 80–89 | **S** | Herói |
| 90–99 | **SS** | Mestre |
| 100–109 | **SSS** | Glorioso |
| 110+ | SSS | Radiante → Supremo → Lendário → Divino → **Deus Único** |

> O array de **ranks** trava em `SSS` (índice ≥ 10); os **títulos** continuam evoluindo até "Deus Único" (16 títulos no total).

---

## Atributos (6)

Representam as áreas da vida. Cada bom hábito, mau hábito, missão e recompensa pode ser ligado a um atributo.

| 🧠 Inteligência | 📚 Sabedoria | 💪 Físico | ⚙️ Disciplina | 🎯 Foco | ❤️ Vitalidade |
|---|---|---|---|---|---|

**Pontos de um atributo** (usado em sugestões e requisitos de recompensa):

Os pontos são **persistidos** na tabela `PontosAtributos` a cada conclusão — imutáveis em relação a mudanças futuras de XP.

- Completar **bom hábito** com `AtributoId` → `+Xp / 10` pontos no atributo.
- Completar **missão** (ou secundária vinculada) com `AtributoId` → `+RecompensaXp / 10` pontos no atributo.

> Perfis criados antes da v0.14.0 começam com 0 pontos — sem retroativo.

---

## Classes (6)

Identidade do herói, cada uma atrelada a um atributo. Têm **nome masculino e feminino** — exibido conforme o `Genero` do perfil.

| Classe (M / F) | Emoji | Atributo |
|----------------|-------|----------|
| Mago / Maga | 🧙 | Inteligência |
| Bardo / Barda | 🎵 | Sabedoria |
| Guerreiro / Guerreira | ⚔️ | Físico |
| Escudeiro / Escudeira | 🛡️ | Disciplina |
| Executor / Executora | 🎯 | Foco |
| Clérigo / Clériga | ✨ | Vitalidade |

> Ideia futura: classes especiais não-unissex serão adicionadas no banco com a mesma lógica de gênero.

---

## Bons e maus hábitos

- **Bom hábito** completado → **+Xp** (via `XpService`), `Streak++`, entrada no diário ✅.
- **Mau hábito** registrado → **−Xp** (pode regredir nível), `Streak++`, entrada no diário ❌.
- **Cooldown** controlado por `Frequencia` + `UltimaExecucao`:
  - `Diário` → 1×/dia
  - `Semanal` → 1×/semana (semana começa no **domingo**)
  - `Mensal` → 1×/mês
  - `Livre` → sem cooldown, sempre disponível (não grava `UltimaExecucao`)

---

## Missões

Motor narrativo: dão **XP e moedas** (única fonte de moedas do jogo).

### Tipos
| TipoId | Tipo | Papel |
|--------|------|-------|
| 1 | **Principal** | grandes arcos, recompensa alta |
| 2 | **Secundária** | objetivos menores, podem ser vinculados a uma principal |
| 3 | **Desafio** | missões especiais de alto risco/recompensa |

### Vínculo principal ↔ secundária
Uma secundária aponta para uma principal via `MissaoPrincipalId`. **Ao ativar a principal** no catálogo, todas as secundárias vinculadas aprovadas são **auto-ativadas** junto. **Ao concluir a principal**, todas as secundárias vinculadas ainda abertas são **auto-concluídas** (somando seus XP e moedas).

Secundárias vinculadas não aparecem no catálogo de configurações — são gerenciadas exclusivamente via a principal. Secundárias sem `MissaoPrincipalId` (isoladas) continuam aparecendo normalmente.

### Conclusão
`POST /missoes/{id}/completar`:
1. Marca `Concluida = true`, grava `ConcluidaEm`, `Streak++`.
2. Conclui secundárias vinculadas.
3. Soma `RecompensaXp` (via `XpService`) e `RecompensaMoedas` ao perfil.
4. Registra ⚔️ no diário.

### Prazo
`DataLimite` opcional. No frontend, missões vencidas aparecem como **"Expirou"** e não podem ser concluídas (a expiração de uma principal também expira suas secundárias).

### Jornada
`GET /missoes/jornada` agrega as conclusões das **últimas 12 semanas** (agrupadas por segunda-feira) — alimenta a timeline SVG na página de Missões.

> `TiposMissao` tem seed fixo (migration `SeedTiposMissao`) — aplicado automaticamente. A página de Missões filtra por `tipo?.nome` igual a `Principal`, `Secundária` e `Desafio`.

---

## Recompensas e moedas

- **Moedas** só vêm de **missões concluídas**.
- **Recompensas** são prazeres reais que o jogador define (ex.: "pedir comida", "sessão de jogo").
- **Resgatar** (`/recompensas/{id}/resgatar`):
  - exige moedas suficientes;
  - se a recompensa tiver `AtributoId` + `PontosNecessarios > 0`, valida os **pontos do atributo**;
  - desconta as moedas e cria um `ItemInventario` (cópia desnormalizada).
- **Inventário:** itens resgatados podem ser marcados como **usados** (`/inventario/{id}/usar`).

---

## Lootbox diária

Implementada no `LootboxService`, exposta pelo `PerfilController` (`/lootbox/status` e `/lootbox/abrir`):

- **Requisito:** acumular **1000 XP no mesmo dia** (`XpHoje`), que reseta à meia-noite.
- **Frequência:** **1× por dia** (`UltimaLootbox`).
- **Sorteio ponderado:** entre as recompensas **ativas** do perfil, mais barata = mais chance.
  `peso = precoMáximo − preço + 1`. Retorna a recompensa sorteada e a **chance %**.
- Registra 📦 no diário.

> O "dia" da lootbox e do XP diário usa **UTC** (`DateTime.UtcNow.Date`) — consistente entre usuários de qualquer fuso. *(O antigo `GachaService` de sorteio uniforme foi removido.)*

---

## Desafio do dia

O perfil tem `DesafioRecusadoEm` e `DesafioConcluidoEm`. Endpoints `/desafio/recusar` e `/desafio/concluir` registram a interação do dia. A sugestão é montada no frontend a partir do **atributo mais fraco**.

---

## Experimentos (hábitos em teste)

Para testar um hábito antes de adotá-lo de vez:
- Cria-se um experimento com **duração** (default 21 dias).
- Marca-se **um dia por vez** (`/dia`); ao completar a duração, fica inativo.
- Pode ser **convertido** num `BomHabito` (10 XP, frequência diária).

---

## Contas e perfis

> ✅ **Implementado** (web). Login por **Google** e por **e-mail/senha**.

- Modelo: **`Usuario`** (conta) **1 → N** `Perfil` (heróis). A conta pode ser Google (`GoogleId`), e-mail/senha (`SenhaHash` BCrypt) ou ambos no mesmo e-mail.
- `Perfil.UsuarioId` é nullable: **null = convidado**, preenchido = pertence à conta.
- **Limite de 3 perfis** por conta (`PerfilController.MaxPerfisPorConta`); convidados não contam.
- Apagar a conta **não apaga** os heróis — eles viram convidados (progresso preservado).
- **Reivindicar convidados:** após logar, o usuário vê perfis órfãos (`/perfil/orfaos`) e pode vinculá-los à conta (`/perfil/{id}/vincular`).

**Fluxo Google:** frontend obtém ID Token (Google Identity Services) → `POST /api/auth/google` → backend valida (`Google.Apis.Auth`) → upsert `Usuario` → emite JWT próprio com `UsuarioId` no claim `sub`. O JWT protege todas as rotas de dados, e cada acesso a perfil confere o dono (anti-IDOR via `GarantirDonoDoPerfilAsync`).

**Fases:** 1-Banco ✅ · 2-OAuth Client ID ✅ · 3-Backend JWT ✅ · 4-Frontend (botão, interceptor, guards) ✅ · 5-Vincular convidados ✅.

> 📱 **Mobile:** dentro de uma WebView (Capacitor) o login Google via JS é bloqueado pelo Google — exigirá plugin nativo. Login por e-mail/senha funciona normalmente.
