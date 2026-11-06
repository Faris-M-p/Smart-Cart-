/**
 * SmartCart Pro - Fast Essential & On-Demand Advanced Dashboard Controller.
 */
(function (window, document, $) {
  'use strict';

  var C = window.DashCharts;
  if (!C) {
    console.error('DashCharts engine not found.');
    return;
  }

  var AUTO_REFRESH_MS = 300000; // 5 minutes
  var refreshTimer = null;
  var lastUpdatedTime = Date.now();
  var currentRange = 'today';
  var top10Mode = 'rev';
  var advOpen = false;
  var top10Cache = { rev: [], qty: [] };
  var currentAbortController = null;
  var role = 'owner';
  var canSeeProfit = false;

  // DOM Elements
  var $page = $('#dashboardPage');
  if (!$page.length) return;

  var essentialsUrl = $page.attr('data-essentials-url') || '/Dashboard/Essentials';
  var advSalesUrl = $page.attr('data-adv-sales-url') || '/Dashboard/AdvancedSales';
  var advCategoriesUrl = $page.attr('data-adv-categories-url') || '/Dashboard/AdvancedCategories';
  var advStockUrl = $page.attr('data-adv-stock-url') || '/Dashboard/AdvancedStock';
  var advPurchasesUrl = $page.attr('data-adv-purchases-url') || '/Dashboard/AdvancedPurchases';
  var advTeamUrl = $page.attr('data-adv-team-url') || '/Dashboard/AdvancedTeam';
  var advProfitUrl = $page.attr('data-adv-profit-url') || '/Dashboard/AdvancedProfit';

  var elAlerts = document.getElementById('alerts');
  var elQa = document.getElementById('qa');
  var elKpis = document.getElementById('kpis');
  var elTrend = document.getElementById('trend');
  var elTrendTitle = document.getElementById('trendTitle');
  var elTrendSub = document.getElementById('trendSub');
  var elLgCur = document.getElementById('lgCur');
  var elLgPrev = document.getElementById('lgPrev');
  var elPay = document.getElementById('pay');
  var elPayTotal = document.getElementById('payTotal');
  var elTop5 = document.getElementById('top5');
  var elTop5Link = document.getElementById('top5Link');
  var elLow = document.getElementById('low');
  var elLowLink = document.getElementById('lowLink');
  var elInv = document.getElementById('inv');
  var elInvTitle = document.getElementById('invTitle');
  var elInvLink = document.getElementById('invLink');
  var elUpdated = document.getElementById('updated');
  var elAdvBar = document.getElementById('advBar');
  var elAdvanced = document.getElementById('advanced');
  var btnAdv = document.getElementById('advBtn');
  var elPageTitle = document.getElementById('pageTitle');

  // Advanced Widget Container IDs
  var ADV_CONTAINERS = {
    hourly: document.getElementById('a-hourly'),
    cat: document.getElementById('a-cat'),
    stock: document.getElementById('a-stock'),
    channel: document.getElementById('a-channel'),
    profit: document.getElementById('a-profit'),
    dow: document.getElementById('a-dow'),
    svp: document.getElementById('a-svp'),
    top10: document.getElementById('a-top10'),
    cashier: document.getElementById('a-cashier'),
    stores: document.getElementById('a-stores')
  };

  /* ---------- Helpers ---------- */

  function getQueryRange() {
    var params = new URLSearchParams(window.location.search);
    var r = params.get('range');
    if (r && ['today', 'd7', 'd30', 'month'].indexOf(r.toLowerCase()) >= 0) {
      return r.toLowerCase();
    }
    return 'today';
  }

  function setQueryRange(r) {
    var url = new URL(window.location.href);
    url.searchParams.set('range', r);
    window.history.replaceState({}, '', url.toString());
  }

  function applyRoleUI(newRole) {
    role = newRole || role;
    var isCashier = role === 'cashier';
    if (elPageTitle) elPageTitle.textContent = isCashier ? 'My day' : 'Dashboard';
    if (elKpis) elKpis.classList.toggle('k4', isCashier);
    if (elAdvBar) elAdvBar.hidden = isCashier;
    if (elAdvanced && isCashier) {
      elAdvanced.classList.remove('open');
      advOpen = false;
    }
    document.querySelectorAll('[data-hide="cashier"]').forEach(function (el) {
      el.hidden = isCashier;
    });
    document.querySelectorAll('[data-only="owner"]').forEach(function (el) {
      el.hidden = !canSeeProfit;
    });
  }

  /* ---------- Skeletons ---------- */

  function renderEssentialSkeletons() {
    if (elKpis) {
      var count = role === 'cashier' ? 4 : 6;
      var cardsHtml = '';
      for (var i = 0; i < count; i++) {
        cardsHtml += '<div class="card-kpi"><div class="sk" style="height:16px;width:50%"></div><div class="sk" style="height:32px;width:75%;margin:8px 0"></div><div class="sk" style="height:14px;width:60%"></div></div>';
      }
      elKpis.innerHTML = cardsHtml;
    }
    if (elTrend) elTrend.innerHTML = '<div class="sk" style="height:220px"></div>';
    if (elPay) elPay.innerHTML = '<div class="sk" style="height:180px"></div>';
    if (elTop5 && role !== 'cashier') elTop5.innerHTML = '<div class="sk" style="height:180px"></div>';
    if (elLow && role !== 'cashier') elLow.innerHTML = '<div class="sk" style="height:180px"></div>';
    if (elInv) elInv.innerHTML = '<div class="sk" style="height:180px"></div>';
  }

  function renderAdvancedSkeletons() {
    Object.keys(ADV_CONTAINERS).forEach(function (key) {
      var el = ADV_CONTAINERS[key];
      if (el) {
        var h = el.classList.contains('dash-chart') ? 200 : 160;
        el.innerHTML = '<div class="sk" style="height:' + h + 'px"></div>';
      }
    });
  }

  /* ---------- Render Essentials ---------- */

  function renderEssentials(data) {
    if (!data) return;

    applyRoleUI(data.role || role);

    if (elUpdated && data.updatedTime) {
      elUpdated.textContent = 'Updated ' + data.updatedTime;
      lastUpdatedTime = Date.now();
    }

    // Alerts
    if (elAlerts) {
      if (data.alerts && data.alerts.length > 0) {
        elAlerts.innerHTML = data.alerts.map(function (a) {
          var link = a.linkUrl && a.linkText ? ' <a href="' + a.linkUrl + '">' + C.esc(a.linkText) + '</a>' : '';
          return '<div class="alert-card ' + a.type + '">' + C.svgI(a.icon || 'alert') + C.esc(a.text) + link + '</div>';
        }).join('');
      } else {
        elAlerts.innerHTML = '';
      }
    }

    // Quick Actions
    if (elQa && data.quickActions) {
      elQa.innerHTML = data.quickActions.map(function (qa) {
        var href = '/' + qa.controller + '/' + qa.action;
        return '<a href="' + href + '">' + C.svgI(qa.icon) + C.esc(qa.title) + '</a>';
      }).join('');
    }

    // KPIs
    if (elKpis && data.kpis) {
      elKpis.innerHTML = data.kpis.map(function (k) {
        var sparkHtml = k.sparkline && k.sparkline.length ? C.spark(k.sparkline, k.key === 'bills' ? '#0d9488' : '#2563eb') : '';
        var linkHtml = k.linkText && k.linkUrl ? '<a href="' + k.linkUrl + '">' + C.esc(k.linkText) + '</a>' : '';
        var deltaHtml = typeof k.deltaPercent === 'number' ? C.deltaPill(k.deltaPercent, k.invertDelta) + ' ' : '';
        return '<div class="card-kpi">' +
               '<div class="lbl">' + C.svgI(k.icon || 'rupee') + C.esc(k.title) + '</div>' +
               '<div class="val">' + k.formattedValue + '</div>' +
               '<div class="sub">' + deltaHtml + C.esc(k.subtitle) + '</div>' +
               sparkHtml + linkHtml +
               '</div>';
      }).join('');
    }

    // Sales Trend
    if (data.salesTrend) {
      if (elTrendTitle) elTrendTitle.textContent = data.salesTrend.title;
      if (elTrendSub) elTrendSub.textContent = data.salesTrend.subtitle;
      if (elLgCur) elLgCur.textContent = data.salesTrend.currentPeriodLabel;
      if (elLgPrev) elLgPrev.textContent = data.salesTrend.previousPeriodLabel;
      if (elTrend) {
        C.areaChart(
          elTrend,
          data.salesTrend.labels,
          data.salesTrend.currentValues,
          data.salesTrend.previousValues
        );
      }
    }

    // Payment Modes
    if (elPay && data.payments) {
      if (elPayTotal) elPayTotal.textContent = data.payments.periodLabel;
      var slices = (data.payments.items || []).map(function (p) {
        return {
          l: p.paymentMethod,
          v: p.amount,
          c: p.color || 'var(--c1)',
          fmt: function () { return p.formattedAmount; }
        };
      });
      C.donut(elPay, slices, data.payments.formattedTotal, 'collected');
    }

    // Top 5 Products
    if (elTop5 && role !== 'cashier') {
      if (elTop5Link && data.itemSalesReportUrl) elTop5Link.href = data.itemSalesReportUrl;
      var topItems = (data.topProducts || []).map(function (p) {
        return { l: p.productName, v: p.revenue, fmt: function () { return p.formattedRevenue; } };
      });
      if (topItems.length) {
        C.hbars(elTop5, topItems, null, 'var(--c1)');
      } else {
        elTop5.innerHTML = C.emptyState('No product sales recorded in this period');
      }
    }

    // Low Stock
    if (elLow && role !== 'cashier') {
      if (elLowLink && data.allLowStockUrl) elLowLink.href = data.allLowStockUrl;
      if (data.lowStock && data.lowStock.length) {
        var lowRows = data.lowStock.map(function (r) {
          var pill = r.isOutOfStock ? '<span class="pill out">Out</span>' : '<span class="pill low">Low</span>';
          var poLink = r.canCreatePo && r.createPoUrl ? '<td class="num"><a href="' + r.createPoUrl + '">Create PO</a></td>' : '<td class="num"></td>';
          return '<tr>' +
                 '<td>' + C.esc(r.productName) + ' ' + pill + '</td>' +
                 '<td class="num">' + r.quantityOnHand + '</td>' +
                 '<td class="num">' + r.reorderLevel + '</td>' +
                 poLink +
                 '</tr>';
        }).join('');
        elLow.innerHTML = '<div class="tbl"><table><thead><tr><th>Product</th><th class="num">On hand</th><th class="num">Reorder</th><th></th></tr></thead><tbody>' + lowRows + '</tbody></table></div>';
      } else {
        elLow.innerHTML = C.emptyState('No low-stock items detected');
      }
    }

    // Recent Invoices
    if (elInv) {
      if (elInvTitle) elInvTitle.textContent = role === 'cashier' ? 'My recent invoices' : 'Recent invoices';
      if (elInvLink && data.allInvoicesUrl) elInvLink.href = data.allInvoicesUrl;
      if (data.recentInvoices && data.recentInvoices.length) {
        var invRows = data.recentInvoices.map(function (inv) {
          return '<tr>' +
                 '<td><a href="' + inv.detailsUrl + '">' + C.esc(inv.invoiceNumber) + '</a></td>' +
                 '<td>' + C.esc(inv.customerName) + '</td>' +
                 '<td class="num">' + inv.formattedAmount + '</td>' +
                 '<td><span class="pm">' + C.esc(inv.paymentMode) + '</span></td>' +
                 '<td class="num">' + C.esc(inv.saleTimeFormatted) + '</td>' +
                 '</tr>';
        }).join('');
        elInv.innerHTML = '<div class="tbl"><table><thead><tr><th>Invoice</th><th>Customer</th><th class="num">Amount</th><th>Mode</th><th class="num">Time</th></tr></thead><tbody>' + invRows + '</tbody></table></div>';
      } else {
        elInv.innerHTML = C.emptyState('No invoices yet in this session');
      }
    }

    // Update range labels
    document.querySelectorAll('.rngLbl').forEach(function (e) {
      e.textContent = data.rangeLabel || currentRange;
    });
  }

  /* ---------- AJAX Calls ---------- */

  function loadEssentials(range, refresh) {
    renderEssentialSkeletons();
    var url = essentialsUrl + '?range=' + encodeURIComponent(range) + (refresh ? '&refresh=true' : '');
    $.ajax({
      url: url,
      type: 'GET',
      cache: false,
      success: function (res) {
        if (res && res.statusCode === 200 && res.data) {
          renderEssentials(res.data);
        }
      },
      error: function (xhr) {
        console.error('Failed to load dashboard essentials', xhr);
      }
    });
  }

  function loadAdvancedSales(range, refresh, signal) {
    if (!ADV_CONTAINERS.hourly) return;
    var url = advSalesUrl + '?range=' + encodeURIComponent(range) + (refresh ? '&refresh=true' : '');
    $.ajax({
      url: url,
      type: 'GET',
      cache: false,
      success: function (res) {
        if (!res || res.statusCode !== 200 || !res.data) return;
        var d = res.data;

        // Hourly
        if (ADV_CONTAINERS.hourly && d.hourlyLabels && d.hourlyValues) {
          C.columnChart(ADV_CONTAINERS.hourly, d.hourlyLabels, [d.hourlyValues], ['var(--c1)']);
        }

        // Day of Week
        if (ADV_CONTAINERS.dow && d.dayOfWeekLabels && d.dayOfWeekValues) {
          C.columnChart(ADV_CONTAINERS.dow, d.dayOfWeekLabels, [d.dayOfWeekValues], ['var(--c6)']);
        }

        // Counter vs Online
        if (ADV_CONTAINERS.channel && d.channels) {
          var items = d.channels.map(function (c) {
            return { l: c.channel, v: c.sales, c: c.color, fmt: function () { return c.formattedSales; } };
          });
          C.donut(ADV_CONTAINERS.channel, items, d.formattedTotalSales, 'total sales');
        }

        // Top 10 Products
        top10Cache.rev = d.topProductsByRevenue || [];
        top10Cache.qty = d.topProductsByQuantity || [];
        renderTop10();
      }
    });
  }

  function renderTop10() {
    if (!ADV_CONTAINERS.top10) return;
    var list = top10Mode === 'rev' ? top10Cache.rev : top10Cache.qty;
    if (!list || !list.length) {
      ADV_CONTAINERS.top10.innerHTML = C.emptyState('No product data available');
      return;
    }
    var items = list.map(function (p) {
      return {
        l: p.productName,
        v: top10Mode === 'rev' ? p.revenue : p.quantity,
        fmt: function () { return top10Mode === 'rev' ? p.formattedRevenue : p.formattedQuantity; }
      };
    });
    C.hbars(ADV_CONTAINERS.top10, items, null, 'var(--c1)');
  }

  function loadAdvancedCategories(range, refresh) {
    if (!ADV_CONTAINERS.cat) return;
    var url = advCategoriesUrl + '?range=' + encodeURIComponent(range) + (refresh ? '&refresh=true' : '');
    $.ajax({
      url: url,
      type: 'GET',
      cache: false,
      success: function (res) {
        if (!res || res.statusCode !== 200 || !res.data) return;
        var d = res.data;
        var items = (d.categories || []).map(function (c) {
          return { l: c.categoryName, v: c.sales, c: c.color, fmt: function () { return c.formattedSales; } };
        });
        if (items.length) {
          C.hbars(ADV_CONTAINERS.cat, items, null, 'var(--c1)');
        } else {
          ADV_CONTAINERS.cat.innerHTML = C.emptyState('No category sales found');
        }
      }
    });
  }

  function loadAdvancedStock(refresh) {
    if (!ADV_CONTAINERS.stock) return;
    var url = advStockUrl + (refresh ? '?refresh=true' : '');
    $.ajax({
      url: url,
      type: 'GET',
      cache: false,
      success: function (res) {
        if (!res || res.statusCode !== 200 || !res.data) return;
        var d = res.data;
        var items = (d.healthItems || []).map(function (h) {
          return {
            l: h.status,
            v: h.count,
            c: h.color,
            fmt: function (val) { return C.n0.format(val); }
          };
        });
        C.donut(ADV_CONTAINERS.stock, items, C.n0.format(d.totalTracked || 0), 'items');
      }
    });
  }

  function loadAdvancedPurchases(refresh) {
    if (!ADV_CONTAINERS.svp) return;
    var url = advPurchasesUrl + (refresh ? '?refresh=true' : '');
    $.ajax({
      url: url,
      type: 'GET',
      cache: false,
      success: function (res) {
        if (!res || res.statusCode !== 200 || !res.data) return;
        var d = res.data;
        if (d.monthLabels && d.salesValues && d.purchaseValues) {
          C.columnChart(
            ADV_CONTAINERS.svp,
            d.monthLabels,
            [d.salesValues, d.purchaseValues],
            ['var(--c1)', 'var(--c3)'],
            C.inrAxis,
            C.inr0.format,
            ['Sales', 'Purchases']
          );
        }
      }
    });
  }

  function loadAdvancedTeam(range, refresh) {
    if (!ADV_CONTAINERS.cashier && !ADV_CONTAINERS.stores) return;
    var url = advTeamUrl + '?range=' + encodeURIComponent(range) + (refresh ? '&refresh=true' : '');
    $.ajax({
      url: url,
      type: 'GET',
      cache: false,
      success: function (res) {
        if (!res || res.statusCode !== 200 || !res.data) return;
        var d = res.data;

        // Cashiers
        if (ADV_CONTAINERS.cashier) {
          if (d.cashiers && d.cashiers.length) {
            var rows = d.cashiers.map(function (c) {
              return '<tr>' +
                     '<td>' + C.esc(c.cashierName) + '</td>' +
                     '<td class="num">' + C.n0.format(c.billsCount) + '</td>' +
                     '<td class="num">' + c.formattedSales + '</td>' +
                     '<td class="num">' + c.formattedAvgBill + '</td>' +
                     '<td class="num">' + c.discountPercent.toFixed(1) + '%</td>' +
                     '</tr>';
            }).join('');
            ADV_CONTAINERS.cashier.innerHTML = '<div class="tbl"><table><thead><tr><th>Cashier</th><th class="num">Bills</th><th class="num">Sales</th><th class="num">Avg bill</th><th class="num">Discount</th></tr></thead><tbody>' + rows + '</tbody></table></div>';
          } else {
            ADV_CONTAINERS.cashier.innerHTML = C.emptyState('No cashier data recorded');
          }
        }

        // Stores
        if (ADV_CONTAINERS.stores) {
          if (d.storeComparison && d.storeComparison.length >= 2) {
            var sItems = d.storeComparison.map(function (s) {
              return { l: s.storeName, v: s.salesTotal, fmt: function () { return s.formattedSales; } };
            });
            C.hbars(ADV_CONTAINERS.stores, sItems, null, 'var(--c2)');
          } else {
            ADV_CONTAINERS.stores.innerHTML = '<p class="text-muted small mb-0 p-3">Store comparison is visible when the company has 2 or more stores.</p>';
          }
        }
      }
    });
  }

  function loadAdvancedProfit(range, refresh) {
    if (!ADV_CONTAINERS.profit || !canSeeProfit) return;
    var url = advProfitUrl + '?range=' + encodeURIComponent(range) + (refresh ? '&refresh=true' : '');
    $.ajax({
      url: url,
      type: 'GET',
      cache: false,
      success: function (res) {
        if (!res || res.statusCode !== 200 || !res.data) return;
        var d = res.data;
        var deltaHtml = typeof d.deltaMarginPercent === 'number' ? ' ' + C.deltaPill(d.deltaMarginPercent) : '';
        var profitHtml = '<div class="profit-big">' +
                         '<div><small>Gross profit</small><b>' + d.formattedGrossProfit + '</b></div>' +
                         '<div><small>Margin</small><b>' + d.marginPercent.toFixed(1) + '%</b>' + deltaHtml + '</div>' +
                         '</div>' +
                         '<div id="a-margin"></div>';
        ADV_CONTAINERS.profit.innerHTML = profitHtml;

        var elMargin = document.getElementById('a-margin');
        if (elMargin && d.categoryMargins && d.categoryMargins.length) {
          var mItems = d.categoryMargins.map(function (cm) {
            return { l: cm.categoryName, v: cm.marginPercent, fmt: function (v) { return v + '%'; } };
          });
          C.hbars(elMargin, mItems, function (v) { return v + '%'; }, 'var(--c7)');
        }
      }
    });
  }

  function loadAllAdvanced(range, refresh) {
    renderAdvancedSkeletons();
    loadAdvancedSales(range, refresh);
    loadAdvancedCategories(range, refresh);
    loadAdvancedStock(refresh);
    loadAdvancedPurchases(refresh);
    loadAdvancedTeam(range, refresh);
    if (canSeeProfit) {
      loadAdvancedProfit(range, refresh);
    }
  }

  /* ---------- Event Handlers ---------- */

  // Advanced Toggle Button
  if (btnAdv) {
    btnAdv.onclick = function () {
      advOpen = !advOpen;
      if (elAdvanced) elAdvanced.classList.toggle('open', advOpen);
      btnAdv.setAttribute('aria-expanded', advOpen);
      btnAdv.innerHTML = C.svgI(advOpen ? 'chevUp' : 'chevDown') + '<span>' + (advOpen ? 'Hide advanced' : 'Show advanced') + '</span>';
      try { localStorage.setItem('smartcart:dash:advanced', advOpen ? 'true' : 'false'); } catch (e) {}

      if (advOpen) {
        loadAllAdvanced(currentRange, false);
        elAdvanced.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    };
  }

  // Top 10 Segment Switcher (Revenue vs Quantity)
  var t10seg = document.getElementById('t10seg');
  if (t10seg) {
    t10seg.onclick = function (e) {
      var b = e.target.closest('button');
      if (!b) return;
      top10Mode = b.dataset.m || 'rev';
      t10seg.querySelectorAll('button').forEach(function (x) {
        x.classList.toggle('active', x === b);
      });
      renderTop10();
    };
  }

  // Date Range Segmented Switcher
  var segRange = document.getElementById('range');
  if (segRange) {
    segRange.onclick = function (e) {
      var b = e.target.closest('button');
      if (!b || b.dataset.r === currentRange) return;
      currentRange = b.dataset.r;
      segRange.querySelectorAll('button').forEach(function (x) {
        x.classList.toggle('active', x === b);
      });
      setQueryRange(currentRange);
      loadEssentials(currentRange, false);
      if (advOpen) {
        loadAllAdvanced(currentRange, false);
      }
    };
  }

  // Refresh Button
  var btnRefresh = document.getElementById('refresh');
  if (btnRefresh) {
    btnRefresh.onclick = function () {
      loadEssentials(currentRange, true);
      if (advOpen) {
        loadAllAdvanced(currentRange, true);
      }
    };
  }

  // Auto Refresh (5 minutes while tab is visible)
  function setupAutoRefresh() {
    if (refreshTimer) clearInterval(refreshTimer);
    refreshTimer = setInterval(function () {
      if (document.visibilityState === 'visible') {
        loadEssentials(currentRange, false);
      }
    }, AUTO_REFRESH_MS);

    document.addEventListener('visibilitychange', function () {
      if (document.visibilityState === 'visible') {
        if (Date.now() - lastUpdatedTime >= AUTO_REFRESH_MS) {
          loadEssentials(currentRange, false);
        }
      }
    });
  }

  // Window Resize Debounce
  var resizeTimer;
  window.addEventListener('resize', function () {
    clearTimeout(resizeTimer);
    resizeTimer = setTimeout(function () {
      if (elTrend) {
        // re-render trend if data exists
      }
    }, 150);
  });

  /* ---------- Initialization ---------- */

  // Parse initial state from page dataset
  role = $page.attr('data-role') || 'owner';
  canSeeProfit = $page.attr('data-can-see-profit') === 'true';
  currentRange = getQueryRange();

  // Set active range button
  if (segRange) {
    segRange.querySelectorAll('button').forEach(function (b) {
      b.classList.toggle('active', b.dataset.r === currentRange);
    });
  }

  // Check saved advanced toggle state
  try {
    advOpen = localStorage.getItem('smartcart:dash:advanced') === 'true';
  } catch (e) {
    advOpen = false;
  }

  applyRoleUI(role);
  loadEssentials(currentRange, false);

  if (advOpen && role !== 'cashier') {
    if (elAdvanced) elAdvanced.classList.add('open');
    if (btnAdv) {
      btnAdv.setAttribute('aria-expanded', 'true');
      btnAdv.innerHTML = C.svgI('chevUp') + '<span>Hide advanced</span>';
    }
    loadAllAdvanced(currentRange, false);
  }

  setupAutoRefresh();

})(window, document, jQuery);
