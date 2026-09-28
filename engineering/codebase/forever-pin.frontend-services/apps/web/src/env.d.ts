/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** The Google OAuth web client id used when the API's runtime configuration omits it. */
  readonly VITE_GOOGLE_CLIENT_ID?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
