/* Invoice list formatting and cancel modal on details. */
(function (window, $) {
  'use strict';

  var A = window.SmartCartAjax;
  if (!A) return;

  document.querySelectorAll('[data-sc-money]').forEach(function (el) {
    var value = Number(el.getAttribute('data-sc-money'));
    if (isFinite(value)) el.textContent = A.fmt.money(value);
  });

  var $page = $('#invoiceDetailsPage');
  if (!$page.length) return;

  var $modal = $('#invoiceCancelModal');
  var $form = $('#invoiceCancelForm');
  if (!$modal.length || !$form.length) return;

  function show() { $modal.removeClass('d-none').removeAttr('hidden'); }
  function hide() { $modal.addClass('d-none').attr('hidden', 'hidden'); }

  $('#invoiceCancelOpen').on('click', show);
  $('#invoiceCancelClose').on('click', hide);

  $form.on('submit', function (e) {
    e.preventDefault();
    var reason = $.trim($('#cancelReason').val());
    if (reason.length < 3) {
      A.toast('Please enter a reason of at least 3 characters.', 'warning');
      return;
    }
    A.post($page.attr('data-cancel-url'), {
      id: $page.attr('data-sale-id'),
      reason: reason
    }, { $form: $form });
  });
})(window, window.jQuery);
