// scripts/generate-icon-subset.mjs
//
// Regenerates src-modern/styles/scss/components/_bootstrap-icons-subset.scss.
//
// Bootstrap Icons ships ~2,000 glyphs; this template uses a fraction of them, so
// the build emits CSS for only the ones actually referenced. The catch is that
// the subset is a *generated* file — add `bi-kanban` to a page without
// regenerating and the class exists, the markup looks right, and the icon
// silently renders as nothing.
//
// This scans the source for `bi-*` references and rewrites the partial. Run it
// whenever you add an icon:
//
//   npm run icons
//
// Exits 1 if a referenced icon is not in Bootstrap Icons at all (a typo), so it
// is safe to run in CI as a check.

import { readFile, writeFile, readdir } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const ROOT = path.resolve(__dirname, '..');
const SRC = path.join(ROOT, 'src-modern');
const OUT = path.join(SRC, 'styles/scss/components/_bootstrap-icons-subset.scss');
const MAP = path.join(ROOT, 'node_modules/bootstrap-icons/font/bootstrap-icons.json');

// `bi-` prefixed tokens in markup, class strings and Alpine bindings.
const ICON_RE = /\bbi-([a-z0-9]+(?:-[a-z0-9]+)*)\b/g;

// Tokens that match the pattern but are not icons.
const NOT_ICONS = new Set(['bi-font-path']);

async function* walk(dir) {
  for (const entry of await readdir(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      yield* walk(full);
    } else if (/\.(html|js)$/.test(entry.name)) {
      yield full;
    }
  }
}

const icons = new Set();
for await (const file of walk(SRC)) {
  const text = await readFile(file, 'utf8');
  for (const match of text.matchAll(ICON_RE)) {
    if (!NOT_ICONS.has(`bi-${match[1]}`)) icons.add(match[1]);
  }
}

const codepoints = JSON.parse(await readFile(MAP, 'utf8'));
const missing = [...icons].filter((name) => !(name in codepoints)).sort();
const found = [...icons].filter((name) => name in codepoints).sort();

const header = `// ==========================================================================
// Bootstrap Icons subset — only the icons actually referenced in this project.
//
// GENERATED FILE — do not edit by hand. Run \`npm run icons\` after adding an
// icon to any page, or the class will exist while the glyph renders as nothing.
// ==========================================================================

$bi-font-path: '~bootstrap-icons/font/fonts' !default;

@font-face {
  font-display: block;
  font-family: 'bootstrap-icons';
  src: url('#{$bi-font-path}/bootstrap-icons.woff2') format('woff2'),
       url('#{$bi-font-path}/bootstrap-icons.woff') format('woff');
}

.bi::before,
[class^='bi-']::before,
[class*=' bi-']::before {
  display: inline-block;
  font-family: bootstrap-icons !important;
  font-style: normal;
  font-weight: normal !important;
  font-variant: normal;
  text-transform: none;
  line-height: 1;
  vertical-align: -0.125em;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}
`;

const rules = found
  .map((name) => `.bi-${name}::before { content: "\\${codepoints[name].toString(16)}"; }`)
  .join('\n');

await writeFile(OUT, `${header}\n${rules}\n`);

console.log(`✓ ${found.length} icons written to ${path.relative(ROOT, OUT)}`);
if (missing.length) {
  console.error(`✗ ${missing.length} referenced icon(s) do not exist in Bootstrap Icons:`);
  missing.forEach((name) => console.error(`    bi-${name}`));
  process.exit(1);
}
