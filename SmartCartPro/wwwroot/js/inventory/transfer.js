/**
 * Store-to-store transfer document (#transferForm inside #transferPage).
 * Lines are kept in memory and posted as one JSON document; the server re-reads every SKU, checks the stock of the
 * signed-in store and moves all lines in a single database transaction (all lines move or none does).
 */
(function (window, $) {
  'use strict';

  var Ajax = window.SmartCartAjax;
  var Inv = window.ScInventory;
  if (!$ || !Ajax || !Inv) { return; }

  function init() {
    var $page = $('#transferPage');
    var $form = $('#transferForm');
    if (!$page.length || !$form.length) { return; }

    var $tbody = $form.find('#transferLines tbody');
    var $total = $form.find('[data-total-qty]');
    var $search = $form.find('#transferProductSearch');
    var $toStore = $form.find('#ToStoreId');
    var $notes = $form.find('#Notes');
    var $submit = $form.find('[type="submit"]').first();
    var postUrl = $form.attr('action');

    var lines = [];   // { idProduct, idProductVariant, name, code, unit, onHand, qty }

    function sameSku(a, b) {
      return a.idProduct === b.idProduct && (a.idProductVariant || 0) === (b.idProductVariant || 0);
    }

    function totalQty() {
      return lines.reduce(function (sum, line) { return sum + (line.qty > 0 ? line.qty : 0); }, 0);
    }

    function updateTotals() {
      $total.text(Inv.qty(totalQty()));
    }

    /** Live hint under the quantity box; the server stays the judge of what is really in stock. */
    function updateStockHint($hint, line) {
      if (line.qty > line.onHand) {
        $hint.text('Only ' + Inv.qty(line.onHand) + ' on hand').removeClass('d-none');
      } else {
        $hint.text('').addClass('d-none');
      }
    }

    function buildRow(line, index) {
      var $tr = $('<tr></tr>');

      var $productCell = $('<td></td>');
      $('<div class="fw-medium"></div>').text(line.name).appendTo($productCell);
      if (line.code) {
        $('<div class="small text-muted"></div>').text(line.code).appendTo($productCell);
      }
      $tr.append($productCell);

      $('<td class="text-end text-nowrap"></td>')
        .text(Inv.qty(line.onHand) + (line.unit ? ' ' + line.unit : ''))
        .appendTo($tr);

      var $qtyCell = $('<td class="text-end"></td>');
      var $hint = $('<div class="small text-warning-emphasis d-none"></div>');
      var $error = $('<div class="small text-danger field-validation-valid"></div>')
        .attr('data-valmsg-for', 'Lines[' + index + '].Quantity');
      var $input = $('<input type="number" class="form-control form-control-sm text-end" min="0.001" step="0.001" inputmode="decimal" />')
        .attr('name', 'Lines[' + index + '].Quantity')
        .attr('aria-label', 'Quantity of ' + line.name)
        .val(line.qty > 0 ? line.qty : '')
        .on('input', function () {
          var value = parseFloat($(this).val());
          line.qty = isFinite(value) ? value : 0;
          $(this).removeClass('is-invalid');
          $error.removeClass('field-validation-error').addClass('field-validation-valid').text('');
          updateStockHint($hint, line);
          updateTotals();
        });
      $qtyCell.append($input, $hint, $error);
      $tr.append($qtyCell);
      updateStockHint($hint, line);

      var $remove = $('<button type="button" class="btn btn-sm btn-outline-danger"></button>')
        .attr('title', 'Remove line')
        .attr('aria-label', 'Remove ' + line.name)
        .append('<i class="bi bi-trash" aria-hidden="true"></i>')
        .on('click', function () {
          var at = lines.indexOf(line);
          if (at >= 0) { lines.splice(at, 1); }
          render();
        });
      $('<td class="text-end"></td>').append($remove).appendTo($tr);

      return $tr;
    }

    function render() {
      $tbody.empty();

      if (!lines.length) {
        $('<tr></tr>')
          .append($('<td colspan="4" class="text-center text-muted py-4"></td>')
            .text('No products added yet. Search above to add items.'))
          .appendTo($tbody);
      } else {
        lines.forEach(function (line, index) {
          $tbody.append(buildRow(line, index));
        });
      }

      updateTotals();
    }

    function focusLine(line) {
      var at = lines.indexOf(line);
      if (at >= 0) {
        $tbody.find('input[type="number"]').eq(at).trigger('focus').trigger('select');
      }
    }

    function addItem(item) {
      var line = {
        idProduct: item.idProduct,
        idProductVariant: item.idProductVariant == null ? null : item.idProductVariant,
        name: Inv.skuLabel(item),
        code: item.productCode || '',
        unit: item.unitCode || '',
        onHand: Number(item.quantityOnHand) || 0,
        qty: 1
      };

      var existing = null;
      lines.forEach(function (candidate) {
        if (sameSku(candidate, line)) { existing = candidate; }
      });

      if (existing) {
        // Picking the same product again adds one more instead of a duplicate line.
        existing.qty = (existing.qty > 0 ? existing.qty : 0) + 1;
        line = existing;
      } else {
        lines.push(line);
      }

      Ajax.clearFieldErrors($form);
      render();
      focusLine(line);
    }

    function send() {
      var payload = {
        toStoreId: parseInt($toStore.val(), 10),
        notes: $.trim($notes.val()) || null,
        lines: lines.map(function (line) {
          return { fkProduct: line.idProduct, fkProductVariant: line.idProductVariant, quantity: line.qty };
        })
      };

      Ajax.setBusy($submit, true);
      Ajax.postJson(postUrl, payload, {
        $form: $form,
        silent: true,
        onSuccess: function (res) {
          var data = (res && res.data) || {};
          Ajax.flash((res && res.message) || 'Stock transfer completed.');
          // Keep the button busy while the browser navigates away.
          window.location.href = data.redirectUrl || window.location.href;
        },
        onError: function () {
          Ajax.setBusy($submit, false);
        }
      });
    }

    // Prefilled from a stock row (?productId=…): the server rendered the SKU into data-prefill-* attributes.
    var prefillProduct = parseInt($page.attr('data-prefill-product'), 10);
    if (prefillProduct > 0) {
      var prefillVariant = parseInt($page.attr('data-prefill-variant'), 10);
      lines.push({
        idProduct: prefillProduct,
        idProductVariant: prefillVariant > 0 ? prefillVariant : null,
        name: $page.attr('data-prefill-name') || '',
        code: $page.attr('data-prefill-code') || '',
        unit: $page.attr('data-prefill-unit') || '',
        onHand: parseFloat($page.attr('data-prefill-onhand')) || 0,
        qty: 1
      });
    }

    Inv.productPicker($search, { clearOnSelect: true, onSelect: addItem });

    // A barcode scanner ends with Enter: never let that submit the whole document.
    $search.on('keydown', function (e) {
      if (e.key === 'Enter') { e.preventDefault(); }
    });

    $toStore.on('change', function () {
      $form.find('[data-valmsg-for="ToStoreId"]')
        .removeClass('field-validation-error').addClass('field-validation-valid').text('');
      $toStore.removeClass('is-invalid');
    });

    $form.on('submit', function (e) {
      e.preventDefault();
      Ajax.clearFieldErrors($form);

      var errors = {};

      var storeId = parseInt($toStore.val(), 10);
      if (!(storeId > 0)) {
        errors.ToStoreId = ['Please choose the destination store.'];
      }

      if (!lines.length) {
        errors.Lines = ['Add at least one product to transfer.'];
      }

      lines.forEach(function (line, index) {
        if (!(line.qty > 0)) {
          errors['Lines[' + index + '].Quantity'] = ['Enter a quantity greater than zero.'];
        }
      });

      if (Object.keys(errors).length) {
        Ajax.applyValidationErrors($form, errors);
        Ajax.toast('Please fix the highlighted fields.', 'warning');
        return;
      }

      var storeName = $.trim($toStore.find('option:selected').text());
      var count = lines.length;

      Ajax.confirm({
        title: 'Transfer stock',
        body: 'Move ' + count + (count === 1 ? ' product' : ' products') + ' (total quantity ' + Inv.qty(totalQty()) +
          ') to ' + storeName + '? Both stores are updated straight away.',
        confirmText: 'Transfer',
        confirmClass: 'btn-primary',
        onConfirm: send
      });
    });

    render();
    if (lines.length) {
      focusLine(lines[0]);
    } else {
      $search.trigger('focus');
    }
  }

  $(init);
})(window, window.jQuery);
