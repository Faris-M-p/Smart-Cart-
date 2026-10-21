/* Purchases: list filters, details formatting and the cancel form.
 * Create/edit, receive and print have their own scripts (purchase-form.js, purchase-receive.js, purchase-print.js). */
(function (window, $) {
  'use strict';

  var A = window.SmartCartAjax;

  /** Show amounts and UTC timestamps in the browser's locale / time zone (the server renders a readable fallback). */
  function localize() {
    if (!A || !A.fmt) return;

    $('[data-sc-money]').each(function () {
      var raw = this.getAttribute('data-sc-money');
      if (raw !== null && raw !== '') this.textContent = A.fmt.money(raw);
    });

    $('time[data-sc-local-datetime]').each(function () {
      var text = A.fmt.dateTime(this.getAttribute('datetime'));
      if (text) this.textContent = text;
    });
  }

  function initList() {
    var $page = $('#purchaseListPage');
    if (!$page.length) return;

    // Visual feedback while the filter form reloads the page; reset when the page comes back from the back/forward cache.
    $page.find('form[method="get"]').on('submit', function () {
      $page.addClass('opacity-50').attr('aria-busy', 'true');
    });
    $(window).on('pageshow', function () {
      $page.removeClass('opacity-50').removeAttr('aria-busy');
    });
  }

  function initCancelForm() {
    var $form = $('#purchaseCancelForm');
    if (!$form.length || !A) return;

    $form.on('submit', function (e) {
      e.preventDefault();
      A.submitForm($form);
    });
  }

  $(function () {
    localize();
    initList();
    initCancelForm();
  });
})(window, window.jQuery);
