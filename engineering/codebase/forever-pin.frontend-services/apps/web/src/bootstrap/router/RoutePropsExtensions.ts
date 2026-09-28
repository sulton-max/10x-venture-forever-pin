import { lazyRoute, type LazyRoute } from '@wow-two-beta/ui-vue/router';
import { defineComponent, h, type Component } from 'vue';
import { useRoute, useRouter, type RouteLocationNormalizedLoaded, type Router } from 'vue-router';

/** Extends route declarations with the decoding of the address into typed page props and callbacks. */
export const RoutePropsExtensions = {
  /** Lazy-loads a page and renders it with props decoded from the active route; pages never read the router. */
  page<TProps extends Record<string, unknown>>(
    load: () => Promise<{ default: Component }>,
    decode: (route: RouteLocationNormalizedLoaded, router: Router) => TProps,
  ): LazyRoute {
    return lazyRoute(async () => {
      const page = (await load()).default;
      return {
        default: defineComponent({
          name: 'RoutePropsAdapter',
          setup() {
            const route = useRoute();
            const router = useRouter();
            return () => h(page, decode(route, router));
          },
        }),
      };
    });
  },

  /** Reads one route parameter or query value as text; an empty string when absent. */
  toText(value: unknown): string {
    const text = Array.isArray(value) ? value[0] : value;
    return typeof text === 'string' ? text : '';
  },

  /** Reads one value as a member of `values`, or `undefined` when absent or unknown. */
  toMember<TValue extends string>(value: unknown, values: Readonly<Record<string, TValue>>): TValue | undefined {
    const text = RoutePropsExtensions.toText(value);
    return Object.values(values).find((member) => member === text);
  },
} as const;
