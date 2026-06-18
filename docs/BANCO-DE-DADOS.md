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
Usuario (conta Google)
  └─1:N─ Perfil  (UsuarioId nullable → null = convidado)
            │
            ├─ ClasseId ──→ Classe ──→ Atributo
            ├─1:N─ BomHabito      ── AtributoId ─→ Atributo
            ├─1:N─ MauHabito      ── AtributoId ─→ Atributo
            ├─1:N─ Missao         ── TipoId ─→ TipoMissao
            │                       ── AtributoId ─→ Atributo
            │                       ── MissaoPrincipalId ─→ Missao (auto-relação)
            ├─1:N─ Recompensa     ── AtributoId ─→ Atributo
            ├─1:N─ ItemInventario (snapshot de Recompensa)
            ├─1:N─ HistoricoXp
            ├─1:N─ DiarioAcao
            ├─1:N─ SnapshotAtributo ── AtributoId ─→ Atributo
            └─1:N─ Experimento    ─1:N─ ExperimentoDia
```

### Regras de exclusão (`OnDelete`)
- **Apagar `Usuario`** → seus `Perfil` viram **convidados** (`UsuarioId = null`). O progresso é **preservado**.
- **Apagar `Classe`/`Atributo`** → FKs viram `null` (`SetNull`) nos perfis/hábitos/missões/recompensas.
- **Apagar `Perfil`** → remove o herói (os filhos seguem o cascade padrão do EF).

---

## Modelos

### Usuario — conta Google
| Campo | Tipo | Observação |
|-------|------|-----------|
| Id | int | PK |
| GoogleId | string | `sub` do token Google — **índice único** |
| Email | string | **índice único** |
| EmailVerificado | bool | |
| Nome | string | nome da conta Google |
| FotoUrl | string? | avatar do Google |
| CriadoEm / UltimoLogin | DateTime | auditoria |

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

### BomHabito / MauHabito (mesma estrutura)
| Campo | Tipo | Observação |
|-------|------|-----------|
| Id | int | PK |
| PerfilId | int | FK |
| Habito | string | descrição |
| Xp | int | XP ganho (bom) / perdido (mau) |
| Frequencia | string | `Diário` · `Semanal` · `Mensal` · `Livre` |
| Streak | int | sequência |
| UltimaExecucao | DateTime? | controla cooldown |
| AtributoId | int? | FK → Atributo |

### Missao
| Campo | Tipo | Observação |
|-------|------|-----------|
| Id | int | PK |
| PerfilId | int | FK |
| Titulo | string | |
| TipoId | int | FK → TipoMissao (1=Principal, 2=Secundária, 3=Desafio) |
| RecompensaXp | int | XP ao concluir |
| RecompensaMoedas | int | moedas ao concluir |
| Streak | int | |
| Concluida | bool | |
| ConcluidaEm | DateTime? | usado na Jornada |
| DataLimite | DateTime? | prazo (expira) |
| MissaoPrincipalId | int? | vincula secundária a uma principal |
| AtributoId | int? | FK → Atributo |

> ⚠️ **`TipoMissao` está vazia no banco** — não há seed. Popule antes de criar missões:
> ```sql
> INSERT INTO TiposMissao (Id, Nome) VALUES (1,'Principal'),(2,'Secundária'),(3,'Desafio');
> ```

### Recompensa
| Campo | Tipo | Default | Observação |
|-------|------|---------|-----------|
| Id | int | | PK |
| PerfilId | int | | FK |
| Nome / Descricao | string | | |
| Emoji | string | 🎁 | |
| Preco | int | | custo em moedas |
| Ativa | bool | true | aparece na loja/lootbox |
| AtributoId | int? | | requisito opcional de atributo |
| PontosNecessarios | int | 0 | pontos de atributo exigidos p/ resgatar |

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

## Migrations (27)

Histórico cronológico em `Migrations/`. Destaques:

| Migration | O que adicionou |
|-----------|-----------------|
| `InitialCreate` | esquema base (perfil, hábitos, missões, recompensas) |
| `AddFotoUrlToPerfil` | foto de perfil |
| `AddLootboxToPerfil` | XpHoje/DataXpHoje/UltimaLootbox |
| `AddHistoricoXp` | telemetria diária |
| `AddInventario` | inventário de itens |
| `AddAtributos` | 6 atributos + seed |
| `AddAtributoMissao` / `AddRecompensaAtributo` | FKs de atributo |
| `AddDiarioAcoes` | feed de conquistas |
| `AddDesafioStatus` / `AddDesafioRecusadoEm` | desafio do dia |
| `AddSnapshotAtributo` | snapshots p/ gráficos |
| `AddExperimentos` | hábitos em teste |
| `AddMissaoDataLimite` / `AddMissaoPrincipalId` | prazo e vínculo de missões |
| `AddMissaoConcluidaEm` | data de conclusão (Jornada) |
| `AddUsuarios` | conta Google (auth) |
| `AddClasseEGenero` | classes + gênero |
| `AddClasseNomeFeminino` | nome feminino das classes |

Comandos:
```bash
dotnet ef migrations add NomeDaMigration   # criar
dotnet ef database update                   # aplicar
```
