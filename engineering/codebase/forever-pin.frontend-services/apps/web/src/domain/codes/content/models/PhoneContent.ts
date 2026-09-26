import { ContentType } from "../enums/ContentType";

/** Represents a telephone number to dial. */
export interface PhoneContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Phone;

  /** The phone number to dial. */
  phone: string;
}
