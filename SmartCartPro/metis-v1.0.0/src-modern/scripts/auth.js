// ==========================================================================
// Entry point for the standalone pages
// ==========================================================================
//
// Login, register, forgot/reset password, two-factor, lock screen and the three
// error pages render outside the admin shell — no sidebar, no header, no charts.
// They deliberately do not use `main.js`: that entry pulls in the sidebar
// manager, the notification manager, the dashboard manager and Bootstrap's
// Modal/Tab/Toast/Tooltip/Collapse, none of which exist on these pages.
//
// A sign-in page is also the one page in a template that an unauthenticated
// visitor always hits, so it is the one page where the payload matters most.
//
// ==========================================================================

import { ThemeManager } from './utils/theme-manager.js';
import Alpine from 'alpinejs';
import '@fontsource-variable/inter';
import '../styles/scss/main.scss';

// Registers the Alpine scopes on the `alpine:init` event, which fires inside
// Alpine.start() below. The error pages import this module too — they carry no
// x-data, so nothing is instantiated for them.
import './components/auth.js';

function start() {
  new ThemeManager();

  // A theme toggle is present on these pages, but there is no header component
  // to wire it up, so it is bound here.
  document.querySelectorAll('[data-theme-toggle]').forEach((button) => {
    button.addEventListener('click', () => {
      const next =
        document.documentElement.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
      document.documentElement.setAttribute('data-bs-theme', next);
      try {
        localStorage.setItem('theme', next);
      } catch {
        // Storage unavailable (private mode) — the toggle still works for this
        // page view, it just will not be remembered.
      }
    });
  });

  // Error-page actions. Delegated off data-* attributes rather than inline
  // onclick, so they stay lintable and CSP-safe like the rest of the template.
  document.addEventListener('click', (event) => {
    const back = event.target.closest('[data-go-back]');
    if (back) {
      event.preventDefault();
      // history.back() does nothing when the error page is the first entry in
      // the session (a pasted link, a bookmark) — fall back to the dashboard.
      if (window.history.length > 1) window.history.back();
      else window.location.href = './index.html';
      return;
    }

    const reload = event.target.closest('[data-reload]');
    if (reload) {
      event.preventDefault();
      window.location.reload();
    }
  });

  Alpine.start();
}

// This module is an entry, not a dynamic import, so it can run before
// DOMContentLoaded — but guard anyway to match the rest of the codebase.
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', start, { once: true });
} else {
  start();
}
