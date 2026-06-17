# SKILL.md — Boas Práticas Angular para este Projeto

> Guia de referência para geração de código Angular neste repositório (Isekai Desnecessário — Angular 19, standalone components, sem NgRx).
> Use este arquivo como checklist antes de criar/alterar código no frontend.

## Estado atual do projeto

- Angular 19 com **standalone components** (sem `NgModule`), roteamento via `app.routes.ts` + `app.config.ts`.
- Estrutura atual em `src/app`: `components/` (navbar, profile-selector), `pages/` (dashboard, missoes, loja, inventario, status, mundo, laboratorio, grafico, cadastro, configuracoes), `services/` (api.service.ts, profile.service.ts), `models/models.ts`.
- Sem NgRx instalado — gerenciamento de estado hoje é via Services + RxJS/Signals. Manter essa abordagem; só introduzir NgRx se o estado global crescer muito (muitas features compartilhando o mesmo slice de estado com lógica complexa de transições).

---

## 1. Estrutura de Pastas (Standalone Components)

Não usar `NgModule` para novas features — manter 100% standalone. Organizar por feature, não por tipo de arquivo:

```
src/app/
  core/                 # singletons: interceptors, guards, error handler, services globais (auth, api base)
  shared/               # componentes/pipes/diretivas reutilizáveis entre features (sem lógica de negócio)
  models/               # interfaces/types compartilhados (já existe: models.ts)
  pages/<feature>/      # uma pasta por feature/rota (já é o padrão: missoes/, loja/, status/...)
    <feature>.component.ts
    <feature>.component.html
    <feature>.component.scss
    <feature>.component.spec.ts
    components/         # componentes "dumb" específicos da feature, se necessário
  services/              # services que não são exclusivos de uma feature (api.service.ts, profile.service.ts)
```

- Cada feature em `pages/` deve ser lazy-loaded via rota (ver seção 6), nunca importada estaticamente no `app.routes.ts`.
- Componentes específicos de uma feature (ex.: um card só usado em `missoes`) vivem dentro de `pages/missoes/components/`, não em `shared/`.
- Só promover algo para `shared/` quando for reutilizado por 2+ features.

## 2. Padrões: Smart/Dumb Components, Services, Guards

### Smart (container) vs Dumb (presentational)
- **Smart components** (os de `pages/*.component.ts`): injetam Services, fazem chamadas HTTP/Signals, controlam estado e navegação. Pouco ou nenhum HTML complexo — delegam para dumb components.
- **Dumb components** (em `pages/<feature>/components/` ou `shared/`): recebem dados via `input()`/`@Input()`, emitem eventos via `output()`/`@Output()`, sem injeção de Services de dados, sem chamadas HTTP. Fáceis de testar isoladamente.
- Regra prática: se o componente injeta um Service que faz `HttpClient`, ele é smart; se só recebe `@Input` e emite `@Output`, é dumb.

### Services
- Um Service por domínio (`PerfilService`, `MissoesService`, `HabitosService`), `providedIn: 'root'`.
- Service expõe métodos que retornam `Observable<T>` (chamadas HTTP) e/ou `Signal<T>` (estado local reativo) — não misturar `Subject` cru exposto publicamente; expor `.asObservable()` ou `computed()`.
- Lógica de transformação de dados (mapear DTO da API para o model do front) fica no Service, não no componente.
- `ApiService` (já existente) deve ser a única camada que conhece a URL base e monta os endpoints; services de domínio chamam `ApiService`, componentes nunca chamam `HttpClient` direto.

### Guards
- Guards funcionais (`CanActivateFn`, `CanDeactivateFn`) em `core/guards/`, não guards baseados em classe.
- Guard não deve conter lógica de negócio — apenas decidir redirecionar/bloquear, delegando a verificação real a um Service (ex.: `perfilGuard` chama `ProfileService.temPerfilSelecionado()`).
- Aplicar guard nas rotas via `app.routes.ts` (`canActivate: [perfilGuard]`).

## 3. RxJS e Gerenciamento de Estado (Signals)

- Para estado local de componente/feature: usar **Signals** (`signal()`, `computed()`, `effect()`) — é o padrão recomendado para Angular 19 e mais simples que RxJS para esse caso.
- Para fluxos assíncronos (HTTP, eventos do usuário combinados, debounce, etc.): usar **RxJS** nos Services, convertendo para Signal no componente com `toSignal()` quando o consumo for só leitura síncrona no template.
- Nunca fazer `subscribe()` manual em componente sem `takeUntilDestroyed()` (ou `async` pipe / `toSignal`) — risco de memory leak. Preferir:
  - `async` pipe no template, ou
  - `toSignal(obs$, { initialValue: ... })`, ou
  - se precisar de `subscribe()` explícito, usar `takeUntilDestroyed(this.destroyRef)`.
- Operadores comuns: `switchMap` para cancelar requisição anterior (ex.: trocar de perfil), `catchError` sempre tratando erro e retornando fallback ou re-lançando para o interceptor global, `shareReplay(1)` para cache simples de dados pouco mutáveis (ex.: lista de classes/atributos).
- Estado compartilhado entre features (ex.: perfil ativo) fica em um Service com `signal` privado + `computed`/getter público somente leitura, e métodos explícitos para mutação (`selecionarPerfil(id)`), nunca expondo o `signal` mutável diretamente para os componentes.
- NgRx só deve ser introduzido se: múltiplas features precisarem reagir ao mesmo estado complexo com muitas transições e efeitos colaterais encadeados. Não introduzir por padrão neste projeto.

## 4. HttpClient e Interceptors

- Configurar `provideHttpClient(withInterceptors([...]))` em `app.config.ts` (estilo funcional, sem `HttpClientModule`).
- Interceptors funcionais em `core/interceptors/`:
  - `errorInterceptor`: captura erros HTTP e os repassa para o tratamento de erro global (seção 5).
  - `loadingInterceptor` (se necessário): controla um indicador de loading global via signal/service.
  - `apiPrefixInterceptor` (opcional): só se a URL base não estiver centralizada no `ApiService`.
- Interceptors não tratam lógica de negócio nem mostram UI diretamente — emitem para um serviço central (ex.: `NotificationService`) que decide como exibir.
- Todo método de `ApiService`/services de domínio tipado com a interface do `models.ts` correspondente, nunca `any`.

## 5. Tratamento de Erros Global

- `errorInterceptor` captura toda resposta HTTP de erro e:
  - loga no console em dev (`console.error` só quando `!environment.production`, ou via um logger central),
  - mapeia status codes para mensagens amigáveis (404 → "não encontrado", 500 → "erro no servidor"),
  - dispara notificação via `NotificationService`/`ToastService` central, nunca `alert()`.
- Usar `ErrorHandler` customizado (`provideZonelessChangeDetection` + `{ provide: ErrorHandler, useClass: GlobalErrorHandler }` ou equivalente) para capturar erros não tratados de runtime (fora de HTTP), evitando tela branca silenciosa.
- Componentes não fazem `try/catch` ao redor de chamadas de Service que retornam Observable — tratam erro com `catchError` no próprio Observable ou deixam subir para o interceptor; usar `try/catch` só em código síncrono real.
- Erros de validação de formulário são tratados localmente no componente (mensagens no template), não passam pelo error handler global.

## 6. Lazy Loading e Performance

- Toda rota de feature em `app.routes.ts` usa `loadComponent: () => import('./pages/.../x.component').then(m => m.XComponent)` — nunca import estático de componente de página.
- Evitar bundles grandes: bibliotecas pesadas usadas só em uma feature (ex.: gráfico em `grafico/`) devem ser importadas dentro do componente lazy, não no `app.config.ts` global.
- `ChangeDetectionStrategy.OnPush` em componentes "dumb" e sempre que o componente só depende de `input()`/Signals.
- Usar `@for` com `track` (sintaxe de control flow nativa do Angular 17+) em vez de `*ngFor` com `trackBy` separado.
- Usar `@if`/`@switch` (control flow nativo) em vez de `*ngIf`/`*ngSwitch` em código novo.
- Imagens e assets estáticos grandes (ex.: ícones de gacha) com `NgOptimizedImage` quando aplicável.
- Evitar `effect()` que dispara HTTP a cada mudança de signal sem necessidade — preferir `computed()` para derivação pura e chamar APIs explicitamente em handlers de evento.

## 7. Testes com Jasmine/Jest e Testing Library

- Projeto criado com Angular CLI usa Jasmine + Karma por padrão (`*.spec.ts` já presentes, ex.: `app.component.spec.ts`). Manter Jasmine como runner padrão a menos que o usuário decida migrar para Jest explicitamente.
- Um `.spec.ts` por componente/service novo, no mesmo diretório do arquivo testado.
- Componentes **dumb**: testar via `@testing-library/angular` quando disponível (render + interação por texto/role visível ao usuário) ou `TestBed` + `fixture.nativeElement` simples — focar em comportamento observável (o que aparece na tela, o que é emitido), não em detalhes de implementação.
- Componentes **smart**: mockar Services injetados (`jasmine.createSpyObj` ou providers fake), nunca chamar `HttpClient` real — usar `HttpTestingController` (`provideHttpClientTesting`) para testar Services que fazem requisições.
- Services: testar métodos puros sem mocks; métodos com `HttpClient` testados com `HttpTestingController`, verificando URL, método e payload da requisição.
- Guards funcionais: testar chamando a função diretamente com mocks de `ActivatedRouteSnapshot`/`RouterStateSnapshot`, sem precisar de `TestBed` completo quando possível.
- Nomenclatura de teste (`describe`/`it`): `describe('NomeDoComponenteOuService', () => { it('deve <comportamento esperado> quando <cenário>', () => {...}) })`.
- Evitar testes que dependem de tempo real — usar `fakeAsync`/`tick()` ou `TestScheduler` do RxJS para fluxos assíncronos.

## 8. Convenções de Nomenclatura Angular

- Arquivos: `kebab-case` (`profile-selector.component.ts`, `api.service.ts`) — já é o padrão no projeto, manter.
- Classes: `PascalCase` com sufixo do tipo (`ProfileSelectorComponent`, `ApiService`, `PerfilGuard` → função `perfilGuard` em `camelCase` para guards funcionais).
- Seletores de componente: prefixo do projeto + kebab-case (`app-profile-selector`), evitar seletores genéricos sem prefixo.
- Signals: nomear sem sufixo redundante (`perfil = signal<Perfil | null>(null)`), `computed` pode ter nome descritivo do valor derivado (`xpFaltante = computed(...)`).
- Observables: sufixo `$` (`perfil$`, `missoes$`) para diferenciar de Signals/valores simples.
- Inputs/Outputs: nome direto do dado/evento, sem prefixo `on` no Input (`@Input() habito: Habito`), Output com verbo no infinitivo ou nome de evento (`@Output() concluido = new EventEmitter<void>()` ou `output<void>()`).
- Interfaces/Models em `models.ts`: `PascalCase`, em português consistente com o domínio do projeto (`Perfil`, `Habito`, `Missao`), sem prefixo `I`.
- Pastas de feature em `pages/`: nome no plural quando representa uma lista/seção (`missoes`, `inventario` no caso é singular por convenção de domínio — manter consistência com o que já existe).

---

## Checklist rápido antes de gerar código

1. O componente é standalone e está na pasta certa (`pages/<feature>` ou `shared/`)?
2. É smart (orquestra Services) ou dumb (só `input`/`output`)? Não misturar os dois.
3. Estado usa Signals/RxJS de forma consistente, sem `subscribe()` sem `takeUntilDestroyed`?
4. Chamada HTTP passa por Service de domínio → `ApiService`, nunca `HttpClient` direto no componente?
5. Erros tratados via `errorInterceptor`/`GlobalErrorHandler`, sem `alert()` nem `try/catch` sobre Observable?
6. Rota nova é lazy (`loadComponent`) e componente dumb usa `OnPush`?
7. Existe `.spec.ts` cobrindo o comportamento novo, mockando Services/HTTP corretamente?
8. Nomenclatura segue kebab-case nos arquivos, PascalCase nas classes, `$` para Observables?
