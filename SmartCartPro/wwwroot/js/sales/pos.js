/* POS cashier screen: search, barcode, customer, complete sale, sessionStorage draft. */
(function (window, $) {
  'use strict';

  var A = window.SmartCartAjax;
  var Cart = window.SmartCartPosCart;
  var $page = $('#posPage');
  if (!$page.length || !A || !Cart) {
    var $open = $('#posOpenSessionForm');
    if ($open.length && A) {
      $open.on('submit', function (e) {
        e.preventDefault();
        A.submitForm($open);
      });
    }
    return;
  }

  var storeId = $page.attr('data-store-id');
  var draftKey = 'sc.pos.draft.' + storeId;
  var searchUrl = $page.attr('data-search-url');
  var barcodeUrl = $page.attr('data-barcode-url');
  var createUrl = $page.attr('data-create-url');
  var customerSearchUrl = $page.attr('data-customer-search-url');
  var customerCreateUrl = $page.attr('data-customer-create-url');
  var canQuickCreate = $page.attr('data-can-quick-create') === 'true';

  var lines = [];
  var payments = [{ method: 'cash', amount: '', referenceNo: '' }];
  var customer = null;
  var clientRef = newClientRef();
  var completing = false;

  var $search = $('#posSearch');
  var $results = $('#posResults');
  var $cartBody = $('#posCartBody');
  var $cartEmpty = $('#posCartEmpty');
  var $payments = $('#posPayments');

  function newClientRef() {
    if (window.crypto && crypto.randomUUID) return crypto.randomUUID();
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
      var r = Math.random() * 16 | 0;
      return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
  }

  function money(n) { return A.fmt.money(n); }

  function saveDraft() {
    try {
      window.sessionStorage.setItem(draftKey, JSON.stringify({
        lines: lines,
        payments: payments,
        customer: customer,
        notes: $('#posNotes').val(),
        clientRef: clientRef
      }));
    } catch (e) { /* storage full / blocked */ }
  }

  function loadDraft() {
    try {
      var raw = window.sessionStorage.getItem(draftKey);
      if (!raw) return;
      var d = JSON.parse(raw);
      if (!d) return;
      lines = d.lines || [];
      payments = d.payments && d.payments.length ? d.payments : payments;
      customer = d.customer || null;
      if (d.notes) $('#posNotes').val(d.notes);
      if (d.clientRef) clientRef = d.clientRef;
    } catch (e) { /* ignore */ }
  }

  function clearDraft() {
    try { window.sessionStorage.removeItem(draftKey); } catch (e) { /* ignore */ }
  }

  function resetBill() {
    lines = [];
    payments = [{ method: 'cash', amount: '', referenceNo: '' }];
    customer = null;
    clientRef = newClientRef();
    $('#posNotes').val('');
    $('#posCustomer').val('');
    $('#posCustomerLabel').text('Walk-in');
    clearDraft();
    render();
    $search.trigger('focus');
  }

  function showModal($el) {
    $el.removeClass('d-none').removeAttr('hidden');
  }

  function hideModal($el) {
    $el.addClass('d-none').attr('hidden', 'hidden');
  }

  function renderResults(items) {
    $results.empty();
    if (!items || !items.length) {
      $results.append($('<div class="text-muted small p-3"></div>').text('No matching products.'));
      return;
    }
    items.forEach(function (item) {
      var $btn = $('<button type="button" class="pos-result" role="option"></button>');
      var title = item.name + (item.variantName ? ' · ' + item.variantName : '');
      $('<div class="fw-medium"></div>').text(title).appendTo($btn);
      var sub = [];
      if (item.sku) sub.push(item.sku);
      if (item.barcode) sub.push(item.barcode);
      sub.push(money(item.sellingPrice));
      if (item.stockOnHand != null) sub.push('Stock ' + A.fmt.number(item.stockOnHand));
      $('<div class="small text-muted"></div>').text(sub.join(' · ')).appendTo($btn);
      $btn.on('click', function () { addItem(item); });
      $results.append($btn);
    });
  }

  var search = A.debounce(function () {
    var term = $.trim($search.val());
    if (term.length < 1) {
      $results.empty();
      return;
    }
    A.getJson(searchUrl, { term: term, take: 20 }).done(function (res) {
      renderResults(res && res.data ? res.data : []);
    });
  }, 200);

  function addItem(item, qty) {
    Cart.addOrBump(lines, item, qty);
    render();
    saveDraft();
    $search.val('').trigger('focus');
    $results.empty();
  }

  function scanBarcode() {
    var barcode = $.trim($search.val());
    if (!barcode) return;
    A.getJson(barcodeUrl, { barcode: barcode }).done(function (res) {
      if (res && res.data) addItem(res.data);
    });
  }

  function renderCart() {
    $cartBody.empty();
    $cartEmpty.toggleClass('d-none', lines.length > 0);
    lines.forEach(function (line, index) {
      var c = Cart.calcLine(line);
      var $tr = $('<tr></tr>');
      var $name = $('<td></td>');
      $('<div class="fw-medium"></div>').text(line.name + (line.variantName ? ' · ' + line.variantName : '')).appendTo($name);
      $('<div class="small text-muted"></div>').text(money(c.price) + (line.unitName ? ' / ' + line.unitName : '')).appendTo($name);
      $tr.append($name);

      var $qty = $('<td class="text-end"></td>');
      $('<input type="number" class="form-control form-control-sm text-end" min="0.001" step="0.001" />')
        .val(line.quantity)
        .attr('data-field', 'quantity')
        .attr('data-index', index)
        .appendTo($qty);
      $tr.append($qty);

      var $disc = $('<td class="text-end"></td>');
      $('<input type="number" class="form-control form-control-sm text-end" min="0" step="0.01" />')
        .val(line.discountAmount || 0)
        .attr('data-field', 'discountAmount')
        .attr('data-index', index)
        .appendTo($disc);
      $tr.append($disc);

      $('<td class="text-end text-nowrap"></td>').text(money(c.total)).appendTo($tr);
      var $rm = $('<td class="text-end"></td>');
      $('<button type="button" class="btn btn-sm btn-outline-danger" title="Remove">&times;</button>')
        .on('click', function () {
          lines.splice(index, 1);
          render();
          saveDraft();
        })
        .appendTo($rm);
      $tr.append($rm);
      $cartBody.append($tr);
    });
  }

  function renderPayments() {
    $payments.empty();
    var methods = [
      { v: 'cash', t: 'Cash' },
      { v: 'card', t: 'Card' },
      { v: 'upi', t: 'UPI' },
      { v: 'wallet', t: 'Wallet' },
      { v: 'bank', t: 'Bank' },
      { v: 'other', t: 'Other' }
    ];
    payments.forEach(function (p, index) {
      var $row = $('<div class="pos-pay-row"></div>');
      var $sel = $('<select class="form-select form-select-sm"></select>').attr('data-pay', 'method').attr('data-index', index);
      methods.forEach(function (m) {
        var $o = $('<option></option>').attr('value', m.v).text(m.t);
        if (p.method === m.v) $o.prop('selected', true);
        $sel.append($o);
      });
      $row.append($sel);
      $('<input type="number" min="0.01" step="0.01" class="form-control form-control-sm" placeholder="Amount" />')
        .val(p.amount)
        .attr('data-pay', 'amount')
        .attr('data-index', index)
        .appendTo($row);
      $('<input type="text" class="form-control form-control-sm" placeholder="Ref." maxlength="100" />')
        .val(p.referenceNo || '')
        .attr('data-pay', 'referenceNo')
        .attr('data-index', index)
        .appendTo($row);
      var $rm = $('<button type="button" class="btn btn-sm btn-outline-danger" title="Remove">&times;</button>');
      if (payments.length === 1) $rm.prop('disabled', true);
      $rm.on('click', function () {
        if (payments.length === 1) return;
        payments.splice(index, 1);
        render();
        saveDraft();
      });
      $row.append($rm);
      $payments.append($row);
    });
  }

  function renderTotals() {
    var state = Cart.paymentState(lines, payments);
    $('#posSubtotal').text(money(state.totals.subTotal));
    $('#posDiscount').text(money(state.totals.discount));
    $('#posTax').text(money(state.totals.tax));
    $('#posTotal').text(money(state.totals.total));
    $('#posPaid').text(money(state.paid));
    $('#posDue').text(money(state.due));
    $('#posChange').text(money(state.change));
  }

  function render() {
    renderCart();
    renderPayments();
    renderTotals();
    if (customer) {
      $('#posCustomerLabel').text(customer.fullName + (customer.phone ? ' · ' + customer.phone : ''));
    } else {
      $('#posCustomerLabel').text('Walk-in');
    }
  }

  $cartBody.on('change', 'input', function () {
    var $el = $(this);
    var i = Number($el.attr('data-index'));
    var field = $el.attr('data-field');
    if (!lines[i]) return;
    lines[i][field] = Cart.num($el.val());
    render();
    saveDraft();
  });

  $payments.on('change', 'select, input', function () {
    var $el = $(this);
    var i = Number($el.attr('data-index'));
    var field = $el.attr('data-pay');
    if (!payments[i]) return;
    payments[i][field] = field === 'amount' ? $el.val() : $el.val();
    render();
    saveDraft();
  });

  $('#posAddPayment').on('click', function () {
    payments.push({ method: 'cash', amount: '', referenceNo: '' });
    render();
  });

  $search.on('input', search);
  $search.on('keydown', function (e) {
    if (e.key === 'Enter') {
      e.preventDefault();
      scanBarcode();
    }
  });

  A.typeahead('#posCustomer', {
    minChars: 2,
    source: function (term) {
      return A.getJson(customerSearchUrl, { term: term, take: 10 });
    },
    label: function (item) { return item.fullName || ''; },
    sublabel: function (item) {
      return [item.phone, item.email].filter(Boolean).join(' · ');
    },
    onSelect: function (item) {
      customer = item;
      render();
      saveDraft();
    }
  });

  $('#posCustomerClear').on('click', function () {
    customer = null;
    $('#posCustomer').val('');
    render();
    saveDraft();
  });

  $('#posNotes').on('change', saveDraft);

  if (canQuickCreate) {
    $('#posCustomerAdd').on('click', function () { showModal($('#posQuickCreateModal')); $('#qcFullName').trigger('focus'); });
    $('#posQuickCreateCancel').on('click', function () { hideModal($('#posQuickCreateModal')); });
    $('#posQuickCreateForm').on('submit', function (e) {
      e.preventDefault();
      var $form = $(this);
      A.post(customerCreateUrl, {
        FullName: $('#qcFullName').val(),
        Phone: $('#qcPhone').val(),
        Email: $('#qcEmail').val()
      }, {
        $form: $form,
        onSuccess: function (res) {
          var data = res && res.data;
          if (data) {
            customer = {
              idCustomer: data.idCustomer,
              customerCode: data.customerCode,
              fullName: data.fullName,
              phone: data.phone,
              email: data.email
            };
            $('#posCustomer').val(customer.fullName);
          }
          $form[0].reset();
          hideModal($('#posQuickCreateModal'));
          render();
          saveDraft();
        }
      });
    });
  }

  function completeSale() {
    if (completing) return;
    if (!lines.length) {
      A.toast('Add at least one item.', 'warning');
      return;
    }
    var state = Cart.paymentState(lines, payments);
    if (state.paid + 0.0001 < state.totals.total) {
      A.toast('Payments must cover the total.', 'warning');
      return;
    }
    if (!state.changeOk) {
      A.toast('Change can only be given from a cash payment.', 'warning');
      return;
    }

    var payload = {
      fkCustomer: customer && customer.idCustomer ? customer.idCustomer : null,
      notes: $('#posNotes').val(),
      clientRef: clientRef,
      lines: lines.map(function (l) {
        return {
          fkProduct: l.idProduct,
          fkProductVariant: l.idProductVariant,
          quantity: Cart.num(l.quantity),
          discountAmount: Cart.num(l.discountAmount)
        };
      }),
      payments: payments
        .map(function (p) {
          return {
            paymentMethod: p.method,
            amount: Cart.round(Cart.num(p.amount), 2),
            referenceNo: p.referenceNo || null
          };
        })
        .filter(function (p) { return p.amount > 0; })
    };

    completing = true;
    var $btn = $('#posComplete');
    A.setBusy($btn, true);
    A.postJson(createUrl, payload, {
      silent: true,
      onSuccess: function (res) {
        completing = false;
        A.setBusy($btn, false);
        var data = res && res.data ? res.data : {};
        A.toast(res && res.message ? res.message : 'Sale completed.', 'success');
        $('#posSuccessInvoice').text(data.invoiceNumber || '');
        $('#posSuccessTotal').text(money(data.totalAmount));
        $('#posSuccessChange').text(money(data.changeGiven));
        $('#posSuccessPrint').attr('href', data.printUrl || '#');
        showModal($('#posSuccessModal'));
        clearDraft();
      },
      onError: function () {
        completing = false;
        A.setBusy($btn, false);
      }
    });
  }

  $('#posComplete').on('click', completeSale);
  $('#posSuccessNew').on('click', function () {
    hideModal($('#posSuccessModal'));
    resetBill();
  });

  $(document).on('keydown', function (e) {
    if (e.key === 'F2') {
      e.preventDefault();
      $search.trigger('focus').trigger('select');
    } else if (e.key === 'F9') {
      e.preventDefault();
      completeSale();
    }
  });

  loadDraft();
  render();
  $search.trigger('focus');
})(window, window.jQuery);
