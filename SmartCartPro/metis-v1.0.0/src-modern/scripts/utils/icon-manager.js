// ==========================================================================
// Icon Manager — semantic names on top of the Bootstrap Icons subset
// ==========================================================================
//
// Maps stable, intent-revealing names ('delete', 'success') to the Bootstrap
// Icons classes that render them, so call sites don't hard-code glyph names.
//
// This used to carry a second, Lucide-backed provider that could be swapped in
// at runtime. It was removed: its `create()` returned an SVG containing an
// empty `<path>` (it was never finished — the source said "simplified for
// demo"), so switching providers rendered blank icons, while the dynamic
// `import('lucide')` of the whole barrel added a 424 kB chunk to the build.
// Bootstrap Icons is the template's icon system; see
// styles/scss/components/_bootstrap-icons-subset.scss for the generated subset.
//
// Adding an icon here also requires it to exist in that subset, otherwise the
// class resolves but no glyph is drawn.

const ICONS = new Map([
  ['dashboard', 'bi-speedometer2'],
  ['users', 'bi-people'],
  ['analytics', 'bi-graph-up'],
  ['settings', 'bi-gear'],
  ['notifications', 'bi-bell'],
  ['search', 'bi-search'],
  ['menu', 'bi-list'],
  ['close', 'bi-x'],
  ['check', 'bi-check'],
  ['warning', 'bi-exclamation-triangle'],
  ['info', 'bi-info-circle'],
  ['success', 'bi-check-circle'],
  ['error', 'bi-x-circle'],
  ['arrow-up', 'bi-arrow-up'],
  ['arrow-down', 'bi-arrow-down'],
  ['plus', 'bi-plus'],
  ['edit', 'bi-pencil'],
  ['delete', 'bi-trash'],
  ['download', 'bi-download'],
  ['upload', 'bi-upload'],
  ['home', 'bi-house'],
  ['calendar', 'bi-calendar'],
  ['clock', 'bi-clock'],
  ['mail', 'bi-envelope'],
  ['phone', 'bi-telephone'],
  ['location', 'bi-geo-alt'],
  ['heart', 'bi-heart'],
  ['star', 'bi-star'],
  ['bookmark', 'bi-bookmark'],
  ['share', 'bi-share'],
  ['copy', 'bi-clipboard'],
  ['link', 'bi-link'],
  ['external', 'bi-box-arrow-up-right'],
  ['refresh', 'bi-arrow-clockwise'],
  ['filter', 'bi-funnel'],
  ['sort', 'bi-sort-down'],
  ['grid', 'bi-grid'],
  ['list', 'bi-list-ul'],
  ['image', 'bi-image'],
  ['file', 'bi-file-text'],
  ['folder', 'bi-folder'],
  ['eye', 'bi-eye'],
  ['eye-slash', 'bi-eye-slash'],
  ['lock', 'bi-lock'],
  ['unlock', 'bi-unlock'],
  ['user', 'bi-person'],
  ['team', 'bi-people'],
  ['crown', 'bi-award'],
  ['shield', 'bi-shield-check'],
]);

const FALLBACK_ICON = 'bi-question-circle';

export class IconManager {
  /** Resolve a semantic name to its Bootstrap Icons class. */
  get(iconName, fallback = FALLBACK_ICON) {
    return ICONS.get(iconName) ?? fallback;
  }

  /** True when `iconName` has a mapping — useful before rendering user input. */
  has(iconName) {
    return ICONS.has(iconName);
  }

  /** Build an `<i class="bi bi-…">` element for a semantic name. */
  create(iconName, options = {}) {
    const { className = '', ...attributes } = options;
    const element = document.createElement('i');
    element.className = `bi ${this.get(iconName)} ${className}`.trim();

    for (const [key, value] of Object.entries(attributes)) {
      element.setAttribute(key, value);
    }

    // Icons here are decorative; text labels carry the meaning.
    if (!element.hasAttribute('aria-label')) {
      element.setAttribute('aria-hidden', 'true');
    }

    return element;
  }

  /** Swap the glyph on an existing icon element, preserving other classes. */
  replaceIcon(element, iconName) {
    element.className = element.className.replace(/\bbi-[\w-]+/g, '').trim();
    element.classList.add('bi', this.get(iconName));
  }
}

export const iconManager = new IconManager();
export default iconManager;
