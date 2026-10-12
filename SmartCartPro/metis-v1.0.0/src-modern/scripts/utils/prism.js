// ==========================================================================
// Prism — bundled syntax highlighting for the element showcase pages
// ==========================================================================
//
// Previously loaded from cdnjs (prism-core + the autoloader plugin), which
// meant a blocking third-party request on every element page and a runtime
// dependency on a host we don't control. The showcase only ever highlights
// `language-html` blocks, and Prism's default build already ships
// markup/html/css/clike/javascript — so the autoloader was never needed.
//
// Prism's default export reads `document.currentScript` at load time to decide
// whether to auto-highlight. Under a bundler that is null, so highlighting is
// always manual — call `highlightAll()` explicitly.
//
import Prism from 'prismjs';
import 'prismjs/themes/prism-tomorrow.min.css';

/** Highlight every `<pre><code class="language-*">` block currently in the DOM. */
export function highlightAll(root = document) {
  Prism.highlightAllUnder(root);
}

export default Prism;
