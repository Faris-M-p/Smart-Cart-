// ==========================================================================
// Elements Component - Bootstrap 5 Elements Showcase
// ==========================================================================

import Alpine from 'alpinejs';
import { createSearchComponent } from '../utils/search-component.js';

// Elements data configuration
const elementsData = [
    {
        id: 'buttons',
        title: 'Buttons',
        category: 'components',
        icon: 'bi bi-square',
        description: 'Bootstrap button styles, sizes, and states',
        examples: 12,
        url: '/elements-buttons.html',
        preview: `
            <div class="d-flex gap-2 flex-wrap">
                <button class="btn btn-primary btn-sm">Primary</button>
                <button class="btn btn-outline-secondary btn-sm">Secondary</button>
                <button class="btn btn-success btn-sm">Success</button>
            </div>
        `
    },
    {
        id: 'alerts',
        title: 'Alerts',
        category: 'components',
        icon: 'bi bi-exclamation-triangle',
        description: 'Contextual feedback messages for user actions',
        examples: 8,
        url: '/elements-alerts.html',
        preview: `
            <div class="alert alert-primary alert-sm py-2 px-3 mb-2" role="alert">
                <i class="bi bi-info-circle me-2"></i>Primary alert
            </div>
            <div class="alert alert-success alert-sm py-2 px-3 mb-0" role="alert">
                <i class="bi bi-check-circle me-2"></i>Success alert
            </div>
        `
    },
    {
        id: 'badges',
        title: 'Badges',
        category: 'components',
        icon: 'bi bi-award',
        description: 'Small count and labeling components',
        examples: 6,
        url: '/elements-badges.html',
        preview: `
            <div class="d-flex gap-2 flex-wrap">
                <span class="badge bg-primary">Primary</span>
                <span class="badge bg-secondary">Secondary</span>
                <span class="badge bg-success">Success</span>
                <span class="badge bg-danger">Danger</span>
            </div>
        `
    },
    {
        id: 'cards',
        title: 'Cards',
        category: 'components',
        icon: 'bi bi-card-text',
        description: 'Flexible content containers with headers and footers',
        examples: 10,
        url: '/elements-cards.html',
        preview: `
            <div class="card" style="width: 200px;">
                <div class="card-body p-3">
                    <h6 class="card-title mb-2">Card Title</h6>
                    <p class="card-text mb-2 small">Sample card content with text.</p>
                    <button class="btn btn-primary btn-sm">Action</button>
                </div>
            </div>
        `
    },
    {
        id: 'modals',
        title: 'Modals',
        category: 'components',
        icon: 'bi bi-window',
        description: 'Streamlined modal dialogs with flexible content',
        examples: 9,
        url: '/elements-modals.html',
        preview: `
            <button class="btn btn-primary btn-sm" data-bs-toggle="modal" data-bs-target="#exampleModal">
                Launch Modal
            </button>
            <div class="modal fade" id="exampleModal" tabindex="-1" style="display: none;">
                <div class="modal-dialog modal-sm">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h6 class="modal-title">Modal Title</h6>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <p class="mb-0 small">Modal content goes here.</p>
                        </div>
                    </div>
                </div>
            </div>
        `
    },
    {
        id: 'forms',
        title: 'Form Controls',
        category: 'forms',
        icon: 'bi bi-ui-checks-grid',
        description: 'Form inputs, selects, checkboxes, and validation',
        examples: 15,
        url: '/elements-forms.html',
        preview: `
            <div class="mb-2">
                <input type="text" class="form-control form-control-sm" placeholder="Text input">
            </div>
            <div class="mb-2">
                <select class="form-select form-select-sm">
                    <option>Choose...</option>
                    <option>Option 1</option>
                </select>
            </div>
            <div class="form-check">
                <input class="form-check-input" type="checkbox" checked>
                <label class="form-check-label small">Check me</label>
            </div>
        `
    },
    {
        id: 'tables',
        title: 'Tables',
        category: 'content',
        icon: 'bi bi-table',
        description: 'Responsive tables with various styling options',
        examples: 8,
        url: '/elements-tables.html',
        preview: `
            <table class="table table-sm">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Status</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>John Doe</td>
                        <td><span class="badge bg-success">Active</span></td>
                    </tr>
                    <tr>
                        <td>Jane Smith</td>
                        <td><span class="badge bg-warning">Pending</span></td>
                    </tr>
                </tbody>
            </table>
        `
    }
];

// Register Alpine component
document.addEventListener('alpine:init', () => {
    Alpine.data('elementsComponent', () => ({
        // State
        components: elementsData,
        filteredComponents: elementsData,
        viewMode: 'grid',
        searchQuery: '',
        categoryFilter: '',

        // Initialize component
        init() {
            this.filteredComponents = this.components;
            console.log('🧩 Elements component initialized');
        },

        // Filter components based on search and category
        filterComponents() {
            let filtered = this.components;

            // Apply search filter
            if (this.searchQuery) {
                const query = this.searchQuery.toLowerCase();
                filtered = filtered.filter(component => 
                    component.title.toLowerCase().includes(query) ||
                    component.description.toLowerCase().includes(query) ||
                    component.category.toLowerCase().includes(query)
                );
            }

            // Apply category filter
            if (this.categoryFilter) {
                filtered = filtered.filter(component => 
                    component.category === this.categoryFilter
                );
            }

            this.filteredComponents = filtered;
        },

        // Toggle view mode
        toggleView() {
            this.viewMode = this.viewMode === 'grid' ? 'list' : 'grid';
            localStorage.setItem('elements-view-mode', this.viewMode);
        },

        // Navigate to component page
        navigateToComponent(component) {
            window.location.href = component.url;
        },

        // Show all components
        showAllComponents() {
            this.searchQuery = '';
            this.categoryFilter = '';
            this.filteredComponents = this.components;
        },

        // Clear all filters
        clearFilters() {
            this.searchQuery = '';
            this.categoryFilter = '';
            this.filterComponents();
        },

        // Get component count by category
        getComponentCount(category) {
            return this.components.filter(c => c.category === category).length;
        }
    }));

    // Enhanced search component for elements
    Alpine.data('searchComponent', createSearchComponent({
        getResults(query) {
            const q = query.toLowerCase();
            const elementResults = elementsData
                .filter((c) => c.title.toLowerCase().includes(q) || c.description.toLowerCase().includes(q))
                .map((c) => ({ title: c.title, url: c.url, type: 'element' }));
            const generalResults = [
                { title: 'Dashboard', url: '/', type: 'page' },
                { title: 'Analytics', url: '/analytics', type: 'page' },
                { title: 'Users', url: '/users', type: 'page' },
                { title: 'Elements', url: '/elements', type: 'page' },
            ].filter((item) => item.title.toLowerCase().includes(q));
            return [...elementResults, ...generalResults].slice(0, 8);
        },
    }));
});

// ==========================================================================
// Showcase page behaviour (copy-to-clipboard, live demos, highlighting)
// ==========================================================================
//
// These used to live in a per-page inline <script> on each elements-*.html.
// Six of those blocks had an unbalanced-brace SyntaxError, which meant the
// whole block was discarded by the parser and the copy buttons, the live alert
// demo and syntax highlighting were all dead on those pages. Consolidated here
// so there is one implementation, it is linted, and the pages carry no inline
// JavaScript (which lets the template run under a strict CSP).

const COPY_FEEDBACK_MS = 2000;

/** Briefly swap a button's contents for a "Copied!" confirmation. */
function flashCopied(button, label = 'Copied!') {
    if (button.dataset.copyBusy === 'true') return;
    button.dataset.copyBusy = 'true';

    const originalChildren = Array.from(button.childNodes);
    const icon = document.createElement('i');
    icon.className = 'bi bi-check me-2';
    button.replaceChildren(icon, document.createTextNode(label));
    button.classList.add('btn-success');

    setTimeout(() => {
        button.replaceChildren(...originalChildren);
        button.classList.remove('btn-success');
        delete button.dataset.copyBusy;
    }, COPY_FEEDBACK_MS);
}

/** Copy text, preferring the async clipboard API with a legacy fallback. */
async function copyText(text) {
    if (navigator.clipboard?.writeText) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch {
            // Permission denied or non-secure context — fall through.
        }
    }
    // Fallback for file:// previews and older browsers.
    const helper = document.createElement('textarea');
    helper.value = text;
    helper.setAttribute('readonly', '');
    helper.style.cssText = 'position:absolute;left:-9999px;top:0;';
    document.body.appendChild(helper);
    helper.select();
    let ok;
    try {
        ok = document.execCommand('copy');
    } catch {
        ok = false;
    }
    helper.remove();
    return ok;
}

function codeFor(button) {
    const block = button.closest('.element-example, .card, section') ?? button.parentElement;
    return block?.querySelector('.element-code-block pre code') ?? null;
}

function appendAlert(message, type) {
    const placeholder = document.getElementById('liveAlertPlaceholder');
    if (!placeholder) return;

    const alert = document.createElement('div');
    alert.className = `alert alert-${type} alert-dismissible`;
    alert.setAttribute('role', 'alert');

    const body = document.createElement('div');
    body.textContent = message; // textContent, not innerHTML — message is untrusted input
    alert.appendChild(body);

    const dismiss = document.createElement('button');
    dismiss.type = 'button';
    dismiss.className = 'btn-close';
    dismiss.setAttribute('data-bs-dismiss', 'alert');
    dismiss.setAttribute('aria-label', 'Close');
    alert.appendChild(dismiss);

    placeholder.appendChild(alert);
}

const ALERT_MESSAGES = {
    primary: 'This is a primary alert message!',
    success: 'Success! Your action was completed.',
    warning: 'Warning! Please check your input.',
    danger: 'Error! Something went wrong.',
};

/** One delegated listener for every showcase control on the page. */
function handleShowcaseClick(event) {
    const copyBtn = event.target.closest('[data-copy-code]');
    if (copyBtn) {
        const code = codeFor(copyBtn);
        if (code) copyText(code.textContent).then((ok) => ok && flashCopied(copyBtn));
        return;
    }

    const copyAllBtn = event.target.closest('[data-copy-all]');
    if (copyAllBtn) {
        const blocks = document.querySelectorAll('.element-code-block pre code');
        const all = Array.from(blocks)
            .map((b) => b.textContent)
            .join('\n\n');
        copyText(all).then((ok) => ok && flashCopied(copyAllBtn, 'All copied!'));
        return;
    }

    const alertBtn = event.target.closest('[data-show-alert]');
    if (alertBtn) {
        const type = alertBtn.dataset.showAlert;
        appendAlert(ALERT_MESSAGES[type] ?? 'Alert message', type);
        return;
    }

    const faqBtn = event.target.closest('[data-faq-toggle]');
    if (faqBtn) {
        const answer = faqBtn.nextElementSibling;
        const open = answer?.classList.toggle('show') ?? false;
        faqBtn.setAttribute('aria-expanded', String(open));
        return;
    }

    if (event.target.closest('[data-history-back]')) {
        window.history.back();
    }
}

/** Bootstrap's opt-in `.needs-validation` pattern, used on the forms page. */
function initFormValidation() {
    document.querySelectorAll('.needs-validation').forEach((form) => {
        form.addEventListener(
            'submit',
            (event) => {
                if (!form.checkValidity()) {
                    event.preventDefault();
                    event.stopPropagation();
                }
                form.classList.add('was-validated');
            },
            false
        );
    });
}

// main.js reaches this module through a dynamic `import()`, which usually
// resolves *after* DOMContentLoaded has already fired — a plain
// addEventListener('DOMContentLoaded', …) would then never run. Run now if the
// document is already parsed, otherwise wait for it.
function onReady(fn) {
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', fn, { once: true });
    } else {
        fn();
    }
}

onReady(() => {
    // Tooltips are initialized globally by main.js `initTooltipsAndPopovers()`.
    // This module used to re-initialize them via a `bootstrap.Tooltip` global
    // that the bundled build never defines — it only appeared to work because
    // the surrounding DOMContentLoaded handler never fired.

    document.addEventListener('click', handleShowcaseClick);
    initFormValidation();

    // Syntax highlighting — only pulled in when the page actually has code blocks,
    // so the showcase-only Prism bundle stays off every other page.
    if (document.querySelector('pre code[class*="language-"]')) {
        import('../utils/prism.js')
            .then(({ highlightAll }) => highlightAll())
            .catch((error) => console.error('Prism failed to load:', error));
    }
});

export { elementsData };