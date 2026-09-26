import { ContentType } from "../enums/ContentType";

/** Represents an SMS recipient and message. */
export interface SmsContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Sms;

  /** The recipient phone number. */
  phone: string;

  /** The message text prefilled in the composer. */
  message?: string;
}
