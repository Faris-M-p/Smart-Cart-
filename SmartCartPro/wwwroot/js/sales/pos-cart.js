/* POS cart, totals preview and payments. Totals match the server: tax-exclusive on (qty × price − discount), 2 dp. */
(function (window) {
  'use strict';

  function num(value) {
    var n = parseFloat(value);
    return isFinite(n) ? n : 0;
  }

  function round(n, decimals) {
    var factor = Math.pow(10, decimals);
    return Math.round((n + Number.EPSILON) * factor) / factor;
  }

  function lineKey(item) {
    return String(item.idProduct) + ':' + String(item.idProductVariant || 0);
  }

  function calcLine(line) {
    var qty = round(num(line.quantity), 3);
    var price = round(num(line.sellingPrice), 2);
    var discount = round(num(line.discountAmount), 2);
    var gross = round(qty * price, 2);
    if (discount > gross) discount = gross;
    if (discount < 0) discount = 0;
    var net = gross - discount;
    var tax = round(net * num(line.taxRate) / 100, 2);
    return { qty: qty, price: price, discount: discount, gross: gross, tax: tax, total: net + tax };
  }

  function totals(lines) {
    var sub = 0, disc = 0, tax = 0;
    lines.forEach(function (line) {
      var c = calcLine(line);
      sub += c.gross;
      disc += c.discount;
      tax += c.tax;
    });
    return {
      subTotal: round(sub, 2),
      discount: round(disc, 2),
      tax: round(tax, 2),
      total: round(sub - disc + tax, 2)
    };
  }

  function paymentState(lines, payments) {
    var t = totals(lines);
    var paid = 0;
    var cash = 0;
    (payments || []).forEach(function (p) {
      var amt = round(num(p.amount), 2);
      if (amt > 0) paid += amt;
      if (p.method === 'cash') cash += amt;
    });
    paid = round(paid, 2);
    var due = round(Math.max(0, t.total - paid), 2);
    var change = round(Math.max(0, paid - t.total), 2);
    var changeOk = change === 0 || cash + 0.0001 >= change;
    return { totals: t, paid: paid, due: due, change: change, changeOk: changeOk };
  }

  function addOrBump(lines, item, qty) {
    var key = lineKey(item);
    var addQty = qty == null ? 1 : num(qty);
    for (var i = 0; i < lines.length; i++) {
      if (lineKey(lines[i]) === key) {
        lines[i].quantity = round(num(lines[i].quantity) + addQty, 3);
        return lines[i];
      }
    }
    var line = {
      idProduct: item.idProduct,
      idProductVariant: item.idProductVariant || null,
      name: item.name,
      variantName: item.variantName || '',
      sku: item.sku || '',
      unitName: item.unitName || '',
      allowsFraction: !!item.allowsFraction,
      sellingPrice: num(item.sellingPrice),
      taxRate: num(item.taxRate),
      stockOnHand: num(item.stockOnHand),
      quantity: addQty,
      discountAmount: 0
    };
    lines.push(line);
    return line;
  }

  window.SmartCartPosCart = {
    lineKey: lineKey,
    calcLine: calcLine,
    totals: totals,
    paymentState: paymentState,
    addOrBump: addOrBump,
    num: num,
    round: round
  };
})(window);
