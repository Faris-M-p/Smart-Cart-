/**
 * Employees (users) pages: list, create, edit and password reset.
 * Relies on window.SmartCartAjax (sc-ajax.js) and jQuery from the layout. No Bootstrap JS required.
 *
 * Pages / hooks
 *   #employeeListPage      list with a GET filter form (form[data-sc-list-filter]); status toggles use data-sc-post
 *   #employeeForm          create form; a generated temporary password is shown once in #employeeCreatedPanel
 *   #employeeEditForm      edit form (profile, role, store access)
 *   #resetPasswordForm     admin password reset; a generated password is shown once in #resetPasswordResult
 */
(function (window, $) {
  'use strict';

  if (!$) return;
  var ajax = window.SmartCartAjax;

  /* ------------------------------------------------------------ small helpers */

  /** Render server UTC timestamps (time[data-sc-utc]) in the browser's local time. */
  function initLocalTimes() {
    $('time[data-sc-utc]').each(function () {
      var raw = this.getAttribute('data-sc-utc');
      var date = new Date(raw);
      if (isNaN(date.getTime())) return;
      this.textContent = date.toLocaleString();
      this.setAttribute('title', raw.replace('T', ' ').replace('Z', ' UTC'));
    });
  }

  function setPasswordIcon($btn, visible) {
    $btn.find('i').toggleClass('bi-eye', !visible).toggleClass('bi-eye-slash', visible);
    $btn.attr('aria-label', visible ? 'Hide password' : 'Show password');
  }

  /** Show / hide a password input from a button: data-sc-toggle-password="#inputId". */
  function bindPasswordToggle() {
    $(document).on('click', '[data-sc-toggle-password]', function () {
      var $btn = $(this);
      var $input = $($btn.attr('data-sc-toggle-password'));
      if (!$input.length) return;
      var show = $input.attr('type') === 'password';
      $input.attr('type', show ? 'text' : 'password');
      setPasswordIcon($btn, show);
    });
  }

  /**
   * "Generate a temporary password" switch: hides the manual password box while generating.
   * Hooks inside the form: [data-sc-password-mode] (checkbox), [data-sc-manual-password] (wrapper),
   * optional [data-sc-must-change] (always on while a password is generated).
   */
  function bindPasswordMode($form) {
    var $mode = $form.find('[data-sc-password-mode]');
    if (!$mode.length) return;
    var $manual = $form.find('[data-sc-manual-password]');
    var $must = $form.find('[data-sc-must-change]');

    function apply() {
      var generate = $mode.is(':checked');
      $manual.toggleClass('d-none', generate);
      if (generate) {
        $manual.find('input').val('').filter('[type="text"]').attr('type', 'password');
        setPasswordIcon($manual.find('[data-sc-toggle-password]'), false);
      }
      if ($must.length) {
        if (generate) {
          $must.prop('checked', true).prop('disabled', true);
        } else {
          $must.prop('disabled', false);
        }
      }
    }

    $mode.on('change', apply);
    apply();
  }

  /**
   * Store access picker: a store checkbox enables its "Default" radio; exactly one default is kept
   * among the ticked stores (the first ticked store becomes the default when none is chosen).
   */
  function initStorePicker($scope) {
    $scope.find('[data-sc-store-picker]').each(function () {
      var $picker = $(this);

      function sync() {
        $picker.find('[data-sc-store-row]').each(function () {
          var $row = $(this);
          var checked = $row.find('[data-sc-store-check]').prop('checked');
          var $radio = $row.find('[data-sc-store-default]');
          $radio.prop('disabled', !checked);
          if (!checked) $radio.prop('checked', false);
        });

        if (!$picker.find('[data-sc-store-default]:checked').length) {
          var $first = $picker.find('[data-sc-store-check]:checked').first();
          if ($first.length) {
            $first.closest('[data-sc-store-row]').find('[data-sc-store-default]').prop('checked', true);
          }
        }
      }

      $picker.on('change', '[data-sc-store-check]', sync);
      sync();
    });
  }

  /* --------------------------------------------------- temporary password panel */

  function copyToClipboard($input, done) {
    var text = $input.val();

    function fallback() {
      try {
        $input[0].focus();
        $input[0].select();
        done(document.execCommand('copy'));
      } catch (e) {
        done(false);
      }
    }

    if (window.navigator.clipboard && window.isSecureContext) {
      window.navigator.clipboard.writeText(text).then(function () { done(true); }, fallback);
    } else {
      fallback();
    }
  }

  function bindCopyPassword() {
    $(document).on('click', '[data-sc-copy-password]', function () {
      var $input = $(this).closest('.input-group').find('[data-sc-temp-password]');
      if (!$input.length || !$input.val()) return;
      copyToClipboard($input, function (ok) {
        if (!ajax) return;
        ajax.toast(
          ok ? 'Password copied to the clipboard.' : 'Could not copy automatically. Select the password and copy it manually.',
          ok ? 'success' : 'warning');
      });
    });

    // Never leave a one-time password behind in a page restored from the back/forward cache.
    $(window).on('pagehide', function () {
      $('[data-sc-temp-password]').val('');
    });
  }

  function showTemporaryPassword($container, password) {
    var $input = $container.find('[data-sc-temp-password]');
    $input.val(password);
    $container.removeClass('d-none');
    if ($input.length) {
      $input[0].focus();
      $input[0].select();
    }
  }

  function hideTemporaryPassword($container) {
    $container.find('[data-sc-temp-password]').val('');
    $container.addClass('d-none');
  }

  /* ------------------------------------------------------------------- pages */

  function initList() {
    var $page = $('#employeeListPage');
    if (!$page.length) return;

    $page.find('form[data-sc-list-filter]').on('submit', function () {
      $page.addClass('opacity-50').attr('aria-busy', 'true');
    });
  }

  function initCreateForm() {
    var $form = $('#employeeForm');
    if (!$form.length || !ajax) return;

    var $card = $('#employeeFormCard');
    var $panel = $('#employeeCreatedPanel');

    bindPasswordMode($form);
    initStorePicker($form);

    $form.on('submit', function (e) {
      e.preventDefault();
      var generate = $form.find('[data-sc-password-mode]').is(':checked');

      // A generated password is shown once on this page; otherwise the helper redirects to the list.
      ajax.submitForm($form, {
        stayOnSuccess: generate,
        onSuccess: function (res) {
          if (!generate) return;
          var data = (res && res.data) || {};
          if (data.temporaryPassword) {
            $panel.find('[data-sc-created-user]').text(data.userName ? 'Username: ' + data.userName : '');
            $card.addClass('d-none');
            showTemporaryPassword($panel, data.temporaryPassword);
          } else if (data.redirectUrl) {
            ajax.flash((res && res.message) || 'Employee created successfully.');
            window.location.href = data.redirectUrl;
          }
        }
      });
    });
  }

  function initEditForm() {
    var $form = $('#employeeEditForm');
    if (!$form.length || !ajax) return;

    initStorePicker($form);

    $form.on('submit', function (e) {
      e.preventDefault();
      ajax.submitForm($form);
    });
  }

  function initResetForm() {
    var $form = $('#resetPasswordForm');
    if (!$form.length || !ajax) return;

    var $result = $('#resetPasswordResult');
    bindPasswordMode($form);

    $form.on('submit', function (e) {
      e.preventDefault();
      hideTemporaryPassword($result);

      ajax.submitForm($form, {
        stayOnSuccess: true,
        onSuccess: function (res) {
          var data = (res && res.data) || {};
          $form.find('[data-sc-manual-password] input').val('');
          if (data.temporaryPassword) {
            showTemporaryPassword($result, data.temporaryPassword);
          }
        }
      });
    });
  }

  $(function () {
    initLocalTimes();
    bindPasswordToggle();
    bindCopyPassword();
    initList();
    initCreateForm();
    initEditForm();
    initResetForm();
  });
})(window, window.jQuery);
