<script setup lang="ts">
import QRCode from 'qrcode';
import { computed } from 'vue';

/** Defines the props of {@link StaticQrPreview}. */
export interface StaticQrPreviewProps {
  /** The encoded text. */
  readonly value: string;
  /** The module color. */
  readonly foreground: string;
  /** The tile and quiet-zone color. */
  readonly background: string;
  /** The rendered size in px. Default `200`. */
  readonly size?: number;
}

/** Holds the quiet zone around the symbol, in modules. */
const QuietZone = 2;

/**
 * Renders a decorative QR code on the client with no API call; the builder's live preview is
 * server-rendered instead, so these marketing visuals keep working without the backend.
 */
defineOptions({ name: 'StaticQrPreview' });
const props = withDefaults(defineProps<StaticQrPreviewProps>(), { size: 200 });

const symbol = computed(() => {
  const modules = QRCode.create(props.value || ' ', { errorCorrectionLevel: 'Q' }).modules;
  let path = '';
  for (let row = 0; row < modules.size; row += 1) {
    for (let column = 0; column < modules.size; column += 1) {
      if (modules.get(row, column)) path += `M${column + QuietZone} ${row + QuietZone}h1v1h-1z`;
    }
  }
  return { path, extent: modules.size + QuietZone * 2 };
});
</script>

<template>
  <div
    aria-hidden="true"
    class="inline-flex items-center justify-center rounded-xl p-4"
    :style="{ backgroundColor: background }"
  >
    <svg :width="size" :height="size" :viewBox="`0 0 ${symbol.extent} ${symbol.extent}`" shape-rendering="crispEdges">
      <rect :width="symbol.extent" :height="symbol.extent" :fill="background" />
      <path :d="symbol.path" :fill="foreground" />
    </svg>
  </div>
</template>
