/* Customers: create / edit form and list filters.
 * Activate / deactivate buttons on the list are declarative (data-sc-post + data-sc-confirm in Index.cshtml). */
(function (window, $) {
  'use strict';

  var A = window.SmartCartAjax;

  function initCustomerForm() {
    var $form = $('#customerForm');
    if (!$form.length || !A) return;

    $form.on('submit', function (e) {
      e.preventDefault();
      A.submitForm($form);
    });
  }

  function initCustomerList() {
    var $page = $('#customerListPage');
    if (!$page.length) return;

    $page.find('form[method="get"]').on('submit', function () {
      $page.addClass('opacity-50').attr('aria-busy', 'true');
    });
    $(window).on('pageshow', function () {
      $page.removeClass('opacity-50').removeAttr('aria-busy');
    });
  }

  $(function () {
    initCustomerForm();
    initCustomerList();
  });
})(window, window.jQuery);
