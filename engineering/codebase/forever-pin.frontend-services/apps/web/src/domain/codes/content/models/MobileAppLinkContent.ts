import { ContentType } from "../enums/ContentType";
import { MobileAppStoreType } from "../enums/MobileAppStoreType";

/** Represents an app-store link. */
export interface MobileAppLinkContent {
  /** The content-type discriminator. */
  type: typeof ContentType.MobileApp;

  /** The store this link points at. */
  store: MobileAppStoreType;

  /** The store URL. */
  url: string;
}
