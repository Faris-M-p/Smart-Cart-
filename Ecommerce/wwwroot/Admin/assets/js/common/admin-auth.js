/**
 * Admin authentication helper.
 * Stores the JWT cookie, attaches Bearer tokens to Admin API calls,
 * and handles basic login/logout redirects.
 */
(function (window) {
  var TOKEN_COOKIE = 'smartcart.admin.token';
  var SESSION_KEY = 'smartcart.admin.session';
  var LOGIN_PATH = '/admin/login';
  var DASHBOARD_PATH = '/Admin/Dashboard';

  function getCookie(name) {
    var parts = ('; ' + document.cookie).split('; ' + name + '=');
    if (parts.length < 2) {
      return null;
    }
    return decodeURIComponent(parts.pop().split(';').shift() || '') || null;
  }

  function setCookie(name, value, maxAgeSeconds) {
    var cookie = name + '=' + encodeURIComponent(value) + '; Path=/; SameSite=Lax; Max-Age=' + Math.max(60, maxAgeSeconds);
    if (window.location.protocol === 'https:') {
      cookie += '; Secure';
    }
    document.cookie = cookie;
  }

  function deleteCookie(name) {
    document.cookie = name + '=; Path=/; Max-Age=0; SameSite=Lax';
  }

  function getToken() {
    var token = getCookie(TOKEN_COOKIE);
    if (token && isTokenExpired(token)) {
      clearAuth();
      return null;
    }
    return token;
  }

  function isTokenExpired(token) {
    try {
      var payloadPart = token.split('.')[1];
      if (!payloadPart) {
        return true;
      }
      var normalized = payloadPart.replace(/-/g, '+').replace(/_/g, '/');
      var payload = JSON.parse(atob(normalized));
      return !payload.exp || (payload.exp * 1000) <= Date.now();
    } catch {
      return true;
    }
  }

  function getSession() {
    try {
      return JSON.parse(localStorage.getItem(SESSION_KEY) || 'null');
    } catch {
      return null;
    }
  }

  function setSession(loginData) {
    if (!loginData) {
      return;
    }

    var token = loginData.accessToken || loginData.AccessToken;
    var expiresAt = loginData.expiresAt || loginData.ExpiresAt;
    if (token) {
      var maxAge = 60 * 60;
      if (expiresAt) {
        var remainingMs = new Date(expiresAt).getTime() - Date.now();
        maxAge = Math.max(60, Math.floor(remainingMs / 1000));
      }
      setCookie(TOKEN_COOKIE, token, maxAge);
    }

    localStorage.setItem(SESSION_KEY, JSON.stringify({
      expiresAt: expiresAt,
      employee: loginData.employee || loginData.Employee || null,
      role: loginData.role || loginData.Role || null,
      permissions: loginData.permissions || loginData.Permissions || []
    }));
  }

  function clearAuth() {
    deleteCookie(TOKEN_COOKIE);
    localStorage.removeItem(SESSION_KEY);
  }

  function isAuthenticated() {
    return !!getToken();
  }

  function isLoginPath(pathname) {
    return /^\/admin\/login\/?$/i.test(pathname || '');
  }

  function isLoginApi(url) {
    return /\/api\/admin\/auth\/login/i.test(String(url || ''));
  }

  function isAdminApi(url) {
    var value = String(url || '');
    return /\/api\/admin\//i.test(value) || /\/admin\//i.test(value);
  }

  function guardAdminPage() {
    if (!isAuthenticated()) {
      window.location.replace(LOGIN_PATH);
      return;
    }

    fetch('/api/admin/auth/me').then(function (res) {
      if (!res.ok) {
        clearAuth();
        window.location.replace(LOGIN_PATH);
        return null;
      }
      return res.json();
    }).then(function (body) {
      if (!body) {
        return;
      }
      var data = body.data || body.Data;
      if (data) {
        var current = getSession() || {};
        localStorage.setItem(SESSION_KEY, JSON.stringify({
          expiresAt: current.expiresAt,
          employee: data.employee || data.Employee || current.employee || null,
          role: data.role || data.Role || current.role || null,
          permissions: data.permissions || data.Permissions || []
        }));
        applyHeader();
      }
      if (window.adminPermissions) {
        if (!window.adminPermissions.guardCurrentPage()) {
          return;
        }
        window.adminPermissions.apply();
      }
    }).catch(function () {
      clearAuth();
      window.location.replace(LOGIN_PATH);
    });
  }

  function guardLoginPage() {
    if (!isAuthenticated()) {
      return;
    }

    fetch('/api/admin/auth/me').then(function (res) {
      if (!res.ok) {
        clearAuth();
        return null;
      }
      return res.json();
    }).then(function (body) {
      if (!body) {
        return;
      }
      var data = body.data || body.Data;
      if (data) {
        var current = getSession() || {};
        localStorage.setItem(SESSION_KEY, JSON.stringify({
          expiresAt: current.expiresAt,
          employee: data.employee || data.Employee || current.employee || null,
          role: data.role || data.Role || current.role || null,
          permissions: data.permissions || data.Permissions || []
        }));
      }
      var nextPath = window.adminPermissions && window.adminPermissions.homePath
        ? window.adminPermissions.homePath()
        : DASHBOARD_PATH;
      window.location.replace(nextPath);
    }).catch(function () {
      clearAuth();
    });
  }

  function logout() {
    clearAuth();
    originalFetch('/api/admin/auth/logout', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    }).catch(function () { /* cookie is already cleared locally */ });
    window.location.replace(LOGIN_PATH);
  }

  function displayName(session) {
    var employee = session && (session.employee || session.Employee);
    return employee && (employee.name || employee.Name) || '';
  }

  function displayRole(session) {
    var role = session && (session.role || session.Role);
    return role && (role.name || role.Name) || '';
  }

  function applyHeader() {
    var session = getSession();
    var name = displayName(session);
    var role = displayRole(session);
    document.querySelectorAll('[data-admin-user-name]').forEach(function (el) {
      if (name) {
        el.textContent = name;
      }
    });
    document.querySelectorAll('[data-admin-user-role]').forEach(function (el) {
      if (role) {
        el.textContent = role;
      }
    });
  }

  function refreshHeaderFromApi() {
    applyHeader();
    if (!getToken() || isLoginPath(window.location.pathname)) {
      return;
    }

    fetch('/api/admin/auth/me')
      .then(function (res) {
        if (res.status === 403) {
          clearAuth();
          window.location.replace(LOGIN_PATH);
          return null;
        }
        return res.ok ? res.json() : null;
      })
      .then(function (body) {
        var data = body && (body.data || body.Data);
        if (!data) {
          return;
        }
        var current = getSession() || {};
        localStorage.setItem(SESSION_KEY, JSON.stringify({
          expiresAt: current.expiresAt,
          employee: data.employee || data.Employee || null,
          role: data.role || data.Role || null,
          permissions: data.permissions || data.Permissions || []
        }));
        applyHeader();
        if (window.adminPermissions) {
          if (!window.adminPermissions.guardCurrentPage()) {
            return;
          }
          window.adminPermissions.apply();
        }
      })
      .catch(function () { /* keep existing header */ });
  }

  var originalFetch = window.fetch.bind(window);
  window.fetch = function (input, init) {
    var url = typeof input === 'string' ? input : (input && input.url) || '';
    init = init ? Object.assign({}, init) : {};

    if (isAdminApi(url) && !isLoginApi(url)) {
      var token = getToken();
      if (token) {
        var headers = new Headers(init.headers || (typeof input !== 'string' && input.headers) || undefined);
        if (!headers.has('Authorization')) {
          headers.set('Authorization', 'Bearer ' + token);
        }
        init.headers = headers;
      }
    }

    return originalFetch(input, init).then(function (res) {
      if (res.status === 401 && isAdminApi(url) && !isLoginApi(url)) {
        clearAuth();
        if (!isLoginPath(window.location.pathname)) {
          window.location.replace(LOGIN_PATH);
        }
      }
      return res;
    });
  };

  function bindLogout() {
    document.addEventListener('click', function (event) {
      var target = event.target.closest('.js-admin-logout');
      if (!target) {
        return;
      }
      event.preventDefault();
      logout();
    });
  }

  window.adminAuth = {
    getToken: getToken,
    getSession: getSession,
    setSession: setSession,
    clearAuth: clearAuth,
    isAuthenticated: isAuthenticated,
    guardAdminPage: guardAdminPage,
    guardLoginPage: guardLoginPage,
    logout: logout,
    applyHeader: applyHeader
  };

  document.addEventListener('DOMContentLoaded', function () {
    refreshHeaderFromApi();
    bindLogout();
  });
})(window);
