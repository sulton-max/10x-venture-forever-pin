import { createAppRouter, lazyRoute, type RouteConfig, type RouteHandle } from '@wow-two-beta/ui-vue/router';

import { ContentMode } from '@/domain/codes';
import { AppPaths } from '@/presentation/common';
import NotFoundPage from '@/presentation/common/pages/NotFoundPage.vue';
import { BlogPosts } from '@/presentation/marketing';

import AppLayout from '../AppLayout.vue';
import MarketingLayout from '../MarketingLayout.vue';

import { installCanonicalLink } from './CanonicalLink';
import { RoutePropsExtensions } from './RoutePropsExtensions';

/** Holds the Stripe Checkout return states the billing page reports. */
const CheckoutReturn = { Success: 'success', Cancelled: 'cancelled' } as const;

/** Builds a public page's title plus its description and Open Graph tags. */
function publicPage(title: string, description?: string): RouteHandle {
  return {
    title,
    meta: {
      'og:title': title,
      ...(description ? { description, 'og:description': description } : {}),
    },
  };
}

/** Keeps crawlers off the product pages; they are private to each visitor. */
const PrivatePage = { robots: 'noindex' } as const;

/** Holds the route table — the public marketing surface and the guest-gated product under `/app`. */
export const routes: RouteConfig = [
  {
    path: '/app',
    layout: AppLayout,
    children: [
      {
        index: true,
        id: 'codes',
        handle: { title: 'Your codes · ForeverPin', meta: PrivatePage },
        lazy: RoutePropsExtensions.page(
          () => import('@/presentation/codes/pages/CodesListPage.vue'),
          (_route, router) => ({
            onCreate: () => void router.push(AppPaths.newCode()),
            onEdit: (id: string) => void router.push(AppPaths.editCode(id)),
            onCopy: (id: string, mode: ContentMode) => void router.push(AppPaths.copyCode(id, mode)),
          }),
        ),
      },
      {
        path: 'new',
        id: 'new-code',
        handle: { title: 'New code · ForeverPin', meta: PrivatePage },
        lazy: RoutePropsExtensions.page(
          () => import('@/presentation/codes/pages/CodeBuilderPage.vue'),
          (route, router) => ({
            copyFromId: RoutePropsExtensions.toText(route.query.copyOf) || undefined,
            copyMode: RoutePropsExtensions.toMember(route.query.mode, ContentMode),
            onBack: () => void router.push(AppPaths.codes()),
          }),
        ),
      },
      {
        path: 'billing',
        id: 'billing',
        handle: { title: 'Billing · ForeverPin', meta: PrivatePage },
        lazy: RoutePropsExtensions.page(
          () => import('@/presentation/billing/pages/BillingPage.vue'),
          (route, router) => ({
            checkoutReturn: RoutePropsExtensions.toMember(route.query.status, CheckoutReturn),
            onClearCheckoutReturn: () => void router.replace({ query: {} }),
          }),
        ),
      },
      {
        path: ':id/edit',
        id: 'edit-code',
        handle: { title: 'Edit code · ForeverPin', meta: PrivatePage },
        lazy: RoutePropsExtensions.page(
          () => import('@/presentation/codes/pages/CodeBuilderPage.vue'),
          (route, router) => ({
            codeId: RoutePropsExtensions.toText(route.params.id),
            onBack: () => void router.push(AppPaths.codes()),
          }),
        ),
      },
    ],
  },
  {
    layout: MarketingLayout,
    children: [
      {
        index: true,
        id: 'home',
        handle: publicPage(
          'ForeverPin — programmable codes that never expire',
          "One QR code, many destinations by context — and you'll never reprint it. Smart routing, every code type, unlimited scans, flat pricing. Your codes, forever.",
        ),
        lazy: lazyRoute(() => import('@/presentation/marketing/pages/LandingPage.vue')),
      },
      {
        path: 'pricing',
        id: 'pricing',
        handle: publicPage(
          'Pricing — flat plans, unlimited scans · ForeverPin',
          'Free, $5, $15, or $39 a month — flat. Unlimited scans on every tier, smart routing and a custom domain from $5, and codes that never expire. No scan caps, no surprise bills.',
        ),
        lazy: lazyRoute(() => import('@/presentation/marketing/pages/PricingPage.vue')),
      },
      {
        path: 'blog',
        id: 'blog',
        handle: publicPage(
          'Blog — QR codes, smart routing & best practices · ForeverPin',
          'Guides on why and how to use QR codes: dynamic vs. static, how smart routing works, best practices for codes that actually scan, and why your codes should never expire.',
        ),
        lazy: lazyRoute(() => import('@/presentation/marketing/pages/BlogIndexPage.vue')),
      },
      {
        path: 'blog/:slug',
        id: 'blog-post',
        handle: {
          title: (route) => {
            const post = BlogPosts.find(RoutePropsExtensions.toText(route.params.slug));
            return post ? `${post.title} · ForeverPin` : 'Post not found · ForeverPin';
          },
          meta: (route) => {
            const post = BlogPosts.find(RoutePropsExtensions.toText(route.params.slug));
            return post
              ? { description: post.description, 'og:title': post.title, 'og:description': post.description }
              : { 'og:title': 'Post not found · ForeverPin' };
          },
        },
        lazy: RoutePropsExtensions.page(
          () => import('@/presentation/marketing/pages/BlogPostPage.vue'),
          (route) => ({ slug: RoutePropsExtensions.toText(route.params.slug) }),
        ),
      },
      {
        path: ':pathMatch(.*)*',
        id: 'not-found',
        component: NotFoundPage,
        handle: publicPage('Page not found · ForeverPin'),
      },
    ],
  },
];

/** Holds the app router — per-route titles and meta, house scroll restoration and the not-found place. */
export const router = createAppRouter(routes, { notFound: NotFoundPage, routePersistence: false });

installCanonicalLink(router, (path) => path !== AppPaths.codes() && !path.startsWith(`${AppPaths.codes()}/`));
