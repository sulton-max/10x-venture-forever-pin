<script setup lang="ts">
import { Button } from '@wow-two-beta/ui-vue/presentation/actions';
import { Alert } from '@wow-two-beta/ui-vue/presentation/feedback';
import {
  Field,
  SegmentedPicker,
  SelectPicker,
  SelectPickerContent,
  SelectPickerItem,
  SelectPickerTrigger,
  SelectPickerValue,
  TextInput,
  ToggleInput,
} from '@wow-two-beta/ui-vue/presentation/forms';
import { DividerLayout } from '@wow-two-beta/ui-vue/presentation/layout';
import { computed, watch } from 'vue';

import type { CodeBuilderForm } from '@/application/codes';
import {
  CodeRuleType,
  ContentMode,
  ContentType,
  contentType,
  contentTypeCatalog,
  emptyContent,
  type CodeContent,
  type CodeDto,
  type CodeRuleDto,
} from '@/domain/codes';

import { ContentModeDisplays } from '../content/ContentModeDisplays';
import RuleEditor from '../routing/RuleEditor.vue';

/** Defines the props of {@link ContentPanel}. */
export interface ContentPanelProps {
  /** The builder form. */
  readonly form: CodeBuilderForm;
  /** Whether an existing code is edited; its mode is then fixed. */
  readonly isEdit: boolean;
  /** The loaded code, when editing. */
  readonly existing: CodeDto | null;
}

/** Explains why the mode is locked — two destinations cannot be baked into one symbol. */
const ModeLockedNote =
  'More than one rule means the destination is decided at scan time — only a dynamic code can do that.';

/** Renders the code's name, content type, resolution mode and rules. */
defineOptions({ name: 'ContentPanel' });
const props = defineProps<ContentPanelProps>();
const FormField = props.form.Field;

const rules = computed(() => props.form.values.rules);
const mode = computed(() => props.form.values.mode);
const isModeLocked = computed(() => rules.value.length > 1);
const addBlockedReason = computed(() =>
  props.isEdit && mode.value === ContentMode.Static
    ? 'A static code holds one rule — its content is in the symbol. Copy it as dynamic to add routing.'
    : null,
);

// A second rule needs scan-time resolution, so a new code switches to dynamic the moment one is added.
watch(isModeLocked, (locked) => {
  if (locked && !props.isEdit && mode.value === ContentMode.Static) props.form.setValue('mode', ContentMode.Dynamic);
});

// Authored content per type and rule, so inspecting another type never costs retyping.
const drafts = new Map<string, CodeContent>();
const ruleKey = (rule: CodeRuleDto, index: number) =>
  rule.type === CodeRuleType.Conditional ? `order:${rule.order}` : `fallback:${index}`;

function changeContentType(next: ContentType | null): void {
  const current = props.form.values.contentType;
  if (!next || next === current) return;
  const reseeded = rules.value.map((rule, index): CodeRuleDto => {
    if (rule.type === CodeRuleType.DefaultPointer) return rule;
    drafts.set(`${current}|${ruleKey(rule, index)}`, rule.content);
    return { ...rule, content: drafts.get(`${next}|${ruleKey(rule, index)}`) ?? emptyContent(next) };
  });
  props.form.setValue('contentType', next);
  props.form.setValue('rules', reseeded);
}
</script>

<template>
  <div class="flex flex-col gap-5">
    <FormField v-slot="f" name="name">
      <Field label="Name">
        <TextInput
          ring="sm"
          :model-value="f.value"
          placeholder="Spring menu table tent"
          @update:model-value="(value) => f.setValue(String(value ?? ''))"
          @blur="f.onBlur"
        />
      </Field>
    </FormField>

    <Field label="Content type" helper="Switching keeps what you typed for each type.">
      <SelectPicker
        :model-value="form.values.contentType"
        :get-option-label="(id: ContentType) => contentType(id).label"
        @update:model-value="changeContentType"
      >
        <SelectPickerTrigger><SelectPickerValue /></SelectPickerTrigger>
        <SelectPickerContent>
          <SelectPickerItem
            v-for="entry in contentTypeCatalog"
            :key="entry.id"
            :item-key="entry.id"
            :label="entry.label"
          />
        </SelectPickerContent>
      </SelectPicker>
    </Field>

    <!-- Mode is fixed at creation: the two bake different bytes, so a printed code can never switch. -->
    <Field
      v-if="!isEdit"
      label="How it resolves"
      :helper="isModeLocked ? ModeLockedNote : ContentModeDisplays[mode].note"
    >
      <SegmentedPicker
        type="single"
        :model-value="mode"
        aria-label="How the code resolves"
        @update:model-value="
          (next) => (next === ContentMode.Static || next === ContentMode.Dynamic) && form.setValue('mode', next)
        "
      >
        <ToggleInput
          v-for="option in Object.values(ContentMode)"
          :key="option"
          variant="soft"
          :value="option"
          size="sm"
          :disabled="isModeLocked && option !== ContentMode.Dynamic"
        >
          {{ ContentModeDisplays[option].label }}
        </ToggleInput>
      </SegmentedPicker>
    </Field>

    <Alert
      v-if="!isEdit && mode === ContentMode.Static"
      severity="info"
      title="The content is baked into the symbol"
      description="Anything already printed keeps this content forever — editing it later produces a different code. A dynamic code stays editable after printing."
    >
      <template #actions>
        <Button size="sm" variant="soft" tone="primary" @click="form.setValue('mode', ContentMode.Dynamic)"
          >Use dynamic</Button
        >
      </template>
    </Alert>

    <Field v-if="isEdit && existing?.shortUrl" label="Short link">
      <TextInput :model-value="existing.shortUrl" readonly disabled class="font-mono" />
    </Field>

    <DividerLayout orientation="horizontal" />

    <RuleEditor :form="form" :content-type="form.values.contentType" :add-blocked-reason="addBlockedReason" />
  </div>
</template>
