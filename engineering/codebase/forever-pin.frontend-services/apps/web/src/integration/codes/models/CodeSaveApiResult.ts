import type { CodeDto } from "@/domain/codes/common";
import type { ApiWarning } from "../../common/client";

/** Represents a saved code and the advisory warnings its save raised. */
export interface CodeSaveApiResult {
  /** The saved code. */
  readonly code: CodeDto;
  /** Findings that did not block the save, such as an insecure destination. */
  readonly warnings: readonly ApiWarning[];
}
