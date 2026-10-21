/* Product review moderation list.
 * Approve / Hide buttons are declarative (data-sc-post + data-sc-confirm in Index.cshtml). */
(function (window, $) {
  'use strict';

  function initList() {
    var $page = $('#reviewListPage');
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
