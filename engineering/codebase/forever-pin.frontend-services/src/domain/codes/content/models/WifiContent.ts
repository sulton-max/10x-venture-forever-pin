import { ContentType } from "../enums/ContentType";
import { WifiEncryption } from "../enums/WifiEncryption";

/** Represents Wi-Fi network credentials. */
export interface WifiContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Wifi;

  /** The network SSID. */
  ssid: string;

  /** The network password; absent on an open network. */
  password?: string;

  /** The encryption scheme the network uses. */
  encryption: WifiEncryption;

  /** Whether the network SSID is hidden. */
  hidden: boolean;
}
