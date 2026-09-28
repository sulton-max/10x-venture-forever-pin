import type { CodeDto, ImageFormat } from '@/domain/codes';
import { codesApi } from '@/integration/codes';

/** Builds a saved code's image address; `revision` changes after each save, so a fresh image loads. */
export function codeImageUrl(code: CodeDto, format: ImageFormat, revision = 0): string {
  return `${codesApi.imageUrl(code.id, format)}&revision=${revision}`;
}
