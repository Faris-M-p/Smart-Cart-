import { themeCatalog } from "./catalog";

/** Applies one of ten theme token sets to :root. No separate app per theme. */
export function applyThemeTokens(themeCode: string): void {
  const theme = themeCatalog[themeCode] ?? themeCatalog.default;
  const root = document.documentElement;
  Object.entries(theme).forEach(([key, value]) => {
    root.style.setProperty(key, value);
  });
  root.dataset.theme = themeCode in themeCatalog ? themeCode : "default";
}
