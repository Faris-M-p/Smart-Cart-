/**
 * Store list and create/edit forms.
 * Relies on window.SmartCartAjax (sc-ajax.js) and jQuery from the layout. No Bootstrap JS required.
 * List activate/deactivate uses declarative data-sc-post on Stores/Index.
 *
 * Hooks
 *   #storeForm       create / edit form
 *   #storeListPage   list GET filter; dimmed while the query reloads
 */
(function (window, $) {
  'use strict';

  function initStoreForm() {
    var $form = $('#storeForm');
    if (!$form.length || !window.SmartCartAjax) return;

    $form.on('submit', function (e) {
      e.preventDefault();
      window.SmartCartAjax.submitForm($form);
    });
  }

  function initStoreList() {
    var $page = $('#storeListPage');
    if (!$page.length) return;

    $page.find('form[method="get"]').on('submit', function () {
      $page.addClass('opacity-50').attr('aria-busy', 'true');
    });

    $(window).on('pageshow', function () {
      $page.removeClass('opacity-50').removeAttr('aria-busy');
    });
  }

  $(function () {
    initStoreForm();
    initStoreList();
  });
})(window, window.jQuery);
