import './index.css';

import { createQueryClient, queryPlugin } from '@wow-two-beta/ui-vue/query';
import { createApp } from 'vue';

import { authBridge } from '@/integration/common';

import AppRoot from './AppRoot.vue';
import { router } from './router/Routes';

/** Holds the query client; a new identity clears every private query. */
const queryClient = createQueryClient({
  scope: authBridge.scope,
  onError: (error) => {
    if (import.meta.env.DEV) console.error('[query]', error.status, error.message);
  },
});

createApp(AppRoot).use(router).use(queryPlugin(queryClient)).mount('#root');
