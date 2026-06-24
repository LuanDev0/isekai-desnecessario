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
| `/configuracoes` | `ConfiguracoesComponent` | Configurações (Perfil, hábitos, missões, recompensas, perfil principal, assinatura) |
| `/laboratorio` | `LaboratorioComponent` | Experimentos (hábitos em teste) |
| `/mundo` | `MundoComponent` | Mapa de ranks / mundo |
| `/ranking` | `RankingComponent` | Ranking global — aba **Nível** (top-50 por Nível→XP) e aba **Atributos** (sub-abas: Total + 6 atributos, carregamento lazy) |

**Guards (`services/auth.guard.ts`):** todas as rotas internas usam `canActivate: [authGuard]` — sem JWT, redireciona para `/cadastro`. A rota `/cadastro` usa `guestGuard` — quem já está logado **e** tem perfil ativo é mandado para `/`.

> A **navbar inferior** linka 8 páginas: Início, Missões, Status, Inventário, Loja, Gráfico, Ranking, Config. As páginas **Laboratório** e **Mundo** são rotas acessadas a partir de outras telas (não ficam na navbar).

---

## Páginas

### Cadastro (`/cadastro`)
Porta de entrada. Faz **login** (Google ou e-mail/senha), **registro** de conta, **seleção de perfil** e **criação de herói** (nome, classe, gênero, foto) — tudo numa máquina de estados (`tela: 'inicio' | 'login' | 'registro' | 'perfis' | 'cadastro'`). Após logar, lista os perfis da conta e os **órfãos** (convidados) para reivindicar. Mostra o logo e o subtítulo *"Seu isekai começa aqui. Nenhum caminhão-kun necessário."*. O nome da classe no combobox muda conforme o gênero.

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
Gerencia o **Perfil** (foto, nome, classe, gênero) e os conteúdos do herói. Cada seção (Bons Hábitos, Maus Hábitos, Missões, Recompensas) tem três sub-blocos:

1. **Adicionar do catálogo** — lista itens aprovados do catálogo global + conteúdo próprio; itens exclusivos de outra classe aparecem com 🔒 e opacidade reduzida (bloqueados). Botão **Ativar/Desativar** aciona `POST /{tipo}/{id}/ativar` ou `/desativar`.
2. **Ativos** — lista os itens já ativos no perfil. Botão **Desativar** sempre visível; botões **Editar/Excluir** só para autores e Admins (`auth.podeConteudoProprio()`).
3. **Criar** — formulário de criação, gated por `auth.podeConteudoProprio()` (VIP/Moderador/Admin). Campos extras: **Escopo** (próprio × global — só Admin/Moderador veem a opção global), **Trava (dias)** e **Chips de classe** (só para conteúdo global, seleciona classes exclusivas).

**Seção de Aprovações** (só Admin): lista unificada de itens globais pendentes (`/aprovacoes/pendentes`), com botões **Aprovar** e **Rejeitar** por item.

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
| `notificacoes` | Sininho in-app flutuante (canto superior direito) — badge de não-lidas + painel; renderizado globalmente (exceto `/cadastro`) |

---

## Serviços

### `ApiService` (`services/api.service.ts`)
Fachada única para a API REST. `BASE = environment.apiUrl` (localhost em dev, Railway em produção). Métodos cobrem perfil, classes, hábitos, missões, recompensas, inventário, atributos, lootbox, desafio, diário, jornada, snapshots, experimentos e histórico. Ver [API.md](API.md) para o mapa completo.

### `AuthService` (`services/auth.service.ts`)
Sessão e login. Guarda o JWT e o usuário no `localStorage` (`isekai_jwt`, `isekai_usuario`); expõe `usuario` (signal), `getToken()`, `isLogado()`, `logout()`. Métodos `loginComGoogle()`, `login()`, `registrar()` e a integração com Google Identity Services (`initGoogleSignIn`, `abrirPopupGoogle`).

- **`auth.interceptor.ts`** — injeta `Authorization: Bearer <token>` em toda requisição HTTP.
- **`auth.guard.ts`** — `authGuard` (protege rotas internas) e `guestGuard` (afasta logados da tela de login).

### `ProfileService` (`services/profile.service.ts`)
Estado do **perfil ativo** com signal:
- `perfilAtivo` — signal readonly do perfil atual.
- `getSavedId()` / `setPerfilAtivo()` / `clearPerfil()` — persistência em `localStorage` (chave **`solo_perfil_id`**).
- `id` — getter de conveniência (perfil ativo → localStorage → 0).
- `fotoAtualizada$` — `Subject` emitido quando a foto muda, para o selector recarregar.

> A chave do localStorage ainda é `solo_perfil_id` (legado do nome antigo "Solo Leveling"); funcional, mas pode ser renomeada futuramente.

---

## Models TypeScript (`models/models.ts`)

Espelham as entidades do backend (camelCase): `Perfil`, `Classe`, `Atributo`, `BomHabito`, `MauHabito`, `Missao`, `TipoMissao`, `Recompensa`, `ItemInventario`, `DiarioAcao`, `Experimento`/`ExperimentoDia`, `JornadaSemana`, `Notificacao`.

Tipos de catálogo: `HabitoCatalogo`, `MissaoCatalogo`, `RecompensaCatalogo` (definição + flags `ativo`/`bloqueado`/`classeIds`); `Pendente` (item aguardando aprovação); `EscopoConteudo` (`'Global'|'Proprio'`); `StatusConteudo` (`'Aprovado'|'Pendente'|'Rejeitado'`); `Role` (`'Usuario'|'VIP'|'Moderador'|'Admin'`).

Detalhes de cada campo em [BANCO-DE-DADOS.md](BANCO-DE-DADOS.md).

---

## Padrão de carregamento das páginas

O **`authGuard`** já barra acesso sem JWT (redireciona a `/cadastro`). Dentro da página, o `ngOnInit` garante o **perfil ativo**:

```ts
const savedId = this.profile.getSavedId();
if (!savedId) { this.router.navigate(['/cadastro']); return; }   // logado, mas sem perfil escolhido
if (!this.profile.perfilAtivo()) {
  this.api.getPerfil(savedId).subscribe(p => { this.profile.setPerfilAtivo(p); this.carregar(); });
} else {
  this.carregar();
}
```

`carregar()` dispara as chamadas `ApiService` da página em paralelo.
