# 🖥️ Frontend (Angular 19)

App standalone (sem NgModules), componentes com `inject()` e signals. Estado mínimo no cliente — quase tudo vem da API.

---

## Rotas (`app.routes.ts`)

| Caminho | Componente | Página |
|---------|-----------|--------|
| `/cadastro` | `CadastroComponent` | Login/criação de perfil (tela de entrada) |
| `` (raiz) | `DashboardComponent` | **Início** — visão geral do herói |
| `/missoes` | `MissoesComponent` | Missões (principais, secundárias, desafios, jornada) |
| `/status` | `StatusComponent` | Atributos e estatísticas |
| `/loja` | `LojaComponent` | Loja de recompensas |
| `/grafico` | `GraficoComponent` | Gráficos de evolução |
| `/inventario` | `InventarioComponent` | Itens resgatados |
| `/configuracoes` | `ConfiguracoesComponent` | Configurações (Perfil, hábitos, missões, recompensas) |
| `/laboratorio` | `LaboratorioComponent` | Experimentos (hábitos em teste) |
| `/mundo` | `MundoComponent` | Mapa de ranks / mundo |

> A **navbar inferior** linka 7 páginas: Início, Missões, Status, Inventário, Loja, Gráfico, Config. As páginas **Laboratório** e **Mundo** são rotas acessadas a partir de outras telas (não ficam na navbar).

---

## Páginas

### Cadastro (`/cadastro`)
Porta de entrada. Cria o herói (nome, classe, gênero, foto) ou seleciona um perfil existente. Mostra o logo/ícone e o subtítulo *"Seu isekai começa aqui. Nenhum caminhão-chan necessário."*. O nome da classe no combobox muda conforme o gênero (masculino/feminino).

### Dashboard / Início (`/`)
Visão geral: barra de XP, nível, rank, moedas, lista de bons/maus hábitos para marcar no dia, desafio do dia e diário de conquistas.

### Missões (`/missoes`)
Três seções colapsáveis — **Principais** (⭐ laranja), **Secundárias** (✅ azul) e **Desafios** (⚡ roxo). Cada item mostra +XP, +moedas e prazo. Inclui **Missão Sugerida** (baseada no atributo mais fraco) e a **Jornada** das últimas 12 semanas (timeline SVG).

### Status (`/status`)
Pontuação de cada um dos 6 atributos e estatísticas do herói.

### Loja (`/loja`)
Catálogo de recompensas com emoji, descrição e preço em moedas. Botão **Resgatar** desconta moedas e envia ao inventário.

### Inventário (`/inventario`)
Itens resgatados; permite marcar como **usado**.

### Gráfico (`/grafico`)
Evolução de XP/atributos ao longo do tempo (usa `HistoricoXp` e `SnapshotAtributo`).

### Configurações (`/configuracoes`)
Abas para gerenciar o **Perfil** (foto, nome, classe, gênero) e CRUD de **bons hábitos**, **maus hábitos**, **missões** e **recompensas**.

### Laboratório (`/laboratorio`)
Cria e acompanha **experimentos** (hábitos em teste, default 21 dias). Marca um dia por vez e converte em bom hábito.

### Mundo (`/mundo`)
Visão do progresso por **rank** (H → SSS), com faixas de nível, cor e ícone de cada rank.

---

## Componentes compartilhados

| Componente | Função |
|-----------|--------|
| `navbar` | Barra de navegação inferior (7 ícones SVG) |
| `profile-selector` | Troca de perfil ativo; escuta `fotoAtualizada$` para recarregar a foto |

---

## Serviços

### `ApiService` (`services/api.service.ts`)
Fachada única para a API REST. `BASE = http://localhost:5008/api`. Métodos cobrem perfil, classes, hábitos, missões, recompensas, inventário, atributos, lootbox, desafio, diário, jornada, snapshots, experimentos e histórico. Ver [API.md](API.md) para o mapa completo.

### `ProfileService` (`services/profile.service.ts`)
Estado do **perfil ativo** com signal:
- `perfilAtivo` — signal readonly do perfil atual.
- `getSavedId()` / `setPerfilAtivo()` / `clearPerfil()` — persistência em `localStorage` (chave **`solo_perfil_id`**).
- `id` — getter de conveniência (perfil ativo → localStorage → 0).
- `fotoAtualizada$` — `Subject` emitido quando a foto muda, para o selector recarregar.

> A chave do localStorage ainda é `solo_perfil_id` (legado do nome antigo "Solo Leveling"); funcional, mas pode ser renomeada futuramente.

---

## Models TypeScript (`models/models.ts`)

Espelham as entidades do backend (camelCase): `Perfil`, `Classe`, `Atributo`, `BomHabito`, `MauHabito`, `Missao`, `TipoMissao`, `Recompensa`, `ItemInventario`, `DiarioAcao`, `Experimento`/`ExperimentoDia`, `JornadaSemana`. Detalhes de cada campo em [BANCO-DE-DADOS.md](BANCO-DE-DADOS.md).

---

## Padrão de carregamento das páginas

Quase todas seguem o mesmo `ngOnInit`:

```ts
const savedId = this.profile.getSavedId();
if (!savedId) { this.router.navigate(['/cadastro']); return; }   // sem perfil → cadastro
if (!this.profile.perfilAtivo()) {
  this.api.getPerfil(savedId).subscribe(p => { this.profile.setPerfilAtivo(p); this.carregar(); });
} else {
  this.carregar();
}
```

`carregar()` dispara as chamadas `ApiService` da página em paralelo.
