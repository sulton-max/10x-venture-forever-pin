import type { Temporal } from "temporal-polyfill";

import type { BarcodeFormat, ContentMode, ContentType } from "../../content";
import type { CodeRuleDto } from "../../rules";
import type { CodeStyleDto } from "../../style";

/** Represents an issued code. */
export interface CodeDto {
  /** The unique id. */
  id: string;

  /** The URL-safe short-link slug; absent on a static code. */
  slug?: string;

  /** The short URL a dynamic code encodes; absent on a static code. */
  shortUrl?: string;

  /** The display name. */
  name: string;

  /** The rendering symbology. */
  barcodeFormat: BarcodeFormat;

  /** The content resolution mode, fixed at creation. */
  mode: ContentMode;

  /** The kind of content every rule of this code carries. */
  contentType: ContentType;

  /** Whether the code is active. */
  isActive: boolean;

  /** The recorded scan count. */
  scanCount: number;

  /** The creation timestamp. */
  createdAt: Temporal.Instant;

  /** The content-bearing routing rules. */
  rules: CodeRuleDto[];

  /** The persisted visual style. */
  style: CodeStyleDto;
}
