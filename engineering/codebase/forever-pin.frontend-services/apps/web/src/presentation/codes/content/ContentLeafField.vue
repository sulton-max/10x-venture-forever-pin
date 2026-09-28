<script setup lang="ts">
import {
  DateTimeInput,
  EmailInput,
  Field,
  NumberInput,
  SelectPicker,
  SelectPickerContent,
  SelectPickerItem,
  SelectPickerTrigger,
  SelectPickerValue,
  SwitchInput,
  TelInput,
  TextAreaInput,
  TextInput,
  UrlInput,
} from '@wow-two-beta/ui-vue/presentation/forms';
import { Temporal } from 'temporal-polyfill';
import { computed } from 'vue';

import type { CodeBuilderForm } from '@/application/codes';

import { ContentFieldKind, type ContentFieldDisplay } from './ContentFieldDisplays';

/** Defines the props of {@link ContentLeafField}. */
export interface ContentLeafFieldProps {
  /** The builder form. */
  readonly form: CodeBuilderForm;
  /** The content object's path, such as `rules[0].content`. */
  readonly contentPath: string;
  /** The current content values, for derived placeholders. */
  readonly content: Readonly<Record<string, unknown>>;
  /** The field to edit. */
  readonly field: ContentFieldDisplay;
}

/**
 * Renders one content leaf bound at its own path, so a server error such as `rules[0].content.url` lands on
 * this control and its touched and blur state are its own.
 */
defineOptions({ name: 'ContentLeafField' });
const props = defineProps<ContentLeafFieldProps>();
const FormField = props.form.Field;

const placeholder = computed(() =>
  typeof props.field.placeholder === 'function' ? props.field.placeholder(props.content) : props.field.placeholder,
);

/** Reads a text value; absent values edit as empty. */
function text(value: unknown): string {
  return typeof value === 'string' ? value : '';
}

/** Stores edited text; an emptied optional field is removed rather than kept as `''`. */
function toText(value: string | number | null | undefined): string | undefined {
  const next = value == null ? '' : String(value);
  return props.field.isOptional && next === '' ? undefined : next;
}

function dateTime(value: unknown): Temporal.PlainDateTime | null {
  return value instanceof Temporal.PlainDateTime ? value : null;
}
</script>

<template>
  <FormField v-slot="f" :name="`${contentPath}.${field.key}`">
    <Field :label="field.label" :is-required="!field.isOptional && field.kind !== ContentFieldKind.Switch">
      <TextAreaInput
        v-if="field.kind === ContentFieldKind.TextArea"
        ring="sm"
        :rows="3"
        :model-value="text(f.value)"
        :placeholder="placeholder"
        @update:model-value="(value) => f.setValue(toText(value))"
        @blur="f.onBlur"
      />
      <UrlInput
        v-else-if="field.kind === ContentFieldKind.Url"
        ring="sm"
        :model-value="text(f.value)"
        :placeholder="placeholder"
        @update:model-value="(value) => f.setValue(toText(value))"
        @blur="f.onBlur"
      />
      <EmailInput
        v-else-if="field.kind === ContentFieldKind.Email"
        ring="sm"
        :model-value="text(f.value)"
        :placeholder="placeholder"
        @update:model-value="(value) => f.setValue(toText(value))"
        @blur="f.onBlur"
      />
      <TelInput
        v-else-if="field.kind === ContentFieldKind.Tel"
        ring="sm"
        :model-value="text(f.value)"
        :placeholder="placeholder"
        @update:model-value="(value) => f.setValue(toText(value))"
        @blur="f.onBlur"
      />
      <NumberInput
        v-else-if="field.kind === ContentFieldKind.Number"
        ring="sm"
        :model-value="typeof f.value === 'number' ? f.value : null"
        :min="field.range?.min"
        :max="field.range?.max"
        :step="field.range?.step"
        @update:model-value="(value) => f.setValue(value)"
        @blur="f.onBlur"
      />
      <SelectPicker
        v-else-if="field.kind === ContentFieldKind.Select"
        :model-value="text(f.value) || field.options?.[0]?.value"
        :get-option-label="(key: string) => field.options?.find((option) => option.value === key)?.label ?? key"
        @update:model-value="(value) => value && f.setValue(value)"
      >
        <SelectPickerTrigger><SelectPickerValue /></SelectPickerTrigger>
        <SelectPickerContent>
          <SelectPickerItem
            v-for="option in field.options"
            :key="option.value"
            :item-key="option.value"
            :label="option.label"
          />
        </SelectPickerContent>
      </SelectPicker>
      <SwitchInput
        v-else-if="field.kind === ContentFieldKind.Switch"
        :model-value="f.value === true"
        @update:model-value="(value) => f.setValue(value)"
      />
      <DateTimeInput
        v-else-if="field.kind === ContentFieldKind.DateTime"
        ring="sm"
        :model-value="dateTime(f.value)"
        @update:model-value="(value) => f.setValue(value ?? (field.isOptional ? undefined : null))"
        @blur="f.onBlur"
      />
      <TextInput
        v-else
        ring="sm"
        :model-value="text(f.value)"
        :placeholder="placeholder"
        @update:model-value="(value) => f.setValue(toText(value))"
        @blur="f.onBlur"
      />
    </Field>
  </FormField>
</template>
