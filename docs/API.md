# 🔌 API REST

Base (dev): **`http://localhost:5008/api`** · Swagger UI: `http://localhost:5008/swagger`
Base (produção): **`https://isekai-desnecessario-production.up.railway.app/api`**

A maioria das rotas escopa pelo perfil via query string `?perfilId=X` e **exige JWT** (`Authorization: Bearer <token>`) — ver [Autenticação](#autenticação--apiauth).

---

## Autenticação — `/api/auth`

Rotas **públicas** (sem token). Todas devolvem `{ token, usuario, perfis }`.

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/auth/google` | Login Google — valida o `idToken` (Google Identity Services), faz upsert do `Usuario` e emite JWT próprio |
| POST | `/auth/registrar` | Cria conta por e-mail/senha (`{ nome, email, senha }`) — senha guardada com **BCrypt** |
| POST | `/auth/login` | Login por e-mail/senha (`{ email, senha }`) |

O objeto `usuario` inclui o campo `role` (`Usuario` · `VIP` · `Moderador` · `Admin`) — papel de acesso da conta. A conta `Id = 1` é promovida a `Admin` automaticamente no login (hardcoded por enquanto).

O JWT leva o `UsuarioId` no claim `sub` e expira em **168h** (`Jwt:ExpiresHours`). O frontend o injeta via interceptor em todas as chamadas.

---

## Perfil — `/api/perfil`  *(exige token)*

Escopado pela conta do token. `GarantirDonoDoPerfilAsync` impede acesso a perfil de outro usuário (anti-IDOR).

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/perfil/{id}` | Busca um perfil (valida que pertence ao usuário logado) |
| GET | `/perfil/meus` | Lista os perfis **da conta autenticada** |
| GET | `/perfil/orfaos` | Lista perfis **sem dono** (convidados), para reivindicar após login |
| POST | `/perfil` | Cria perfil na conta logada (limite de **3 por conta**; progressão sempre nasce nos defaults do modelo) |
| POST | `/perfil/{id}/vincular` | Reivindica um perfil órfão para a conta logada |
| POST | `/perfil/{id}/desvincular` | Remove o vínculo (perfil volta a convidado; **não** é excluído) |
| PATCH | `/perfil/{id}/info` | Atualiza só **nome, classe e gênero** (`{ nome, classeId, genero }`) |
| DELETE | `/perfil/{id}` | Exclui o perfil |
| POST | `/perfil/{id}/xp?quantidade=N` | Adiciona XP manualmente |
| POST | `/perfil/{id}/reset` | Zera XP/moedas/nível e **apaga hábitos e missões** do perfil |
| POST | `/perfil/{id}/foto` | Upload de foto (`multipart`, campo `arquivo`; ≤5MB; jpg/jpeg/jfif/png/webp/gif) |
| POST | `/perfil/{id}/desafio/recusar` | Marca o desafio do dia como recusado |
| POST | `/perfil/{id}/desafio/concluir` | Marca o desafio do dia como concluído |
| GET | `/perfil/{id}/lootbox/status` | `{ disponivel, xpHoje, xpNecessario:1000, jaAbriuHoje }` |
| POST | `/perfil/{id}/lootbox/abrir` | Abre a lootbox (precisa 1000 XP no dia, 1×/dia) → `{ recompensa, chance }` |

> O frontend também chama `/perfil/{id}/historico` e `/perfil/{id}/historico/upsert` (controller `HistoricoController`).

---

## Classes — `/api/classes`

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/classes` | Lista as 6 classes com atributo aninhado (inclui `nomeFeminino`) |

---

## Hábitos — `/api/habitos`

### Bons
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/habitos/bons?perfilId=X` | Lista bons hábitos |
| POST | `/habitos/bons` | Cria |
| PUT | `/habitos/bons/{id}` | Edita (Habito, Xp, Frequencia, AtributoId) |
| DELETE | `/habitos/bons/{id}` | Exclui |
| POST | `/habitos/bons/{id}/completar?perfilId=X` | Completa → **+XP**, +streak, registra no diário (respeita cooldown da frequência) |

### Maus
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/habitos/maus?perfilId=X` | Lista maus hábitos |
| POST | `/habitos/maus` | Cria |
| PUT | `/habitos/maus/{id}` | Edita |
| DELETE | `/habitos/maus/{id}` | Exclui |
| POST | `/habitos/maus/{id}/registrar?perfilId=X` | Registra → **−XP** (pode regredir nível) |

**Cooldown por frequência:** `Diário` (1×/dia) · `Semanal` (1×/semana, semana começa no domingo) · `Mensal` (1×/mês) · `Livre` (sem cooldown, sempre disponível).

---

## Missões — `/api/missoes`

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/missoes?perfilId=X` | Lista missões (com `Tipo`, ordenadas por TipoId) |
| GET | `/missoes/tipos` | Lista os tipos de missão |
| POST | `/missoes` | Cria |
| PUT | `/missoes/{id}` | Edita |
| DELETE | `/missoes/{id}` | Exclui |
| POST | `/missoes/{id}/completar?perfilId=X` | Conclui → **+XP +moedas**; auto-conclui secundárias vinculadas (`MissaoPrincipalId`) |
| POST | `/missoes/{id}/resetar` | Marca como não concluída |
| GET | `/missoes/jornada?perfilId=X` | Agrega missões concluídas das **últimas 12 semanas** (semana = segunda-feira) → `[{ semana, total, xp, principais }]` |

---

## Recompensas — `/api/recompensas`

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/recompensas?perfilId=X` | Lista (ordenadas por preço) |
| POST | `/recompensas` | Cria |
| PUT | `/recompensas/{id}` | Edita |
| DELETE | `/recompensas/{id}` | Exclui |
| POST | `/recompensas/{id}/resgatar?perfilId=X` | Gasta moedas, valida requisito de atributo, **adiciona ao inventário** |

---

## Inventário — `/api/inventario`

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/inventario?perfilId=X` | Lista itens (mais recentes primeiro) |
| POST | `/inventario/{id}/usar?perfilId=X` | Marca item como usado |

---

## Atributos — `/api/atributos`

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/atributos` | Lista os 6 atributos |

---

## Snapshots de atributos — `/api/snapshots`

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/snapshots/anterior?perfilId=X` | Snapshot anterior (p/ comparar evolução) |
| POST | `/snapshots/salvar?perfilId=X` | Salva pontos atuais `[{ atributoId, pontos }]` |

---

## Diário de conquistas — `/api/diarioacoes`

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/diarioacoes?perfilId=X` | Feed de ações (níveis, hábitos, missões, lootbox, recompensas) |

---

## Experimentos — `/api/experimentos`

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/experimentos?perfilId=X` | Lista (com `Dias`) |
| POST | `/experimentos` | Cria `{ perfilId, titulo, descricao, duracaoDias }` |
| DELETE | `/experimentos/{id}` | Exclui |
| POST | `/experimentos/{id}/dia` | Marca o dia de hoje (encerra ao completar a duração) |
| POST | `/experimentos/{id}/converter?perfilId=X` | Converte em `BomHabito` (10 XP, diário) |

---

## Histórico XP — `/api/perfil/{id}/historico` *(HistoricoController)*

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/perfil/{id}/historico` | Série histórica de XP |
| POST | `/perfil/{id}/historico/upsert` | Insere/atualiza `{ xpHoje, nivel, moedas, hora }` |

---

## Convenções

- **Autenticação:** quase tudo exige `Authorization: Bearer <jwt>`. Públicas: `/auth/*`, `/classes`, `/atributos`. Sem token → `401`; perfil de outra conta → `403`.
- **Escopo por perfil:** quase tudo exige `?perfilId=X` (e o backend confirma que o perfil pertence ao usuário do token).
- **Erros:** `400` (regra de negócio — ex.: "Moedas insuficientes", "cooldown", "XP insuficiente"), `401` (token ausente/inválido), `403` (perfil alheio), `404` (não encontrado).
- **Diário automático:** ações relevantes geram uma `DiarioAcao` no backend, sem chamada extra do frontend.
