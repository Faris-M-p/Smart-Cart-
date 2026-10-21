/* Cash session open/close forms and day-close print. */
(function (window, $) {
  'use strict';

  var A = window.SmartCartAjax;
  if (!A) return;

  var $open = $('#openSessionForm');
  if ($open.length) {
    $open.on('submit', function (e) {
      e.preventDefault();
      A.submitForm($open);
    });
  }

  var $close = $('#closeSessionForm');
  if ($close.length) {
    $close.on('submit', function (e) {
      e.preventDefault();
      A.confirm({
        title: 'Close cash session',
        body: 'Counted cash will be saved and the variance stored. Continue?',
        confirmText: 'Close session',
        confirmClass: 'btn-danger',
        onConfirm: function () { A.submitForm($close); }
      });
    });
  }

  var $print = $('#dayClosePrint');
  if ($print.length) {
    $print.on('click', function () { window.print(); });
    document.querySelectorAll('[data-sc-money]').forEach(function (el) {
      var value = Number(el.getAttribute('data-sc-money'));
      if (isFinite(value)) el.textContent = A.fmt.money(value);
    });
  }
})(window, window.jQuery);
