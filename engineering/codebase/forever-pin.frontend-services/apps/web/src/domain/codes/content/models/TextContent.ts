import { ContentType } from "../enums/ContentType";

/** Represents free-form text. */
export interface TextContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Text;

  /** The literal text. */
  text: string;
}
