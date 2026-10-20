<script setup lang="ts">
import { Moon, Sun } from 'lucide-vue-next';
import { useColorMode } from '@wow-two-beta/fe/foundation/primitives';
import { Button } from '@wow-two-beta/fe/presentation/actions';

/** Renders the light and dark mode switch. */
defineOptions({ name: 'ColorModeButton' });
const colorMode = useColorMode();
</script>

<template>
  <Button
    variant="ghost"
    tone="neutral"
    shape="square"
    :aria-label="colorMode.mode === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'"
    @click="colorMode.toggle"
  >
    <Sun v-if="colorMode.mode === 'dark'" :size="18" />
    <Moon v-else :size="18" />
  </Button>
</template>
