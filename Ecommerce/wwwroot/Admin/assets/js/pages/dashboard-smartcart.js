'use strict';

(function () {
  var charts = {
    orderTrend: null,
    orderStatus: null,
    salesCompare: null,
    topProducts: null
  };

  var currency = new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    maximumFractionDigits: 0
  });

  var numberFmt = new Intl.NumberFormat('en-IN');

  function money(value) {
    return currency.format(Number(value || 0));
  }

  function num(value) {
    return numberFmt.format(Number(value || 0));
  }

  function statusClass(status) {
    var s = String(status || '').toLowerCase();
    if (s === 'delivered') return 'success';
    if (s === 'shipped' || s === 'confirmed') return 'primary';
    if (s === 'placed' || s === 'pending') return 'warning';
    if (s === 'cancelled') return 'danger';
    return 'secondary';
  }

  function destroyChart(key) {
    if (charts[key]) {
      charts[key].destroy();
      charts[key] = null;
    }
  }

  function setText(id, value) {
    var el = document.getElementById(id);
    if (el) el.textContent = value;
  }

  function renderKpis(kpis) {
    kpis = kpis || {};
    setText('kpiTotalOrders', num(kpis.totalOrders));
    setText('kpiOrdersToday', num(kpis.ordersToday));
    setText('kpiPendingOrders', num(kpis.pendingOrders));
    setText('kpiOnlineRevenue', money(kpis.onlineRevenue));
    setText('kpiOnlineRevenueToday', money(kpis.onlineRevenueToday));
    setText('kpiActiveProducts', num(kpis.activeProducts));
    setText('kpiActiveSkus', num(kpis.activeSkus));
    setText('kpiSellOnlineSkus', num(kpis.sellOnlineSkuCount));
    setText('kpiCustomers', num(kpis.customers));
    setText('kpiLowStock', num(kpis.lowStockSkuCount));
    setText('kpiPosRevenue', money(kpis.posRevenue));
  }

  function renderOrderTrend(points) {
    destroyChart('orderTrend');
    var el = document.querySelector('#chartOrderTrend');
    if (!el || typeof ApexCharts === 'undefined') return;

    var categories = (points || []).map(function (p) { return p.date; });
    var counts = (points || []).map(function (p) { return p.count; });
    var amounts = (points || []).map(function (p) { return Number(p.amount || 0); });

    charts.orderTrend = new ApexCharts(el, {
      chart: { height: 360, type: 'area', toolbar: { show: false }, zoom: { enabled: false } },
      colors: ['#4680ff', '#2ca87f'],
      dataLabels: { enabled: false },
      stroke: { curve: 'smooth', width: 2 },
      fill: {
        type: 'gradient',
        gradient: { shadeIntensity: 1, opacityFrom: 0.35, opacityTo: 0.05, stops: [0, 90, 100] }
      },
      series: [
        { name: 'Orders', type: 'area', data: counts },
        { name: 'Revenue (₹)', type: 'line', data: amounts }
      ],
      xaxis: { categories: categories, labels: { rotate: -45 } },
      yaxis: [
        { title: { text: 'Orders' }, min: 0, forceNiceScale: true },
        { opposite: true, title: { text: 'Revenue' }, min: 0, forceNiceScale: true, labels: { formatter: function (v) { return money(v); } } }
      ],
      tooltip: {
        shared: true,
        y: {
          formatter: function (val, opts) {
            return opts.seriesIndex === 1 ? money(val) : num(val);
          }
        }
      },
      legend: { position: 'top' },
      noData: { text: 'No order activity in the last 14 days' }
    });
    charts.orderTrend.render();
  }

  function renderOrderStatus(rows) {
    destroyChart('orderStatus');
    var el = document.querySelector('#chartOrderStatus');
    if (!el || typeof ApexCharts === 'undefined') return;

    var labels = (rows || []).map(function (r) { return r.name; });
    var values = (rows || []).map(function (r) { return r.count; });
    if (!values.length) {
      labels = ['No orders'];
      values = [0];
    }

    charts.orderStatus = new ApexCharts(el, {
      chart: { type: 'donut', height: 360 },
      labels: labels,
      series: values,
      colors: ['#4680ff', '#2ca87f', '#faad14', '#dc2626', '#722ed1', '#13c2c2'],
      legend: { position: 'bottom' },
      dataLabels: { enabled: true },
      plotOptions: {
        pie: {
          donut: {
            size: '65%',
            labels: {
              show: true,
              total: {
                show: true,
                label: 'Orders',
                formatter: function (w) {
                  return num(w.globals.seriesTotals.reduce(function (a, b) { return a + b; }, 0));
                }
              }
            }
          }
        }
      },
      noData: { text: 'No status data' }
    });
    charts.orderStatus.render();
  }

  function renderSalesCompare(orderTrend, posTrend) {
    destroyChart('salesCompare');
    var el = document.querySelector('#chartSalesCompare');
    if (!el || typeof ApexCharts === 'undefined') return;

    var categories = (orderTrend || []).map(function (p) { return p.date; });
    var online = (orderTrend || []).map(function (p) { return Number(p.amount || 0); });
    var pos = (posTrend || []).map(function (p) { return Number(p.amount || 0); });

    charts.salesCompare = new ApexCharts(el, {
      chart: { type: 'bar', height: 320, toolbar: { show: false }, stacked: false },
      colors: ['#4680ff', '#13c2c2'],
      plotOptions: { bar: { columnWidth: '45%', borderRadius: 3 } },
      dataLabels: { enabled: false },
      series: [
        { name: 'Online orders', data: online },
        { name: 'Counter sales', data: pos }
      ],
      xaxis: { categories: categories, labels: { rotate: -45 } },
      yaxis: { labels: { formatter: function (v) { return money(v); } } },
      tooltip: { y: { formatter: function (v) { return money(v); } } },
      legend: { position: 'top' },
      noData: { text: 'No sales in the selected period' }
    });
    charts.salesCompare.render();
  }

  function renderTopProducts(rows) {
    destroyChart('topProducts');
    var el = document.querySelector('#chartTopProducts');
    if (!el || typeof ApexCharts === 'undefined') return;

    var labels = (rows || []).map(function (r) { return r.name; }).reverse();
    var qty = (rows || []).map(function (r) { return r.quantity; }).reverse();

    charts.topProducts = new ApexCharts(el, {
      chart: { type: 'bar', height: 320, toolbar: { show: false } },
      colors: ['#722ed1'],
      plotOptions: { bar: { horizontal: true, borderRadius: 3, barHeight: '60%' } },
      dataLabels: { enabled: true },
      series: [{ name: 'Qty sold', data: qty }],
      xaxis: { categories: labels.length ? labels : ['No data'], labels: { formatter: function (v) { return num(v); } } },
      tooltip: { y: { formatter: function (v) { return num(v) + ' units'; } } },
      noData: { text: 'No product sales yet' }
    });
    charts.topProducts.render();
  }

  function renderRecentOrders(rows) {
    var body = document.getElementById('recentOrdersBody');
    if (!body) return;

    if (!rows || !rows.length) {
      body.innerHTML = '<tr><td colspan="5" class="text-muted text-center py-4">No orders yet.</td></tr>';
      return;
    }

    body.innerHTML = rows.map(function (o) {
      var dateText = o.orderDate ? new Date(o.orderDate).toLocaleString('en-IN', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' }) : '—';
      var badge = statusClass(o.status);
      return (
        '<tr>' +
          '<td><a href="/Admin/Order/Details/' + o.orderId + '">' + (o.orderNumber || ('#' + o.orderId)) + '</a></td>' +
          '<td>' + (o.customerName || '—') + '</td>' +
          '<td><span class="badge bg-light-' + badge + ' border border-' + badge + '">' + (o.status || '—') + '</span></td>' +
          '<td>' + dateText + '</td>' +
          '<td class="text-end">' + money(o.totalAmount) + '</td>' +
        '</tr>'
      );
    }).join('');
  }

  function renderLowStock(rows) {
    var list = document.getElementById('lowStockList');
    if (!list) return;

    if (!rows || !rows.length) {
      list.innerHTML = '<div class="list-group-item text-success text-center py-4">All tracked SKUs are above reorder level.</div>';
      return;
    }

    list.innerHTML = rows.map(function (item) {
      var danger = item.availableQty <= 0;
      return (
        '<div class="list-group-item list-group-item-action">' +
          '<div class="d-flex justify-content-between align-items-start gap-2">' +
            '<div>' +
              '<h6 class="mb-1">' + (item.productName || 'Product') + '</h6>' +
              '<p class="mb-0 text-muted small">' + (item.sku || '') + '</p>' +
            '</div>' +
            '<span class="badge ' + (danger ? 'bg-light-danger border border-danger' : 'bg-light-warning border border-warning') + '">' +
              num(item.availableQty) + ' left' +
            '</span>' +
          '</div>' +
        '</div>'
      );
    }).join('');
  }

  function renderSummary(data) {
    renderKpis(data.kpis);
    renderOrderTrend(data.orderTrend);
    renderOrderStatus(data.orderStatusCounts);
    renderSalesCompare(data.orderTrend, data.posSalesTrend);
    renderTopProducts(data.topProducts);
    renderRecentOrders(data.recentOrders);
    renderLowStock(data.lowStockItems);

    var updated = document.getElementById('dashboardUpdatedAt');
    if (updated) {
      var when = data.generatedAt ? new Date(data.generatedAt) : new Date();
      updated.textContent = 'Updated ' + when.toLocaleString('en-IN');
    }
  }

  function showError(message) {
    setText('dashboardUpdatedAt', message || 'Failed to load dashboard.');
    var body = document.getElementById('recentOrdersBody');
    if (body) {
      body.innerHTML = '<tr><td colspan="5" class="text-danger text-center py-4">Could not load dashboard data.</td></tr>';
    }
  }

  async function loadDashboard() {
    var btn = document.getElementById('btnDashboardRefresh');
    if (btn) {
      btn.disabled = true;
      btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Loading';
    }

    try {
      var response = await fetch('/Admin/Dashboard/GetSummary', {
        method: 'GET',
        headers: { 'Accept': 'application/json' },
        credentials: 'same-origin'
      });
      var payload = await response.json();
      if (!response.ok || !payload || !payload.success || !payload.data) {
        showError((payload && payload.message) || 'Failed to load dashboard.');
        return;
      }
      renderSummary(payload.data);
    } catch (err) {
      showError('Failed to load dashboard.');
      console.error(err);
    } finally {
      if (btn) {
        btn.disabled = false;
        btn.innerHTML = '<i class="ti ti-refresh"></i> Refresh';
      }
    }
  }

  document.addEventListener('DOMContentLoaded', function () {
    var refresh = document.getElementById('btnDashboardRefresh');
    if (refresh) refresh.addEventListener('click', loadDashboard);
    loadDashboard();
  });
})();
