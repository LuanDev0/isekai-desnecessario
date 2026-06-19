# 📖 Documentação — Isekai Desnecessário

> RPG **habit tracker**: transforme sua rotina numa aventura. Ganhe XP, suba de nível e rank, complete missões, junte moedas e troque por recompensas reais. Nenhum caminhão-chan necessário.

---

## Índice

| Documento | Conteúdo |
|-----------|----------|
| [ARQUITETURA.md](ARQUITETURA.md) | Stack, estrutura de pastas, como rodar |
| [BANCO-DE-DADOS.md](BANCO-DE-DADOS.md) | Modelos, tabelas, relacionamentos e migrations |
| [API.md](API.md) | Todos os endpoints REST |
| [MECANICAS.md](MECANICAS.md) | Regras do jogo: XP, ranks, atributos, classes, lootbox, missões, hábitos, experimentos |
| [FRONTEND.md](FRONTEND.md) | Páginas, componentes, serviços e rotas |
| [SKILLS.md](SKILLS.md) | Boas práticas de código C#/.NET — checklist antes de gerar/alterar código no backend |
| [MOBILE.md](MOBILE.md) | Plano Play Store, responsividade implementada, Capacitor e Google Sign-In nativo |

---

## Visão geral

**Isekai Desnecessário** é um app de produtividade gamificado de uso pessoal. O jogador cria um **herói** (perfil) e usa a rotina real para evoluí-lo:

- **Bons hábitos** dão XP ao serem completados.
- **Maus hábitos** tiram XP ao serem registrados.
- **Missões** (principais, secundárias e desafios) dão XP **e moedas**.
- **Moedas** são gastas em **recompensas** (prazeres reais que o jogador define para si).
- **Atributos** (6) refletem em que áreas da vida o jogador está evoluindo.
- **Classes** (6) dão identidade ao herói, cada uma ligada a um atributo.
- **Lootbox** diária premia quem acumula bastante XP no dia.

O progresso sobe **nível → rank → título**, do rank H ao SSS.

---

## Stack

- **Backend:** ASP.NET Core 10 (Web API) + Entity Framework Core + PostgreSQL (Npgsql)
- **Frontend:** Angular 19 (standalone components)
- **Banco:** PostgreSQL — `isekai` local em dev, instância hospedada (Railway) em produção
- **Auth:** login Google + e-mail/senha, JWT (`[Authorize]` nas rotas de dados)
- **Hospedagem:** API + banco no Railway

## Rodando rápido

```bash
# Backend  (porta 5008)
cd backend/IsekaiDesnecessario.API
dotnet ef database update      # aplica migrations
dotnet run

# Frontend (porta 4200)
cd frontend/isekai-desnecessario-app
npm install
npm start
```

Detalhes completos em [ARQUITETURA.md](ARQUITETURA.md).

---

## Estado do projeto

- ✅ Núcleo do jogo (perfis, hábitos, missões, recompensas, inventário, lootbox, atributos, classes, experimentos)
- ✅ **Autenticação** — login Google **e** e-mail/senha, JWT no backend, todas as rotas de dados protegidas (`[Authorize]` + checagem de dono por perfil contra IDOR), guards no frontend (`authGuard`/`guestGuard`) e reivindicação de perfis convidados. Ver [MECANICAS.md → Contas e perfis](MECANICAS.md#contas-e-perfis-google-auth).
- ✅ **Hospedado em produção** — API + banco no Railway; o app aponta para a URL de produção em `environment.prod.ts`.
- ✅ `TiposMissao` com seed (`Principal`, `Secundária`, `Desafio`) — migration `SeedTiposMissao` aplicada automaticamente no startup.
- ✅ **Responsividade mobile** — todas as páginas adaptadas: `100svh`, safe-area (`env(safe-area-inset-*)`), grids 4→2 colunas em telas pequenas, `minmax` fluidos, hover só em dispositivos que suportam (v0.3.0). Ver [MOBILE.md](MOBILE.md).
- 🚧 **Play Store** — empacotamento com Capacitor ainda não iniciado. Login Google exigirá plugin nativo dentro da WebView (ver [MOBILE.md](MOBILE.md)).

> **Fonte da verdade:** o código sempre vence esta documentação. Se algo divergir, o comportamento real está nos arquivos `.cs`/`.ts` referenciados em cada seção.
