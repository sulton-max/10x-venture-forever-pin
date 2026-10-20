import { downloadBlob, downloadText } from '@wow-two-beta/fe/foundation/files';
import { ApiFailureFactory } from '@wow-two-beta/fe/foundation/http';
import { ResultExtensions } from '@wow-two-beta/fe/foundation/results';
import { useAppQuery } from '@wow-two-beta/fe/query';
import { computed, toValue, type MaybeRefOrGetter } from 'vue';

import { ImageFormat, PrintSizing, type CodeDto } from '@/domain/codes';
import { codesApi } from '@/integration/codes';

import { CodeQueryKeys } from '../CodeQueryKeys';

/**
 * Prepares one saved code's downloads: its SVG is read once to size the print and to save it at a physical
 * width; its PNG is read when asked for. Idle while no code is given.
 */
export function useCodeDownload(code: MaybeRefOrGetter<CodeDto | null>) {
  const svg = useAppQuery({
    key: () => CodeQueryKeys.svg(toValue(code)?.id ?? ''),
    queryFn: ({ signal }) => codesApi.svg(toValue(code)?.id ?? '', signal),
    enabled: () => Boolean(toValue(code)),
  });

  /** Saves the code as a print-sized SVG or as its PNG, named after the code. */
  async function save(format: ImageFormat, widthMm: number) {
    const target = toValue(code);
    if (!target) return ResultExtensions.fail(ApiFailureFactory.create('validation'));
    if (format === ImageFormat.Png) {
      const png = await codesApi.png(target.id);
      if (png.ok) downloadBlob(png.value, `${target.name}.png`);
      return png;
    }
    const sized = svg.data.value && PrintSizing.withPhysicalWidth(svg.data.value, widthMm);
    if (!sized) return ResultExtensions.fail(ApiFailureFactory.create('protocol'));
    downloadText(sized, `${target.name}.svg`, 'image/svg+xml');
    return ResultExtensions.ok(undefined);
  }

  return {
    /** The symbol's size in modules, once its SVG has loaded. */
    modules: computed(() => (svg.data.value ? PrintSizing.modules(svg.data.value) : undefined)),
    /** Whether the SVG is still loading. */
    isLoading: svg.loading,
    /** The SVG read failure, or `null`. */
    error: svg.error,
    save,
  };
}
