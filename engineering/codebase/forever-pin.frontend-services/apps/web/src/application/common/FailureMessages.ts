import { AppErrorType, type AppError } from '@wow-two-beta/fe/foundation/results';

/** Holds the longest server message shown verbatim; anything longer reads as diagnostics. */
const MaxProblemText = 300;

/** Maps expected failures to display text a person can act on. */
export const FailureMessages = {
  /** Describes a failed request; the API's own problem text wins over the fallback. */
  describe(failure: AppError | null | undefined, fallback: string): string {
    if (!failure) return fallback;
    if (failure.type === AppErrorType.Unavailable || failure.type === AppErrorType.Timeout)
      return `${fallback} Check your connection and try again.`;
    return FailureMessages.problemText(failure) ?? fallback;
  },

  /** Reads the problem's `detail`, then `title`, when it is short display text. */
  problemText(failure: AppError): string | undefined {
    const problem = 'problem' in failure ? (failure.problem as Readonly<Record<string, unknown>> | null) : null;
    for (const key of ['detail', 'title']) {
      const text = problem?.[key];
      if (typeof text === 'string' && text.trim() && text.length <= MaxProblemText) return text.trim();
    }
    return undefined;
  },
} as const;
