import type { CodeDto } from '@/domain/codes';

/** Represents one advisory finding a save raised without blocking it. */
export interface CodeWarningModel {
  /** The field path the finding concerns, such as `rules[0].content.url`. */
  readonly property: string;

  /** The display message. */
  readonly message: string;

  /** The stable finding code, such as `RedirectTargetInsecure`. */
  readonly code: string;
}

/** Represents a saved code and the advisory warnings its save raised. */
export interface CodeSaveModel {
  /** The saved code. */
  readonly code: CodeDto;

  /** Findings that did not block the save, such as an insecure destination. */
  readonly warnings: ReadonlyArray<CodeWarningModel>;
}
