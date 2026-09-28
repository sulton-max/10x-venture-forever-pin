import { ResultExtensions } from '@wow-two-beta/ui-vue/foundation/results';
import { useAppMutation } from '@wow-two-beta/ui-vue/query';

import { codesApi } from '@/integration/codes';

import { toCreateCodeRequest, toUpdateCodeRequest } from '../CodeForm';
import { CodeMappers } from '../CodeMappers';
import { CodeQueryKeys } from '../CodeQueryKeys';
import type { CodeFormModel } from '../models/CodeFormModel';

/** Provides the code writes; each refreshes every code read once the API confirms it. */
export function useCodeCommands() {
  const invalidates = () => [CodeQueryKeys.all];

  return {
    /** Creates a code; the result carries destination warnings that did not block it. */
    create: useAppMutation({
      mutationFn: async (values: CodeFormModel, { signal }) =>
        ResultExtensions.map(await codesApi.create(toCreateCodeRequest(values), signal), CodeMappers.toSaveModel),
      invalidates,
    }),

    /** Replaces a code's editable fields; its mode, slug and scans stay. */
    update: useAppMutation({
      mutationFn: async ({ id, values }: { id: string; values: CodeFormModel }, { signal }) =>
        ResultExtensions.map(await codesApi.update(id, toUpdateCodeRequest(values), signal), CodeMappers.toSaveModel),
      invalidates,
    }),

    /** Pauses or resumes a code. */
    setActive: useAppMutation({
      mutationFn: ({ id, isActive }: { id: string; isActive: boolean }, { signal }) =>
        codesApi.setActive(id, isActive, signal),
      invalidates,
    }),

    /** Deletes a code and its rules. */
    remove: useAppMutation({
      mutationFn: (id: string, { signal }) => codesApi.delete(id, signal),
      invalidates,
    }),
  };
}
