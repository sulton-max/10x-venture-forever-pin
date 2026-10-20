<script setup lang="ts">
import { Menu as MenuIcon } from 'lucide-vue-next';
import { Button } from '@wow-two-beta/fe/presentation/actions';
import { DropdownMenu, DropdownMenuContent, DropdownMenuTrigger, MenuItem } from '@wow-two-beta/fe/presentation/nav';
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router';

import { AppPaths, ColorModeButton } from '@/presentation/common';
import { BrandGlyph, MarketingFooterSection } from '@/presentation/marketing';

/** Holds the header's page links; a link stays active on its subpages. */
const NavigationEntries = [
  { to: AppPaths.pricing(), label: 'Pricing' },
  { to: AppPaths.blog(), label: 'Blog' },
] as const;

/** Renders the public marketing shell — frosted header, routed page and footer. */
defineOptions({ name: 'MarketingLayout' });
const route = useRoute();
const router = useRouter();

/** Holds the phone menu's destinations: the page links, then the app. */
const PhoneEntries = [...NavigationEntries, { to: AppPaths.codes(), label: 'Open app' }] as const;

function isActive(to: string): boolean {
  return route.path === to || route.path.startsWith(`${to}/`);
}
</script>

<template>
  <div class="flex min-h-screen flex-col bg-background text-foreground">
    <header
      class="sticky top-0 z-40 border-b border-border bg-card/80 backdrop-blur-xl backdrop-saturate-150 dark:bg-card/60"
    >
      <div class="mx-auto flex w-full max-w-6xl items-center justify-between gap-4 px-6 py-3.5">
        <RouterLink :to="AppPaths.home()" aria-label="ForeverPin home">
          <BrandGlyph />
        </RouterLink>
        <nav aria-label="Site" class="hidden items-center gap-7 sm:flex">
          <RouterLink
            v-for="entry in NavigationEntries"
            :key="entry.to"
            :to="entry.to"
            :aria-current="isActive(entry.to) ? 'page' : undefined"
            class="text-sm transition-colors"
            :class="isActive(entry.to) ? 'font-medium text-foreground' : 'text-muted-foreground hover:text-foreground'"
          >
            {{ entry.label }}
          </RouterLink>
        </nav>
        <div class="flex items-center gap-2">
          <DropdownMenu placement="bottom-end">
            <DropdownMenuTrigger as-child>
              <Button variant="ghost" tone="neutral" shape="square" aria-label="Open the site menu" class="sm:hidden">
                <MenuIcon :size="18" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent>
              <MenuItem v-for="entry in PhoneEntries" :key="entry.to" @select="() => void router.push(entry.to)">
                {{ entry.label }}
              </MenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
          <ColorModeButton />
          <Button as-child variant="ghost" tone="neutral" size="sm" class="hidden sm:inline-flex">
            <RouterLink :to="AppPaths.codes()">Open app</RouterLink>
          </Button>
          <Button as-child tone="primary" size="sm">
            <RouterLink :to="AppPaths.newCode()">Get started</RouterLink>
          </Button>
        </div>
      </div>
    </header>

    <main class="flex-1">
      <RouterView />
    </main>

    <MarketingFooterSection />
  </div>
</template>
