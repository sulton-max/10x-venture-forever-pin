import { ApiFailureCode } from '@wow-two-beta/fe/foundation/http';
import { AppErrorType } from '@wow-two-beta/fe/foundation/results';
import { onScopeDispose, shallowRef, toValue, watch, type MaybeRefOrGetter } from 'vue';

import { FailureMessages } from '@/application/common';
import type { CodeRuleDto, CodeStyleDto, BarcodeFormat, ContentMode } from '@/domain/codes';
import { codesApi } from '@/integration/codes';

/** Defines the values a live preview renders. */
export interface CodePreviewModel {
  readonly barcodeFormat: BarcodeFormat;
  readonly mode: ContentMode;
  readonly rules: ReadonlyArray<CodeRuleDto>;
  readonly style: CodeStyleDto;
}

/** Holds how long input must settle before the server renders a preview. */
const SettleMs = 250;

/**
 * Renders a server-side SVG preview of unsaved values.
 * Every change marks the image stale at once; only the newest request may commit its result,
 * and an older reply that lands late is dropped.
 */
export function useCodePreview(source: MaybeRefOrGetter<CodePreviewModel>) {
  const svg = shallowRef<string | null>(null);
  const error = shallowRef<string | null>(null);
  const isStale = shallowRef(true);
  let generation = 0;
  let timer: ReturnType<typeof setTimeout> | undefined;
  let controller: AbortController | undefined;

  const render = async (model: CodePreviewModel, ticket: number) => {
    controller?.abort();
    controller = new AbortController();
    const result = await codesApi.preview(
      { barcodeFormat: model.barcodeFormat, mode: model.mode, rules: [...model.rules], style: model.style },
      controller.signal,
    );
    if (ticket !== generation) return;
    if (result.ok) {
      svg.value = result.value;
      error.value = null;
    } else if (result.failure.code !== ApiFailureCode.Cancelled) {
      // A draft that does not validate yet is expected while typing; say what unblocks it.
      error.value =
        result.failure.type === AppErrorType.Validation || result.failure.status === 400
          ? 'Complete every rule to see the preview.'
          : FailureMessages.describe(result.failure, 'The preview could not be rendered.');
    }
    isStale.value = false;
  };

  watch(
    () => JSON.stringify(toValue(source)),
    () => {
      const ticket = ++generation;
      isStale.value = true;
      clearTimeout(timer);
      timer = setTimeout(() => void render(toValue(source), ticket), SettleMs);
    },
    { immediate: true },
  );

  onScopeDispose(() => {
    generation += 1;
    clearTimeout(timer);
    controller?.abort();
  });

  return { svg, error, isStale };
}
