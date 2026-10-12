// scripts/hero-screenshot.mjs
//
// Capture the README hero image: the dashboard rendered inside a clean
// browser-window mockup, light theme, at retina resolution.
//
// The stage backdrop and window shadow are neutral zinc. They used to be a
// lavender gradient over a purple-navy shadow, tuned to the old indigo brand —
// which would now tint the hero a colour that appears nowhere in the template.
// Serves the built static output over a tiny in-process HTTP server, measures
// where the chart rows end so the frame cuts cleanly, then screenshots a
// CSS browser frame wrapping the dashboard in an <iframe>.
//
// Usage:  npm run build && node scripts/hero-screenshot.mjs
//
// Env:
//   SCALE=2              (default 2 — retina; deviceScaleFactor)
//   PLAYWRIGHT_PATH=…    (resolve playwright from elsewhere if not a local dep)

import http from 'node:http';
import { readFile } from 'node:fs/promises';
import { existsSync, statSync, mkdirSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createRequire } from 'node:module';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const ROOT = path.resolve(__dirname, '..');

// ── Per-repo config ──────────────────────────────────────────────────────
const SERVE_DIR = path.resolve(ROOT, 'dist-modern'); // built static output
const PAGE = '/index.html'; // dashboard page
// ─────────────────────────────────────────────────────────────────────────

// Everything below is env-overridable so the same frame can produce the README
// hero and the differently-proportioned promo shots the blog listings use,
// without a second copy of the mockup markup drifting away from this one.
//
//   OUT=…         output path        (default: the README hero)
//   ADDRESS=…     address-bar text
//   CONTENT_W=…   dashboard render width in CSS px
//   RATIO=…       target width/height; overrides the content-based cut
//   SCALE=…       deviceScaleFactor  (default 2)
const OUT = process.env.OUT
  ? path.resolve(ROOT, process.env.OUT)
  : path.resolve(ROOT, 'metis-bootstrap-admin-dashboard.png');
const ADDRESS = process.env.ADDRESS || 'preview.colorlib.com/theme/metis';

const SCALE = Number(process.env.SCALE || 2);
const CONTENT_W = Number(process.env.CONTENT_W || 1520); // dashboard render width
const RATIO = process.env.RATIO ? Number(process.env.RATIO) : null;
const STAGE_PAD = 44; // gutter around the window
const BAR_H = 56; // browser toolbar height

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PATH || 'playwright');

const MIME = {
  '.html': 'text/html; charset=utf-8', '.css': 'text/css', '.js': 'text/javascript',
  '.mjs': 'text/javascript', '.json': 'application/json', '.svg': 'image/svg+xml',
  '.png': 'image/png', '.jpg': 'image/jpeg', '.jpeg': 'image/jpeg', '.gif': 'image/gif',
  '.webp': 'image/webp', '.avif': 'image/avif', '.ico': 'image/x-icon',
  '.woff': 'font/woff', '.woff2': 'font/woff2', '.ttf': 'font/ttf',
  '.eot': 'application/vnd.ms-fontobject', '.map': 'application/json', '.txt': 'text/plain'
};

function serve(rootDir) {
  const server = http.createServer(async (req, res) => {
    try {
      let urlPath = decodeURIComponent((req.url || '/').split('?')[0]);
      if (urlPath === '/') urlPath = '/index.html';
      let filePath = path.join(rootDir, urlPath);
      if (existsSync(filePath) && statSync(filePath).isDirectory()) {
        filePath = path.join(filePath, 'index.html');
      }
      if (!existsSync(filePath)) { res.statusCode = 404; res.end('not found'); return; }
      res.setHeader('Content-Type', MIME[path.extname(filePath).toLowerCase()] || 'application/octet-stream');
      res.end(await readFile(filePath));
    } catch (e) { res.statusCode = 500; res.end(String(e)); }
  });
  return new Promise((resolve) => server.listen(0, '127.0.0.1', () =>
    resolve({ port: server.address().port, close: () => server.close() })));
}

const forceLight = () => {
  try {
    ['theme', 'color-theme', 'dash26-theme', 'lte.theme', 'data-theme'].forEach((k) =>
      localStorage.setItem(k, 'light'));
  } catch (_e) { /* ignore */ }
};

function frameHTML(url, iframeH) {
  const stageH = STAGE_PAD * 2 + BAR_H + iframeH;
  return `<!doctype html><html><head><meta charset="utf-8"><style>
  *{box-sizing:border-box} html,body{margin:0;padding:0}
  .stage{width:${CONTENT_W + STAGE_PAD * 2}px;height:${stageH}px;padding:${STAGE_PAD}px;
    background:linear-gradient(135deg,#eff1f4 0%,#e4e6ea 50%,#f4f5f7 100%);
    display:flex;align-items:center;justify-content:center;font-family:Inter,system-ui,-apple-system,sans-serif}
  .win{width:${CONTENT_W}px;border-radius:14px;overflow:hidden;background:#fff;
    border:1px solid rgba(24,24,27,.08);
    box-shadow:0 40px 80px -24px rgba(24,24,27,.28),0 16px 36px -16px rgba(24,24,27,.18)}
  .bar{height:${BAR_H}px;display:flex;align-items:center;gap:16px;padding:0 18px;
    background:#f4f5f8;border-bottom:1px solid #e6e8ee}
  .dots{display:flex;gap:9px}
  .dot{width:13px;height:13px;border-radius:50%}
  .address{flex:1;display:flex;justify-content:center}
  .pill{display:flex;align-items:center;gap:8px;height:34px;padding:0 18px;background:#fff;
    border:1px solid #e3e5ec;border-radius:999px;color:#5b6472;font-size:14px;font-weight:500;
    min-width:360px;justify-content:center}
  .pill svg{flex:none}
  .icons{display:flex;gap:16px;color:#b4bac6}
  iframe{display:block;width:${CONTENT_W}px;height:${iframeH}px;border:0;background:#fff}
  </style></head><body>
  <div class="stage"><div class="win">
    <div class="bar">
      <div class="dots"><span class="dot" style="background:#ff5f57"></span><span class="dot" style="background:#febc2e"></span><span class="dot" style="background:#28c840"></span></div>
      <div class="icons"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 18l-6-6 6-6"/></svg><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 18l6-6-6-6"/></svg></div>
      <div class="address"><div class="pill"><svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg><span>${ADDRESS}</span></div></div>
      <div class="icons"><svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 5v14M5 12h14"/></svg></div>
    </div>
    <iframe src="${url}" scrolling="no"></iframe>
  </div></div>
  </body></html>`;
}

async function main() {
  if (!existsSync(SERVE_DIR)) {
    console.error(`No build output at ${SERVE_DIR} — run the build first.`);
    process.exit(1);
  }
  mkdirSync(path.dirname(OUT), { recursive: true });
  const { port, close } = await serve(SERVE_DIR);
  const url = `http://127.0.0.1:${port}${PAGE}`;

  const browser = await chromium.launch();
  const ctx = await browser.newContext({ deviceScaleFactor: SCALE, colorScheme: 'light' });
  await ctx.addInitScript(forceLight);
  const page = await ctx.newPage();

  // ── Pass 1: measure where the chart rows end so the frame cuts cleanly ──
  await page.setViewportSize({ width: CONTENT_W, height: 1200 });
  await page.goto(url, { waitUntil: 'networkidle', timeout: 45000 });
  await page.evaluate(() => {
    const h = document.documentElement;
    h.setAttribute('data-bs-theme', 'light');
    h.setAttribute('data-theme', 'light');
    h.classList.remove('dark', 'dark-mode', 'theme-dark');
  });
  await page.waitForTimeout(1800);

  // Cut just into the "Recent Orders" row so a sliver of the table peeks through.
  const iframeH = RATIO
    // A fixed aspect ratio, for a promo slot whose crop is already decided.
    ? Math.round((CONTENT_W + STAGE_PAD * 2) / RATIO - STAGE_PAD * 2 - BAR_H)
    : await page.evaluate(() => {
        const heads = [...document.querySelectorAll('h1,h2,h3,h4,h5,h6')];
        const ro = heads.find((el) => /recent orders/i.test(el.textContent || ''));
        const card = ro ? (ro.closest('.card') || ro) : null;
        if (card) return Math.round(card.getBoundingClientRect().top + window.scrollY + 96);
        return 1010; // fallback: pleasant landscape
      });
  console.log(`→ iframe height ${iframeH}px, scale ${SCALE}x`);

  // ── Pass 2: render the browser-frame mockup and screenshot it ──
  const stageH = STAGE_PAD * 2 + BAR_H + iframeH;
  await page.setViewportSize({ width: CONTENT_W + STAGE_PAD * 2, height: stageH });
  await page.setContent(frameHTML(url, iframeH), { waitUntil: 'networkidle', timeout: 45000 });
  await page.waitForTimeout(2200); // let the iframe's charts/animations settle

  // The dashboard scrolls at the window level (fixed header/sidebar); something
  // nudges it down on load, so force the frame back to the top and pin light theme.
  const frame = await (await page.$('iframe')).contentFrame();
  await frame.evaluate(() => {
    const h = document.documentElement;
    h.setAttribute('data-bs-theme', 'light');
    h.setAttribute('data-theme', 'light');
    h.classList.remove('dark', 'dark-mode', 'theme-dark');
    h.style.scrollBehavior = 'auto';
    window.scrollTo(0, 0);
  });
  await page.waitForTimeout(400);
  await frame.evaluate(() => window.scrollTo(0, 0));
  await page.waitForTimeout(150);

  await page.screenshot({ path: OUT, fullPage: false,
    clip: { x: 0, y: 0, width: CONTENT_W + STAGE_PAD * 2, height: stageH } });
  await browser.close();
  close();

  console.log(`✓ ${path.relative(ROOT, OUT)}  (${(CONTENT_W + STAGE_PAD * 2) * SCALE}×${stageH * SCALE})`);
}

main().catch((e) => { console.error(e); process.exit(1); });
