import type { ApiWarningDto } from '@/integration/common';
import type { CodeDto } from '@/domain/codes';

import type { CodeSaveModel } from './models/CodeSaveModel';

/** Maps code payloads onto the builder's result shape. */
export const CodeMappers = {
  /** Maps a saved code and its advisory warnings. */
  toSaveModel(saved: { data: CodeDto; warnings: ReadonlyArray<ApiWarningDto> }): CodeSaveModel {
    return {
      code: saved.data,
      warnings: saved.warnings.map(({ property, message, code }) => ({ property, message, code })),
    };
  },
} as const;
