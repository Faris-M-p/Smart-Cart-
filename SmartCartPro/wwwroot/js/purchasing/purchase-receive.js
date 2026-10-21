/* Goods receipt (GRN): enter what arrived per purchase order line and post it as JSON.
 * The server re-reads the pending quantities and rejects over-receipt; the checks here are only a convenience. */
(function (window, $) {
  'use strict';

  var A = window.SmartCartAjax;
  var $page = $('#purchaseReceivePage');
  if (!$page.length || !A) return;

  var $form = $('#purchaseReceiveForm');
  var $submit = $form.find('[data-submit-receipt]');
  var $rows = $form.find('tr[data-id-purchase-detail]');
  var submitUrl = $page.attr('data-submit-url');

  function num(value) {
    var n = parseFloat(value);
    return isFinite(n) ? n : 0;
  }

  function round3(n) {
    return Math.round((n + Number.EPSILON) * 1000) / 1000;
  }

  function inputOf($row) {
    return $row.find('[data-field="receiveQty"]');
  }

  function showRowError($row, message) {
    var $msg = $row.find('[data-cell="rowError"]');
    var $input = inputOf($row);
    if (message) {
      $input.addClass('is-invalid');
      $msg.removeClass('d-none').text(message);
    } else {
      $input.removeClass('is-invalid');
      $msg.addClass('d-none').text('');
    }
  }

  function updateTotal() {
    var total = 0;
    $rows.each(function () {
      total += num(inputOf($(this)).val());
    });
    $form.find('[data-total-receive]').text(A.fmt.number(round3(total), 3));
  }

  /* ------------------------------------------------------------------ events */

  $form.on('input change', '[data-field="receiveQty"]', function () {
    showRowError($(this).closest('tr'), '');
    updateTotal();
  });

  $form.on('keydown', '[data-field="receiveQty"]', function (e) {
    // Enter moves to the next line instead of submitting the form.
    if (e.key !== 'Enter') return;
    e.preventDefault();
    var $inputs = $form.find('[data-field="receiveQty"]');
    var next = $inputs.eq($inputs.index(this) + 1);
    if (next.length) next.trigger('focus').trigger('select');
  });

  $form.on('click', '[data-fill-all]', function () {
    $rows.each(function () {
      var $row = $(this);
      var $input = inputOf($row);
      if (!$input.length) return;
      $input.val($row.attr('data-remaining'));
      showRowError($row, '');
    });
    updateTotal();
  });

  $form.on('click', '[data-clear-all]', function () {
    $rows.each(function () {
      var $row = $(this);
      inputOf($row).val('');
      showRowError($row, '');
    });
    updateTotal();
  });

  $form.on('submit', function (e) { e.preventDefault(); });
  $submit.on('click', submit);

  /* ------------------------------------------------------------------ submit */

  function submit() {
    A.clearFieldErrors($form);
    $rows.each(function () { showRowError($(this), ''); });

    var lines = [];
    var rowsByIndex = [];
    var hasProblem = false;

    $rows.each(function () {
      var $row = $(this);
      var $input = inputOf($row);
      if (!$input.length) return; // fully received line

      var qty = num($input.val());
      if (qty <= 0) return; // nothing arrived for this line

      var remaining = num($row.attr('data-remaining'));
      if (round3(qty) > remaining) {
        showRowError($row, 'Only ' + A.fmt.number(remaining, 3) + ' is still pending.');
        hasProblem = true;
        return;
      }

      lines.push({
        idPurchaseDetail: parseInt($row.attr('data-id-purchase-detail'), 10),
        receiveQty: qty
      });
      rowsByIndex.push($row);
    });

    var errors = {};
    if (!$form.find('[name="ReceiptDate"]').val()) errors.ReceiptDate = ['The Receipt Date field is required.'];
    if (!lines.length && !hasProblem) errors.Lines = ['Enter a receive quantity for at least one line.'];

    if (hasProblem || Object.keys(errors).length) {
      A.applyValidationErrors($form, errors);
      A.toast('Please fix the highlighted fields and try again.', 'warning');
      return;
    }

    var payload = {
      idPurchase: parseInt($page.attr('data-id-purchase'), 10) || 0,
      receiptDate: $form.find('[name="ReceiptDate"]').val(),
      notes: $.trim($form.find('[name="Notes"]').val()) || null,
      lines: lines
    };

    A.setBusy($submit, true);
    A.postJson(submitUrl, payload, {
      $form: $form,
      silent: true,
      onSuccess: function (res) {
        var redirect = res && res.data && res.data.redirectUrl;
        A.flash((res && res.message) || 'Goods receipt recorded.');
        if (redirect) {
          window.location.href = redirect;
        } else {
          A.setBusy($submit, false);
        }
      },
      onError: function (res) {
        A.setBusy($submit, false);
        showLineErrors(res, rowsByIndex);
      }
    });
  }

  /** Server errors keyed "Lines[i].ReceiveQty" refer to the i-th posted line. */
  function showLineErrors(res, rowsByIndex) {
    var errors = res && res.validationErrors;
    if (!errors) return;

    var pattern = /^Lines\[(\d+)\]\.(\w+)$/i;
    Object.keys(errors).forEach(function (key) {
      var match = pattern.exec(key);
      if (!match) return;
      var $row = rowsByIndex[parseInt(match[1], 10)];
      var messages = errors[key];
      if ($row && messages && messages.length) showRowError($row, messages[0]);
    });
  }

  updateTotal();
})(window, window.jQuery);
