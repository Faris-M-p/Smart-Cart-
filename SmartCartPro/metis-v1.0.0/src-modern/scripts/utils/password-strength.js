// ==========================================================================
// Password strength scoring
// ==========================================================================
//
// Shared by the register and reset-password pages and by the forms showcase.
// It lived privately inside components/forms.js; the auth pages need the exact
// same scoring, and two copies of a rule like this drift the moment one of them
// is tuned.
//
// This is a composition check, not an entropy estimate — it will happily call
// "Passw0rd!" strong. Real deployments should score server-side against a
// breached-password list. It is here so the meter has honest, consistent
// behaviour across pages, not as a security control.
//
// ==========================================================================

const CHECKS = [
  (p) => p.length >= 8,
  (p) => p.length >= 12,
  (p) => /[a-z]/.test(p),
  (p) => /[A-Z]/.test(p),
  (p) => /\d/.test(p),
  (p) => /[^A-Za-z0-9]/.test(p),
];

/**
 * @param {string} password
 * @returns {{score:number, percentage:number, level:string, text:string, color:string}}
 *   `level` is a CSS class, `percentage` a bar width, `color` a Bootstrap
 *   contextual name. `score` is 0–6 and maps to the four-segment meter as
 *   `Math.ceil(score / 1.5)`.
 */
export function scorePassword(password) {
  if (!password) {
    return { score: 0, percentage: 0, level: 'weak', text: 'Too short', color: 'muted' };
  }

  const score = CHECKS.filter((check) => check(password)).length;
  const percentage = Math.round((score / CHECKS.length) * 100);

  if (score <= 2) return { score, percentage, level: 'weak', text: 'Weak', color: 'danger' };
  if (score <= 4) return { score, percentage, level: 'fair', text: 'Fair', color: 'warning' };
  if (score === 5) return { score, percentage, level: 'good', text: 'Good', color: 'info' };
  return { score, percentage, level: 'strong', text: 'Strong', color: 'success' };
}
