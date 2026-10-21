/* Online orders: list filters and detail status actions.
 * Status buttons on Details are declarative (data-sc-post + data-sc-confirm). */
(function (window, $) {
  'use strict';

  function initList() {
    var $page = $('#onlineOrderListPage');
    if (!$page.length) return;

    $page.find('form[method="get"]').on('submit', function () {
      $page.addClass('opacity-50').attr('aria-busy', 'true');
    });
    $(window).on('pageshow', function () {
      $page.removeClass('opacity-50').removeAttr('aria-busy');
    });
  }

  $(function () {
    initList();
  });
})(window, window.jQuery);
