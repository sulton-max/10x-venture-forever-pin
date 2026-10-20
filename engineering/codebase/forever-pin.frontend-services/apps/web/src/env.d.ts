/// <reference types="vite/client" />

/** The build's version: `X.Y.Z` (or a pre-release) from CI's `APP_VERSION`; `dev` in a local build. */
declare const __APP_VERSION__: string;

interface ImportMetaEnv {
  /** The Google OAuth web client id used when the API's runtime configuration omits it. */
  readonly VITE_GOOGLE_CLIENT_ID?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
