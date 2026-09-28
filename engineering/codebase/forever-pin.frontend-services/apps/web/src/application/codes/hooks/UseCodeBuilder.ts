import type { AppForm } from '@wow-two-beta/ui-vue/forms-engine';
import { ResultExtensions } from '@wow-two-beta/ui-vue/foundation/results';
import { computed, shallowRef, toValue, watch, type MaybeRefOrGetter } from 'vue';

import { useAppForm } from '@/application/common';
import { ContentMode, type CodeDto } from '@/domain/codes';

import { CodeFormSchema, emptyCodeForm, mapCodeFieldPath, oppositeMode, toCodeForm, toCopyCodeForm } from '../CodeForm';
import { CodeBuilderPhase } from '../models/CodeBuilderPhase';
import type { CodeFormModel } from '../models/CodeFormModel';
import type { CodeWarningModel } from '../models/CodeSaveModel';

import { useCode } from './UseCode';
import { useCodeCommands } from './UseCodeCommands';

/** Defines which code the builder opens. */
export interface CodeBuilderSource {
  /** The code to edit; absent for a new code. */
  readonly codeId?: string;
  /** The code a new one is copied from. */
  readonly copyFromId?: string;
  /** The mode a copy is created in; the source's opposite when absent. */
  readonly copyMode?: ContentMode;
}

/** Represents the builder's form. */
export type CodeBuilderForm = AppForm<CodeFormModel>;

/**
 * Owns the builder's lifecycle: every change of the opened code starts a fresh session (values, saved result,
 * warnings and any running save are discarded), a failed load blocks editing, and a save establishes a new
 * baseline so later edits read as unsaved.
 */
export function useCodeBuilder(source: MaybeRefOrGetter<CodeBuilderSource>) {
  const commands = useCodeCommands();
  const codeId = computed(() => toValue(source).codeId);
  const sourceId = computed(() => toValue(source).codeId ?? toValue(source).copyFromId);
  const sourceQuery = useCode(sourceId);

  const saved = shallowRef<CodeDto | null>(null);
  const warnings = shallowRef<ReadonlyArray<CodeWarningModel>>([]);
  const copiedInto = shallowRef<ContentMode | null>(null);
  const appliedFor = shallowRef<string | undefined>();
  const savedRevision = shallowRef(0);

  const form = useAppForm<CodeFormModel>({
    defaultValues: emptyCodeForm(),
    schema: CodeFormSchema,
    mapFieldPath: mapCodeFieldPath,
    fallbackErrorMessage: 'The code could not be saved. Try again.',
    onSubmit: async (values) => {
      warnings.value = [];
      const id = codeId.value;
      const result = id ? await commands.update.mutateAsync({ id, values }) : await commands.create.mutateAsync(values);
      if (!result.ok) return result;
      saved.value = result.value.code;
      savedRevision.value += 1;
      warnings.value = result.value.warnings;
      // An edit's baseline is the saved code; a new code keeps its values as the baseline for a quick variant.
      form.reset(id ? toCodeForm(result.value.code) : values);
      return ResultExtensions.ok(undefined);
    },
  });

  // Each opened code is a new session; a late reply for the previous code can no longer commit.
  watch(
    () => [sourceId.value, codeId.value, toValue(source).copyMode] as const,
    ([id]) => {
      saved.value = null;
      warnings.value = [];
      copiedInto.value = null;
      appliedFor.value = undefined;
      form.invalidateSession(emptyCodeForm());
      if (!id) appliedFor.value = '';
    },
    { immediate: true },
  );

  // A source applies once per session; later refetches never overwrite what the person is editing.
  watch(
    () => sourceQuery.data.value,
    (code) => {
      if (!code || code.id !== sourceId.value || appliedFor.value === code.id) return;
      appliedFor.value = code.id;
      if (codeId.value) {
        form.invalidateSession(toCodeForm(code));
        return;
      }
      const mode = toValue(source).copyMode ?? oppositeMode(code.mode);
      copiedInto.value = mode;
      form.invalidateSession(toCopyCodeForm(code, mode));
    },
    { immediate: true },
  );

  const phase = computed<CodeBuilderPhase>(() => {
    if (appliedFor.value !== undefined) return CodeBuilderPhase.Ready;
    return sourceQuery.error.value ? CodeBuilderPhase.Failed : CodeBuilderPhase.Loading;
  });

  return {
    form,
    phase,
    /** Whether an existing code is being edited. */
    isEdit: computed(() => Boolean(codeId.value)),
    /** The loaded code being edited, or `null`. */
    existing: computed(() => (codeId.value && appliedFor.value ? (sourceQuery.data.value ?? null) : null)),
    /** The mode a copy is being created in, or `null`. */
    copiedInto,
    /** The code the last save returned, or `null` before one. */
    saved,
    /** Counts saves, so the saved image reloads after each one. */
    savedRevision,
    /** The advisory findings of the last save. */
    warnings,
    /** The failed load, or `null`. */
    loadFailure: sourceQuery.error,
    /** Retries a failed load. */
    retryLoad: () => sourceQuery.refetch(),
    /** Clears the saved panel; the values stay for a quick variant. */
    startAnother: () => {
      saved.value = null;
      warnings.value = [];
    },
  };
}
