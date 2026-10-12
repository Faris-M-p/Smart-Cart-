export type StorefrontConfig = {
  companyCode: string;
  companyName: string;
  themeCode: string;
  themeVariables: string;
};

const API_BASE = import.meta.env.VITE_API_BASE || "";

export async function fetchStorefrontConfig(companyCode: string): Promise<StorefrontConfig> {
  const response = await fetch(`${API_BASE}/api/storefront/config/${encodeURIComponent(companyCode)}`);
  if (!response.ok) {
    throw new Error("storefront_config_unavailable");
  }
  return response.json();
}
