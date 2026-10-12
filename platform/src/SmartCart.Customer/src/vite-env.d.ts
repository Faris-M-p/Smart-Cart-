/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE?: string;
  readonly VITE_COMPANY_CODE?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
