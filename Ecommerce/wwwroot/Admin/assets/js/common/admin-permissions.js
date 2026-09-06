/**
 * Central Admin permission checks for sidebar, pages, and action buttons.
 * Uses the permission codes stored from login / /api/admin/auth/me.
 */
(function (window) {
  var ACCESS_DENIED_PATH = '/admin/access-denied';
  var DASHBOARD_PATH = '/Admin/Dashboard';
  var HIDDEN_CLASS = 'js-permission-hidden';
  var applying = false;

  var PAGE_RULES = [
    { test: /^\/admin\/access-denied\/?$/i, permission: null },
    { test: /^\/admin\/login\/?$/i, permission: null },
    { test: /^\/admin\/dashboard\/?$/i, permission: 'Dashboard.View' },
    { test: /^\/admin\/category(\/|$)/i, permission: 'Categories.View' },
    { test: /^\/admin\/subcategory(\/|$)/i, permission: 'SubCategories.View' },
    { test: /^\/admin\/brand(\/|$)/i, permission: 'Brands.View' },
    { test: /^\/admin\/productvariant(\/|$)/i, permission: 'ProductVariants.View' },
    { test: /^\/admin\/product(\/|$)/i, permission: 'Products.View' },
    { test: /^\/admin\/variantvalue(\/|$)/i, permission: 'VariantValues.View' },
    { test: /^\/admin\/variant(\/|$)/i, permission: 'Variants.View' },
    { test: /^\/admin\/supplier(\/|$)/i, permission: 'Suppliers.View' },
    { test: /^\/admin\/purchase(\/|$)/i, permission: 'Purchases.View' },
    { test: /^\/admin\/inventory(\/|$)/i, permission: 'Stock.View' },
    { test: /^\/admin\/employees(\/|$)/i, permission: 'Employees.View' },
    { test: /^\/admin\/employee(\/|$)/i, permission: 'Employees.View' },
    { test: /^\/admin\/user-roles\/\d+\/permissions\/?$/i, permission: 'UserRoles.Edit' },
    { test: /^\/admin\/user-roles(\/|$)/i, permission: 'UserRoles.View' },
    { test: /^\/admin\/userrole(\/|$)/i, permission: 'UserRoles.View' }
  ];

  var HOME_CANDIDATES = [
    { path: DASHBOARD_PATH, permission: 'Dashboard.View' },
    { path: '/Admin/Category', permission: 'Categories.View' },
    { path: '/Admin/SubCategory', permission: 'SubCategories.View' },
    { path: '/Admin/Brand', permission: 'Brands.View' },
    { path: '/Admin/Product', permission: 'Products.View' },
    { path: '/Admin/Variant', permission: 'Variants.View' },
    { path: '/Admin/VariantValue', permission: 'VariantValues.View' },
    { path: '/Admin/ProductVariant', permission: 'ProductVariants.View' },
    { path: '/Admin/Supplier', permission: 'Suppliers.View' },
    { path: '/Admin/Purchase', permission: 'Purchases.View' },
    { path: '/Admin/Inventory', permission: 'Stock.View' },
    { path: '/admin/employees', permission: 'Employees.View' },
    { path: '/Admin/UserRole', permission: 'UserRoles.View' }
  ];

  function getSession() {
    return window.adminAuth && window.adminAuth.getSession && window.adminAuth.getSession();
  }

  function sessionHasPermissionList() {
    var session = getSession();
    return !!(session && (Array.isArray(session.permissions) || Array.isArray(session.Permissions)));
  }

  function getPermissions() {
    var session = getSession();
    var list = (session && (session.permissions || session.Permissions)) || [];
    if (!Array.isArray(list)) {
      return [];
    }
    return list.map(function (code) { return String(code || '').trim(); }).filter(Boolean);
  }

  function normalize(code) {
    return String(code || '').trim().toLowerCase();
  }

  function has(code) {
    if (!code) {
      return true;
    }
    var needed = normalize(code);
    return getPermissions().some(function (item) {
      return normalize(item) === needed;
    });
  }

  function hasAny(codes) {
    if (!codes || !codes.length) {
      return true;
    }
    return codes.some(function (code) { return has(code); });
  }

  function can(moduleName, action) {
    return has(String(moduleName || '') + '.' + capitalize(action));
  }

  function capitalize(value) {
    var text = String(value || '').trim();
    if (!text) {
      return '';
    }
    if (text.indexOf('.') >= 0) {
      return text;
    }
    return text.charAt(0).toUpperCase() + text.slice(1);
  }

  function requiredForPath(pathname) {
    var path = (pathname || window.location.pathname || '').replace(/\/+$/, '') || '/';
    for (var i = 0; i < PAGE_RULES.length; i++) {
      if (PAGE_RULES[i].test.test(path)) {
        return PAGE_RULES[i].permission;
      }
    }
    return null;
  }

  function homePath() {
    for (var i = 0; i < HOME_CANDIDATES.length; i++) {
      if (has(HOME_CANDIDATES[i].permission)) {
        return HOME_CANDIDATES[i].path;
      }
    }
    return ACCESS_DENIED_PATH;
  }

  function toggleHidden(element, shouldHide) {
    if (!element) {
      return;
    }
    element.classList.toggle(HIDDEN_CLASS, !!shouldHide);
  }

  function applySidebar() {
    if (!sessionHasPermissionList()) {
      return;
    }
    document.querySelectorAll('.pc-sidebar [data-permission]').forEach(function (el) {
      var item = el.closest('.pc-item') || el;
      toggleHidden(item, !has(el.getAttribute('data-permission')));
    });

    var parents = Array.from(document.querySelectorAll('.pc-sidebar .pc-hasmenu'));
    parents.sort(function (a, b) {
      return b.querySelectorAll('.pc-hasmenu').length - a.querySelectorAll('.pc-hasmenu').length;
    });
    parents.forEach(function (parent) {
      var submenu = parent.querySelector(':scope > .pc-submenu');
      if (!submenu) {
        return;
      }
      var children = Array.from(submenu.children).filter(function (node) {
        return node.classList && node.classList.contains('pc-item');
      });
      var anyVisible = children.some(function (child) {
        return !child.classList.contains(HIDDEN_CLASS);
      });
      toggleHidden(parent, !anyVisible);
    });
  }

  function resolveActionCode(el) {
    var explicit = el.getAttribute('data-permission');
    if (explicit) {
      return explicit;
    }
    var action = el.getAttribute('data-action');
    if (!action) {
      return null;
    }
    if (action.indexOf('.') >= 0) {
      return action;
    }
    var moduleName = el.getAttribute('data-permission-module');
    if (!moduleName) {
      var host = el.closest('[data-permission-module]');
      moduleName = host ? host.getAttribute('data-permission-module') : '';
    }
    if (!moduleName) {
      return null;
    }
    return moduleName + '.' + capitalize(action);
  }

  function applyActions() {
    if (!sessionHasPermissionList()) {
      return;
    }
    document.querySelectorAll('[data-permission], [data-action]').forEach(function (el) {
      if (el.closest('.pc-sidebar')) {
        return;
      }
      var code = resolveActionCode(el);
      if (!code) {
        return;
      }
      toggleHidden(el, !has(code));
    });
  }

  function applyHomeLinks() {
    var path = homePath();
    document.querySelectorAll('[data-admin-home-link]').forEach(function (el) {
      if (el.tagName === 'A') {
        el.setAttribute('href', path);
      }
    });
  }

  function apply() {
    if (applying) {
      return;
    }
    applying = true;
    try {
      applySidebar();
      applyActions();
      applyHomeLinks();
    } finally {
      applying = false;
    }
  }

  function guardCurrentPage() {
    var required = requiredForPath(window.location.pathname);
    if (!required || !sessionHasPermissionList() || has(required)) {
      return true;
    }
    if (!/\/admin\/access-denied\/?$/i.test(window.location.pathname)) {
      window.location.replace(ACCESS_DENIED_PATH);
    }
    return false;
  }

  function watch() {
    if (window.__adminPermissionObserver || !document.body) {
      return;
    }
    window.__adminPermissionObserver = new MutationObserver(function () {
      apply();
    });
    window.__adminPermissionObserver.observe(document.body, { childList: true, subtree: true });
  }

  window.adminPermissions = {
    has: has,
    hasAny: hasAny,
    can: can,
    apply: apply,
    guardCurrentPage: guardCurrentPage,
    homePath: homePath,
    getPermissions: getPermissions,
    requiredForPath: requiredForPath
  };

  document.addEventListener('DOMContentLoaded', function () {
    apply();
    watch();
  });
})(window);
