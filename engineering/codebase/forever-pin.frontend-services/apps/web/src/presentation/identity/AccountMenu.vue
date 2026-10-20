<script lang="ts">
/** Defines who the account menu names and which versions it reports. */
export interface AccountMenuProps {
  /** The signed-in account's name, or `Guest`. */
  readonly displayName: string;
  /** The account's email; absent for a guest. */
  readonly email?: string;
  /** The API's informational version, once its status answers. */
  readonly apiVersion: string | null;
}

/** Holds the colour modes the menu offers, in order. */
const Appearances = [
  { value: 'light', label: 'Light' },
  { value: 'dark', label: 'Dark' },
  { value: 'system', label: 'System' },
] as const;

/** Shortens a build's `+<commit>` suffix to seven characters. */
function shortVersion(version: string): string {
  return version.replace(/\+([0-9a-f]{7})[0-9a-f]*$/i, '+$1');
}
</script>

<script setup lang="ts">
import { Check, ChevronDown, Globe, LogOut } from 'lucide-vue-next';
import { useColorMode } from '@wow-two-beta/fe/foundation/primitives';
import { Button } from '@wow-two-beta/fe/presentation/actions';
import { Avatar } from '@wow-two-beta/fe/presentation/display';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuTrigger,
  MenuItem,
  MenuLabel,
  MenuRadioGroup,
  MenuRadioItem,
  MenuSeparator,
} from '@wow-two-beta/fe/presentation/nav';
import { computed } from 'vue';

/**
 * Renders the account menu that ends the app bar: who is signed in, the app and API versions, the colour mode, the
 * way back to the public site and sign-out.
 */
defineOptions({ name: 'AccountMenu' });
const props = defineProps<AccountMenuProps>();
const emit = defineEmits<{ signOut: []; openSite: [] }>();
const colorMode = useColorMode();
const appVersion = __APP_VERSION__;
/** The appearance in force: `system` while the mode follows the operating system. */
const appearance = computed(() => (colorMode.followsSystem ? 'system' : colorMode.mode));

/** Applies an appearance row; the provider remembers the choice. */
function choose(value: string | null): void {
  if (value === 'system') colorMode.followSystem();
  else if (value === 'light' || value === 'dark') colorMode.setMode(value);
}
</script>

<template>
  <DropdownMenu placement="bottom-end">
    <DropdownMenuTrigger as-child>
      <Button
        variant="outline"
        tone="neutral"
        size="md"
        class="gap-1.5 ps-1 pe-2"
        :aria-label="`Account menu for ${props.displayName}`"
      >
        <Avatar :name="props.displayName" size="sm" />
        <ChevronDown :size="14" class="text-muted-foreground" />
      </Button>
    </DropdownMenuTrigger>
    <DropdownMenuContent aria-label="Account" class="w-64">
      <MenuLabel>{{ props.displayName }}</MenuLabel>
      <p v-if="props.email" class="truncate px-2 pb-1 text-xs text-muted-foreground">{{ props.email }}</p>
      <p class="px-2 pb-2 font-mono text-[11px] text-muted-foreground">
        App {{ appVersion }} · API {{ props.apiVersion ? shortVersion(props.apiVersion) : '—' }}
      </p>
      <MenuSeparator />
      <MenuRadioGroup label="Appearance" :model-value="appearance" @update:model-value="choose">
        <MenuRadioItem v-for="item in Appearances" :key="item.value" :value="item.value">
          <template #indicator="{ checked }"><Check v-if="checked" :size="14" /></template>
          {{ item.label }}
        </MenuRadioItem>
      </MenuRadioGroup>
      <MenuSeparator />
      <MenuItem @select="emit('openSite')"><Globe :size="15" />ForeverPin site</MenuItem>
      <MenuItem state="destructive" @select="emit('signOut')"><LogOut :size="15" />Sign out</MenuItem>
    </DropdownMenuContent>
  </DropdownMenu>
</template>
