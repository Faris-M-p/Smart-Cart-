/**
 * License page: save the plan/dates form and suspend/activate with a written reason.
 * Relies on window.SmartCartAjax (sc-ajax.js) and jQuery from the layout. No Bootstrap JS required.
 *
 * Hooks
 *   #licenseForm          Save (expiry >= start, MaxStores >= usage enforced server-side)
 *   #licenseReason        reason posted with Suspend / Activate
 *   #licenseSuspendBtn    confirm + POST License/Suspend
 *   #licenseActivateBtn   confirm + POST License/Activate
 */
(function (window, $) {
  'use strict';

  var ajax = window.SmartCartAjax;

  function initLicenseForm() {
    var $form = $('#licenseForm');
    if (!$form.length || !ajax) return;

    $form.on('submit', function (e) {
      e.preventDefault();
      ajax.submitForm($form);
    });
  }

  function initLicenseStatus() {
    var $page = $('#licensePage');
    if (!$page.length || !ajax) return;

    function postStatus(url, title, body, confirmText, confirmClass) {
      var reason = ($('#licenseReason').val() || '').trim();
      if (reason.length < 3) {
        ajax.toast('Enter a reason of at least 3 characters.', 'danger');
        $('#licenseReason').focus();
        return;
      }

      ajax.confirm({
        title: title,
        body: body,
        confirmText: confirmText,
        confirmClass: confirmClass,
        onConfirm: function () {
          ajax.post(url, { Reason: reason }, {
            onSuccess: function (res) {
              var msg = (res && res.message) || 'Done.';
              if (res && res.data && res.data.redirectUrl) {
                ajax.flash(msg);
                window.location.href = res.data.redirectUrl;
              } else {
                ajax.reloadWithToast(msg);
              }
            }
          });
        }
      });
    }

    $('#licenseSuspendBtn').on('click', function () {
      postStatus(
        $page.data('suspend-url'),
        'Suspend license',
        'Everyone except the platform administrator will be blocked until you activate again.',
        'Suspend',
        'btn-warning');
    });

    $('#licenseActivateBtn').on('click', function () {
      postStatus(
        $page.data('activate-url'),
        'Activate license',
        'Restore normal access. If the expiry date has passed, writes stay blocked until you extend it.',
        'Activate',
        'btn-success');
    });
  }

  $(function () {
    initLicenseForm();
    initLicenseStatus();
  });
})(window, window.jQuery);
