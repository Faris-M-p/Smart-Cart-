/**
 * Opening stock + Stock adjustment forms (#stockEntryForm, data-mode="opening" | "adjust").
 * Product comes from the typeahead (one row per sellable SKU, so a variant is picked together with its product);
 * the server re-reads the SKU and the on-hand quantity, nothing sent from here is trusted.
 */
(function (window, $) {
  'use strict';

  var Ajax = window.SmartCartAjax;
  var Inv = window.ScInventory;
  if (!$ || !Ajax || !Inv) { return; }

  function init() {
    var $form = $('#stockEntryForm');
    if (!$form.length) { return; }

    var mode = $form.attr('data-mode') || 'opening';
    var $search = $form.find('#productSearch');
    var $product = $form.find('input[name="FkProduct"]');
    var $variant = $form.find('input[name="FkProductVariant"]');
    var $qty = $form.find('input[name="Quantity"]');
    var $card = $form.find('[data-sku-card]');
    var $unitAddon = $form.find('[data-sku-unit-addon]');
    var $preview = $form.find('[data-adjust-preview]');
    var $hint = $form.find('[data-opening-hint]');

    var selected = null;      // { onHand, unit } once a SKU is chosen
    var selectedLabel = '';   // text the picker wrote into the search box

    function direction() {
      return $form.find('input[name="Direction"]:checked').val();
    }

    function clearError(name) {
      $form.find('[data-valmsg-for="' + name + '"]')
        .removeClass('field-validation-error')
        .addClass('field-validation-valid')
        .text('');
      if (name === 'FkProduct') { $search.removeClass('is-invalid'); }
    }

    function updateHint() {
      if (mode !== 'opening') { return; }
      $hint.toggleClass('d-none', !(selected && selected.onHand > 0));
    }

    function updatePreview() {
      if (mode !== 'adjust') { return; }

      var q = parseFloat($qty.val());
      if (!selected || !isFinite(q) || q <= 0) {
        $preview.addClass('d-none').text('');
        return;
      }

      var after = direction() === 'remove' ? selected.onHand - q : selected.onHand + q;
      var text = 'On hand now ' + Inv.qty(selected.onHand) + ' \u2192 ' + Inv.qty(after) + ' after this adjustment';
      if (after < 0) { text += ' (not enough stock)'; }

      $preview
        .removeClass('d-none text-danger text-success')
        .addClass(after < 0 ? 'text-danger' : 'text-success')
        .text(text);
    }

    function clearSelection() {
      selected = null;
      selectedLabel = '';
      $product.val('');
      $variant.val('');
      $card.addClass('d-none');
      $unitAddon.text('units');
      updateHint();
      updatePreview();
    }

    function chooseSku(item) {
      var label = Inv.skuLabel(item);
      var codeParts = [];
      if (item.productCode) { codeParts.push(item.productCode); }
      if (item.sku && item.sku !== item.productCode) { codeParts.push('SKU ' + item.sku); }

      selected = { onHand: Number(item.quantityOnHand) || 0, unit: item.unitCode || '' };
      selectedLabel = label;

      $product.val(item.idProduct);
      $variant.val(item.idProductVariant == null ? '' : item.idProductVariant);

      $card.removeClass('d-none').attr('data-on-hand', selected.onHand);
      $card.find('[data-sku-name]').text(label);
      $card.find('[data-sku-code]').text(codeParts.join(' \u00b7 '));
      $card.find('[data-sku-onhand]').text(Inv.qty(selected.onHand));
      $card.find('[data-sku-unit]').text(selected.unit);
      $unitAddon.text(selected.unit || 'units');

      clearError('FkProduct');
      updateHint();
      updatePreview();
      $qty.trigger('focus');
    }

    // Opened from a stock row (?productId=…): the server already filled the SKU in.
    if ($product.val()) {
      selected = {
        onHand: parseFloat($card.attr('data-on-hand')) || 0,
        unit: $.trim($card.find('[data-sku-unit]').text())
      };
      selectedLabel = $search.val();
    }

    Inv.productPicker($search, { onSelect: chooseSku });

    // Typing over a chosen product un-selects it, so a stale id can never be submitted.
    $search.on('input', function () {
      if (selected && $.trim($search.val()) !== selectedLabel) { clearSelection(); }
    });

    // A barcode scanner ends with Enter: until a product is chosen that must not submit the form.
    $search.on('keydown', function (e) {
      if (e.key === 'Enter' && !$product.val()) { e.preventDefault(); }
    });

    $form.on('input change', 'input[name="Quantity"], input[name="Direction"]', function () {
      clearError('Quantity');
      $qty.removeClass('is-invalid');
      updatePreview();
    });
    $form.on('change', '[name="Reason"]', function () {
      clearError('Reason');
      $(this).removeClass('is-invalid');
    });

    $form.on('submit', function (e) {
      e.preventDefault();
      Ajax.clearFieldErrors($form);

      var errors = {};

      if (!$product.val()) {
        errors.FkProduct = ['Please select a product from the search list.'];
      }

      var q = parseFloat($qty.val());
      if (!isFinite(q) || q <= 0) {
        errors.Quantity = [mode === 'opening' ? 'Enter the opening quantity.' : 'Enter a quantity greater than zero.'];
      } else if (mode === 'adjust' && selected && direction() === 'remove' && q > selected.onHand) {
        errors.Quantity = ['You cannot remove more than the ' + Inv.qty(selected.onHand) + ' on hand.'];
      }

      if (mode === 'adjust' && !$form.find('[name="Reason"]').val()) {
        errors.Reason = ['Please choose a reason.'];
      }

      if (Object.keys(errors).length) {
        Ajax.applyValidationErrors($form, errors);
        if (errors.FkProduct) {
          $search.addClass('is-invalid').trigger('focus');
        } else {
          $form.find('.is-invalid').first().trigger('focus');
        }
        return;
      }

      Ajax.submitForm($form, {
        onError: function (res) {
          if (res && res.validationErrors && res.validationErrors.FkProduct) {
            $search.addClass('is-invalid');
          }
        }
      });
    });

    updateHint();
    updatePreview();
    ($product.val() ? $qty : $search).trigger('focus');
  }

  $(init);
})(window, window.jQuery);
