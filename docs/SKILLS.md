# ⚙️ Boas Práticas de Código (SKILL.md)

> Resumo do guia de referência em [`SKILL.md`](../SKILL.md) — leia o original para detalhes completos.
> Aplicar ao criar ou alterar código no backend ASP.NET Core.

---

## Estado atual do backend

Projeto único (`IsekaiDesnecessario.API`) — Controllers acessam `AppDbContext` diretamente. Ainda **não segue Clean Architecture nem CQRS**. Ao tocar em código existente, evoluir incrementalmente (uma feature por vez), não reescrever tudo, salvo pedido explícito.

---

## Estrutura desejada (Clean Architecture)

```
IsekaiDesnecessario.Domain/          # Entidades, Value Objects, interfaces (sem deps externas)
IsekaiDesnecessario.Application/     # Casos de uso (Commands/Queries/Handlers), DTOs
IsekaiDesnecessario.Infrastructure/  # EF Core, repositórios, serviços externos
IsekaiDesnecessario.API/             # Controllers, Program.cs, DI, middlewares
```

Regra de dependência: `API → Application → Domain`; `Infrastructure → Application/Domain`. Domain nunca depende de EF Core ou ASP.NET.

---

## Padrões

### CQRS
- Separar **Commands** (escrita) de **Queries** (leitura) — um Handler por cada.
- Commands retornam DTOs/IDs, nunca entidades de domínio diretamente.
- MediatR é opcional — Handlers simples injetados via DI são aceitáveis no tamanho deste projeto.

### Repository
- Interface em Application/Domain, implementação em Infrastructure com `AppDbContext`.
- Métodos de domínio (`ObterPorIdAsync`), não `IQueryable` exposto para fora.
- Repositório específico por entidade com regras próprias (ex.: `IPerfilRepository`), não genérico.
- Unit of Work (`SaveChangesAsync` centralizado) para operações multi-tabela.

### Dependency Injection
- `Scoped` para repositórios e DbContext; `Singleton` só para serviços sem estado.
- Injetar interfaces, nunca classes concretas, em Controllers/Handlers.

---

## Entity Framework Core

- Migrations com nome descritivo, uma por mudança lógica.
- `AsNoTracking()` em queries de leitura que não serão atualizadas.
- Evitar N+1: usar `Include`/`ThenInclude` ou `Select` para projetar DTOs.
- Transações explícitas (`BeginTransactionAsync`) quando uma ação grava em mais de uma tabela.
- Configurações via `IEntityTypeConfiguration<T>`, não tudo em `OnModelCreating`.
- Nunca usar `float`/`double` para XP ou valores monetários — usar `int` ou `decimal`.

---

## Tratamento de Erros e Logging

- Exceções de domínio customizadas (`DomainException`), traduzidas por middleware global em `ProblemDetails`.
- `ILogger<T>` com log estruturado: `LogInformation` (eventos de negócio), `LogWarning` (situações recuperáveis), `LogError(ex, "msg")` (falhas).
- Nunca logar dados sensíveis (senhas, tokens JWT, `SenhaHash`).
- Sem `try/catch` vazio — deixar subir até o middleware quando não há tratamento real.

---

## Testes (xUnit + Moq)

- Um projeto de testes por camada (`Application.Tests`, `Domain.Tests`).
- Nomenclatura: `MetodoTestado_Cenario_ResultadoEsperado` (ex.: `AdicionarXp_QuandoAtingeLimite_DeveSubirDeNivel`).
- Padrão AAA (Arrange / Act / Assert).
- Mockar interfaces de repositório com `Moq`; nunca mockar `DbContext` diretamente — usar InMemory/SQLite para integração.
- Regras puras de domínio (cálculo de XP, rank, lootbox) — testes unitários sem mocks.
- `Theory` + `InlineData`/`MemberData` para múltiplas combinações.

---

## Convenções de Nomenclatura C\#

| Caso | Uso |
|------|-----|
| `PascalCase` | Classes, métodos, propriedades, interfaces (`IPerfilRepository`), enums |
| `camelCase` | Variáveis locais, parâmetros |
| `_camelCase` | Campos privados (`_perfilRepository`) |
| Sufixo `Async` | Todo método assíncrono |
| Sufixo `Dto` / `Request` / `Response` | DTOs de entrada/saída |
| Sufixo `Command` / `Query` | CQRS |

- Nomes de domínio em **português** (`Perfil`, `Habito`, `Missao`, `Recompensa`); termos técnicos em inglês (`Repository`, `Handler`, `Command`, `Service`).
- Um arquivo por classe/interface, nome do arquivo = nome do tipo.
- `Xp` e `Id` são abreviações aceitáveis (já usadas no domínio).

---

## Checklist rápido antes de gerar código

1. Lógica de negócio em Domain/Application, não no Controller?
2. Acesso a dados via interface de Repository?
3. Dependências injetadas por interface?
4. Erros via exceção de domínio + middleware, com log estruturado?
5. Teste xUnit/Moq cobrindo a regra nova ou alterada?
6. Nomenclatura correta (PascalCase / camelCase / `_camelCase` / sufixos)?
