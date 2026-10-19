/**
 * Movement history filters (#historyPage): product picker, local-day offset and local time display.
 * The product filter matches every variant of the chosen product.
 */
(function (window, $) {
  'use strict';

  var Ajax = window.SmartCartAjax;
  var Inv = window.ScInventory;
  if (!$ || !Ajax || !Inv) { return; }

  $(function () {
    var $page = $('#historyPage');
    if (!$page.length) { return; }

    var $search = $('#historyProduct');
    var $productId = $('#historyProductId');
    var chosenText = $.trim($search.val());

    // Day filters are the user's local days; the server turns them into UTC with this offset (minutes, UTC minus local).
    $('#tzOffset').val(new Date().getTimezoneOffset());

    Inv.localizeTimes($page);

    Inv.productPicker($search, {
      onSelect: function (item) {
        $productId.val(item.idProduct);
        // The filter is by product, so show the product name rather than "Product — Variant".
        $search.val(item.productName || '');
        chosenText = $.trim($search.val());
      }
    });

    // Typing over the chosen product drops the filter until a new product is picked.
    $search.on('input', function () {
      var text = $.trim($search.val());
      if (text === '' || text !== chosenText) { $productId.val(''); }
    });

    // A barcode scanner ends with Enter: let the picker use it, never submit a half-typed filter.
    $search.on('keydown', function (e) {
      if (e.key === 'Enter') { e.preventDefault(); }
    });

    $page.on('click', '[data-clear-product]', function () {
      $search.val('');
      $productId.val('');
      chosenText = '';
      $search.trigger('focus');
    });
  });
})(window, window.jQuery);
