/**
 * Reports pages: print the current view and mount SVG charts from data attributes.
 */
(function (window, $) {
  'use strict';

  function bindPrint($root) {
    $root.on('click', '[data-sc-print]', function (e) {
      e.preventDefault();
      window.print();
    });
  }

  $(function () {
    var $page = $('[data-sc-report]');
    if (!$page.length) return;
    bindPrint($page);
    if (window.SmartCharts && typeof window.SmartCharts.mount === 'function') {
      window.SmartCharts.mount($page[0]);
    }
  });
})(window, window.jQuery);
