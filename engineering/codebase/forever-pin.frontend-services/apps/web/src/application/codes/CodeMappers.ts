import type { ApiWarned } from '@wow-two-beta/fe/foundation/http';
import type { CodeDto } from '@/domain/codes';

import type { CodeSaveModel } from './models/CodeSaveModel';

/** Maps code payloads onto the builder's result shape. */
export const CodeMappers = {
  /** Maps a saved code and its advisory warnings. */
  toSaveModel(saved: ApiWarned<CodeDto>): CodeSaveModel {
    return {
      code: saved.data,
      warnings: saved.warnings.map(({ property, message, code }) => ({ property, message, code })),
    };
  },
} as const;
