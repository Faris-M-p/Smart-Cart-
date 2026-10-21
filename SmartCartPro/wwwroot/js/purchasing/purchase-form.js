/* Purchase order create / edit: dynamic line grid, product typeahead and a live totals preview.
 * The totals shown here are only a preview — the server recomputes tax and every amount from master data when saving.
 * The document is posted as JSON through SmartCartAjax.postJson (controller binds it with [FromBody]). */
(function (window, $) {
  'use strict';

  var A = window.SmartCartAjax;
  var $page = $('#purchaseFormPage');
  if (!$page.length || !A) return;

  var $form = $('#purchaseForm');
  var $tableWrap = $page.find('.table-responsive');
  var $body = $page.find('[data-lines-body]');
  var $empty = $page.find('[data-lines-empty]');
  var $picker = $('#productPicker');
  var $spinner = $page.find('[data-picker-spinner]');
  var $count = $page.find('[data-line-count]');

  var mode = $page.attr('data-mode') || 'create';
  var submitUrl = $page.attr('data-submit-url');
  var lookupUrl = $page.attr('data-lookup-url');

  var lines = [];
  var seq = 0;

  /* ------------------------------------------------------------------ helpers */

  function num(value) {
    var n = parseFloat(value);
    return isFinite(n) ? n : 0;
  }

  function round(n, decimals) {
    var factor = Math.pow(10, decimals);
    return Math.round((n + Number.EPSILON) * factor) / factor;
  }

  /** Same arithmetic as PurchaseService: tax-exclusive, tax on the discounted amount, 2-decimal rounding. */
  function calc(line) {
    var qty = round(num(line.quantity), 3);
    var cost = round(num(line.unitCost), 2);
    var discount = round(num(line.discountAmount), 2);
    var gross = round(qty * cost, 2);
    var net = gross - discount;
    var tax = round(net * num(line.taxRate) / 100, 2);
    return { gross: gross, discount: discount, tax: tax, total: net + tax };
  }

  function rowProblem(line, c) {
    if (num(line.quantity) <= 0) return { field: 'quantity', message: 'The quantity must be greater than zero.' };
    if (num(line.unitCost) < 0) return { field: 'unitCost', message: 'The unit cost cannot be negative.' };
    if (num(line.discountAmount) < 0) return { field: 'discountAmount', message: 'The discount cannot be negative.' };
    if (c.discount > c.gross) return { field: 'discountAmount', message: 'The discount cannot be more than the line amount.' };
    return null;
  }

  function rowFor(line) {
    return $body.children('tr').filter(function () {
      return String($(this).attr('data-key')) === String(line.key);
    });
  }

  function lineFor($tr) {
    var key = String($tr.attr('data-key'));
    for (var i = 0; i < lines.length; i++) {
      if (String(lines[i].key) === key) return lines[i];
    }
    return null;
  }

  function showRowProblem($tr, problem) {
    $tr.find('[data-field]').removeClass('is-invalid');
    var $msg = $tr.find('[data-cell="rowError"]');
    if (!problem) {
      $msg.addClass('d-none').text('');
      return;
    }
    $tr.find('[data-field="' + problem.field + '"]').addClass('is-invalid');
    $msg.removeClass('d-none').text(problem.message);
  }

  /* ------------------------------------------------------------------ rendering */

  function numberCell(line, field, label, step, min) {
    var $td = $('<td></td>');
    $('<input type="number" class="form-control form-control-sm text-end" />')
      .attr({
        'data-field': field,
        step: step,
        min: min,
        inputmode: 'decimal',
        'aria-label': label + ' for ' + line.productName
      })
      .val(line[field])
      .appendTo($td);
    return $td;
  }

  function buildRow(line) {
    var $tr = $('<tr data-line></tr>').attr('data-key', line.key);

    $('<td class="text-muted" data-cell="index"></td>').appendTo($tr);

    var $product = $('<td></td>').appendTo($tr);
    $('<div class="fw-medium" data-cell="name"></div>').text(line.productName).appendTo($product);
    var sub = [line.variantName, line.productCode].filter(Boolean).join(' · ');
    $('<div class="small text-muted" data-cell="sub"></div>').text(sub).appendTo($product);
    $('<div class="small text-danger d-none" data-cell="rowError"></div>').appendTo($product);

    $tr.append(numberCell(line, 'quantity', 'Quantity', '0.001', '0'));
    $tr.append(numberCell(line, 'unitCost', 'Unit cost', '0.01', '0'));
    $tr.append(numberCell(line, 'discountAmount', 'Discount', '0.01', '0'));

    $('<td class="text-end" data-cell="taxRate"></td>')
      .text(A.fmt.number(num(line.taxRate), 2) + '%')
      .appendTo($tr);
    $('<td class="text-end" data-cell="tax"></td>').appendTo($tr);
    $('<td class="text-end fw-medium" data-cell="lineTotal"></td>').appendTo($tr);

    var $remove = $('<button type="button" class="btn btn-sm btn-outline-danger" data-action="remove"></button>')
      .attr({ 'aria-label': 'Remove ' + line.productName, title: 'Remove line' })
      .append('<i class="bi bi-trash" aria-hidden="true"></i>');
    $('<td class="text-end"></td>').append($remove).appendTo($tr);

    return $tr;
  }

  function refreshRow(line) {
    var $tr = rowFor(line);
    var c = calc(line);
    $tr.find('[data-cell="tax"]').text(A.fmt.money(c.tax));
    $tr.find('[data-cell="lineTotal"]').text(A.fmt.money(c.total));
    showRowProblem($tr, rowProblem(line, c));
  }

  function renumber() {
    $body.children('tr').each(function (i) {
      $(this).find('[data-cell="index"]').text(i + 1);
    });
    $count.text(lines.length);
  }

  function toggleEmpty() {
    $empty.toggleClass('d-none', lines.length > 0);
    $tableWrap.toggleClass('d-none', lines.length === 0);
  }

  function renderTotals() {
    var subtotal = 0;
    var discount = 0;
    var tax = 0;
    lines.forEach(function (line) {
      var c = calc(line);
      subtotal += c.gross;
      discount += c.discount;
      tax += c.tax;
    });

    function set(name, value) {
      $page.find('[data-total="' + name + '"]').text(A.fmt.money(round(value, 2)));
    }
    set('subtotal', subtotal);
    set('discount', discount);
    set('tax', tax);
    set('total', subtotal - discount + tax);
  }

  function refreshAll() {
    renumber();
    toggleEmpty();
    renderTotals();
  }

  /* ------------------------------------------------------------------ lines */

  function addLine(data) {
    var line = {
      key: ++seq,
      fkProduct: data.fkProduct,
      fkProductVariant: data.fkProductVariant || null,
      productName: data.productName || '',
      productCode: data.productCode || '',
      variantName: data.variantName || '',
      quantity: data.quantity != null ? data.quantity : 1,
      unitCost: data.unitCost != null ? data.unitCost : 0,
      discountAmount: data.discountAmount != null ? data.discountAmount : 0,
      taxRate: num(data.taxRate)
    };
    lines.push(line);

    var $tr = buildRow(line);
    $body.append($tr);
    refreshRow(line);
    return { line: line, $tr: $tr };
  }

  /** Picking a product that is already on the order adds 1 to its quantity (the server rejects duplicate lines). */
  function addOrIncrement(product) {
    var variant = product.fkProductVariant || 0;
    var existing = null;
    for (var i = 0; i < lines.length; i++) {
      if (lines[i].fkProduct === product.fkProduct && (lines[i].fkProductVariant || 0) === variant) {
        existing = lines[i];
        break;
      }
    }

    var $tr;
    if (existing) {
      existing.quantity = round(num(existing.quantity) + 1, 3);
      $tr = rowFor(existing);
      $tr.find('[data-field="quantity"]').val(existing.quantity);
      refreshRow(existing);
      $tr.addClass('table-warning');
      window.setTimeout(function () { $tr.removeClass('table-warning'); }, 900);
    } else {
      $tr = addLine({
        fkProduct: product.fkProduct,
        fkProductVariant: product.fkProductVariant,
        productName: product.productName,
        productCode: product.productCode,
        variantName: product.variantName,
        quantity: 1,
        unitCost: product.unitCost,
        discountAmount: 0,
        taxRate: product.taxRate
      }).$tr;
    }

    refreshAll();
    $form.find('[data-valmsg-for="Lines"]').text('');
    $tr.find('[data-field="quantity"]').trigger('focus').trigger('select');
  }

  function removeLine($tr) {
    var line = lineFor($tr);
    if (!line) return;
    lines.splice(lines.indexOf(line), 1);
    $tr.remove();
    refreshAll();
    if (!lines.length) $picker.trigger('focus');
  }

  /* ------------------------------------------------------------------ events */

  $body.on('input change', '[data-field]', function () {
    var $input = $(this);
    var line = lineFor($input.closest('tr'));
    if (!line) return;
    line[$input.attr('data-field')] = $input.val();
    refreshRow(line);
    renderTotals();
  });

  $body.on('keydown', '[data-field]', function (e) {
    // Enter moves on to the next product search instead of submitting anything.
    if (e.key === 'Enter') {
      e.preventDefault();
      $picker.trigger('focus');
    }
  });

  $body.on('click', '[data-action="remove"]', function () {
    removeLine($(this).closest('tr'));
  });

  $form.on('submit', function (e) { e.preventDefault(); });

  $form.on('click', '[data-save]', function () {
    submit($(this).attr('data-save') === 'draft', $(this));
  });

  /* ------------------------------------------------------------------ product picker */

  function productSublabel(p) {
    var parts = [];
    if (p.productCode) parts.push(p.productCode);
    if (p.sku) parts.push('SKU ' + p.sku);
    if (p.barcode) parts.push(p.barcode);
    parts.push('Cost ' + A.fmt.money(p.unitCost));
    if (num(p.taxRate) > 0) parts.push('Tax ' + A.fmt.number(p.taxRate, 2) + '%');
    return parts.join(' · ');
  }

  A.typeahead($picker, {
    minChars: 2,
    limit: 25,
    clearOnSelect: true,
    emptyText: 'No active products match your search.',
    source: function (term) {
      $spinner.removeClass('d-none');
      return A.getJson(lookupUrl, { term: term }).always(function () {
        $spinner.addClass('d-none');
      });
    },
    label: function (p) {
      return p.variantName ? p.productName + ' — ' + p.variantName : p.productName;
    },
    sublabel: productSublabel,
    onSelect: addOrIncrement
  });

  /* ------------------------------------------------------------------ submit */

  function setSaving(saving, $clicked) {
    var $buttons = $form.find('[data-save]');
    if (saving) {
      $buttons.prop('disabled', true);
      A.setBusy($clicked, true);
    } else {
      A.setBusy($clicked, false);
      $buttons.prop('disabled', false);
    }
  }

  /** Quick checks that mirror the server messages; the server stays the authority. */
  function clientErrors() {
    var errors = {};
    var supplier = parseInt($form.find('[name="FkSupplier"]').val(), 10) || 0;
    if (supplier <= 0) errors.FkSupplier = ['Please select a Supplier.'];
    if (!$form.find('[name="PurchaseDate"]').val()) errors.PurchaseDate = ['The Purchase Date field is required.'];
    if (!lines.length) errors.Lines = ['At least one purchase line is required.'];
    return errors;
  }

  function checkRows() {
    var bad = false;
    lines.forEach(function (line) {
      var problem = rowProblem(line, calc(line));
      showRowProblem(rowFor(line), problem);
      if (problem) bad = true;
    });
    return bad;
  }

  /** Server errors keyed "Lines[i].Field" point at the i-th row of the posted array (same order as the grid). */
  function showLineErrors(res) {
    var errors = res && res.validationErrors;
    if (!errors) return;

    var pattern = /^Lines\[(\d+)\]\.(\w+)$/i;
    var fields = { quantity: 'quantity', unitcost: 'unitCost', discountamount: 'discountAmount' };

    Object.keys(errors).forEach(function (key) {
      var match = pattern.exec(key);
      if (!match) return;
      var line = lines[parseInt(match[1], 10)];
      var messages = errors[key];
      if (!line || !messages || !messages.length) return;

      var $tr = rowFor(line);
      var target = fields[match[2].toLowerCase()];
      if (target) $tr.find('[data-field="' + target + '"]').addClass('is-invalid');
      $tr.find('[data-cell="rowError"]').removeClass('d-none').text(messages[0]);
    });
  }

  function submit(saveAsDraft, $clicked) {
    A.clearFieldErrors($form);

    var errors = clientErrors();
    var badRows = checkRows();
    if (Object.keys(errors).length || badRows) {
      A.applyValidationErrors($form, errors);
      A.toast('Please fix the highlighted fields and try again.', 'warning');
      return;
    }

    var payload = {
      fkSupplier: parseInt($form.find('[name="FkSupplier"]').val(), 10) || 0,
      purchaseDate: $form.find('[name="PurchaseDate"]').val(),
      invoiceNumber: $.trim($form.find('[name="InvoiceNumber"]').val()) || null,
      notes: $.trim($form.find('[name="Notes"]').val()) || null,
      saveAsDraft: saveAsDraft,
      lines: lines.map(function (line) {
        return {
          fkProduct: line.fkProduct,
          fkProductVariant: line.fkProductVariant,
          quantity: num(line.quantity),
          unitCost: num(line.unitCost),
          discountAmount: num(line.discountAmount)
        };
      })
    };
    if (mode === 'edit') payload.idPurchase = parseInt($page.attr('data-id-purchase'), 10) || 0;

    setSaving(true, $clicked);
    A.postJson(submitUrl, payload, {
      $form: $form,
      silent: true,
      onSuccess: function (res) {
        // postJson never redirects by itself: show the message on the next page and open the saved order.
        var redirect = res && res.data && res.data.redirectUrl;
        A.flash((res && res.message) || 'Saved.');
        if (redirect) {
          window.location.href = redirect;
        } else {
          setSaving(false, $clicked);
        }
      },
      onError: function (res) {
        setSaving(false, $clicked);
        showLineErrors(res);
      }
    });
  }

  /* ------------------------------------------------------------------ init */

  (function loadInitialLines() {
    var initial = [];
    try {
      initial = JSON.parse($page.attr('data-lines') || '[]');
    } catch (e) {
      initial = [];
    }
    if (!$.isArray(initial)) initial = [];

    initial.forEach(addLine);
    refreshAll();
  })();
})(window, window.jQuery);
