import { useEffect, useState } from "react";
import { fetchStorefrontConfig } from "../api/storefrontApi";
import { applyThemeTokens } from "../themes/applyTheme";

export function HomePage() {
  const [status, setStatus] = useState("Loading storefront config…");

  useEffect(() => {
    const companyCode = import.meta.env.VITE_COMPANY_CODE || "demo";
    fetchStorefrontConfig(companyCode)
      .then((cfg) => {
        applyThemeTokens(cfg.themeCode || "default");
        setStatus(`Ready: ${cfg.companyName} · theme ${cfg.themeCode}`);
      })
      .catch(() => {
        applyThemeTokens("default");
        setStatus("Using default theme (API not connected yet).");
      });
  }, []);

  return (
    <section>
      <h1 className="text-3xl font-semibold mb-2">Neighbourhood storefront</h1>
      <p className="opacity-80 mb-4">{status}</p>
      <p className="text-sm opacity-70">
        One React app, ten configurable themes via CSS variables. Homepage sections
        (banners, featured categories/products, best sellers, new arrivals, tiles)
        will render from published API config only.
      </p>
    </section>
  );
}
