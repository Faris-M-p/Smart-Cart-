/**
 * SmartCart Pro shared AJAX helpers for MVC forms and list actions.
 * Expects CommonResponseModel JSON: statusCode, status, message, errors, validationErrors, data, timestamp.
 */
(function (window, $) {
  'use strict';

  if (!$) {
    console.error('sc-ajax requires jQuery');
    return;
  }

  function getAntiForgeryToken($form) {
    var $token = ($form && $form.length)
      ? $form.find('input[name="__RequestVerificationToken"]')
      : $('input[name="__RequestVerificationToken"]').first();
    return $token.length ? $token.val() : '';
  }

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
    var id = 'sc-toast-' + Date.now();
    var html =
      '<div id="' + id + '" class="toast align-items-center text-bg-' + bg + ' border-0" role="alert" aria-live="assertive" aria-atomic="true">' +
      '<div class="d-flex"><div class="toast-body"></div>' +
      '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>' +
      '</div></div>';
    container.insertAdjacentHTML('beforeend', html);
    var toastEl = document.getElementById(id);
    toastEl.querySelector('.toast-body').textContent = message || '';
    var ToastCtor = window.bootstrap && window.bootstrap.Toast;
    if (ToastCtor) {
      var instance = ToastCtor.getOrCreateInstance(toastEl, { delay: 4000 });
      toastEl.addEventListener('hidden.bs.toast', function () { toastEl.remove(); });
      instance.show();
    } else {
      setTimeout(function () { toastEl.remove(); }, 4000);
    }
  }

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

  function setSubmitting($btn, submitting) {
    if (!$btn || !$btn.length) return;
    if (submitting) {
      $btn.data('sc-original-html', $btn.html());
      $btn.prop('disabled', true).attr('aria-busy', 'true');
      $btn.html('<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>Saving…');
    } else {
      $btn.prop('disabled', false).removeAttr('aria-busy');
      var original = $btn.data('sc-original-html');
      if (original) $btn.html(original);
    }
  }

  /**
   * POST a form via AJAX. Options: onSuccess(res), onError(res), stayOnSuccess.
   */
  function submitForm($form, options) {
    options = options || {};
    var $btn = $form.find('[type="submit"]').first();
    clearFieldErrors($form);

    if ($.fn.valid && $form.valid && !$form.valid()) {
      return $.Deferred().reject({ clientValidation: true }).promise();
    }

    setSubmitting($btn, true);
    var url = $form.attr('action') || window.location.href;
    var method = ($form.attr('method') || 'POST').toUpperCase();

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
      toast(msg, 'success');
      if (typeof options.onSuccess === 'function') options.onSuccess(res);
      if (!options.stayOnSuccess) {
        var redirect = res && res.data && res.data.redirectUrl;
        if (redirect) {
          window.location.href = redirect;
        }
      }
    }).fail(function (xhr) {
      handleAjaxError(xhr, $form, options.onError);
    }).always(function () {
      setSubmitting($btn, false);
    });
  }

  /**
   * POST form-urlencoded body (e.g. activate/deactivate).
   */
  function post(url, data, options) {
    options = options || {};
    var token = getAntiForgeryToken(options.$form);
    var payload = $.extend({}, data || {});
    if (token && !payload.__RequestVerificationToken) {
      payload.__RequestVerificationToken = token;
    }

    return $.ajax({
      url: url,
      method: 'POST',
      data: payload,
      headers: {
        'X-Requested-With': 'XMLHttpRequest',
        'RequestVerificationToken': token
      }
    }).done(function (res) {
      var msg = (res && res.message) || 'Done.';
      toast(msg, 'success');
      if (typeof options.onSuccess === 'function') options.onSuccess(res);
    }).fail(function (xhr) {
      handleAjaxError(xhr, options.$form, options.onError);
    });
  }

  function handleAjaxError(xhr, $form, onError) {
    if (xhr.status === 401) {
      toast('Your session has expired. Please sign in again.', 'warning');
      var login = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);
      setTimeout(function () { window.location.href = login; }, 600);
      return;
    }

    if (xhr.status === 403) {
      var forbidMsg = 'You do not have permission to perform this action.';
      try {
        var fj = xhr.responseJSON;
        if (fj && fj.message) forbidMsg = fj.message;
      } catch (e) { /* ignore */ }
      toast(forbidMsg, 'danger');
      if (typeof onError === 'function') onError(xhr.responseJSON || {}, xhr);
      return;
    }

    var res = xhr.responseJSON;
    if (!res) {
      try { res = JSON.parse(xhr.responseText); } catch (e) { res = null; }
    }

    var message = (res && res.message) || 'Something went wrong. Please try again.';
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
   * Bootstrap confirm modal helper.
   * options: { title, body, confirmText, confirmClass, onConfirm }
   */
  function confirm(options) {
    options = options || {};
    var $modal = $('#scConfirmModal');
    if (!$modal.length) {
      if (window.confirm(options.body || 'Are you sure?')) {
        if (typeof options.onConfirm === 'function') options.onConfirm();
      }
      return;
    }
    $modal.find('[data-sc-confirm-title]').text(options.title || 'Confirm');
    $modal.find('[data-sc-confirm-body]').text(options.body || 'Are you sure?');
    var $ok = $modal.find('[data-sc-confirm-ok]');
    $ok.text(options.confirmText || 'Confirm')
      .removeClass('btn-primary btn-danger btn-warning')
      .addClass(options.confirmClass || 'btn-primary');

    $ok.off('click.scConfirm').on('click.scConfirm', function () {
      var ModalCtor = window.bootstrap && window.bootstrap.Modal;
      if (ModalCtor) ModalCtor.getOrCreateInstance($modal[0]).hide();
      if (typeof options.onConfirm === 'function') options.onConfirm();
    });

    var ModalCtor2 = window.bootstrap && window.bootstrap.Modal;
    if (ModalCtor2) ModalCtor2.getOrCreateInstance($modal[0]).show();
  }

  window.SmartCartAjax = {
    toast: toast,
    submitForm: submitForm,
    post: post,
    confirm: confirm,
    getAntiForgeryToken: getAntiForgeryToken,
    clearFieldErrors: clearFieldErrors,
    applyValidationErrors: applyValidationErrors
  };
})(window, window.jQuery);
