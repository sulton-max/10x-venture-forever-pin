import { ContentType } from "../enums/ContentType";

/** Represents a contact card. */
export interface VCardContent {
  /** The content-type discriminator. */
  type: typeof ContentType.VCard;

  /** The first name. */
  firstName: string;

  /** The last name. */
  lastName?: string;

  /** The organization. */
  org?: string;

  /** The job title. */
  title?: string;

  /** The phone number. */
  phone?: string;

  /** The email address. */
  email?: string;

  /** The website URL. */
  url?: string;

  /** The postal address. */
  address?: string;

  /** The free-text note. */
  note?: string;
}
