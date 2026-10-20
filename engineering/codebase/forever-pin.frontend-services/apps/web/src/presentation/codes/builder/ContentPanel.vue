<script setup lang="ts">
import { Button } from '@wow-two-beta/fe/presentation/actions';
import { Alert } from '@wow-two-beta/fe/presentation/feedback';
import {
  Field,
  ToggleGroup,
  SelectPicker,
  SelectPickerContent,
  SelectPickerItem,
  SelectPickerTrigger,
  SelectPickerValue,
  TextInput,
  ToggleInput,
} from '@wow-two-beta/fe/presentation/forms';
import { DividerLayout } from '@wow-two-beta/fe/presentation/layout';
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
import { InfoPopover } from '@/presentation/common';

import { ContentModeDisplays } from '../content/ContentModeDisplays';
import RuleEditor from '../routing/RuleEditor.vue';

import { BuilderHelp } from './BuilderHelp';

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
      <div class="relative">
        <Field label="Name">
          <TextInput
            ring="sm"
            :model-value="f.value"
            placeholder="Spring menu table tent"
            @update:model-value="(value) => f.setValue(String(value ?? ''))"
            @blur="f.onBlur"
          />
        </Field>
        <InfoPopover topic="the name" :text="BuilderHelp.name" class="absolute top-0 right-0" />
      </div>
    </FormField>

    <div class="relative">
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
      <InfoPopover topic="content types" :text="BuilderHelp.contentType" class="absolute top-0 right-0" />
    </div>

    <!-- Mode is fixed at creation: the two bake different bytes, so a printed code can never switch. -->
    <div v-if="!isEdit" class="relative">
      <Field label="How it resolves" :helper="isModeLocked ? ModeLockedNote : ContentModeDisplays[mode].note">
        <ToggleGroup
          variant="segmented"
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
            :is-disabled="isModeLocked && option !== ContentMode.Dynamic"
          >
            {{ ContentModeDisplays[option].label }}
          </ToggleInput>
        </ToggleGroup>
      </Field>
      <InfoPopover topic="static and dynamic codes" :text="BuilderHelp.mode" class="absolute top-0 right-0" />
    </div>

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

    <div v-if="isEdit && existing?.shortUrl" class="relative">
      <Field label="Short link">
        <TextInput :model-value="existing.shortUrl" readonly disabled class="font-mono" />
      </Field>
      <InfoPopover topic="the short link" :text="BuilderHelp.shortLink" class="absolute top-0 right-0" />
    </div>

    <DividerLayout orientation="horizontal" />

    <RuleEditor :form="form" :content-type="form.values.contentType" :add-blocked-reason="addBlockedReason" />
  </div>
</template>
