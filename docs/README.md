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

- **Backend:** ASP.NET Core 10 (Web API) + Entity Framework Core + SQL Server
- **Frontend:** Angular 19 (standalone components)
- **Banco:** SQL Server local (`IsekaIDesnecessarioDB`)

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
- 🚧 **Autenticação Google** — modelo `Usuario` já existe no banco; backend de auth e frontend ainda em construção. Ver [MECANICAS.md → Contas e perfis](MECANICAS.md#contas-e-perfis-google-auth).
- 📌 `TiposMissao` precisa ser populada no banco (`Principal`, `Secundária`, `Desafio`) — ainda não há seed.

> **Fonte da verdade:** o código sempre vence esta documentação. Se algo divergir, o comportamento real está nos arquivos `.cs`/`.ts` referenciados em cada seção.
