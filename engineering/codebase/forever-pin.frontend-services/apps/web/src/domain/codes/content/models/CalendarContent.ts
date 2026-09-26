import { Temporal } from "temporal-polyfill";

import { ContentType } from "../enums/ContentType";

/** Represents a calendar event. */
export interface CalendarContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Calendar;

  /** The event title. */
  title: string;

  /** The event start date-time. */
  start: Temporal.PlainDateTime;

  /** The event end date-time; later than the start when given. */
  end?: Temporal.PlainDateTime;

  /** The event location. */
  location?: string;

  /** The event description. */
  description?: string;
}
