/**
 * Variant form (/Product/CreateVariant, /Product/EditVariant).
 * Relies on window.SmartCartAjax (sc-ajax.js). The list screens need no page script (declarative data-sc-* hooks).
 */
(function (window, $) {
  'use strict';

  var Ajax = window.SmartCartAjax;

  function initVariantForm() {
    var $form = $('#variantForm');
    if (!$form.length || !Ajax) return;

    var basePrice = parseFloat($form.attr('data-base-price')) || 0;
    var $price = $form.find('[name="AdditionalPrice"]');
    var $preview = $form.find('[data-variant-price-preview]');

    // Live preview of the variant's selling price (display only; the server always recalculates it).
    function updatePreview() {
      if (!$preview.length) return;
      var extra = parseFloat($.trim($price.val()).replace(/,/g, ''));
      $preview.text(Ajax.fmt.money(basePrice + (isFinite(extra) ? extra : 0)));
    }
    $price.on('input blur', updatePreview);

    // Tidy plain numbers to two decimals; anything else is left for the server to validate.
    $form.on('blur', '[data-decimal]', function () {
      var $el = $(this);
      var value = $.trim($el.val());
      if (!/^\d+(\.\d+)?$/.test(value)) return;
      $el.val(parseFloat(value).toFixed(parseInt($el.attr('data-decimal'), 10) || 2));
      updatePreview();
    });

    // Barcode scanners end with Enter; that must not submit the form half-filled.
    $form.on('keydown', '[data-no-enter-submit]', function (e) {
      if (e.key === 'Enter') e.preventDefault();
    });

    $form.on('submit', function (e) {
      e.preventDefault();
      Ajax.submitForm($form);
    });
  }

  $(function () {
    initVariantForm();
  });
})(window, window.jQuery);
