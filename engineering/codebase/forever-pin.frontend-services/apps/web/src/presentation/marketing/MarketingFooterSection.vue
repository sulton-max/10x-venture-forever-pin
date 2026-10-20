<script setup lang="ts">
import { Heading, Text } from '@wow-two-beta/fe/presentation/display';
import { RouterLink } from 'vue-router';

import { AppPaths } from '@/presentation/common';

import BrandGlyph from './BrandGlyph.vue';
import { Brand } from './MarketingContent';

/** Holds the footer's link columns. */
const LinkColumns = [
  {
    title: 'Product',
    links: [
      { to: AppPaths.pricing(), label: 'Pricing' },
      { to: AppPaths.newCode(), label: 'Create a code' },
      { to: AppPaths.codes(), label: 'Open the app' },
    ],
  },
  {
    title: 'Learn',
    links: [
      { to: AppPaths.blog(), label: 'Blog' },
      { to: AppPaths.blogPost('why-qr-codes-should-never-expire'), label: 'Why codes never expire' },
      { to: AppPaths.blogPost('smart-routing-one-code-many-destinations'), label: 'How smart routing works' },
    ],
  },
] as const;

/** Renders the site footer: pitch, link columns, promises and the DB-IP attribution. */
defineOptions({ name: 'MarketingFooterSection' });
const year = new Date().getFullYear();
</script>

<template>
  <footer class="border-t border-border bg-muted/30">
    <div class="mx-auto w-full max-w-6xl px-6 py-12">
      <div class="grid grid-cols-1 gap-10 sm:grid-cols-2 lg:grid-cols-4">
        <div class="lg:col-span-2">
          <BrandGlyph />
          <Text size="sm" color="muted" class="mt-3 max-w-xs">{{ Brand.pitch }}</Text>
          <Text size="xs" color="muted" class="mt-4">No hostage codes. No scan caps. No nags.</Text>
        </div>
        <div v-for="column in LinkColumns" :key="column.title">
          <Heading :level="3" size="xs" weight="semibold" class="tracking-normal">{{ column.title }}</Heading>
          <ul class="mt-3 flex flex-col gap-2">
            <li v-for="link in column.links" :key="link.to">
              <RouterLink :to="link.to" class="text-sm text-muted-foreground transition-colors hover:text-foreground">
                {{ link.label }}
              </RouterLink>
            </li>
          </ul>
        </div>
      </div>

      <div
        class="mt-10 flex flex-col items-start justify-between gap-2 border-t border-border pt-6 text-xs text-muted-foreground sm:flex-row sm:items-center"
      >
        <span>© {{ year }} ForeverPin. Your codes, forever.</span>
        <span>Cancel, export, and delete everything — anytime.</span>
        <!-- DB-IP Lite data is licensed CC BY 4.0; country routing requires this attribution. -->
        <a href="https://db-ip.com" class="transition-colors hover:text-foreground">IP Geolocation by DB-IP</a>
      </div>
    </div>
  </footer>
</template>
