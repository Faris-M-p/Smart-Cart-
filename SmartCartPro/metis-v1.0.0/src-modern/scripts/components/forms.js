// ==========================================================================
// Forms Component - Validation, upload and wizard demos
// ==========================================================================
//
// forms.html declares four Alpine scopes — contactForm(), registrationForm(),
// fileUploadForm() and enhancedFormWizard(). None of them were registered:
// this module previously exported a single `formsComponent` that no markup
// referenced, so every expression on the page threw ("formData is not
// defined", "getFieldClass is not defined", …) and the whole page was inert.
// Each scope below is registered under the exact name the markup calls, with
// the property and method names its bindings expect.

import Alpine from 'alpinejs';
import { scorePassword } from '../utils/password-strength.js';

// ── Shared helpers ─────────────────────────────────────────────────────────

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const PHONE_RE = /^[+\d][\d\s()-]{6,}$/;

function notify(type, message) {
  window.AdminApp?.notificationManager?.[type]?.(message);
}

/** Bootstrap validation classes, driven by the component's `errors` map. */
function fieldClass(errors, touched, field) {
  if (errors[field]) return 'is-invalid';
  return touched[field] ? 'is-valid' : '';
}

/** Human-readable file size, used by the upload list. */
function formatBytes(bytes) {
  if (bytes === 0) return '0 B';
  const units = ['B', 'KB', 'MB', 'GB'];
  const i = Math.min(Math.floor(Math.log(bytes) / Math.log(1024)), units.length - 1);
  return `${(bytes / 1024 ** i).toFixed(i === 0 ? 0 : 1)} ${units[i]}`;
}

const LABELS = {
  firstName: 'First name',
  lastName: 'Last name',
  email: 'Email',
  phone: 'Phone',
  birthDate: 'Date of birth',
  address: 'Address',
  city: 'City',
  state: 'State',
  zipCode: 'ZIP code',
  username: 'Username',
  password: 'Password',
  confirmPassword: 'Confirmation',
  message: 'Message',
  subject: 'Subject',
};

const label = (field) => LABELS[field] ?? field;

/** Validate one field against a rule set; returns an error string or ''. */
function validate(field, value, rules = {}, all = {}) {
  const v = typeof value === 'string' ? value.trim() : value;

  if (rules.required && (v === '' || v === undefined || v === null || v === false)) {
    return rules.requiredMessage ?? `${label(field)} is required`;
  }
  if (v === '' || v === undefined || v === null) return '';
  if (rules.email && !EMAIL_RE.test(v)) return 'Enter a valid email address';
  if (rules.phone && !PHONE_RE.test(v)) return 'Enter a valid phone number';
  if (rules.minLength && String(v).length < rules.minLength) {
    return `${label(field)} must be at least ${rules.minLength} characters`;
  }
  if (rules.matches && v !== all[rules.matches]) return 'Passwords do not match';
  return '';
}

// ── Components ─────────────────────────────────────────────────────────────

document.addEventListener('alpine:init', () => {
  // Contact form — inline validation on input, simulated async submit.
  Alpine.data('contactForm', () => ({
    form: { firstName: '', lastName: '', email: '', subject: '', message: '' },
    errors: {},
    touched: {},
    isSubmitting: false,

    rules: {
      firstName: { required: true, minLength: 2 },
      lastName: { required: true, minLength: 2 },
      email: { required: true, email: true },
      message: { required: true, minLength: 10 },
    },

    validateField(field) {
      this.touched[field] = true;
      const error = validate(field, this.form[field], this.rules[field], this.form);
      if (error) this.errors[field] = error;
      else delete this.errors[field];
      return !error;
    },

    getFieldClass(field) {
      return fieldClass(this.errors, this.touched, field);
    },

    async submitForm() {
      const valid = Object.keys(this.rules).map((f) => this.validateField(f)).every(Boolean);
      if (!valid) {
        notify('warning', 'Please fix the highlighted fields.');
        return;
      }

      this.isSubmitting = true;
      await new Promise((resolve) => setTimeout(resolve, 1200)); // demo: stand-in for a POST
      this.isSubmitting = false;

      notify('success', 'Message sent — we will be in touch shortly.');
      this.form = { firstName: '', lastName: '', email: '', subject: '', message: '' };
      this.errors = {};
      this.touched = {};
    },
  }));

  // Registration form — adds a live password-strength meter.
  Alpine.data('registrationForm', () => ({
    form: { username: '', email: '', password: '', confirmPassword: '', agreeTerms: false },
    errors: {},
    touched: {},
    isSubmitting: false,
    showPassword: false,
    passwordStrength: scorePassword(''),

    rules: {
      username: { required: true, minLength: 3 },
      email: { required: true, email: true },
      password: { required: true, minLength: 8 },
      confirmPassword: { required: true, matches: 'password' },
    },

    validateField(field) {
      this.touched[field] = true;
      const error = validate(field, this.form[field], this.rules[field], this.form);
      if (error) this.errors[field] = error;
      else delete this.errors[field];
      return !error;
    },

    validatePassword() {
      this.passwordStrength = scorePassword(this.form.password);
      this.validateField('password');
      // Keep the confirmation in sync once the user has reached it.
      if (this.touched.confirmPassword) this.validateField('confirmPassword');
    },

    getFieldClass(field) {
      return fieldClass(this.errors, this.touched, field);
    },

    get isFormValid() {
      const filled = Object.keys(this.rules).every((f) => String(this.form[f] ?? '').trim() !== '');
      return filled && this.form.agreeTerms && Object.keys(this.errors).length === 0;
    },

    async submitForm() {
      const valid = Object.keys(this.rules).map((f) => this.validateField(f)).every(Boolean);
      if (!valid || !this.form.agreeTerms) {
        notify('warning', 'Please complete the form and accept the terms.');
        return;
      }

      this.isSubmitting = true;
      await new Promise((resolve) => setTimeout(resolve, 1400)); // demo: stand-in for a POST
      this.isSubmitting = false;

      notify('success', `Account created for ${this.form.username}.`);
      this.form = { username: '', email: '', password: '', confirmPassword: '', agreeTerms: false };
      this.errors = {};
      this.touched = {};
      this.passwordStrength = scorePassword('');
    },
  }));

  // Drag-and-drop upload with simulated per-file progress.
  Alpine.data('fileUploadForm', () => ({
    files: [],
    dragOver: false,
    nextId: 1,
    timers: [],
    maxBytes: 10 * 1024 * 1024,

    handleDrop(event) {
      this.dragOver = false;
      this.handleFiles(event.dataTransfer?.files);
    },

    handleFiles(fileList) {
      if (!fileList?.length) return;

      for (const file of Array.from(fileList)) {
        const entry = {
          id: this.nextId++,
          name: file.name,
          size: formatBytes(file.size),
          progress: 0,
          status: file.size > this.maxBytes ? 'error' : 'uploading',
        };
        this.files.push(entry);

        if (entry.status === 'error') {
          notify('error', `${file.name} exceeds the 10 MB limit.`);
          continue;
        }
        this.simulateUpload(entry.id);
      }
    },

    // Demo only — steps a progress bar instead of performing a real transfer.
    simulateUpload(id) {
      const timer = setInterval(() => {
        const file = this.files.find((f) => f.id === id);
        if (!file) {
          clearInterval(timer);
          this.timers = this.timers.filter((t) => t !== timer);
          return;
        }
        file.progress = Math.min(file.progress + Math.round(8 + Math.random() * 18), 100);
        if (file.progress >= 100) {
          file.status = 'completed';
          clearInterval(timer);
          this.timers = this.timers.filter((t) => t !== timer);
        }
      }, 220);
      this.timers.push(timer);
    },

    removeFile(id) {
      this.files = this.files.filter((f) => f.id !== id);
    },

    // Alpine calls destroy() when the scope is torn down; don't leak intervals.
    destroy() {
      this.timers.forEach(clearInterval);
      this.timers = [];
    },
  }));

  // Four-step wizard plus a success panel (step 5).
  Alpine.data('enhancedFormWizard', () => ({
    currentStep: 1,
    totalSteps: 4,
    isSubmitting: false,
    errors: {},
    touched: {},
    visitedSteps: [],
    passwordStrength: scorePassword(''),

    steps: [
      { id: 1, title: 'Personal', description: 'Your details' },
      { id: 2, title: 'Address', description: 'Where you live' },
      { id: 3, title: 'Account', description: 'Login and security' },
      { id: 4, title: 'Preferences', description: 'Notifications' },
    ],

    formData: {
      firstName: '', lastName: '', email: '', phone: '', birthDate: '', gender: '',
      address: '', city: '', state: '', zipCode: '', country: 'US',
      username: '', password: '', confirmPassword: '', securityQuestion: '', securityAnswer: '',
      emailNotifications: true, smsNotifications: false, marketingEmails: false,
      profilePublic: false, agreeToTerms: false,
    },

    // Which fields must pass before a step is considered complete.
    stepFields: {
      1: ['firstName', 'lastName', 'email', 'phone', 'birthDate'],
      2: ['address', 'city', 'state', 'zipCode'],
      3: ['username', 'password', 'confirmPassword'],
      4: ['agreeToTerms'],
    },

    rules: {
      firstName: { required: true, minLength: 2 },
      lastName: { required: true, minLength: 2 },
      email: { required: true, email: true },
      phone: { required: true, phone: true },
      birthDate: { required: true },
      address: { required: true },
      city: { required: true },
      state: { required: true },
      zipCode: { required: true, minLength: 4 },
      username: { required: true, minLength: 3 },
      password: { required: true, minLength: 8 },
      confirmPassword: { required: true, matches: 'password' },
      agreeToTerms: { required: true, requiredMessage: 'You must accept the terms to continue' },
    },

    validateField(field) {
      this.touched[field] = true;
      const error = validate(field, this.formData[field], this.rules[field], this.formData);
      if (error) this.errors[field] = error;
      else delete this.errors[field];
      return !error;
    },

    updatePasswordStrength() {
      this.passwordStrength = scorePassword(this.formData.password);
      this.validateField('password');
      if (this.touched.confirmPassword) this.validateField('confirmPassword');
    },

    getFieldClass(field) {
      return fieldClass(this.errors, this.touched, field);
    },

    /** True when every required field on `step` currently passes. */
    stepIsValid(step) {
      return (this.stepFields[step] ?? []).every(
        (f) => !validate(f, this.formData[f], this.rules[f], this.formData)
      );
    },

    // canProceed() must not mutate state — it runs inside a :disabled binding,
    // and writing to `errors` there would feed back into Alpine's reactivity.
    canProceed() {
      return this.stepIsValid(this.currentStep);
    },

    isStepCompleted(step) {
      return this.visitedSteps.includes(step) && this.stepIsValid(step);
    },

    hasStepError(step) {
      return this.visitedSteps.includes(step) && !this.stepIsValid(step);
    },

    markVisited(step) {
      if (!this.visitedSteps.includes(step)) this.visitedSteps.push(step);
    },

    nextStep() {
      const fields = this.stepFields[this.currentStep] ?? [];
      const valid = fields.map((f) => this.validateField(f)).every(Boolean);
      this.markVisited(this.currentStep);

      if (!valid) {
        notify('warning', 'Please fix the highlighted fields before continuing.');
        return;
      }

      if (this.currentStep === this.totalSteps) {
        this.handleSubmit();
        return;
      }
      this.currentStep += 1;
    },

    prevStep() {
      if (this.currentStep > 1) this.currentStep -= 1;
    },

    /** Allow jumping back freely, or forward only through already-valid steps. */
    goToStep(step) {
      if (step === this.currentStep) return;
      if (step < this.currentStep) {
        this.currentStep = step;
        return;
      }
      for (let s = this.currentStep; s < step; s += 1) {
        if (!this.stepIsValid(s)) {
          this.markVisited(s);
          notify('warning', `Complete step ${s} first.`);
          return;
        }
        this.markVisited(s);
      }
      this.currentStep = step;
    },

    async handleSubmit() {
      const fields = this.stepFields[this.currentStep] ?? [];
      if (!fields.map((f) => this.validateField(f)).every(Boolean)) return;

      this.isSubmitting = true;
      await new Promise((resolve) => setTimeout(resolve, 1500)); // demo: stand-in for a POST
      this.isSubmitting = false;

      this.markVisited(this.totalSteps);
      this.currentStep = 5; // success panel
      notify('success', 'Registration complete.');
    },

    saveDraft() {
      try {
        localStorage.setItem('wizard-draft', JSON.stringify(this.formData));
        notify('info', 'Draft saved.');
      } catch {
        notify('error', 'Could not save draft — storage unavailable.');
      }
    },

    resetWizard() {
      this.currentStep = 1;
      this.errors = {};
      this.touched = {};
      this.visitedSteps = [];
      this.passwordStrength = scorePassword('');
      this.formData = {
        firstName: '', lastName: '', email: '', phone: '', birthDate: '', gender: '',
        address: '', city: '', state: '', zipCode: '', country: 'US',
        username: '', password: '', confirmPassword: '', securityQuestion: '', securityAnswer: '',
        emailNotifications: true, smsNotifications: false, marketingEmails: false,
        profilePublic: false, agreeToTerms: false,
      };
      try {
        localStorage.removeItem('wizard-draft');
      } catch {
        // Storage unavailable — nothing to clear.
      }
    },

    init() {
      // Restore a previously saved draft, ignoring anything malformed.
      try {
        const saved = localStorage.getItem('wizard-draft');
        if (saved) Object.assign(this.formData, JSON.parse(saved));
      } catch {
        try {
          localStorage.removeItem('wizard-draft');
        } catch {
          // Storage unavailable — nothing to clear.
        }
      }
    },
  }));
});
