/**
 * Home dashboard: refresh KPIs/tables from Dashboard/Snapshot and optional 60s poll while visible.
 */
(function (window, $) {
  'use strict';

  var POLL_MS = 60000;
  var timer = null;

  function invoiceHref(base, id) {
    if (!base) return '#';
    if (base.indexOf('__id__') >= 0) return base.replace('__id__', String(id));
    return base.replace(/0(?!.*\d)/, String(id));
  }

  function money(currency, n) {
    var formatted = window.SmartCartAjax && SmartCartAjax.fmt
      ? SmartCartAjax.fmt.money(n)
      : Number(n || 0).toFixed(2);
    return (currency || '') + ' ' + formatted;
  }

  function number(n) {
    return window.SmartCartAjax && SmartCartAjax.fmt
      ? SmartCartAjax.fmt.number(n)
      : String(n || 0);
  }

  function changeBadge(pct) {
    var value = Number(pct) || 0;
    var cls = value > 0 ? 'text-bg-success-subtle text-success' : (value < 0 ? 'text-bg-danger-subtle text-danger' : 'text-bg-secondary-subtle text-secondary');
    var sign = value > 0 ? '+' : '';
    return { cls: cls, text: sign + value.toFixed(1) + '% vs yesterday' };
  }

  function renderChart(el, kind, rows, title) {
    if (!el || !window.SmartCharts) return;
    el.setAttribute('data-sc-chart-title', title || '');
    if (kind === 'line') SmartCharts.line(el, rows, { title: title });
    else if (kind === 'donut') SmartCharts.donut(el, rows, { title: title });
    else SmartCharts.bars(el, rows, { title: title });
  }

  function fillKpis($page, kpis, currency) {
    if (!kpis) return;
    $page.find('[data-kpi="salesToday"]').text(money(currency, kpis.salesToday));
    $page.find('[data-kpi="salesMonth"]').text(money(currency, kpis.salesMonth));
    $page.find('[data-kpi="invoicesToday"]').text(number(kpis.invoiceCountToday) + ' invoices today');
    $page.find('[data-kpi="lowStock"]').text(number(kpis.lowStockCount));
    $page.find('[data-kpi="pendingPo"]').text(number(kpis.pendingPurchaseOrders));
    $page.find('[data-kpi="pendingOrders"]').text(number(kpis.pendingOnlineOrders));
    $page.find('[data-kpi="openCash"]').text(number(kpis.openCashSessions));
    var badge = changeBadge(kpis.todayChangePercent);
    var $badge = $page.find('[data-kpi="todayChange"]');
    $badge.attr('class', 'badge ' + badge.cls).text(badge.text);
  }

  function fillRecent($page, rows, currency) {
    var $body = $page.find('[data-table="recent"]');
    if (!$body.length) return;
    $body.empty();
    if (!rows || !rows.length) {
      $body.append($('<tr>').append($('<td>').attr('colspan', 4).addClass('text-muted text-center py-4').text('No invoices yet')));
      return;
    }
    var detailsBase = $page.attr('data-invoice-details') || '';
    rows.forEach(function (row) {
      var href = invoiceHref(detailsBase, row.idSale);
      var $a = $('<a>').attr('href', href).text(row.invoiceNumber);
      var $tr = $('<tr>');
      $tr.append($('<td>').append($a));
      $tr.append($('<td>').text(row.customerName));
      $tr.append($('<td>').addClass('text-end').text(money(currency, row.totalAmount)));
      var $time = $('<time>').attr('data-sc-utc', row.saleDate).attr('data-sc-format', 'datetime').text(row.saleDate);
      $tr.append($('<td>').append($time));
      $body.append($tr);
    });
    if (window.SmartCartAjax) SmartCartAjax.localizeTimes($body);
  }

  function fillLowStock($page, rows) {
    var $body = $page.find('[data-table="lowstock"]');
    if (!$body.length) return;
    $body.empty();
    if (!rows || !rows.length) {
      $body.append($('<tr>').append($('<td>').attr('colspan', 3).addClass('text-muted text-center py-4').text('No low-stock items')));
      return;
    }
    rows.forEach(function (row) {
      var $tr = $('<tr>');
      $tr.append($('<td>').text(row.productName));
      $tr.append($('<td>').addClass('text-end').text(number(row.quantityOnHand)));
      $tr.append($('<td>').addClass('text-end').text(number(row.reorderLevel)));
      $body.append($tr);
    });
  }

  function applySnapshot($page, data) {
    if (!data) return;
    var currency = data.currency || $page.attr('data-currency') || '';
    fillKpis($page, data.kpis, currency);
    fillRecent($page, data.recentInvoices, currency);
    fillLowStock($page, data.lowStock);

    var trend7 = (data.trend7 || []).map(function (p) { return { label: p.label, value: p.netSales }; });
    var trend30 = (data.trend30 || []).map(function (p) { return { label: p.label, value: p.netSales }; });
    var payments = (data.payments || []).map(function (p) { return { label: p.paymentMethod, value: p.amount }; });
    var products = (data.topProducts || []).map(function (p) { return { label: p.productName, value: p.revenue }; });
    var $trend = $page.find('[data-chart="trend"]');
    $trend.attr('data-trend-7', JSON.stringify(trend7));
    $trend.attr('data-trend-30', JSON.stringify(trend30));
    var days = $page.find('[data-trend].active').attr('data-trend') === '30' ? '30' : '7';
    renderChart($trend[0], 'line', days === '30' ? trend30 : trend7, days === '30' ? '30-day sales' : '7-day sales');
    renderChart($page.find('[data-chart="payments"]')[0], 'donut', payments, 'Payments this month');
    renderChart($page.find('[data-chart="products"]')[0], 'bars', products, 'Top products');

    if (data.generatedAt && window.SmartCartAjax) {
      var $stamp = $('[data-generated]');
      $stamp.attr('data-sc-utc', data.generatedAt).attr('data-sc-format', 'datetime');
      $stamp.text(data.generatedAt);
      SmartCartAjax.localizeTimes($stamp.parent());
    }
  }

  function refresh($page, silent) {
    var url = $page.attr('data-snapshot-url');
    if (!url || !window.SmartCartAjax) return $.Deferred().resolve().promise();
    $page.find('[data-sc-refresh]').prop('disabled', true);
    return SmartCartAjax.getJson(url, null, { silent: silent !== false }).done(function (res) {
      applySnapshot($page, res && res.data);
    }).always(function () {
      $page.find('[data-sc-refresh]').prop('disabled', false);
    });
  }

  function startPoll($page) {
    stopPoll();
    if ($page.attr('data-auto-refresh') !== 'true') return;
    timer = window.setInterval(function () {
      if (document.visibilityState === 'visible') refresh($page, true);
    }, POLL_MS);
  }

  function stopPoll() {
    if (timer) {
      window.clearInterval(timer);
      timer = null;
    }
  }

  $(function () {
    var $page = $('[data-sc-dashboard]');
    if (!$page.length) return;

    $(document).on('click', '[data-sc-refresh]', function (e) {
      e.preventDefault();
      refresh($page, true);
    });

    $page.on('click', '[data-trend]', function () {
      var days = $(this).attr('data-trend') === '30' ? '30' : '7';
      $page.find('[data-trend]').removeClass('active');
      $(this).addClass('active');
      $page.find('[data-trend-title]').text(days === '30' ? '30-day sales' : '7-day sales');
      var $trend = $page.find('[data-chart="trend"]');
      var raw = $trend.attr(days === '30' ? 'data-trend-30' : 'data-trend-7');
      var rows = [];
      try { rows = JSON.parse(raw || '[]'); } catch (e) { rows = []; }
      renderChart($trend[0], 'line', rows, days === '30' ? '30-day sales' : '7-day sales');
    });

    document.addEventListener('visibilitychange', function () {
      if (document.visibilityState === 'visible') startPoll($page);
      else stopPoll();
    });

    startPoll($page);
    if (window.SmartCharts) SmartCharts.mount($page[0]);
  });
})(window, window.jQuery);
