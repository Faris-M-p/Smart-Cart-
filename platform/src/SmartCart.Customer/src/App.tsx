import { Link, Route, Routes } from "react-router-dom";
import { HomePage } from "./pages/HomePage";
import { applyThemeTokens } from "./themes/applyTheme";

applyThemeTokens("default");

export default function App() {
  return (
    <div className="min-h-screen bg-[var(--sc-bg)] text-[var(--sc-fg)]">
      <header className="border-b border-black/10 px-4 py-3 flex gap-4 items-center">
        <strong className="text-[var(--sc-brand)] text-xl">SmartCart</strong>
        <nav className="flex gap-3 text-sm">
          <Link to="/">Home</Link>
          <Link to="/shop">Shop</Link>
          <Link to="/cart">Cart</Link>
          <Link to="/account">Account</Link>
        </nav>
      </header>
      <main className="mx-auto max-w-5xl px-4 py-6">
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/shop" element={<p>Shop listing will call /api/storefront/* only.</p>} />
          <Route path="/cart" element={<p>Cart — API only, no direct DB access.</p>} />
          <Route path="/account" element={<p>Customer account — JWT via API.</p>} />
        </Routes>
      </main>
    </div>
  );
}
