<script setup lang="ts">
import { Button } from '@wow-two-beta/ui-vue/presentation/actions';
import { Text } from '@wow-two-beta/ui-vue/presentation/display';
import { Alert } from '@wow-two-beta/ui-vue/presentation/feedback';
import {
  ChoiceCard,
  ControlGroupField,
  RadioGroup,
  SegmentedPicker,
  ToggleInput,
} from '@wow-two-beta/ui-vue/presentation/forms';
import {
  Modal,
  ModalBody,
  ModalContent,
  ModalDescription,
  ModalFooter,
  ModalHeader,
  ModalTitle,
} from '@wow-two-beta/ui-vue/presentation/overlays';
import { computed, ref, watch } from 'vue';

import { codeImageUrl, useCodeDownload } from '@/application/codes';
import { FailureMessages } from '@/application/common';
import { ImageFormat, PrintSizing, type CodeDto } from '@/domain/codes';

/** Defines the props of {@link DownloadCodeModal}. */
export interface DownloadCodeModalProps {
  /** The code to download, or `null` when closed. */
  readonly code: CodeDto | null;
}

/** Defines the events of {@link DownloadCodeModal}. */
export interface DownloadCodeModalEmits {
  /** Dismisses the modal. */
  (event: 'close'): void;
}

/** Lists the offered print widths, in millimetres — business card to poster. */
const PrintWidths = [20, 30, 50, 100, 200] as const;

/** Describes each downloadable format. */
const FormatChoices = [
  { format: ImageFormat.Svg, label: 'SVG · for print', note: 'Vector, sharp at any size — saved at the width below.' },
  { format: ImageFormat.Png, label: 'PNG · for screens', note: 'An image for slides, documents and the web.' },
] as const;

/** Formats a length for reading: centimetres up to a metre, metres beyond. */
function length(mm: number): string {
  return mm >= 1000 ? `${Math.round(mm / 100) / 10} m` : `${Math.round(mm / 10)} cm`;
}

/**
 * Downloads a saved code: an SVG saved at a chosen print width, with the scanning distance and the module size
 * that width gives, or a PNG for screens. A width too small for the symbol's density is flagged before saving.
 */
defineOptions({ name: 'DownloadCodeModal' });
const props = defineProps<DownloadCodeModalProps>();
const emit = defineEmits<DownloadCodeModalEmits>();

const download = useCodeDownload(() => props.code);
const format = ref<ImageFormat>(ImageFormat.Svg);
const widthMm = ref<number>(30);
const isSaving = ref(false);
const failure = ref<string | null>(null);

const minimumWidthMm = computed(() =>
  props.code && download.modules.value
    ? PrintSizing.minimumWidthMm(props.code.barcodeFormat, download.modules.value.across)
    : PrintSizing.MinimumWidthMm,
);
const moduleMm = computed(() =>
  download.modules.value ? PrintSizing.moduleMm(widthMm.value, download.modules.value.across) : undefined,
);
const isTooSmall = computed(() => widthMm.value < minimumWidthMm.value);
const readFailure = computed(() =>
  download.error.value ? FailureMessages.describe(download.error.value, 'The code image could not be loaded.') : null,
);

watch(
  () => props.code,
  (code) => {
    if (!code) return;
    format.value = ImageFormat.Svg;
    widthMm.value = 30;
    failure.value = null;
  },
);

function selectFormat(next: unknown): void {
  if (next === ImageFormat.Svg || next === ImageFormat.Png) format.value = next;
}

function selectWidth(next: unknown): void {
  if (typeof next === 'string') widthMm.value = Number(next);
}

async function save(): Promise<void> {
  isSaving.value = true;
  failure.value = null;
  const result = await download.save(format.value, widthMm.value);
  isSaving.value = false;
  if (result.ok) emit('close');
  else failure.value = FailureMessages.describe(result.failure, 'The download could not be prepared.');
}
</script>

<template>
  <Modal :open="code !== null" @update:open="(open) => !open && emit('close')">
    <ModalContent>
      <ModalHeader>
        <ModalTitle>Download “{{ code?.name }}”</ModalTitle>
        <ModalDescription>Print the SVG; it stays sharp at any size. The PNG suits screens.</ModalDescription>
      </ModalHeader>
      <ModalBody class="flex flex-col gap-4">
        <div class="flex items-center gap-4">
          <img
            v-if="code"
            :src="codeImageUrl(code, ImageFormat.Svg)"
            alt=""
            class="size-24 shrink-0 rounded-md bg-white object-contain p-1"
          />
          <RadioGroup legend="Format" :model-value="format" class="flex-1" @update:model-value="selectFormat">
            <ChoiceCard
              v-for="choice in FormatChoices"
              :key="choice.format"
              :value="choice.format"
              :label="choice.label"
              :description="choice.note"
            />
          </RadioGroup>
        </div>

        <template v-if="format === ImageFormat.Svg">
          <ControlGroupField label="Print width">
            <SegmentedPicker
              type="single"
              :model-value="String(widthMm)"
              aria-label="Print width"
              @update:model-value="selectWidth"
            >
              <ToggleInput v-for="width in PrintWidths" :key="width" variant="soft" size="sm" :value="String(width)">
                {{ length(width) }}
              </ToggleInput>
            </SegmentedPicker>
          </ControlGroupField>
          <Text size="sm" color="muted">
            Scans comfortably from about {{ length(PrintSizing.scanDistanceMm(widthMm)) }} away.
            <template v-if="moduleMm !== undefined"> Each module prints {{ moduleMm.toFixed(2) }} mm wide. </template>
          </Text>
          <Alert
            v-if="isTooSmall"
            severity="warning"
            :description="`Too small for this code: its modules need at least ${minimumWidthMm} mm of width to scan reliably. Pick a larger width, or shorten the content.`"
          />
          <Text size="xs" color="muted">Keep the white margin around the code, and print dark on a light surface.</Text>
        </template>

        <Alert v-if="readFailure" severity="danger" :description="readFailure" />
        <Alert v-if="failure" severity="danger" :description="failure" />
      </ModalBody>
      <ModalFooter>
        <Button tone="neutral" variant="outline" @click="emit('close')">Cancel</Button>
        <Button
          tone="primary"
          :is-loading="isSaving"
          :is-disabled="format === ImageFormat.Svg && !download.modules.value"
          @click="save"
        >
          Download {{ format === ImageFormat.Svg ? `SVG · ${length(widthMm)}` : 'PNG' }}
        </Button>
      </ModalFooter>
    </ModalContent>
  </Modal>
</template>
