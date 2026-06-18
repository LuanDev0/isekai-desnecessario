# SKILL.md — Boas Práticas C# / .NET para este Projeto

> Guia de referência para geração de código C#/.NET neste repositório (Isekai Desnecessário — backend .NET + EF Core + PostgreSQL, frontend Angular).
> Use este arquivo como checklist antes de criar/alterar código no backend.

## Estado atual do projeto

O backend hoje é um projeto único (`IsekaiDesnecessario.API`) com Controllers acessando `AppDbContext` diretamente — **ainda não segue Clean Architecture nem CQRS**. As seções abaixo descrevem o destino desejado. Ao tocar em código existente, prefira evoluir incrementalmente (extrair um Controller/feature por vez) em vez de reescrever tudo de uma vez, a menos que o usuário peça explicitamente uma migração completa.

---

## 1. Estrutura de Projeto (Clean Architecture)

Para novas features ou ao refatorar, organizar em camadas por solution, não por pastas dentro de um único projeto:

```
IsekaiDesnecessario.Domain/        # Entidades, Value Objects, interfaces de domínio (sem dependências externas)
IsekaiDesnecessario.Application/   # Casos de uso (Commands/Queries/Handlers), DTOs, interfaces de repositório/serviços
IsekaiDesnecessario.Infrastructure/# EF Core (DbContext, Migrations), implementação de repositórios, serviços externos
IsekaiDesnecessario.API/           # Controllers, Program.cs, DI, middlewares
```

Regra de dependência: `API → Application → Domain`; `Infrastructure → Application/Domain`. Nunca o Domain depende de EF Core ou ASP.NET.

- Entidades de domínio não devem ter atributos de EF Core (`[Key]`, `[ForeignKey]`) quando possível — preferir Fluent API em `Infrastructure/Persistence/Configurations/*.cs`.
- Controllers ficam magros: recebem request → montam Command/Query → chamam Mediator/Handler → retornam resultado. Nada de lógica de negócio no Controller.

## 2. Padrões

### CQRS
- Separar **Commands** (escrita: `CriarHabitoCommand`, `ConcluirMissaoCommand`) de **Queries** (leitura: `ObterPerfilQuery`).
- Um Handler por Command/Query (`IRequestHandler<TCommand, TResult>` se usar MediatR).
- Commands não retornam entidades de domínio diretamente — retornam DTOs/IDs.
- Para o tamanho deste projeto, MediatR é opcional: se a equipe preferir simplicidade, usar Handlers simples injetados via DI sem biblioteca extra, mas manter a separação Command/Query.

### Repository
- Interface no Application/Domain (`IPerfilRepository`), implementação em Infrastructure usando `AppDbContext`.
- Repositório expõe métodos de domínio (`ObterPorIdAsync`, `AdicionarXpAsync`), não `IQueryable` cru vazando para fora da Infrastructure.
- Evitar Repositório genérico (`IRepository<T>`) quando a entidade tem regras específicas (ex.: Perfil com cálculo de XP/Rank) — preferir repositórios específicos.
- Unit of Work (`SaveChangesAsync` centralizado) quando uma operação envolve múltiplas entidades.

### Dependency Injection
- Registrar serviços por camada em `Program.cs` (ou `DependencyInjection.cs` por projeto, com método de extensão `AddApplication()`, `AddInfrastructure()`).
- Preferir `Scoped` para repositórios e DbContext, `Singleton` apenas para serviços sem estado (ex.: cálculo puro de fórmulas).
- Injetar interfaces, nunca classes concretas, em Controllers/Handlers.

## 3. Entity Framework Core

- Migrations sempre com nome descritivo (`dotnet ef migrations add AddAtributoMissao`), uma migration por mudança lógica — já é o padrão observado no projeto, manter.
- Configurações de entidade via `IEntityTypeConfiguration<T>` em vez de tudo dentro de `OnModelCreating`.
- Usar `AsNoTracking()` em queries de leitura que não serão atualizadas.
- Evitar N+1: usar `Include`/`ThenInclude` ou projeção direta com `Select` para DTOs.
- Nunca expor `DbSet` público fora do `AppDbContext`/Infrastructure.
- Strings com tamanho definido (`HasMaxLength`) e tipos `decimal` para valores monetários/XP fracionado, nunca `float`/`double`.
- Transações explícitas (`BeginTransactionAsync`) quando uma ação de negócio grava em mais de uma tabela (ex.: concluir missão → XP + moedas + histórico).

## 4. Tratamento de Erros e Logging

- Exceções de domínio customizadas (`DomainException`, `PerfilNaoEncontradoException`) lançadas no Application/Domain.
- Middleware global de exceção (`ExceptionHandlingMiddleware` ou `IExceptionHandler` do .NET 8+) traduz exceções em respostas HTTP padronizadas (`ProblemDetails`).
- Nunca usar `try/catch` vazio ou apenas para logar e relançar sem valor agregado — deixar a exceção subir até o middleware quando não há tratamento real a fazer.
- Logging estruturado com `ILogger<T>` (nunca `Console.WriteLine`):
  - `LogInformation` para eventos de negócio relevantes (missão concluída, level up).
  - `LogWarning` para situações recuperáveis/inesperadas mas não fatais.
  - `LogError` com a exceção (`LogError(ex, "mensagem")`) para falhas.
- Não logar dados sensíveis (senhas, tokens) — atenção especial ao planejar login Google.

## 5. Testes com xUnit e Moq

- Um projeto de testes por camada (`IsekaiDesnecessario.Application.Tests`, `IsekaiDesnecessario.Domain.Tests`).
- Nomenclatura de teste: `MetodoTestado_Cenario_ResultadoEsperado` (ex.: `AdicionarXp_QuandoAtingeLimite_DeveSubirDeNivel`).
- Padrão AAA (Arrange, Act, Assert) com comentários opcionais apenas se o teste for complexo.
- Mockar interfaces de repositório/serviços externos com `Moq`; nunca mockar o `DbContext` diretamente — preferir testar repositórios contra um banco InMemory/SQLite para testes de integração leves.
- Testes de Handlers de Command/Query: mockar repositórios, validar que o método correto foi chamado (`Verify`) e que o resultado/estado é o esperado.
- Regras de negócio puras (cálculo de XP, rank, gacha) devem ter testes unitários no Domain sem nenhum mock — são funções determinísticas.
- Usar `Theory` + `InlineData`/`MemberData` para testar várias combinações de XP/Rank/Streak.

## 6. Convenções de Nomenclatura C#

- `PascalCase`: classes, métodos, propriedades, interfaces (com prefixo `I`), enums.
- `camelCase`: variáveis locais, parâmetros.
- `_camelCase`: campos privados (`private readonly IPerfilRepository _perfilRepository`).
- Async sempre com sufixo `Async` (`ObterPerfilAsync`).
- DTOs de entrada/saída com sufixo claro: `CriarHabitoRequest`, `HabitoResponse`, `PerfilDto`.
- Commands/Queries com sufixo do padrão: `ConcluirMissaoCommand`, `ObterHistoricoQuery`.
- Nomes de domínio em português (consistente com o projeto: `Perfil`, `Habito`, `Missao`, `Recompensa`), mas termos técnicos/genéricos em inglês (`Repository`, `Handler`, `Command`, `Service`).
- Um arquivo por classe/interface, nome do arquivo = nome do tipo.
- Evitar abreviações não óbvias; `Xp` e `Id` são aceitáveis pois já usados no domínio do projeto.

---

## Checklist rápido antes de gerar código

1. A lógica de negócio está em Domain/Application, não no Controller? 
2. Acesso a dados passa por interface de Repository?
3. Dependências injetadas por interface?
4. Erros tratados via exceção de domínio + middleware, com log estruturado?
5. Existe teste xUnit/Moq cobrindo a regra nova ou alterada?
6. Nomenclatura segue PascalCase/camelCase/_camelCase e sufixos (`Async`, `Dto`, `Command`, `Query`)?
