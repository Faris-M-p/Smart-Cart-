/** License page interactions: edit/create, coverage warning, and audited record actions. */
(function (window, $) {
  'use strict';

  var ajax = window.SmartCartAjax;

  function hasCandidateCoverageGap(periods, today) {
    if (periods.length === 0) return false;
    periods.sort(function (a, b) { return a.start.localeCompare(b.start); });
    var latestExpiry = periods[0].expiry;
    for (var i = 1; i < periods.length; i += 1) {
      var nextDay = new Date(latestExpiry + 'T00:00:00Z');
      nextDay.setUTCDate(nextDay.getUTCDate() + 1);
      var nextDayText = nextDay.toISOString().slice(0, 10);
      if (periods[i].start > nextDayText && (periods[i].candidate || periods[i - 1].candidate)) return true;
      if (periods[i].expiry > latestExpiry) latestExpiry = periods[i].expiry;
    }
    var candidate = periods.filter(function (period) { return period.candidate; })[0];
    var hasCurrentCoverage = periods.some(function (period) {
      return !period.candidate && period.start <= today && period.expiry >= today;
    });
    return !!candidate && candidate.start > today && !hasCurrentCoverage;
  }

  function initCoverageWarning($page, $form) {
    if (!$form.length) return;
    var today = $page.attr('data-today');
    var editedId = String($form.attr('data-license-id') || '0');
    var originalPeriods = [];
    $('#licenseHistoryTable [data-license-period]').each(function () {
      var $row = $(this);
      if ($row.attr('data-cancelled') === 'true' || $row.attr('data-license-id') === editedId) return;
      originalPeriods.push({ start: $row.attr('data-start'), expiry: $row.attr('data-expiry') });
    });
    function updateWarnings() {
      var start = $('#StartDate').val();
      var expiry = $('#ExpiryDate').val();
      var candidate = originalPeriods.slice();
      if (start && expiry && expiry >= start) candidate.push({ start: start, expiry: expiry, candidate: true });
      $('#licenseGapWarning').toggleClass('d-none', !hasCandidateCoverageGap(candidate, today));

      var active = $form.attr('data-original-status') === 'Active';
      var endsCurrentCoverage = start > today || expiry < today;
      $('#licenseImmediateExpiryWarning').toggleClass('d-none', !(active && endsCurrentCoverage));
    }

    $('#StartDate, #ExpiryDate').on('change input', updateWarnings);
    updateWarnings();
  }

  function initLicenseForm($page) {
    var $form = $('#licenseForm');
    if (!$form.length || !ajax) return;
    var submitting = false;
    var immediateExpiryConfirmed = false;
    initCoverageWarning($page, $form);
    $('#StartDate, #ExpiryDate').on('change input', function () {
      immediateExpiryConfirmed = false;
    });

    $form.on('submit', function (event) {
      event.preventDefault();
      if (submitting) return;

      var active = $form.attr('data-original-status') === 'Active';
      var today = $page.attr('data-today');
      var endsCurrentCoverage = $('#StartDate').val() > today || $('#ExpiryDate').val() < today;
      if (active && endsCurrentCoverage && !immediateExpiryConfirmed) {
        ajax.confirm({
          title: 'Confirm immediate expiry',
          body: 'These dates will end the currently valid license coverage immediately. Save this change?',
          confirmText: 'Save changes',
          confirmClass: 'btn-warning',
          onConfirm: function () {
            immediateExpiryConfirmed = true;
            $form.trigger('submit');
          }
        });
        return;
      }

      submitting = true;
      ajax.submitForm($form).always(function () { submitting = false; });
    });
  }

  function initLicenseActions($page) {
    if (!$page.length || !ajax) return;
    var statusSubmitting = false;

    $('#licenseHistoryTable').on('click', '[data-license-action]', function () {
      if (statusSubmitting) return;
      var $button = $(this);
      var action = $button.attr('data-license-action');
      var id = Number($button.attr('data-license-id'));
      var reason = ($('#licenseActionReason').val() || '').trim();
      if (reason.length < 3) {
        ajax.toast('Enter a reason of at least 3 characters.', 'danger');
        $('#licenseActionReason').trigger('focus');
        return;
      }

      var config = {
        suspend: {
          url: $page.attr('data-suspend-url'), title: 'Suspend license',
          body: 'Suspend this license? Access behavior remains governed by the existing license gate.',
          confirmText: 'Suspend', confirmClass: 'btn-warning'
        },
        resume: {
          url: $page.attr('data-resume-url'), title: 'Resume license',
          body: 'Resume this license without changing its dates. Its effective state will still follow the validity period.',
          confirmText: 'Resume', confirmClass: 'btn-success'
        },
        cancel: {
          url: $page.attr('data-cancel-url'), title: 'Cancel future license',
          body: 'Cancel this future license? The record remains in history and its dates become available for other licenses.',
          confirmText: 'Cancel license', confirmClass: 'btn-danger'
        }
      }[action];
      if (!config) return;

      ajax.confirm({
        title: config.title,
        body: config.body,
        confirmText: config.confirmText,
        confirmClass: config.confirmClass,
        onConfirm: function () {
          statusSubmitting = true;
          ajax.post(config.url, { IdLicense: id, Reason: reason }, {
            onSuccess: function (response) {
              var message = (response && response.message) || 'License updated.';
              if (response && response.data && response.data.redirectUrl) {
                ajax.flash(message);
                window.location.href = response.data.redirectUrl;
              } else {
                ajax.reloadWithToast(message);
              }
            }
          }).always(function () { statusSubmitting = false; });
        }
      });
    });
  }

  $(function () {
    var $page = $('#licensePage');
    initLicenseForm($page);
    initLicenseActions($page);
  });
})(window, window.jQuery);
