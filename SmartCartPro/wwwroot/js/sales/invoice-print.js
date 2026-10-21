/* Standalone invoice print sheet (no layout, no jQuery). */
(function () {
  'use strict';

  document.addEventListener('DOMContentLoaded', function () {
    var money = new Intl.NumberFormat(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    Array.prototype.forEach.call(document.querySelectorAll('[data-sc-money]'), function (el) {
      var value = Number(el.getAttribute('data-sc-money'));
      if (isFinite(value)) el.textContent = money.format(value);
    });

    var printed = document.querySelector('[data-printed-at]');
    if (printed) printed.textContent = new Date().toLocaleString();

    var printButton = document.querySelector('[data-print]');
    if (printButton) printButton.addEventListener('click', function () { window.print(); });

    var closeButton = document.querySelector('[data-close]');
    if (closeButton) closeButton.addEventListener('click', function () { window.close(); });

    if (document.body.getAttribute('data-autoprint') === '1') {
      window.setTimeout(function () { window.print(); }, 300);
    }
  });
})();
