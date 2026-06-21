# 🗄️ Banco de Dados

Banco: **`isekai`** (PostgreSQL). Mapeado por EF Core (provider Npgsql) em `Data/AppDbContext.cs`.

> 💡 PostgreSQL guarda texto em UTF-8 nativamente — emoji em `INSERT` manual **não** precisa de prefixo especial (o `N'🍕'` era exigência do SQL Server).

---

## DbSets (tabelas)

| Propriedade (`AppDbContext`) | Tabela | Modelo |
|------------------------------|--------|--------|
| `Usuarios` | Usuarios | `Usuario` |
| `Perfis` | Perfis | `Perfil` |
| `Classes` | Classes | `Classe` |
| `BonsHabitos` | BonsHabitos | `BomHabito` |
| `MausHabitos` | MausHabitos | `MauHabito` |
| `Missoes` | Missoes | `Missao` |
| `TiposMissao` | TiposMissao | `TipoMissao` |
| `Recompensas` | Recompensas | `Recompensa` |
| `Inventario` | Inventario | `ItemInventario` |
| `HistoricoXp` | HistoricoXp | `HistoricoXp` |
| `Atributos` | Atributos | `Atributo` |
| `DiarioAcoes` | DiarioAcoes | `DiarioAcao` |
| `SnapshotsAtributo` | SnapshotsAtributo | `SnapshotAtributo` |
| `Experimentos` | Experimentos | `Experimento` |
| `ExperimentosDia` | ExperimentosDia | `ExperimentoDia` |

---

## Diagrama de relacionamentos

```
Usuario (conta Google) ── Role (papel de acesso)
  └─1:N─ Perfil  (UsuarioId nullable → null = convidado)
            │
            ├─ ClasseId ──→ Classe ──→ Atributo
            ├─1:N─ ItemInventario (snapshot de Recompensa)
            ├─1:N─ HistoricoXp
            ├─1:N─ DiarioAcao
            ├─1:N─ SnapshotAtributo ── AtributoId ─→ Atributo
            ├─1:N─ Experimento    ─1:N─ ExperimentoDia
            │
            │   ── Ativações (estado por-perfil; N:N com as definições) ──
            ├─1:N─ PerfilBomHabito  ──→ BomHabito   (definição)
            ├─1:N─ PerfilMauHabito  ──→ MauHabito   (definição)
            ├─1:N─ PerfilMissao     ──→ Missao      (definição)
            └─1:N─ PerfilRecompensa ──→ Recompensa  (definição)

Catálogo (definições — global ou próprio; sem PerfilId):
  BomHabito / MauHabito / Missao / Recompensa
    ── AtributoId ─→ Atributo
    ── CriadoPorUsuarioId ─→ Usuario (autor; SetNull)
    ── Escopo (Global/Proprio) · Status (Aprovado/Pendente/Rejeitado)
    ── N:N ─→ Classe (vínculo multi-classe; vazio = vale p/ todas)
  Missao ── TipoId ─→ TipoMissao · MissaoPrincipalId ─→ Missao (auto-relação)
```

> **Modelo de catálogo (v0.7):** o conteúdo (hábitos/missões/recompensas) deixou de pertencer
> a um perfil. Cada item é uma **definição** (global ou própria) e cada perfil **ativa** itens via
> as tabelas `Perfil*`. O estado por-perfil (streak, conclusão, última execução, trava) vive na ativação.

### Regras de exclusão (`OnDelete`)
- **Apagar `Usuario`** → seus `Perfil` viram **convidados** (`UsuarioId = null`); as definições que ele criou viram **sem autor** (`CriadoPorUsuarioId = null`).
- **Apagar `Classe`/`Atributo`** → FKs viram `null` (`SetNull`) nos perfis/hábitos/missões/recompensas.
- **Apagar `Perfil`** → remove o herói e suas **ativações** (`Cascade`); as definições do catálogo permanecem.
- **Apagar uma definição** → remove em **cascata** as ativações dela em todos os perfis.

---

## Modelos

### Usuario — conta (Google **ou** e-mail/senha)
| Campo | Tipo | Observação |
|-------|------|-----------|
| Id | int | PK |
| GoogleId | string? | `sub` do token Google — **índice único**; null em conta criada por e-mail/senha |
| Email | string | **índice único** |
| EmailVerificado | bool | |
| Nome | string | nome da conta |
| FotoUrl | string? | avatar (Google) |
| SenhaHash | string? | hash **BCrypt** do login próprio; null em conta só-Google. `[JsonIgnore]` — nunca sai na API |
| Role | string | papel de acesso — `Usuario` (default) · `VIP` · `Moderador` · `Admin`. Gravado como texto (`varchar(20)`) p/ ajuste manual. `UsuarioId = 1` é forçado a `Admin` no login (hardcoded). **Não confundir com Classe RPG** (que é do Perfil) |
| CriadoEm / UltimoLogin | DateTime | auditoria (UTC) |

### Perfil — o herói
| Campo | Tipo | Default | Observação |
|-------|------|---------|-----------|
| Id | int | | PK |
| UsuarioId | int? | null | null = convidado |
| Nome | string | | |
| Genero | string? | | "Masculino" / "Feminino" / "Não-binário" |
| ClasseId | int? | | FK → Classe |
| Xp | int | 0 | XP **dentro do nível atual** |
| Moedas | int | 0 | moeda de troca |
| Rank | string | "H" | calculado por `XpService` |
| Titulo | string | "Iniciante" | calculado por `XpService` |
| Nivel | int | 1 | |
| ProximoNivelXp | int | 100 | XP necessário p/ o próximo nível |
| FotoUrl | string? | | `/uploads/...` |
| XpHoje | int | 0 | XP acumulado no dia (lootbox) |
| DataXpHoje | DateTime? | | reseta `XpHoje` à meia-noite |
| UltimaLootbox | DateTime? | | controla 1×/dia |
| DesafioRecusadoEm / DesafioConcluidoEm | DateTime? | | desafio do dia |

### Classe — seed fixo (6)
| Id | Nome | Feminino | Emoji | AtributoId |
|----|------|----------|-------|------------|
| 1 | Mago | Maga | 🧙 | 1 — Inteligência |
| 2 | Bardo | Barda | 🎵 | 2 — Sabedoria |
| 3 | Guerreiro | Guerreira | ⚔️ | 3 — Físico |
| 4 | Escudeiro | Escudeira | 🛡️ | 4 — Disciplina |
| 5 | Executor | Executora | 🎯 | 5 — Foco |
| 6 | Clérigo | Clériga | ✨ | 6 — Vitalidade |

### Atributo — seed fixo (6)
| Id | Nome | Emoji | Cor | Descrição |
|----|------|-------|-----|-----------|
| 1 | Inteligência | 🧠 | `#58a6ff` | Estudar, fazer cursos, resolver exercícios |
| 2 | Sabedoria | 📚 | `#bc8cff` | Ler livros, podcasts, reflexão |
| 3 | Físico | 💪 | `#3fb950` | Treinar, academia, exercícios físicos |
| 4 | Disciplina | ⚙️ | `#f78166` | Tarefas domésticas, rotina, pontualidade |
| 5 | Foco | 🎯 | `#ffd700` | Pomodoro, sem celular, deep work |
| 6 | Vitalidade | ❤️ | `#f85149` | Sono, hidratação, dieta, pausas |

> **Campos de catálogo** (presentes em **BomHabito, MauHabito, Missao e Recompensa**): `Escopo`
> (`Global`/`Proprio`, texto, default `Global`), `Status` (`Aprovado`/`Pendente`/`Rejeitado`, texto,
> default `Aprovado`) e `CriadoPorUsuarioId` (int? → `Usuario`, autor; `SetNull`). Definições **não têm `PerfilId`**.

### BomHabito / MauHabito — definição (mesma estrutura)
| Campo | Tipo | Observação |
|-------|------|-----------|
| Id | int | PK |
| Habito | string | descrição |
| Xp | int | XP ganho (bom) / perdido (mau) |
| Frequencia | string | `Diário` · `Semanal` · `Mensal` · `Livre` |
| AtributoId | int? | FK → Atributo |
| Escopo / Status / CriadoPorUsuarioId | — | campos de catálogo (acima) |

### Missao — definição
| Campo | Tipo | Observação |
|-------|------|-----------|
| Id | int | PK |
| Titulo | string | |
| TipoId | int | FK → TipoMissao (1=Principal, 2=Secundária, 3=Desafio) |
| RecompensaXp | int | XP ao concluir |
| RecompensaMoedas | int | moedas ao concluir |
| DataLimite | DateTime? | prazo (expira) |
| MissaoPrincipalId | int? | vincula secundária a uma principal (id de outra definição) |
| AtributoId | int? | FK → Atributo |
| Escopo / Status / CriadoPorUsuarioId | — | campos de catálogo (acima) |

> `TipoMissao` tem seed fixo via migration `SeedTiposMissao` — aplicado automaticamente no startup.

### Recompensa — definição
| Campo | Tipo | Default | Observação |
|-------|------|---------|-----------|
| Id | int | | PK |
| Nome / Descricao | string | | |
| Emoji | string | 🎁 | |
| Preco | int | | custo em moedas |
| Ativa | bool | true | vitrine ligada/desligada (criador) |
| AtributoId | int? | | requisito opcional de atributo |
| PontosNecessarios | int | 0 | pontos de atributo exigidos p/ resgatar |
| Escopo / Status / CriadoPorUsuarioId | — | | campos de catálogo (acima) |

### Vínculo multi-classe — join tables N:N (item ↔ `Classe`)
Tabelas implícitas (geradas pelo EF) ligando cada definição às Classes RPG às quais o item é exclusivo:
`BomHabitoClasse`, `ClasseMauHabito`, `ClasseMissao`, `ClasseRecompensa` — PK composta `(DefId, ClassesId)`, cascade nas duas pontas.
Sem linhas para um item = item **sem classe** (aparece para todos). Só conteúdo **global** usa o vínculo.

### Ativações — `PerfilBomHabito` / `PerfilMauHabito` / `PerfilMissao` / `PerfilRecompensa`
Estado por-perfil de cada definição ativada. Índice **único** `(PerfilId, <Def>Id)`. Cascade ao apagar perfil **ou** definição.
| Campo | Tipo | Observação |
|-------|------|-----------|
| Id | int | PK |
| PerfilId | int | FK → Perfil |
| BomHabitoId / MauHabitoId / MissaoId / RecompensaId | int | FK → definição |
| Ativo | bool | item está na lista/loja do perfil |
| Streak | int | hábitos e missões |
| UltimaExecucao | DateTime? | cooldown (hábitos) |
| Concluida / ConcluidaEm | bool / DateTime? | missões |
| TravadoAte | DateTime? | trava por timer (enforcement na Parte 5) |

### ItemInventario — recompensa comprada (desnormalizada)
Guarda cópia de Nome/Emoji/Descricao/Preco para sobreviver à exclusão da recompensa. Campos: `RecompensaId`, `DataCompra`, `DataUso`, `Usado`.

### HistoricoXp — telemetria diária
`Data` (DateOnly), `XpHoje`, `Nivel`, `Moedas` e `XpPorHora` — array de **24 ints** serializado em JSON na coluna `XpPorHoraJson` (`[NotMapped]` no getter/setter).

### SnapshotAtributo — foto do progresso
`AtributoId`, `Pontos`, `Data`. Usado para comparar a evolução dos atributos ao longo do tempo (página de gráficos).

### DiarioAcao — feed de conquistas
`Mensagem`, `Emoji`, `Tipo` (`nivel`, `habito_bom`, `habito_mau`, `missao`, `recompensa`, `lootbox`), `Data`. Gerado automaticamente pelos controllers a cada ação relevante.

### Experimento / ExperimentoDia — hábito em teste
`Titulo`, `Descricao`, `DuracaoDias` (default 21), `DataInicio`, `Ativo`, `Convertido`. Cada `ExperimentoDia` registra um dia marcado. Ao completar a duração, vira inativo; pode ser **convertido** em `BomHabito`.

---

## Migrations

O histórico foi **squashado** num único `InitialCreate` (o esquema inteiro — perfis, hábitos, missões, recompensas, inventário, atributos, classes, usuários, experimentos, snapshots, diário — nasce nele, já com o seed de atributos e classes). Estado atual em `Migrations/`:

| Migration | O que traz |
|-----------|-----------|
| `InitialCreate` | esquema completo do banco + seed de atributos e classes |
| `AddSenhaHashEGoogleIdNullable` | login por e-mail/senha — coluna `SenhaHash` em `Usuario` e `GoogleId` agora nullable |
| `RenameDesafioConcluidoEm` | renomeia a coluna do desafio do dia |
| `SeedTiposMissao` | seed dos 3 tipos de missão: `Principal`, `Secundária`, `Desafio` |
| `AddRoleUsuario` | papel de acesso — coluna `Role` (`varchar(20)`, default `Usuario`) em `Usuario` |
| `CatalogoGlobalEAtivacoes` | catálogo: definições ganham `Escopo`/`Status`/`CriadoPorUsuarioId` e perdem `PerfilId`; cria as 4 tabelas `Perfil*` (ativações). **Apaga o conteúdo antigo** (virada limpa) |
| `VinculoMultiClasse` | vínculo N:N item ↔ Classe — join tables `BomHabitoClasse`, `ClasseMauHabito`, `ClasseMissao`, `ClasseRecompensa` |

> Em produção (e no `dotnet run` local) as migrations são aplicadas **automaticamente** no startup — `Database.Migrate()` no `Program.cs`.

Comandos (dev):
```bash
dotnet ef migrations add NomeDaMigration   # criar
dotnet ef database update                   # aplicar manualmente
```
