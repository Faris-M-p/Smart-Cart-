/**
 * Inventory page helpers (window.ScInventory), built on window.SmartCartAjax.
 *
 *   productPicker(input, { onSelect, clearOnSelect })  product / variant typeahead; the input carries data-picker-url
 *   skuLabel(item) · skuSublabel(item) · qty(n) · localizeTimes(root)
 *
 * Also wires every [data-inv-list] page: its GET filter forms show a busy state while the page reloads.
 * All text is written with .text() / textContent, never as HTML.
 */
(function (window, $) {
  'use strict';

  var Ajax = window.SmartCartAjax;
  if (!$ || !Ajax) { return; }

  /** Quantity with up to 3 decimals in the user's number format. */
  function qty(value) {
    return Ajax.fmt.number(value, 3);
  }

  /** "Product — Variant" (variant part only when the SKU is a variant). */
  function skuLabel(item) {
    var name = item.productName || '';
    return item.variantName ? name + ' \u2014 ' + item.variantName : name;
  }

  /** Second line of a picker row: code · SKU · on hand · Inactive. */
  function skuSublabel(item) {
    var parts = [];
    if (item.productCode) { parts.push(item.productCode); }
    if (item.sku && item.sku !== item.productCode) { parts.push('SKU ' + item.sku); }
    parts.push('On hand: ' + qty(item.quantityOnHand) + (item.unitCode ? ' ' + item.unitCode : ''));
    if (item.isActive === false) { parts.push('Inactive'); }
    return parts.join(' \u00b7 ');
  }

  /** Attach the product typeahead to an input. Returns the SmartCartAjax.typeahead handle. */
  function productPicker(input, options) {
    options = options || {};
    var $input = $(input);
    var url = $input.attr('data-picker-url');

    return Ajax.typeahead($input, {
      minChars: 2,
      limit: 15,
      clearOnSelect: !!options.clearOnSelect,
      emptyText: 'No matching products',
      source: function (term) { return Ajax.getJson(url, { term: term }); },
      label: skuLabel,
      sublabel: skuSublabel,
      onSelect: options.onSelect
    });
  }

  /** Replace the text of <time data-sc-local-time datetime="…Z"> elements with the browser's local date and time. */
  function localizeTimes(root) {
    $(root || document).find('time[data-sc-local-time]').each(function () {
      var text = Ajax.fmt.dateTime(this.getAttribute('datetime'));
      if (text) { this.textContent = text; }
    });
  }

  window.ScInventory = {
    qty: qty,
    skuLabel: skuLabel,
    skuSublabel: skuSublabel,
    productPicker: productPicker,
    localizeTimes: localizeTimes
  };

  $(function () {
    $('[data-inv-list]').each(function () {
      var $page = $(this);
      $page.find('form[method="get"]').on('submit', function () {
        $page.addClass('opacity-50').attr('aria-busy', 'true');
      });
    });
  });

  // Coming back with the browser's Back button must not leave a list greyed out.
  $(window).on('pageshow', function (e) {
    if (e.originalEvent && e.originalEvent.persisted) {
      $('[data-inv-list]').removeClass('opacity-50').removeAttr('aria-busy');
    }
  });
})(window, window.jQuery);
