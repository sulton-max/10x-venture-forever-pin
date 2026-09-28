<script setup lang="ts">
import { ChevronDown, ChevronUp, GripVertical, Plus, Trash2 } from '@lucide/vue';
import { useFieldArray } from '@wow-two-beta/ui-vue/forms-engine';
import { Button } from '@wow-two-beta/ui-vue/presentation/actions';
import {
  SelectPicker,
  SelectPickerContent,
  SelectPickerItem,
  SelectPickerTrigger,
  SelectPickerValue,
  SortableGroup,
  SortableGroupHandle,
  SortableGroupItem,
  SortableGroupMoveButton,
  TextInput,
} from '@wow-two-beta/ui-vue/presentation/forms';
import { computed } from 'vue';

import { emptyConditionalRule, emptyDefaultRule, type CodeBuilderForm } from '@/application/codes';
import {
  CodeRuleType,
  RuleConditionType,
  type CodeRuleDto,
  type ConditionalRuleDto,
  type ContentType,
  type DefaultPointerRuleDto,
} from '@/domain/codes';

import ContentFieldControls from '../content/ContentFieldControls.vue';

import { RuleConditionTypeDisplays } from './RuleConditionTypeDisplays';

/** Defines the props of {@link RuleEditor}. */
export interface RuleEditorProps {
  /** The builder form; it owns the `rules` array. */
  readonly form: CodeBuilderForm;
  /** The content type every rule carries. */
  readonly contentType: ContentType;
  /** Why no further rule can be added, or `null` when one can — a saved static code holds one rule. */
  readonly addBlockedReason: string | null;
}

/**
 * Renders a lone catch-all as plain content fields, or an ordered rule list: conditionals reorder by drag or
 * keyboard, and the catch-all stays last, where the router evaluates it. A catch-all pointing at a conditional
 * names its target by position and re-targets explicitly when that rule is removed.
 */
defineOptions({ name: 'RuleEditor' });
const props = defineProps<RuleEditorProps>();
const FormField = props.form.Field;

const rules = useFieldArray<CodeRuleDto>(props.form, 'rules');
const conditional = rules.variant((rule): rule is ConditionalRuleDto => rule.type === CodeRuleType.Conditional);
const pointer = rules.variant((rule): rule is DefaultPointerRuleDto => rule.type === CodeRuleType.DefaultPointer);

const values = computed(() => props.form.values.rules);
const conditionalCount = computed(() => values.value.filter((rule) => rule.type === CodeRuleType.Conditional).length);
const fallbackIndex = computed(() => values.value.findIndex((rule) => rule.type !== CodeRuleType.Conditional));
const isSingleDefault = computed(() => values.value.length === 1 && values.value[0]?.type === CodeRuleType.Default);
const conditionalRows = computed(() =>
  rules.rows.filter((row) => values.value[row.index]?.type === CodeRuleType.Conditional),
);
const fallbackRow = computed(() => rules.rows.find((row) => row.index === fallbackIndex.value));
const targets = computed(() =>
  values.value.flatMap((rule, index) =>
    rule.type === CodeRuleType.Conditional ? [{ order: rule.order, label: `Rule ${index + 1}` }] : [],
  ),
);

function ruleAt(index: number): CodeRuleDto | undefined {
  return values.value[index];
}

/** Adds a conditional ahead of the catch-all — conditionals match in order, the catch-all serves the rest. */
function addRule(): void {
  const rule = emptyConditionalRule(props.contentType, values.value);
  if (fallbackIndex.value < 0) rules.push(rule);
  else rules.insert(fallbackIndex.value, rule);
}

function reorder(from: number, to: number): void {
  const clamped = Math.max(0, Math.min(conditionalCount.value - 1, to));
  if (clamped !== from) rules.move(from, clamped);
}
</script>

<template>
  <div v-if="isSingleDefault" class="flex flex-col gap-4">
    <FormField v-slot="f" name="rules">
      <ul v-if="f.errors.length" class="flex flex-col gap-1 text-sm text-destructive">
        <li v-for="message in f.errors" :key="message">{{ message }}</li>
      </ul>
    </FormField>
    <ContentFieldControls
      v-if="values[0] && values[0].type !== CodeRuleType.DefaultPointer"
      :form="form"
      content-path="rules[0].content"
      :content="values[0].content"
    />
    <div class="flex flex-col items-start gap-1">
      <Button
        variant="ghost"
        tone="neutral"
        size="sm"
        class="-ml-2"
        :is-disabled="addBlockedReason !== null"
        @click="addRule"
      >
        <template #leading><Plus :size="16" aria-hidden="true" /></template>
        Add a routing rule
      </Button>
      <p v-if="addBlockedReason" class="text-xs text-muted-foreground">{{ addBlockedReason }}</p>
    </div>
  </div>

  <div v-else class="flex flex-col gap-2.5">
    <div class="flex flex-wrap items-baseline justify-between gap-x-3">
      <p class="text-sm font-medium">Routing rules</p>
      <p class="text-xs text-muted-foreground">Matched top-down — the first match wins</p>
    </div>

    <FormField v-slot="f" name="rules">
      <ul v-if="f.errors.length" class="flex flex-col gap-1 text-sm text-destructive">
        <li v-for="message in f.errors" :key="message">{{ message }}</li>
      </ul>
    </FormField>

    <div class="overflow-hidden rounded-xl border border-border bg-card">
      <SortableGroup @reorder="reorder">
        <SortableGroupItem
          v-for="row in conditionalRows"
          :key="row.key"
          :index="row.index"
          class="flex items-start gap-2.5 border-b border-border p-3"
        >
          <div class="flex flex-col items-center gap-1 pt-1">
            <SortableGroupHandle class="text-subtle-foreground hover:text-foreground" aria-label="Drag to reorder">
              <GripVertical :size="15" />
            </SortableGroupHandle>
            <span
              class="grid size-5 shrink-0 place-items-center rounded-md bg-muted text-xs font-medium text-muted-foreground"
            >
              {{ row.index + 1 }}
            </span>
            <SortableGroupMoveButton
              direction="previous"
              :label="`Move rule ${row.index + 1} up`"
              class="text-subtle-foreground"
            >
              <ChevronUp :size="14" />
            </SortableGroupMoveButton>
            <SortableGroupMoveButton
              direction="next"
              :label="`Move rule ${row.index + 1} down`"
              class="text-subtle-foreground"
            >
              <ChevronDown :size="14" />
            </SortableGroupMoveButton>
          </div>

          <div
            v-if="ruleAt(row.index)?.type === CodeRuleType.Conditional"
            class="grid min-w-0 flex-1 grid-cols-1 gap-2"
          >
            <div class="grid grid-cols-1 gap-2 sm:grid-cols-[minmax(0,9rem)_minmax(0,1fr)]">
              <conditional.Field v-slot="f" :index="row.index" name="condition">
                <SelectPicker
                  :model-value="f.value"
                  :get-option-label="(type: RuleConditionType) => RuleConditionTypeDisplays[type].label"
                  aria-label="Condition"
                  @update:model-value="(value) => value && f.setValue(value)"
                >
                  <SelectPickerTrigger><SelectPickerValue /></SelectPickerTrigger>
                  <SelectPickerContent>
                    <SelectPickerItem
                      v-for="type in Object.values(RuleConditionType)"
                      :key="type"
                      :item-key="type"
                      :label="RuleConditionTypeDisplays[type].label"
                    />
                  </SelectPickerContent>
                </SelectPicker>
              </conditional.Field>
              <conditional.Field v-slot="f" :index="row.index" name="conditionValue">
                <TextInput
                  ring="sm"
                  :model-value="f.value"
                  :placeholder="
                    RuleConditionTypeDisplays[(ruleAt(row.index) as ConditionalRuleDto).condition].placeholder
                  "
                  aria-label="Condition value"
                  @update:model-value="(value) => f.setValue(String(value ?? ''))"
                  @blur="f.onBlur"
                />
              </conditional.Field>
              <p
                v-if="(ruleAt(row.index) as ConditionalRuleDto).condition === RuleConditionType.Country"
                class="text-xs text-muted-foreground sm:col-span-2"
              >
                Scans are located with
                <a href="https://db-ip.com" class="underline hover:text-foreground">IP Geolocation by DB-IP</a>.
              </p>
            </div>
            <ContentFieldControls
              :form="form"
              :content-path="`rules[${row.index}].content`"
              :content="(ruleAt(row.index) as ConditionalRuleDto).content"
            />
          </div>

          <Button
            tone="danger"
            variant="ghost"
            shape="square"
            :aria-label="`Remove rule ${row.index + 1}`"
            @click="rules.remove(row.index)"
          >
            <Trash2 :size="16" />
          </Button>
        </SortableGroupItem>
      </SortableGroup>

      <div v-if="fallbackRow" class="flex items-start gap-2.5 border-b border-border bg-muted/30 p-3">
        <span
          class="mt-1.5 grid size-5 shrink-0 place-items-center rounded-md bg-muted text-xs text-muted-foreground"
          aria-hidden="true"
        >
          ∗
        </span>
        <div class="grid min-w-0 flex-1 grid-cols-1 gap-2">
          <template v-if="ruleAt(fallbackRow.index)?.type === CodeRuleType.DefaultPointer">
            <pointer.Field v-slot="f" :index="fallbackRow.index" name="targetOrder">
              <div class="flex flex-wrap items-center gap-2 text-sm">
                <span class="text-muted-foreground">Everyone else gets the same destination as</span>
                <SelectPicker
                  :model-value="targets.some((target) => target.order === f.value) ? f.value : null"
                  :get-option-label="
                    (order: number) => targets.find((target) => target.order === order)?.label ?? 'a removed rule'
                  "
                  aria-label="Rule everyone else follows"
                  @update:model-value="(value) => value != null && f.setValue(value)"
                >
                  <SelectPickerTrigger><SelectPickerValue placeholder="Choose a rule" /></SelectPickerTrigger>
                  <SelectPickerContent>
                    <SelectPickerItem
                      v-for="target in targets"
                      :key="target.order"
                      :item-key="target.order"
                      :label="target.label"
                    />
                  </SelectPickerContent>
                </SelectPicker>
              </div>
              <p v-for="message in f.errors" :key="message" class="text-sm text-destructive">{{ message }}</p>
            </pointer.Field>
          </template>
          <template v-else-if="ruleAt(fallbackRow.index)?.type === CodeRuleType.Default">
            <p class="text-sm text-muted-foreground">Everyone else</p>
            <ContentFieldControls
              :form="form"
              :content-path="`rules[${fallbackRow.index}].content`"
              :content="(ruleAt(fallbackRow.index) as Exclude<CodeRuleDto, DefaultPointerRuleDto>).content"
            />
          </template>
        </div>
        <Button
          tone="danger"
          variant="ghost"
          shape="square"
          aria-label="Remove the catch-all"
          @click="rules.remove(fallbackRow.index)"
        >
          <Trash2 :size="16" />
        </Button>
      </div>

      <div class="flex flex-wrap">
        <Button
          variant="ghost"
          tone="neutral"
          size="sm"
          class="flex-1 justify-start rounded-none px-4 py-3 text-muted-foreground"
          :is-disabled="addBlockedReason !== null"
          @click="addRule"
        >
          <template #leading><Plus :size="16" aria-hidden="true" /></template>
          Add a routing rule
        </Button>
        <!-- Without a catch-all an unmatched scan does not resolve — but removing one must not be one-way. -->
        <Button
          v-if="fallbackIndex < 0"
          variant="ghost"
          tone="neutral"
          size="sm"
          class="flex-1 justify-start rounded-none px-4 py-3 text-muted-foreground"
          @click="rules.push(emptyDefaultRule(contentType))"
        >
          <template #leading><Plus :size="16" aria-hidden="true" /></template>
          Add a catch-all
        </Button>
      </div>
    </div>
    <p v-if="fallbackIndex < 0" class="text-xs text-muted-foreground">
      No catch-all: a scan that matches no rule does not resolve.
    </p>
  </div>
</template>
