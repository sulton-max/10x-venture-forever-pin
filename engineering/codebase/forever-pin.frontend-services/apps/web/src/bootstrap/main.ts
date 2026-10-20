import './index.css';

import { feedbackQueryErrors, feedbackSessionExpiry } from '@wow-two-beta/fe/feedback';
import { createQueryClient, queryPlugin } from '@wow-two-beta/fe/query';
import { createApp } from 'vue';

import { authBridge } from '@/integration/common';

import AppRoot from './AppRoot.vue';
import { router } from './router/Routes';

/**
 * Holds the query client; a new identity clears every private query. A failed read or write becomes one notice in the
 * toast host, and a region settles into neutral copy; a call that shows its failure in place opts out with
 * `meta: { suppressGlobalError: true }`. A 401 never toasts: the expired session publishes its one notice instead.
 */
const queryClient = createQueryClient({ scope: authBridge.scope, onError: feedbackQueryErrors() });
authBridge.subscribeExpired(feedbackSessionExpiry());

createApp(AppRoot).use(router).use(queryPlugin(queryClient)).mount('#root');
