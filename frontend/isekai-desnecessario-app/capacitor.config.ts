import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.luandev.isekai',
  appName: 'Isekai Desnecessário',
  webDir: 'dist/isekai-desnecessario/browser',
  android: {
    allowMixedContent: true,
  },
};

export default config;
