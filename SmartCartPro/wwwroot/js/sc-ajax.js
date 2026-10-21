/**
 * SmartCart Pro shared front-end helpers (window.SmartCartAjax).
 *
 * Server contract (CommonResponseModel, camelCase):
 *   { statusCode, status, message, data, errors, validationErrors, timestamp }
 * Success data may carry { redirectUrl }. Failures carry message + optional validationErrors { Field: [msg] }.
 *
 * API
 *   Feedback   : toast(message, type) · flash(message, type) · reloadWithToast(message) · confirm({...})
 *   Forms      : submitForm($form, {onSuccess,onError,stayOnSuccess}) · clearFieldErrors · applyValidationErrors
 *                setBusy($btn, busy, text) · formToObject($form)
 *   Requests   : post(url, data, opts) · postJson(url, obj, opts) · getJson(url, params, opts)
 *                opts: { $form, onSuccess(res), onError(res, xhr), silent }
 *   Widgets    : typeahead(input, {...}) · fillSelect($select, items, {...})
 *   Utilities  : debounce · escapeHtml · fmt.money/number/date/dateTime/time · parseServerDate · localizeTimes(root)
 *                (timestamps are UTC; [data-sc-utc] elements are localised on load — see localizeTimes)
 *
 * Declarative (no page JS needed)
 *   <button data-sc-post="/url" data-sc-param-id="5" data-sc-param-isactive="false"
 *           data-sc-confirm="Deactivate X?" data-sc-confirm-title=".." data-sc-confirm-ok="Deactivate"
 *           data-sc-confirm-class="btn-warning" data-sc-after="reload|none|redirect|remove-row">
 *   <select data-sc-auto-submit>   (submits its closest form on change)
 */
(function (window, $) {
  'use strict';

  if (!$) {
    console.error('sc-ajax requires jQuery');
    return;
  }

  var FLASH_KEY = 'sc.flash';

  /* ------------------------------------------------------------------ utils */

  function escapeHtml(value) {
    return String(value == null ? '' : value)
      .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
  }

  function debounce(fn, wait) {
    var timer = null;
    return function () {
      var ctx = this;
      var args = arguments;
      clearTimeout(timer);
      timer = setTimeout(function () { fn.apply(ctx, args); }, wait || 250);
    };
  }

  /**
   * Parses a value coming from the server. Timestamps are stored and serialised in UTC (often without a "Z"), so a
   * missing offset means UTC; a bare calendar date (yyyy-MM-dd) is kept as that local calendar day (no shifting).
   */
  function parseServerDate(v) {
    if (!v) return null;
    if (v instanceof Date) return isNaN(v.getTime()) ? null : v;
    var s = String(v).trim();
    var m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(s);
    if (m) return new Date(Number(m[1]), Number(m[2]) - 1, Number(m[3]));
    if (!/[zZ]$|[+-]\d{2}:?\d{2}$/.test(s)) s += 'Z';
    var d = new Date(s);
    return isNaN(d.getTime()) ? null : d;
  }

  var DATE_OPTS = {
    datetime: { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' },
    date: { day: '2-digit', month: 'short', year: 'numeric' },
    time: { hour: '2-digit', minute: '2-digit' }
  };

  var fmt = (function () {
    function num(n) { var v = Number(n); return isFinite(v) ? v : 0; }
    function show(v, kind) {
      var d = parseServerDate(v);
      return d ? d.toLocaleString(undefined, DATE_OPTS[kind]) : '';
    }
    var money = new Intl.NumberFormat(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    return {
      money: function (n) { return money.format(num(n)); },
      number: function (n, maxDecimals) {
        return new Intl.NumberFormat(undefined, {
          maximumFractionDigits: maxDecimals == null ? 3 : maxDecimals
        }).format(num(n));
      },
      date: function (v) { return show(v, 'date'); },
      dateTime: function (v) { return show(v, 'datetime'); },
      time: function (v) { return show(v, 'time'); }
    };
  })();

  /**
   * Server-rendered timestamps are shown in the user's own time zone:
   *   <time data-sc-utc="2026-10-19T14:05:00Z" data-sc-format="datetime|date|time">19 Oct 2026 14:05</time>
   * (build the attribute with TimeDisplay.Iso(value); the element text is the no-JS fallback).
   */
  function localizeTimes(root) {
    var $root = $(root || document);
    $root.find('[data-sc-utc]').addBack('[data-sc-utc]').each(function () {
      var $el = $(this);
      var d = parseServerDate($el.attr('data-sc-utc'));
      if (!d) return;
      var kind = $el.attr('data-sc-format');
      $el.text(d.toLocaleString(undefined, DATE_OPTS[kind] || DATE_OPTS.datetime));
      $el.attr('title', d.toLocaleString());
    });
  }

  function getAntiForgeryToken($form) {
    var $token = ($form && $form.length)
      ? $form.find('input[name="__RequestVerificationToken"]')
      : $();
    if (!$token.length) {
      $token = $('input[name="__RequestVerificationToken"]').first();
    }
    return $token.length ? $token.val() : '';
  }

  function loginUrl() {
    var base = (document.body && document.body.getAttribute('data-sc-login-url')) || '/Auth/Login';
    return base + '?returnUrl=' + encodeURIComponent(window.location.pathname + window.location.search) + '&notice=expired';
  }

  /* --------------------------------------------------------------- feedback */

  function ensureToastContainer() {
    var el = document.getElementById('sc-toast-container');
    if (el) return el;
    el = document.createElement('div');
    el.id = 'sc-toast-container';
    el.className = 'toast-container position-fixed top-0 end-0 p-3';
    el.style.zIndex = '1090';
    document.body.appendChild(el);
    return el;
  }

  function toast(message, type) {
    type = type || 'success';
    var bg = type === 'danger' || type === 'error' ? 'danger'
      : type === 'warning' ? 'warning'
      : type === 'info' ? 'info'
      : 'success';
    var container = ensureToastContainer();
    var id = 'sc-toast-' + Date.now() + '-' + Math.floor(Math.random() * 1000);
    var html =
      '<div id="' + id + '" class="toast align-items-center text-bg-' + bg + ' border-0" role="alert" aria-live="assertive" aria-atomic="true">' +
      '<div class="d-flex"><div class="toast-body"></div>' +
      '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-sc-toast-close aria-label="Close"></button>' +
      '</div></div>';
    container.insertAdjacentHTML('beforeend', html);
    var toastEl = document.getElementById(id);
    toastEl.querySelector('.toast-body').textContent = message || '';

    function dismiss() {
      toastEl.classList.remove('show');
      setTimeout(function () { if (toastEl.parentNode) toastEl.parentNode.removeChild(toastEl); }, 200);
    }

    toastEl.querySelector('[data-sc-toast-close]').addEventListener('click', dismiss);

    var ToastCtor = window.bootstrap && window.bootstrap.Toast;
    if (ToastCtor) {
      var instance = ToastCtor.getOrCreateInstance(toastEl, { delay: 4500 });
      toastEl.addEventListener('hidden.bs.toast', function () {
        if (toastEl.parentNode) toastEl.parentNode.removeChild(toastEl);
      });
      instance.show();
    } else {
      // Bootstrap JS not exposed on this page: show/hide with the Bootstrap CSS classes only.
      toastEl.classList.add('show');
      setTimeout(dismiss, 4500);
    }
  }

  /** Remember a toast across a redirect / reload; shown on the next page load. */
  function flash(message, type) {
    try {
      window.sessionStorage.setItem(FLASH_KEY, JSON.stringify({ m: message, t: type || 'success' }));
    } catch (e) { /* storage unavailable */ }
  }

  function showPendingFlash() {
    try {
      var raw = window.sessionStorage.getItem(FLASH_KEY);
      if (!raw) return;
      window.sessionStorage.removeItem(FLASH_KEY);
      var f = JSON.parse(raw);
      if (f && f.m) toast(f.m, f.t);
    } catch (e) { /* ignore */ }
  }

  function reloadWithToast(message) {
    flash(message);
    window.location.reload();
  }

  /**
   * Bootstrap confirm modal (#scConfirmModal from _ConfirmModal partial) with native confirm() fallback.
   * options: { title, body, confirmText, confirmClass, onConfirm }
   */
  function confirmDialog(options) {
    options = options || {};
    var ModalCtor = window.bootstrap && window.bootstrap.Modal;
    var $modal = $('#scConfirmModal');

    if (!$modal.length || !ModalCtor) {
      if (window.confirm(options.body || 'Are you sure?')) {
        if (typeof options.onConfirm === 'function') options.onConfirm();
      }
      return;
    }

    $modal.find('[data-sc-confirm-title]').text(options.title || 'Confirm');
    $modal.find('[data-sc-confirm-body]').text(options.body || 'Are you sure?');
    var $ok = $modal.find('[data-sc-confirm-ok]');
    $ok.text(options.confirmText || 'Confirm')
      .removeClass('btn-primary btn-danger btn-warning btn-success')
      .addClass(options.confirmClass || 'btn-primary');

    $ok.off('click.scConfirm').on('click.scConfirm', function () {
      ModalCtor.getOrCreateInstance($modal[0]).hide();
      if (typeof options.onConfirm === 'function') options.onConfirm();
    });

    ModalCtor.getOrCreateInstance($modal[0]).show();
  }

  /* ----------------------------------------------------------------- forms */

  function clearFieldErrors($form) {
    $form.find('.is-invalid').removeClass('is-invalid');
    $form.find('[data-valmsg-for], .field-validation-error, .field-validation-valid')
      .removeClass('field-validation-error')
      .addClass('field-validation-valid')
      .text('');
    $form.find('[data-sc-form-summary]').addClass('d-none').empty();
  }

  function applyValidationErrors($form, validationErrors) {
    if (!validationErrors) return;
    Object.keys(validationErrors).forEach(function (key) {
      var messages = validationErrors[key];
      if (!messages || !messages.length) return;
      var name = key;
      var $input = $form.find('[name="' + name + '"]');
      if (!$input.length && name.indexOf('.') >= 0) {
        $input = $form.find('[name="' + name.split('.').pop() + '"]');
      }
      if ($input.length) $input.addClass('is-invalid');
      var $span = $form.find('[data-valmsg-for="' + name + '"]');
      if (!$span.length) {
        $span = $form.find('[data-valmsg-for="' + name.split('.').pop() + '"]');
      }
      if ($span.length) {
        $span.removeClass('field-validation-valid').addClass('field-validation-error text-danger')
          .text(messages[0]);
      }
    });
  }

  /** Disable/enable a submit button and show a busy label (data-sc-busy-text, default "Saving…"). */
  function setBusy($btn, busy, text) {
    if (!$btn || !$btn.length) return;
    if (busy) {
      $btn.data('sc-original-html', $btn.html());
      var label = text || $btn.attr('data-sc-busy-text') || 'Saving…';
      $btn.prop('disabled', true).attr('aria-busy', 'true');
      $btn.html('<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>' + escapeHtml(label));
    } else {
      $btn.prop('disabled', false).removeAttr('aria-busy');
      var original = $btn.data('sc-original-html');
      if (original) $btn.html(original);
    }
  }

  /** Serialize a form into a plain object (repeated field names become arrays; antiforgery field omitted). */
  function formToObject($form) {
    var obj = {};
    $.each($form.serializeArray(), function (_, field) {
      if (field.name === '__RequestVerificationToken') return;
      if (Object.prototype.hasOwnProperty.call(obj, field.name)) {
        if (!$.isArray(obj[field.name])) obj[field.name] = [obj[field.name]];
        obj[field.name].push(field.value);
      } else {
        obj[field.name] = field.value;
      }
    });
    return obj;
  }

  /**
   * POST a form via AJAX (url-encoded). options: onSuccess(res), onError(res, xhr), stayOnSuccess.
   * Redirects to res.data.redirectUrl on success unless stayOnSuccess is set.
   */
  function submitForm($form, options) {
    options = options || {};
    var $btn = $form.find('[type="submit"]').first();
    clearFieldErrors($form);

    if ($.fn.valid && $form.valid && !$form.valid()) {
      return $.Deferred().reject({ clientValidation: true }).promise();
    }

    setBusy($btn, true);
    var url = $form.attr('action') || window.location.href;
    var method = ($form.attr('method') || 'POST').toUpperCase();
    var redirecting = false;

    return $.ajax({
      url: url,
      method: method,
      data: $form.serialize(),
      headers: {
        'X-Requested-With': 'XMLHttpRequest',
        'RequestVerificationToken': getAntiForgeryToken($form)
      }
    }).done(function (res) {
      var msg = (res && res.message) || 'Saved successfully.';
      var redirect = res && res.data && res.data.redirectUrl;
      redirecting = !options.stayOnSuccess && !!redirect;
      if (redirecting) {
        flash(msg);
      } else {
        toast(msg, 'success');
      }
      if (typeof options.onSuccess === 'function') options.onSuccess(res);
      if (redirecting) {
        window.location.href = redirect;
      }
    }).fail(function (xhr) {
      handleAjaxError(xhr, $form, options.onError);
    }).always(function () {
      // Keep the button disabled while the browser navigates away.
      if (!redirecting) setBusy($btn, false);
    });
  }

  /* -------------------------------------------------------------- requests */

  function handleAjaxError(xhr, $form, onError) {
    if (xhr.status === 401) {
      toast('Your session has expired. Please sign in again.', 'warning');
      setTimeout(function () { window.location.href = loginUrl(); }, 600);
      return;
    }

    var res = xhr.responseJSON;
    if (!res) {
      try { res = JSON.parse(xhr.responseText); } catch (e) { res = null; }
    }

    if (xhr.status === 403) {
      var forbidMsg = (res && res.message) || 'You do not have permission to perform this action.';
      toast(forbidMsg, 'danger');
      if (typeof onError === 'function') onError(res || {}, xhr);
      return;
    }

    var message = (res && res.message)
      || (xhr.status === 0 ? 'Cannot reach the server. Check your connection and try again.' : 'Something went wrong. Please try again.');
    if ($form && $form.length) {
      clearFieldErrors($form);
      if (res && res.validationErrors) {
        applyValidationErrors($form, res.validationErrors);
      }
      var $summary = $form.find('[data-sc-form-summary]');
      if ($summary.length) {
        $summary.removeClass('d-none').text(message);
      }
    }
    toast(message, 'danger');
    if (typeof onError === 'function') onError(res || {}, xhr);
  }

  /**
   * Core request. method: 'POST' | 'GET'. payload: object. options.json => JSON body.
   * Success toast uses res.message unless options.silent (GET is silent by default).
   */
  function request(method, url, payload, options) {
    options = options || {};
    var isGet = method === 'GET';
    var token = getAntiForgeryToken(options.$form);
    var ajax = {
      url: url,
      method: method,
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    };

    if (isGet) {
      ajax.data = payload || {};
      ajax.cache = false;
    } else {
      ajax.headers['RequestVerificationToken'] = token;
      if (options.json) {
        ajax.contentType = 'application/json; charset=utf-8';
        ajax.data = JSON.stringify(payload || {});
      } else {
        var form = $.extend({}, payload || {});
        if (token && !form.__RequestVerificationToken) form.__RequestVerificationToken = token;
        ajax.data = form;
      }
    }

    var silent = options.silent === undefined ? isGet : !!options.silent;

    return $.ajax(ajax).done(function (res) {
      if (!silent) toast((res && res.message) || 'Done.', 'success');
      if (typeof options.onSuccess === 'function') options.onSuccess(res);
    }).fail(function (xhr) {
      handleAjaxError(xhr, options.$form, options.onError);
    });
  }

  /** POST form-urlencoded fields (e.g. activate/deactivate). */
  function post(url, data, options) { return request('POST', url, data, options); }

  /** POST a JSON body (e.g. sale / purchase documents with line items). */
  function postJson(url, obj, options) {
    return request('POST', url, obj, $.extend({}, options, { json: true }));
  }

  /** GET JSON (lookups, list data). Silent by default. */
  function getJson(url, params, options) { return request('GET', url, params, options); }

  /* --------------------------------------------------------------- widgets */

  /** Fill a <select> from an array of items without injecting HTML. opts: valueField, textField, placeholder, selected */
  function fillSelect($select, items, opts) {
    opts = opts || {};
    var valueField = opts.valueField || 'id';
    var textField = opts.textField || 'name';
    var select = $select[0];
    if (!select) return;
    while (select.firstChild) select.removeChild(select.firstChild);
    if (opts.placeholder != null) {
      var ph = document.createElement('option');
      ph.value = '';
      ph.textContent = opts.placeholder;
      select.appendChild(ph);
    }
    (items || []).forEach(function (item) {
      var o = document.createElement('option');
      o.value = item[valueField];
      o.textContent = item[textField];
      if (opts.selected != null && String(opts.selected) === String(item[valueField])) o.selected = true;
      select.appendChild(o);
    });
  }

  /**
   * Accessible typeahead for product / customer / supplier lookups.
   * opts: {
   *   source(term) -> Array | jqXHR/Promise resolving to an Array (or {data:[...]}),
   *   label(item) -> string, sublabel(item) -> string (optional),
   *   onSelect(item), minChars (2), delay (250), limit (10), clearOnSelect (false), emptyText ('No matches')
   * }
   * Returns { clear(), destroy(), close() }.
   */
  function typeahead(input, opts) {
    var $input = $(input);
    opts = $.extend({ minChars: 2, delay: 250, limit: 10, clearOnSelect: false, emptyText: 'No matches' }, opts || {});
    var seq = 0;
    var items = [];
    var active = -1;
    var $list = $('<div class="list-group shadow-sm sc-typeahead" role="listbox"></div>')
      .css({ position: 'absolute', zIndex: 1100, display: 'none', maxHeight: '320px', overflowY: 'auto' })
      .appendTo(document.body);
    var listId = 'sc-ta-' + Math.floor(Math.random() * 1e9);
    $list.attr('id', listId);
    $input.attr({ role: 'combobox', 'aria-autocomplete': 'list', 'aria-expanded': 'false', 'aria-controls': listId, autocomplete: 'off' });

    function position() {
      var r = $input[0].getBoundingClientRect();
      $list.css({
        top: (r.bottom + window.pageYOffset) + 'px',
        left: (r.left + window.pageXOffset) + 'px',
        minWidth: r.width + 'px'
      });
    }

    function close() {
      $list.hide().empty();
      items = [];
      active = -1;
      $input.attr('aria-expanded', 'false');
    }

    function setActive(i) {
      var $opts = $list.children();
      $opts.removeClass('active').attr('aria-selected', 'false');
      active = i;
      if (i >= 0 && i < $opts.length) {
        var $a = $opts.eq(i).addClass('active').attr('aria-selected', 'true');
        var el = $a[0];
        if (el && el.scrollIntoView) el.scrollIntoView({ block: 'nearest' });
      }
    }

    function choose(i) {
      var item = items[i];
      if (!item) return;
      var text = opts.label ? opts.label(item) : String(item);
      close();
      if (opts.clearOnSelect) { $input.val(''); } else { $input.val(text); }
      if (typeof opts.onSelect === 'function') opts.onSelect(item);
    }

    function render(list) {
      items = (list || []).slice(0, opts.limit);
      $list.empty();
      if (!items.length) {
        $('<div class="list-group-item text-muted small"></div>').text(opts.emptyText).appendTo($list);
      } else {
        items.forEach(function (item, i) {
          var $row = $('<button type="button" class="list-group-item list-group-item-action py-2" role="option" aria-selected="false"></button>');
          $('<div class="fw-medium"></div>').text(opts.label ? opts.label(item) : String(item)).appendTo($row);
          if (opts.sublabel) {
            var sub = opts.sublabel(item);
            if (sub) $('<div class="small text-muted"></div>').text(sub).appendTo($row);
          }
          $row.on('mousedown', function (e) { e.preventDefault(); choose(i); });
          $list.append($row);
        });
      }
      position();
      $list.show();
      $input.attr('aria-expanded', 'true');
      setActive(items.length ? 0 : -1);
    }

    var search = debounce(function () {
      var term = $.trim($input.val());
      if (term.length < opts.minChars) { close(); return; }
      var mine = ++seq;
      $.when(opts.source(term)).done(function (result) {
        if (mine !== seq) return;
        var list = $.isArray(result) ? result : (result && $.isArray(result.data) ? result.data : []);
        render(list);
      }).fail(function () {
        if (mine === seq) close();
      });
    }, opts.delay);

    function onKeydown(e) {
      if (e.key === 'ArrowDown') {
        if ($list.is(':visible') && items.length) { e.preventDefault(); setActive((active + 1) % items.length); }
      } else if (e.key === 'ArrowUp') {
        if ($list.is(':visible') && items.length) { e.preventDefault(); setActive((active - 1 + items.length) % items.length); }
      } else if (e.key === 'Enter') {
        if ($list.is(':visible') && active >= 0) { e.preventDefault(); choose(active); }
      } else if (e.key === 'Escape') {
        close();
      }
    }

    $input.on('input.scTa', search).on('keydown.scTa', onKeydown).on('blur.scTa', function () { setTimeout(close, 120); });
    var onViewportChange = function () { if ($list.is(':visible')) position(); };
    $(window).on('resize.' + listId + ' scroll.' + listId, onViewportChange);

    return {
      close: close,
      clear: function () { $input.val(''); close(); },
      destroy: function () {
        $input.off('.scTa').removeAttr('role aria-autocomplete aria-expanded aria-controls');
        $(window).off('.' + listId);
        $list.remove();
      }
    };
  }

  /* ---------------------------------------------------- declarative hooks */

  function collectParams($el) {
    var out = {};
    $.each($el[0].attributes, function (_, attr) {
      if (attr.name.indexOf('data-sc-param-') === 0) {
        out[attr.name.substring('data-sc-param-'.length)] = attr.value;
      }
    });
    return out;
  }

  $(document).on('click', '[data-sc-post]', function (e) {
    e.preventDefault();
    var $btn = $(this);
    if ($btn.prop('disabled') || $btn.attr('aria-busy') === 'true') return;

    var url = $btn.attr('data-sc-post');
    var data = collectParams($btn);

    function run() {
      $btn.prop('disabled', true).attr('aria-busy', 'true');
      post(url, data, {
        silent: true,
        onSuccess: function (res) {
          var after = $btn.attr('data-sc-after') || 'reload';
          var msg = (res && res.message) || 'Done.';
          if (after === 'reload') {
            reloadWithToast(msg);
          } else if (after === 'redirect' && res && res.data && res.data.redirectUrl) {
            flash(msg);
            window.location.href = res.data.redirectUrl;
          } else if (after === 'remove-row') {
            toast(msg, 'success');
            $btn.closest('tr').fadeOut(150, function () { $(this).remove(); });
          } else {
            toast(msg, 'success');
            $btn.prop('disabled', false).removeAttr('aria-busy');
          }
          $btn.trigger('sc:posted', [res]);
        },
        onError: function () {
          $btn.prop('disabled', false).removeAttr('aria-busy');
        }
      });
    }

    var body = $btn.attr('data-sc-confirm');
    if (body) {
      confirmDialog({
        title: $btn.attr('data-sc-confirm-title'),
        body: body,
        confirmText: $btn.attr('data-sc-confirm-ok'),
        confirmClass: $btn.attr('data-sc-confirm-class'),
        onConfirm: run
      });
    } else {
      run();
    }
  });

  $(document).on('change', '[data-sc-auto-submit]', function () {
    var $form = $(this).closest('form');
    if ($form.length) $form.trigger('submit');
  });

  $(showPendingFlash);
  $(function () { localizeTimes(document); });

  window.SmartCartAjax = {
    toast: toast,
    flash: flash,
    reloadWithToast: reloadWithToast,
    confirm: confirmDialog,
    submitForm: submitForm,
    setBusy: setBusy,
    formToObject: formToObject,
    post: post,
    postJson: postJson,
    getJson: getJson,
    typeahead: typeahead,
    fillSelect: fillSelect,
    debounce: debounce,
    escapeHtml: escapeHtml,
    fmt: fmt,
    parseServerDate: parseServerDate,
    localizeTimes: localizeTimes,
    getAntiForgeryToken: getAntiForgeryToken,
    clearFieldErrors: clearFieldErrors,
    applyValidationErrors: applyValidationErrors
  };
})(window, window.jQuery);
