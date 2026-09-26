import type { BarcodeFormat, ContentMode, ContentType, CodeRuleDto, CodeStyleDto } from "@/domain/codes";

/** Defines the builder request; omit `mode` when submitting an update. */
export interface CodeCreateUpdateApiRequest {
  /** The code's display name. */
  name: string;

  /** The symbology the code renders as. */
  barcodeFormat: BarcodeFormat;

  /** The content resolution mode; omit it from updates. */
  mode: ContentMode;

  /** The kind of content every rule carries. */
  contentType: ContentType;

  /** The routing rules, each carrying the content it serves. At least one is required. */
  rules: CodeRuleDto[];

  /** The visual style to persist. */
  style: CodeStyleDto;
}

/** Defines an update request without the immutable content mode. */
export type CodeUpdateApiRequest = Omit<CodeCreateUpdateApiRequest, "mode">;
