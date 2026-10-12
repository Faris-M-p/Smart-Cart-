export type ThemeTokens = Record<string, string>;

/** Ten configurable visual themes — shared layout, different tokens. */
export const themeCatalog: Record<string, ThemeTokens> = {
  default: {
    "--sc-bg": "#f7f7f5",
    "--sc-fg": "#1a1a1a",
    "--sc-brand": "#c62828"
  },
  ocean: {
    "--sc-bg": "#f0f7fb",
    "--sc-fg": "#0d2137",
    "--sc-brand": "#0277bd"
  },
  forest: {
    "--sc-bg": "#f3f7f2",
    "--sc-fg": "#1b2e1c",
    "--sc-brand": "#2e7d32"
  },
  sunset: {
    "--sc-bg": "#fff8f1",
    "--sc-fg": "#3e2723",
    "--sc-brand": "#ef6c00"
  },
  slate: {
    "--sc-bg": "#f5f6f8",
    "--sc-fg": "#212121",
    "--sc-brand": "#455a64"
  },
  grape: {
    "--sc-bg": "#faf5ff",
    "--sc-fg": "#2e1a47",
    "--sc-brand": "#6a1b9a"
  },
  mint: {
    "--sc-bg": "#f2fffb",
    "--sc-fg": "#14352e",
    "--sc-brand": "#00897b"
  },
  cocoa: {
    "--sc-bg": "#faf6f2",
    "--sc-fg": "#3e2723",
    "--sc-brand": "#6d4c41"
  },
  midnight: {
    "--sc-bg": "#12141a",
    "--sc-fg": "#e8eaed",
    "--sc-brand": "#90caf9"
  },
  coral: {
    "--sc-bg": "#fff5f4",
    "--sc-fg": "#3b1110",
    "--sc-brand": "#e64a19"
  }
};
