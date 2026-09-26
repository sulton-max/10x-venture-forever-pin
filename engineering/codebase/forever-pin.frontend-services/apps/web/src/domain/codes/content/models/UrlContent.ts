import { ContentType } from "../enums/ContentType";

/** Represents a destination URL. */
export interface UrlContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Url;

  /** The destination URL. */
  url: string;
}
