import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.luandev.isekai',
  appName: 'Isekai Desnecessário',
  webDir: 'dist/isekai-desnecessario/browser',
  android: {
    allowMixedContent: true,
  },
  plugins: {
    GoogleAuth: {
      scopes: ['profile', 'email'],
      // serverClientId = web client ID — gera o idToken que o backend valida
      serverClientId: '730600507732-hh6r77d7a3aasnhcds1ghua6d0vf6nh7.apps.googleusercontent.com',
    },
  },
};

export default config;
