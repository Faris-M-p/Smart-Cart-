// ==========================================================================
// Authentication components
// ==========================================================================
//
// Alpine scopes for login, register, forgot-password, reset-password,
// two-factor and lock-screen. Each is registered under the exact name its
// markup calls in `x-data`.
//
// These are demo flows: nothing is sent anywhere and no credential is checked.
// `submit()` validates, shows a pending state for a beat, then routes on. Every
// timer is tracked and cleared in `destroy()` so a fast navigation cannot leave
// a callback running against a torn-down scope.
//
// ==========================================================================

import Alpine from 'alpinejs';
import { scorePassword } from '../utils/password-strength.js';

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

/** Pending-state duration for the simulated round trip. */
const SUBMIT_DELAY_MS = 700;

/** Seconds before a two-factor code can be resent. */
const RESEND_COOLDOWN_S = 30;

/**
 * Merge mixins while preserving property descriptors.
 *
 * Object spread (`{...mixin()}`) copies the *result* of a getter, not the getter
 * itself. `passwordField()` exposes `strength` as a getter over `this.form`, and
 * spreading it evaluated that getter once — before `form` existed — so every
 * password meter froze at "no password entered" no matter what was typed.
 */
function compose(...sources) {
  const target = {};
  for (const source of sources) {
    Object.defineProperties(target, Object.getOwnPropertyDescriptors(source));
  }
  return target;
}

/**
 * Timer bookkeeping shared by every scope below. Alpine calls `destroy()` when
 * the component is removed; the page entry also calls it on `pagehide`.
 */
const timers = () => ({
  _timers: [],
  later(fn, ms) {
    const id = setTimeout(() => {
      this._timers = this._timers.filter((t) => t !== id);
      fn();
    }, ms);
    this._timers.push(id);
    return id;
  },
  every(fn, ms) {
    const id = setInterval(fn, ms);
    this._timers.push(id);
    return id;
  },
  destroy() {
    this._timers.forEach((id) => {
      clearTimeout(id);
      clearInterval(id);
    });
    this._timers = [];
  },
});

/** Shared password field behaviour: reveal toggle + strength meter. */
const passwordField = () => ({
  showPassword: false,
  togglePassword() {
    this.showPassword = !this.showPassword;
  },
  get strength() {
    return scorePassword(this.form?.password ?? '');
  },
  /** Four meter segments; each is filled once the score reaches its threshold. */
  segmentClass(index) {
    const filled = Math.ceil(this.strength.score / 1.5);
    if (index >= filled || this.strength.score === 0) return '';
    return `bg-${this.strength.color}`;
  },
});

document.addEventListener('alpine:init', () => {
  // ── Login ───────────────────────────────────────────────────────────────
  Alpine.data('loginForm', () =>
    compose(timers(), passwordField(), {
      form: { email: '', password: '', remember: true },
      errors: {},
      submitting: false,
      failed: false,

      validate() {
        const errors = {};
        if (!this.form.email) errors.email = 'Enter your email address.';
        else if (!EMAIL_RE.test(this.form.email))
          errors.email = 'That does not look like an email address.';
        if (!this.form.password) errors.password = 'Enter your password.';
        this.errors = errors;
        return Object.keys(errors).length === 0;
      },

      fieldClass(field) {
        return this.errors[field] ? 'is-invalid' : '';
      },

      submit() {
        this.failed = false;
        if (!this.validate()) return;
        this.submitting = true;
        this.later(() => {
          this.submitting = false;
          // Demo template: there is no backend, so this always succeeds and
          // lands on the dashboard.
          window.location.href = './index.html';
        }, SUBMIT_DELAY_MS);
      },
    })
  );

  // ── Register ────────────────────────────────────────────────────────────
  Alpine.data('registerForm', () =>
    compose(timers(), passwordField(), {
      form: { name: '', email: '', password: '', terms: false },
      errors: {},
      submitting: false,

      validate() {
        const errors = {};
        if (!this.form.name.trim()) errors.name = 'Enter your full name.';
        if (!this.form.email) errors.email = 'Enter your email address.';
        else if (!EMAIL_RE.test(this.form.email))
          errors.email = 'That does not look like an email address.';
        if (!this.form.password) errors.password = 'Choose a password.';
        else if (this.form.password.length < 8) errors.password = 'Use at least 8 characters.';
        if (!this.form.terms) errors.terms = 'You need to accept the terms to continue.';
        this.errors = errors;
        return Object.keys(errors).length === 0;
      },

      fieldClass(field) {
        return this.errors[field] ? 'is-invalid' : '';
      },

      submit() {
        if (!this.validate()) return;
        this.submitting = true;
        this.later(() => {
          this.submitting = false;
          window.location.href = './two-factor.html';
        }, SUBMIT_DELAY_MS);
      },
    })
  );

  // ── Forgot password ─────────────────────────────────────────────────────
  Alpine.data('forgotPasswordForm', () =>
    compose(timers(), {
      form: { email: '' },
      errors: {},
      submitting: false,
      sent: false,

      validate() {
        const errors = {};
        if (!this.form.email) errors.email = 'Enter your email address.';
        else if (!EMAIL_RE.test(this.form.email))
          errors.email = 'That does not look like an email address.';
        this.errors = errors;
        return Object.keys(errors).length === 0;
      },

      fieldClass(field) {
        return this.errors[field] ? 'is-invalid' : '';
      },

      submit() {
        if (!this.validate()) return;
        this.submitting = true;
        this.later(() => {
          this.submitting = false;
          // Confirm without disclosing whether the address is registered —
          // otherwise this form is an account-enumeration oracle.
          this.sent = true;
        }, SUBMIT_DELAY_MS);
      },
    })
  );

  // ── Reset password ──────────────────────────────────────────────────────
  Alpine.data('resetPasswordForm', () =>
    compose(timers(), passwordField(), {
      form: { password: '', confirm: '' },
      errors: {},
      submitting: false,
      done: false,

      validate() {
        const errors = {};
        if (!this.form.password) errors.password = 'Choose a new password.';
        else if (this.form.password.length < 8) errors.password = 'Use at least 8 characters.';
        if (this.form.confirm !== this.form.password)
          errors.confirm = 'The two passwords do not match.';
        this.errors = errors;
        return Object.keys(errors).length === 0;
      },

      fieldClass(field) {
        return this.errors[field] ? 'is-invalid' : '';
      },

      submit() {
        if (!this.validate()) return;
        this.submitting = true;
        this.later(() => {
          this.submitting = false;
          this.done = true;
        }, SUBMIT_DELAY_MS);
      },
    })
  );

  // ── Two-factor ──────────────────────────────────────────────────────────
  Alpine.data('twoFactorForm', () =>
    compose(timers(), {
      digits: ['', '', '', '', '', ''],
      error: '',
      submitting: false,
      cooldown: RESEND_COOLDOWN_S,

      init() {
        this.startCooldown();
      },

      startCooldown() {
        this.cooldown = RESEND_COOLDOWN_S;
        const id = this.every(() => {
          this.cooldown -= 1;
          if (this.cooldown <= 0) clearInterval(id);
        }, 1000);
      },

      get code() {
        return this.digits.join('');
      },

      get complete() {
        return this.code.length === 6;
      },

      /** Keep one digit per box and advance focus. */
      onInput(index, event) {
        const value = event.target.value.replace(/\D/g, '');
        this.digits[index] = value.slice(-1);
        event.target.value = this.digits[index];
        this.error = '';
        if (this.digits[index] && index < 5) {
          this.focusBox(index + 1);
        }
      },

      /** Backspace on an empty box steps back to the previous one. */
      onKeydown(index, event) {
        if (event.key === 'Backspace' && !this.digits[index] && index > 0) {
          event.preventDefault();
          this.digits[index - 1] = '';
          this.focusBox(index - 1);
        }
      },

      /** Pasting a whole code fills every box rather than only the first. */
      onPaste(event) {
        event.preventDefault();
        const pasted = (event.clipboardData?.getData('text') ?? '').replace(/\D/g, '').slice(0, 6);
        if (!pasted) return;
        for (let i = 0; i < 6; i += 1) {
          this.digits[i] = pasted[i] ?? '';
        }
        this.focusBox(Math.min(pasted.length, 5));
      },

      focusBox(index) {
        const box = this.$refs[`digit${index}`];
        if (box) {
          box.focus();
          box.select();
        }
      },

      resend() {
        if (this.cooldown > 0) return;
        this.digits = ['', '', '', '', '', ''];
        this.error = '';
        this.startCooldown();
        this.focusBox(0);
      },

      submit() {
        if (!this.complete) {
          this.error = 'Enter all six digits.';
          return;
        }
        this.submitting = true;
        this.later(() => {
          this.submitting = false;
          window.location.href = './index.html';
        }, SUBMIT_DELAY_MS);
      },
    })
  );

  // ── Lock screen ─────────────────────────────────────────────────────────
  Alpine.data('lockScreenForm', () =>
    compose(timers(), passwordField(), {
      form: { password: '' },
      error: '',
      submitting: false,

      submit() {
        if (!this.form.password) {
          this.error = 'Enter your password to unlock.';
          return;
        }
        this.error = '';
        this.submitting = true;
        this.later(() => {
          this.submitting = false;
          window.location.href = './index.html';
        }, SUBMIT_DELAY_MS);
      },
    })
  );
});
