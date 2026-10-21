/* Suppliers: create / edit form and list filters.
 * Activate / deactivate buttons on the list are declarative (data-sc-post + data-sc-confirm in sc-ajax.js). */
(function (window, $) {
  'use strict';

  var A = window.SmartCartAjax;

  function initSupplierForm() {
    var $form = $('#supplierForm');
    if (!$form.length || !A) return;

    $form.on('submit', function (e) {
      e.preventDefault();
      A.submitForm($form);
    });
  }

  function initSupplierList() {
    var $page = $('#supplierListPage');
    if (!$page.length) return;

    // Visual feedback while the filter form reloads the page; reset when the page comes back from the back/forward cache.
    $page.find('form[method="get"]').on('submit', function () {
      $page.addClass('opacity-50').attr('aria-busy', 'true');
    });
    $(window).on('pageshow', function () {
      $page.removeClass('opacity-50').removeAttr('aria-busy');
    });
  }

  $(function () {
    initSupplierForm();
    initSupplierList();
  });
})(window, window.jQuery);
