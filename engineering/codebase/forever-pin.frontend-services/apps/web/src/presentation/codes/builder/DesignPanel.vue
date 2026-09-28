<script setup lang="ts">
import {
  AccordionGroup,
  AccordionGroupContent,
  AccordionGroupItem,
  AccordionGroupTrigger,
} from '@wow-two-beta/ui-vue/presentation/display';
import { Alert } from '@wow-two-beta/ui-vue/presentation/feedback';
import {
  Field,
  SelectPicker,
  SelectPickerContent,
  SelectPickerItem,
  SelectPickerTrigger,
  SelectPickerValue,
} from '@wow-two-beta/ui-vue/presentation/forms';

import type { CodeBuilderForm } from '@/application/codes';
import { BarcodeFormat, type CodeStyleDto } from '@/domain/codes';

import BackgroundControls from '../design/BackgroundControls.vue';
import { BarcodeFormatDisplays } from '../design/BarcodeFormatDisplays';
import EmojiControls from '../design/EmojiControls.vue';
import FillControls from '../design/FillControls.vue';
import ShapeControls from '../design/ShapeControls.vue';

/** Defines the props of {@link DesignPanel}. */
export interface DesignPanelProps {
  /** The builder form. */
  readonly form: CodeBuilderForm;
}

/** Renders the symbology and, for QR codes, the colors, shapes and center emoji; barcodes render plain. */
defineOptions({ name: 'DesignPanel' });
const props = defineProps<DesignPanelProps>();
const FormField = props.form.Field;

function setStyle(style: CodeStyleDto): void {
  props.form.setValue('style', style);
}
</script>

<template>
  <div class="flex flex-col gap-5">
    <FormField v-slot="f" name="barcodeFormat">
      <Field label="Code type">
        <SelectPicker
          :model-value="f.value"
          :get-option-label="(format: BarcodeFormat) => BarcodeFormatDisplays[format].label"
          @update:model-value="(value) => value && f.setValue(value)"
        >
          <SelectPickerTrigger><SelectPickerValue /></SelectPickerTrigger>
          <SelectPickerContent>
            <SelectPickerItem
              v-for="format in Object.values(BarcodeFormat)"
              :key="format"
              :item-key="format"
              :label="BarcodeFormatDisplays[format].label"
            />
          </SelectPickerContent>
        </SelectPicker>
      </Field>
    </FormField>

    <Alert
      v-if="form.values.barcodeFormat !== BarcodeFormat.QrCode"
      severity="info"
      description="Barcodes render plain black on white. Colors, shapes and the center emoji apply to QR codes; your style is kept for when you switch back."
    />
    <AccordionGroup
      v-else
      type="single"
      default-value="colors"
      is-collapsible
      class="overflow-hidden rounded-lg border border-border"
    >
      <AccordionGroupItem value="colors">
        <AccordionGroupTrigger>Colors &amp; fill</AccordionGroupTrigger>
        <AccordionGroupContent>
          <div class="px-3 py-2">
            <FillControls :style="form.values.style" @change="setStyle" />
            <BackgroundControls :style="form.values.style" @change="setStyle" />
          </div>
        </AccordionGroupContent>
      </AccordionGroupItem>
      <AccordionGroupItem value="shape">
        <AccordionGroupTrigger>Shape &amp; eyes</AccordionGroupTrigger>
        <AccordionGroupContent>
          <div class="px-3 py-2">
            <ShapeControls :style="form.values.style" @change="setStyle" />
          </div>
        </AccordionGroupContent>
      </AccordionGroupItem>
      <AccordionGroupItem value="center">
        <AccordionGroupTrigger>Center</AccordionGroupTrigger>
        <AccordionGroupContent>
          <div class="px-3 py-2">
            <EmojiControls :style="form.values.style" @change="setStyle" />
          </div>
        </AccordionGroupContent>
      </AccordionGroupItem>
    </AccordionGroup>
  </div>
</template>
