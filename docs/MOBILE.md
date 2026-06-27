# Mobile — Isekai Desnecessário

Documentação do plano de publicação na Google Play Store via Capacitor.

---

## Estado atual (v0.20.0)

| Etapa | Status | Detalhe |
|-------|--------|---------|
| Responsividade CSS | ✅ | Todas as páginas adaptadas para mobile |
| Safe-area (notch/home indicator) | ✅ | `env(safe-area-inset-*)` + `viewport-fit=cover` |
| Viewport estável | ✅ | `100svh` em todos os `:host`/`.page` |
| Hover só em dispositivos com mouse | ✅ | `@media (hover: hover)` em todos os componentes |
| Capacitor instalado | ✅ | `@capacitor/core`, `@capacitor/cli`, `@capacitor/android` |
| Ícones do app (Android) | ✅ | Gerados via `@capacitor/assets` a partir de `assets/icon.png` (1024×1024) |
| Splash screen (Android) | ✅ | Gerada automaticamente pelo `@capacitor/assets` |
| Google Sign-In nativo (WebView) | 🚧 | Exige plugin nativo |
| Conta Play Store | 🚧 | Taxa única de $25 |
| Política de privacidade | 🚧 | Obrigatória para publicação |
| Keystore (assinatura APK) | 🚧 | Gerar e guardar com segurança |

---

## Responsividade — o que foi feito

### Fundação global (`index.html` + `styles.scss`)

```html
<!-- index.html -->
<meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">
```

```scss
// styles.scss
body {
  min-height: 100svh;
  padding-top: env(safe-area-inset-top);
  padding-left: env(safe-area-inset-left);
  padding-right: env(safe-area-inset-right);
}
@media (hover: none) {
  * { -webkit-tap-highlight-color: transparent; }
}
```

### Navbar (`navbar.component.scss`)

```scss
.navbar {
  height: calc(64px + env(safe-area-inset-bottom));
  padding-bottom: env(safe-area-inset-bottom);
  padding-top: 8px;
  align-items: flex-start; // ícones ficam no topo do espaço, não atrás do indicador home
}
```

### Páginas internas (padrão aplicado em todas)

```scss
// Padrão para páginas com .page
.page {
  padding: 24px 20px calc(84px + env(safe-area-inset-bottom));
  padding-left: max(20px, env(safe-area-inset-left));
  padding-right: max(20px, env(safe-area-inset-right));
  min-height: 100svh;
}

// Padrão para páginas com :host + .xxx-page separados
:host { min-height: 100svh; }
.xxx-page {
  padding: 16px max(16px, env(safe-area-inset-left)) calc(84px + env(safe-area-inset-bottom));
  padding-right: max(16px, env(safe-area-inset-right));
}
```

### Grids corrigidos

| Página | Grid original | Mobile (≤480px) |
|--------|--------------|-----------------|
| `status` — quick-stats | `repeat(4, 1fr)` | `repeat(2, 1fr)` |
| `grafico` — stats-grid | `repeat(4, 1fr)` | `repeat(2, 1fr)` |
| `grafico` — resumo-grid | `repeat(3, 1fr)` | `repeat(2, 1fr)` ≤360px |
| `inventario` — stats-row | `repeat(4, 1fr)` | `repeat(2, 1fr)` |
| `inventario` — grid itens | `minmax(155px, 1fr)` | `minmax(min(155px, 45vw), 1fr)` |
| `loja` — recompensas-grid | `minmax(160px, 1fr)` | `minmax(min(160px, 45vw), 1fr)` |

### Outros ajustes

- **`laboratorio` — heatmap cells:** 12×12 → 16×16 px (alvo de toque mínimo recomendado: 16px)
- **`configuracoes` — edit-row:** adicionado `flex-wrap: wrap`; larguras fixas de 90/120/150 px → `min(Npx, Nvw)`
- **`cadastro` — foto-genero-row:** `flex-wrap: wrap`
- **`cadastro` — version-badge:** `bottom/right` com `max(Npx, env(safe-area-inset-*))`
- **`loja` — modal lootbox:** `inset: env(safe-area-inset-*)` para não sobrepor notch
- **`dashboard` — avatar overlay:** em touch (hover: none) fica levemente visível (opacity 1, fundo escuro sutil) para indicar que é clicável
- **`dashboard` — xp-track:** `width: min(200px, 100%)` para não overflow

---

## Próximo passo: instalar Capacitor

```bash
cd frontend/isekai-desnecessario-app

# 1. Instalar dependências
npm install @capacitor/core @capacitor/cli @capacitor/android

# 2. Inicializar (nome do app + package ID)
npx cap init "Isekai Desnecessário" "com.luandev.isekai"

# 3. Build do Angular
npm run build

# 4. Adicionar plataforma Android
npx cap add android

# 5. Sincronizar e abrir no Android Studio
npx cap sync
npx cap open android
```

---

## Google Sign-In dentro do Capacitor (WebView)

O login Google via JavaScript (`google.accounts.id`) **não funciona** na WebView nativa do Capacitor. É bloqueado pela política de segurança do Google para apps nativos.

### Solução

Usar o plugin `@codetrix-studio/capacitor-google-auth`:

```bash
npm install @codetrix-studio/capacitor-google-auth
npx cap sync
```

No `CadastroComponent`, detectar plataforma:

```typescript
import { Capacitor } from '@capacitor/core';
import { GoogleAuth } from '@codetrix-studio/capacitor-google-auth';

async loginGoogle() {
  if (Capacitor.isNativePlatform()) {
    // Fluxo nativo via plugin
    const user = await GoogleAuth.signIn();
    // user.authentication.idToken → enviar para o backend
  } else {
    // Fluxo web atual (google.accounts.id)
    google.accounts.id.prompt();
  }
}
```

O backend recebe o `idToken` em ambos os casos — o endpoint `/api/auth/google` já funciona.

---

## Checklist Play Store

Antes de publicar:

- [ ] Conta de desenvolvedor Google Play ($25 taxa única)
- [ ] Política de privacidade hospedada em URL pública
- [ ] Ícones do app (512×512 PNG para Play Store, densidades para o app)
- [ ] Splash screen (opcional mas recomendada)
- [ ] `applicationId` correto no `build.gradle` (ex: `com.luandev.isekai`)
- [ ] Keystore gerada e **armazenada em local seguro** — perder a keystore = impossível atualizar o app
- [ ] APK/AAB assinado com a keystore
- [ ] Teste em dispositivo físico antes de submeter
- [ ] Screenshots para a loja (mínimo 2, recomendado 4-8)
- [ ] Descrição curta (80 chars) e longa (4000 chars)

---

## Notas de ambiente

- O campo `applicationId` em `android/app/build.gradle` deve bater com o Client ID do Google OAuth configurado no Cloud Console.
- Em desenvolvimento com Capacitor, usar `npx cap run android` para rodar direto no dispositivo/emulador.
- Após cada build Angular, rodar `npx cap sync` para copiar os assets para a pasta `android/`.
