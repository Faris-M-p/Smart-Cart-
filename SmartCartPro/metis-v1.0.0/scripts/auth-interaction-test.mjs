// scripts/auth-interaction-test.mjs
//
// Drives the auth pages the way a user does. The smoke test only proves a page
// loads without errors — it cannot tell that a password meter is stuck or that
// focus fails to advance between code boxes, because neither logs anything.
//
// It earned its place immediately: it caught the password strength meter frozen
// at zero (object spread copies a getter's *value*, so `strength` was evaluated
// once before `form` existed) and the two-factor boxes not advancing focus.
//
// Usage:  npm run build && node scripts/auth-interaction-test.mjs
//
// Env:
//   PLAYWRIGHT_PATH=…   resolve playwright from elsewhere if not a local dep
//   HEADED=1            watch it run

import http from 'node:http';
import { readFile } from 'node:fs/promises';
import { existsSync, statSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createRequire } from 'node:module';

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PATH || 'playwright');

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const ROOT = path.resolve(__dirname, '..');
const SERVE_DIR = path.resolve(ROOT, 'dist-modern');

const MIME = {
  '.html': 'text/html; charset=utf-8',
  '.css': 'text/css',
  '.js': 'text/javascript',
  '.json': 'application/json',
  '.svg': 'image/svg+xml',
  '.png': 'image/png',
  '.jpg': 'image/jpeg',
  '.woff': 'font/woff',
  '.woff2': 'font/woff2',
};

function serve(dir) {
  const server = http.createServer(async (req, res) => {
    try {
      let urlPath = decodeURIComponent(req.url.split('?')[0]);
      let filePath = path.join(dir, urlPath);
      if (existsSync(filePath) && statSync(filePath).isDirectory()) {
        filePath = path.join(filePath, 'index.html');
      }
      if (!filePath.startsWith(dir) || !existsSync(filePath)) {
        res.writeHead(404).end('Not found');
        return;
      }
      const body = await readFile(filePath);
      res.writeHead(200, {
        'Content-Type': MIME[path.extname(filePath)] || 'application/octet-stream',
      });
      res.end(body);
    } catch (error) {
      res.writeHead(500).end(String(error));
    }
  });
  return new Promise((resolve) => {
    server.listen(0, '127.0.0.1', () => resolve({ server, port: server.address().port }));
  });
}

const results = [];
function check(name, ok, detail = '') {
  results.push({ name, ok, detail });
}

async function main() {
  if (!existsSync(SERVE_DIR)) {
    console.error('✗ dist-modern not found — run `npm run build` first.');
    process.exit(1);
  }

  const { server, port } = await serve(SERVE_DIR);
  const base = `http://127.0.0.1:${port}`;
  const browser = await chromium.launch({ headless: !process.env.HEADED });
  const ctx = await browser.newContext({ viewport: { width: 1280, height: 900 } });

  // ── Login ────────────────────────────────────────────────────────────────
  {
    const page = await ctx.newPage();
    await page.goto(`${base}/login.html`);
    await page.waitForTimeout(400);

    await page.click('button[type=submit]');
    await page.waitForTimeout(200);
    check('login blocks an empty submit', page.url().includes('login.html'));
    check(
      'login reports a missing email',
      /Enter your email/i.test(await page.textContent('#login-email ~ .invalid-feedback'))
    );

    await page.fill('#login-email', 'notanemail');
    await page.click('button[type=submit]');
    await page.waitForTimeout(200);
    check(
      'login rejects a malformed email',
      /email address/i.test(await page.textContent('#login-email ~ .invalid-feedback'))
    );

    await page.fill('#login-password', 'secret123');
    await page.click('.auth-password-toggle');
    await page.waitForTimeout(150);
    check(
      'password reveal toggles the input type',
      (await page.getAttribute('#login-password', 'type')) === 'text'
    );

    await page.fill('#login-email', 'a@b.com');
    await page.click('button[type=submit]');
    await page.waitForTimeout(1400);
    check('a valid login lands on the dashboard', page.url().endsWith('index.html'), page.url());
    await page.close();
  }

  // ── Two-factor ───────────────────────────────────────────────────────────
  {
    const page = await ctx.newPage();
    await page.goto(`${base}/two-factor.html`);
    await page.waitForTimeout(400);
    const boxes = page.locator('.auth-code input');

    check('two-factor renders six code boxes', (await boxes.count()) === 6);
    check(
      'verify is disabled while the code is incomplete',
      await page.isDisabled('button[type=submit]')
    );

    await boxes.nth(0).click();
    await page.keyboard.type('123');
    await page.waitForTimeout(200);
    const focused = await page.evaluate(() => document.activeElement?.getAttribute('aria-label'));
    check(
      'focus advances as digits are typed',
      focused === 'Digit 4',
      focused || 'nothing focused'
    );

    await page.evaluate(() => {
      const data = new DataTransfer();
      data.setData('text', '987654');
      document
        .querySelector('.auth-code input')
        .dispatchEvent(new ClipboardEvent('paste', { clipboardData: data, bubbles: true }));
    });
    await page.waitForTimeout(250);
    const pasted = await boxes.evaluateAll((els) => els.map((el) => el.value).join(''));
    check('pasting a whole code fills every box', pasted === '987654', pasted);
    check(
      'verify enables once the code is complete',
      !(await page.isDisabled('button[type=submit]'))
    );
    await page.close();
  }

  // ── Forgot password ──────────────────────────────────────────────────────
  {
    const page = await ctx.newPage();
    await page.goto(`${base}/forgot-password.html`);
    await page.waitForTimeout(400);
    await page.fill('#forgot-email', 'someone@example.com');
    await page.click('button[type=submit]');
    await page.waitForTimeout(1200);
    const body = await page.textContent('.auth-body');
    check('reset request confirms', /Check your inbox/.test(body));
    check(
      'reset request does not reveal whether the account exists',
      /If an account exists/.test(body)
    );
    await page.close();
  }

  // ── Register ─────────────────────────────────────────────────────────────
  {
    const page = await ctx.newPage();
    await page.goto(`${base}/register.html`);
    await page.waitForTimeout(400);
    const litSegments = () => page.locator('.auth-strength span[class]:not([class=""])').count();

    await page.fill('#register-password', 'abc');
    await page.waitForTimeout(200);
    const weak = await litSegments();
    await page.fill('#register-password', 'Str0ng!Passw0rd#2026');
    await page.waitForTimeout(200);
    const strong = await litSegments();
    check('the strength meter tracks the password', strong > weak, `weak=${weak} strong=${strong}`);

    await page.click('button[type=submit]');
    await page.waitForTimeout(200);
    check(
      'register blocks submit until the terms are accepted',
      page.url().includes('register.html')
    );
    await page.close();
  }

  await browser.close();
  server.close();

  const width = Math.max(...results.map((r) => r.name.length));
  for (const { name, ok, detail } of results) {
    console.log(`${ok ? '✓' : '✗'} ${name.padEnd(width)}${detail ? `  ${detail}` : ''}`);
  }

  const failed = results.filter((r) => !r.ok).length;
  if (failed) {
    console.error(`\n✗ ${failed} of ${results.length} interaction checks failed.`);
    process.exit(1);
  }
  console.log(`\n✓ All ${results.length} auth interaction checks passed.`);
}

main().catch((error) => {
  console.error(error);
  process.exit(1);
});
