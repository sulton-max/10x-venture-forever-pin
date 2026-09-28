import type { BarcodeFormat, CodeRuleDto, CodeStyleDto, ContentMode, ContentType } from '@/domain/codes';

/** Represents the builder's editable values; `mode` is fixed once a code exists. */
export interface CodeFormModel {
  /** The code's display name. */
  name: string;

  /** The symbology the code renders as. */
  barcodeFormat: BarcodeFormat;

  /** Whether the content is baked into the symbol or resolved at scan time. */
  mode: ContentMode;

  /** The kind of content every rule carries. */
  contentType: ContentType;

  /** The routing rules; a conditional rule's `order` is its identity until submission. */
  rules: CodeRuleDto[];

  /** The visual style. */
  style: CodeStyleDto;
}
