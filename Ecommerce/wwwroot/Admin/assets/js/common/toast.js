/**
 * Toast Notification System
 * Modern, animated toast notifications with auto-dismiss
 */
class ToastNotification {
    constructor() {
        this.container = document.getElementById('toastContainer');
        if (!this.container) {
            this.container = document.createElement('div');
            this.container.id = 'toastContainer';
            this.container.className = 'toast-container';
            document.body.appendChild(this.container);
        }
        this.toasts = [];
    }

    /**
     * Create and show a toast notification
     * @param {string} type - Type of toast (success, error, warning, info)
     * @param {string} title - Toast title
     * @param {string} message - Toast message
     * @param {number} duration - Auto-dismiss duration in ms (0 for no auto-dismiss)
     */
    show(type, title, message, duration = 5000) {
        if (!this.container) {
            this.container = document.getElementById('toastContainer');
            if (!this.container) {
                this.container = document.createElement('div');
                this.container.id = 'toastContainer';
                this.container.className = 'toast-container';
                document.body.appendChild(this.container);
            }
        }

        const toastId = `toast-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        
        const icons = {
            success: '<i class="ti ti-circle-check"></i>',
            error: '<i class="ti ti-circle-x"></i>',
            warning: '<i class="ti ti-alert-triangle"></i>',
            info: '<i class="ti ti-info-circle"></i>'
        };

        const toast = document.createElement('div');
        toast.className = `toast-notification toast-${type}`;
        toast.id = toastId;
        toast.innerHTML = `
            <div class="toast-content">
                <div class="toast-icon">
                    ${icons[type] || icons.info}
                </div>
                <div class="toast-body">
                    <div class="toast-title">${title}</div>
                    <div class="toast-message">${message}</div>
                </div>
                <button class="toast-close" onclick="toastSystem.dismiss('${toastId}')">
                    <i class="ti ti-x"></i>
                </button>
            </div>
            ${duration > 0 ? '<div class="toast-progress"></div>' : ''}
        `;

        this.container.appendChild(toast);
        this.toasts.push({ id: toastId, element: toast });

        setTimeout(() => {
            toast.classList.add('show');
        }, 10);

        if (duration > 0) {
            const progressBar = toast.querySelector('.toast-progress');
            if (progressBar) {
                progressBar.style.width = '100%';
                progressBar.style.transition = `width ${duration}ms linear`;
                setTimeout(() => {
                    progressBar.style.width = '0%';
                }, 10);
            }

            setTimeout(() => {
                this.dismiss(toastId);
            }, duration);
        }

        return toastId;
    }

    /**
     * Dismiss a toast notification
     * @param {string} toastId - ID of the toast to dismiss
     */
    dismiss(toastId) {
        const toast = document.getElementById(toastId);
        if (toast) {
            toast.classList.remove('show');
            toast.classList.add('hide');
            
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
                this.toasts = this.toasts.filter(t => t.id !== toastId);
            }, 400);
        }
    }
}

// Initialize toast system
const toastSystem = new ToastNotification();

/**
 * Helper functions for easy toast creation
 */
function showSuccess(title, message, duration = 5000) {
    return toastSystem.show('success', title, message, duration);
}

function showError(title, message, duration = 7000) {
    return toastSystem.show('error', title, message, duration);
}

function showWarning(title, message, duration = 6000) {
    return toastSystem.show('warning', title, message, duration);
}
