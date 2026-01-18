/**
 * Empty State Handler - Common utility for all pages
 * Handles all empty state scenarios: no data, errors, network issues, etc.
 */

class EmptyStateHandler {
    constructor() {
        this.states = {
            noData: 'no-data',
            noInternet: 'no-internet',
            serverError: 'server-error',
            serviceUnavailable: 'service-unavailable',
            maintenance: 'maintenance',
            notFound: 'not-found',
            accessDenied: 'access-denied',
            comingSoon: 'coming-soon',
            sessionExpired: 'session-expired'
        };
    }

    /**
     * Show empty state in a table body
     * @param {string} tbodyId - ID of the tbody element
     * @param {string} stateType - Type of empty state (noData, serverError, etc.)
     * @param {object} options - Custom options (title, message, buttons, etc.)
     * @param {number} colspan - Number of columns to span (default: 6)
     */
    showInTable(tbodyId, stateType, options = {}, colspan = 6) {
        const tbody = document.getElementById(tbodyId);
        if (!tbody) {
            console.error(`Table body with ID "${tbodyId}" not found`);
            return;
        }

        const stateHtml = this.getStateHtml(stateType, options, true);
        tbody.innerHTML = `
            <tr>
                <td colspan="${colspan}" class="p-0 text-center" style="text-align: center !important;">
                    ${stateHtml}
                </td>
            </tr>
        `;
    }

    /**
     * Show empty state in a container
     * @param {string} containerId - ID of the container element
     * @param {string} stateType - Type of empty state
     * @param {object} options - Custom options
     */
    showInContainer(containerId, stateType, options = {}) {
        const container = document.getElementById(containerId);
        if (!container) {
            console.error(`Container with ID "${containerId}" not found`);
            return;
        }

        container.innerHTML = this.getStateHtml(stateType, options, false);
    }

    /**
     * Get HTML for a specific state
     * @param {string} stateType - Type of empty state
     * @param {object} options - Custom options
     * @param {boolean} compact - Use compact version for tables
     */
    getStateHtml(stateType, options = {}, compact = true) {
        const config = this.getStateConfig(stateType, options);
        const compactClass = compact ? 'empty-state-compact' : '';
        const errorClass = stateType.includes('error') || stateType.includes('unavailable') || stateType === 'access-denied' ? 'empty-state-error' : '';

        return `
            <div class="empty-state ${compactClass}">
                ${this.getIconHtml(stateType)}
                ${config.badge ? `<span class="status-badge ${config.badgeClass}">${config.badge}</span>` : ''}
                <h2 class="empty-title">${config.title}</h2>
                <p class="empty-message">${config.message}</p>
                ${this.getButtonsHtml(config.buttons)}
            </div>
        `;
    }

    /**
     * Get icon HTML based on state type
     */
    getIconHtml(stateType) {
        const icons = {
            'no-data': `
                <div class="empty-icon no-data-icon">
                    <div class="search-circle">
                        <div class="search-handle"></div>
                        <div class="x-mark">✕</div>
                    </div>
                </div>
            `,
            'no-internet': `
                <div class="empty-icon no-internet-icon">
                    <div class="wifi-symbol">
                        <div class="wifi-arc wifi-arc-1"></div>
                        <div class="wifi-arc wifi-arc-2"></div>
                        <div class="wifi-arc wifi-arc-3"></div>
                        <div class="wifi-slash"></div>
                    </div>
                </div>
            `,
            'server-error': `
                <div class="empty-icon server-error-icon">
                    <div class="server-box">
                        <div class="server-line"></div>
                        <div class="server-line"></div>
                        <div class="server-line"></div>
                        <div class="server-x">✕</div>
                    </div>
                </div>
            `,
            'service-unavailable': `
                <div class="empty-icon server-error-icon">
                    <div class="server-box">
                        <div class="server-line"></div>
                        <div class="server-line"></div>
                        <div class="server-line"></div>
                        <div class="server-x">!</div>
                    </div>
                </div>
            `,
            'maintenance': `
                <div class="empty-icon maintenance-icon">
                    <div class="gear"></div>
                    <div class="gear-center"></div>
                    <i class="bi bi-wrench wrench"></i>
                </div>
            `,
            'not-found': `
                <div class="empty-icon">
                    <div class="not-found-icon">404</div>
                </div>
            `,
            'access-denied': `
                <div class="empty-icon permission-icon">
                    <div class="shield">
                        <div class="shield-x">🔒</div>
                    </div>
                </div>
            `,
            'coming-soon': `
                <div class="empty-icon coming-soon-icon">
                    <div class="rocket">🚀</div>
                </div>
            `,
            'session-expired': `
                <div class="empty-icon session-icon">
                    <div class="clock-circle">
                        <div class="clock-hand-hour"></div>
                        <div class="clock-hand-minute"></div>
                    </div>
                </div>
            `
        };

        return icons[stateType] || icons['no-data'];
    }

    /**
     * Get state configuration
     */
    getStateConfig(stateType, options = {}) {
        const defaults = {
            'no-data': {
                title: options.title || 'No result found',
                message: options.message || 'We can\'t find any item matching your search.<br>Try adjusting your filters or search terms.',
                badge: null,
                buttons: options.buttons || [
                    { text: 'Reset filter', class: 'btn-secondary', onclick: options.onReset || 'resetFilters()', icon: 'ti-arrow-clockwise' }
                ]
            },
            'no-internet': {
                title: options.title || 'No internet connection',
                message: options.message || 'Please check your internet connection and try again.<br>Make sure you\'re connected to a network.',
                badge: 'OFFLINE',
                badgeClass: 'badge-error',
                buttons: options.buttons || [
                    { text: 'Try again', class: 'btn-primary', onclick: 'location.reload()', icon: 'ti-arrow-clockwise' }
                ]
            },
            'server-error': {
                title: options.title || 'Something went wrong',
                message: options.message || 'We\'re having trouble processing your request.<br>Our team has been notified and is working on it.',
                badge: options.badge || 'ERROR 500',
                badgeClass: 'badge-error',
                buttons: options.buttons || [
                    { text: 'Refresh page', class: 'btn-primary', onclick: options.onRetry || 'location.reload()', icon: 'ti-arrow-clockwise' }
                ]
            },
            'service-unavailable': {
                title: options.title || 'Service unavailable',
                message: options.message || 'The service is temporarily unavailable.<br>Please try again in a few moments.',
                badge: 'ERROR 503',
                badgeClass: 'badge-error',
                buttons: options.buttons || [
                    { text: 'Try again', class: 'btn-secondary', onclick: options.onRetry || 'location.reload()', icon: 'ti-arrow-clockwise' }
                ]
            },
            'maintenance': {
                title: options.title || 'We\'ll be back soon',
                message: options.message || 'We\'re performing scheduled maintenance to improve our services.<br>We\'ll be back online shortly.',
                badge: 'MAINTENANCE',
                badgeClass: 'badge-warning',
                buttons: options.buttons || [
                    { text: 'Notify me', class: 'btn-warning', onclick: options.onNotify || 'void(0)', icon: 'ti-bell' }
                ]
            },
            'not-found': {
                title: options.title || 'Page not found',
                message: options.message || 'The page you\'re looking for doesn\'t exist or has been moved.<br>Let\'s get you back on track.',
                badge: null,
                buttons: options.buttons || [
                    { text: 'Go back', class: 'btn-primary', onclick: 'window.history.back()', icon: 'ti-arrow-left' }
                ]
            },
            'access-denied': {
                title: options.title || 'Access denied',
                message: options.message || 'You don\'t have permission to access this resource.<br>Contact your administrator if you need access.',
                badge: 'ERROR 403',
                badgeClass: 'badge-error',
                buttons: options.buttons || [
                    { text: 'Go back', class: 'btn-danger', onclick: 'window.history.back()', icon: 'ti-arrow-left' }
                ]
            },
            'coming-soon': {
                title: options.title || 'Exciting things ahead',
                message: options.message || 'We\'re working hard to bring you something amazing.<br>Stay tuned for updates!',
                badge: 'COMING SOON',
                badgeClass: 'badge-info',
                buttons: options.buttons || [
                    { text: 'Notify me when ready', class: 'btn-primary', onclick: options.onNotify || 'void(0)', icon: 'ti-bell' }
                ]
            },
            'session-expired': {
                title: options.title || 'Your session has expired',
                message: options.message || 'For your security, we\'ve logged you out after inactivity.<br>Please sign in again to continue.',
                badge: 'SESSION EXPIRED',
                badgeClass: 'badge-warning',
                buttons: options.buttons || [
                    { text: 'Sign in again', class: 'btn-primary', onclick: options.onLogin || 'window.location.href="/Login"', icon: 'ti-box-arrow-in-right' }
                ]
            }
        };

        return defaults[stateType] || defaults['no-data'];
    }

    /**
     * Get buttons HTML
     */
    getButtonsHtml(buttons) {
        if (!buttons || buttons.length === 0) {
            return '';
        }

        const buttonsHtml = buttons.map(btn => {
            let icon = '';
            if (btn.icon) {
                // Map Tabler icons to Bootstrap icons for consistency
                const iconMap = {
                    'ti-arrow-clockwise': 'bi bi-arrow-clockwise',
                    'ti-x': 'bi bi-x-circle',
                    'ti-refresh': 'bi bi-arrow-clockwise',
                    'ti-arrow-left': 'bi bi-arrow-left',
                    'ti-bell': 'bi bi-bell',
                    'ti-box-arrow-in-right': 'bi bi-box-arrow-in-right'
                };
                const iconClass = iconMap[btn.icon] || `ti ${btn.icon}`;
                icon = `<i class="${iconClass}"></i>`;
            }
            return `<button class="btn ${btn.class}" onclick="${btn.onclick}">${icon} ${btn.text}</button>`;
        }).join('');

        return `<div class="action-buttons">${buttonsHtml}</div>`;
    }

    /**
     * Detect error type from error object
     */
    detectErrorType(error) {
        if (!error) return 'server-error';

        // Network errors
        if (error.message && (
            error.message.includes('Failed to fetch') ||
            error.message.includes('NetworkError') ||
            error.message.includes('Network request failed')
        )) {
            return 'no-internet';
        }

        // HTTP status codes
        if (error.status) {
            switch (error.status) {
                case 403:
                    return 'access-denied';
                case 404:
                    return 'not-found';
                case 503:
                    return 'service-unavailable';
                case 500:
                case 502:
                case 504:
                    return 'server-error';
            }
        }

        // Default to server error
        return 'server-error';
    }

    /**
     * Show error state (auto-detects error type)
     */
    showError(tbodyId, error, options = {}, colspan = 6) {
        const errorType = this.detectErrorType(error);
        this.showInTable(tbodyId, errorType, options, colspan);
    }

    /**
     * Show no data state
     */
    showNoData(tbodyId, options = {}, colspan = 6) {
        this.showInTable(tbodyId, 'no-data', options, colspan);
    }
}

// Create global instance
const emptyState = new EmptyStateHandler();

// Global helper functions for backward compatibility
function showErrorState(error, tbodyId = 'productTableBody', options = {}, colspan = 6) {
    emptyState.showError(tbodyId, error, options, colspan);
}

function showNoDataState(tbodyId = 'productTableBody', options = {}, colspan = 6) {
    emptyState.showNoData(tbodyId, options, colspan);
}

function clearSearch() {
    const searchInput = document.getElementById('searchText');
    if (searchInput) {
        searchInput.value = '';
    }
    // Reload products if function exists
    if (typeof loadProducts === 'function') {
        loadProducts(1);
    }
}
