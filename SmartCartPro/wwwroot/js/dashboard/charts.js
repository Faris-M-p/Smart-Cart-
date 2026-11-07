/**
 * Vanilla SVG Charting & Formatting engine for SmartCart Pro Dashboard.
 * Zero external libraries, high performance, native browser SVG.
 */
(function (window) {
  'use strict';

  var ICONS = {
    menu: '<path d="M4 6h16M4 12h16M4 18h16"/>',
    refresh: '<path d="M20 11a8 8 0 1 0-2.3 5.7"/><path d="M20 4v7h-7"/>',
    dashboard: '<rect x="3" y="3" width="7" height="9" rx="1"/><rect x="14" y="3" width="7" height="5" rx="1"/><rect x="14" y="12" width="7" height="9" rx="1"/><rect x="3" y="16" width="7" height="5" rx="1"/>',
    folder: '<path d="M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>',
    box: '<path d="M21 8 12 3 3 8v8l9 5 9-5z"/><path d="m3 8 9 5 9-5M12 13v8"/>',
    truck: '<path d="M3 6h11v10H3zM14 9h4l3 3v4h-7"/><circle cx="7" cy="18" r="2"/><circle cx="17" cy="18" r="2"/>',
    file: '<path d="M14 3H6a1 1 0 0 0-1 1v16a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V8z"/><path d="M14 3v5h5M9 13h6M9 17h6"/>',
    cart: '<circle cx="9" cy="20" r="1.5"/><circle cx="18" cy="20" r="1.5"/><path d="M3 4h2l2.4 11h11.2L21 8H6.2"/>',
    receipt: '<path d="M6 3h12v18l-3-2-3 2-3-2-3 2z"/><path d="M9 8h6M9 12h6"/>',
    users: '<circle cx="9" cy="8" r="3.5"/><path d="M2.5 20a6.5 6.5 0 0 1 13 0"/><path d="M16 4.5a3.5 3.5 0 0 1 0 7M18 14a6 6 0 0 1 3.5 6"/>',
    chart: '<path d="M3 3v18h18"/><path d="m7 15 4-4 3 3 5-6"/>',
    settings: '<circle cx="12" cy="12" r="3"/><path d="M12 2v3M12 19v3M4.9 4.9 7 7M17 17l2.1 2.1M2 12h3M19 12h3M4.9 19.1 7 17M17 7l2.1-2.1"/>',
    alert: '<path d="M12 3 2 20h20z"/><path d="M12 10v4M12 17h.01"/>',
    cash: '<rect x="3" y="6" width="18" height="12" rx="2"/><circle cx="12" cy="12" r="2.5"/>',
    globe: '<circle cx="12" cy="12" r="9"/><path d="M3 12h18M12 3a14 14 0 0 1 0 18M12 3a14 14 0 0 0 0 18"/>',
    plus: '<path d="M12 5v14M5 12h14"/>',
    download: '<path d="M12 3v12m-4-4 4 4 4-4"/><path d="M4 17v3h16v-3"/>',
    rupee: '<path d="M6 4h12M6 9h12M14 4c3 0 4 2 4 3.5S17 11 14 11H6l9 9"/>',
    bag: '<path d="M5 8h14l-1 12H6z"/><path d="M9 8V6a3 3 0 0 1 6 0v2"/>',
    ret: '<path d="M9 14 4 9l5-5"/><path d="M4 9h11a5 5 0 0 1 0 10h-3"/>',
    clock: '<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>',
    spark: '<path d="M12 3v4M12 17v4M3 12h4M17 12h4M5.6 5.6l2.8 2.8M15.6 15.6l2.8 2.8M5.6 18.4l2.8-2.8M15.6 8.4l2.8-2.8"/>',
    chevDown: '<path d="m6 9 6 6 6-6"/>',
    chevUp: '<path d="m6 15 6-6 6 6"/>',
    inbox: '<path d="M3 13h5l2 3h4l2-3h5"/><path d="M5 5h14l2 8v6H3v-6z"/>',
    award: '<circle cx="12" cy="8" r="6"/><path d="M15.477 12.89 17 22l-5-3-5 3 1.523-9.11"/>',
    building: '<rect x="4" y="2" width="16" height="20" rx="2" ry="2"/><path d="M9 22v-4h6v4M8 6h.01M16 6h.01M12 6h.01M12 10h.01M12 14h.01M16 10h.01M16 14h.01M8 10h.01M8 14h.01"/>',
    shop: '<path d="m2 7 4.41-4.41A2 2 0 0 1 7.83 2h8.34a2 2 0 0 1 1.42.59L22 7"/><path d="M4 12v8a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-8"/><path d="M15 22v-4a2 2 0 0 0-2-2h-2a2 2 0 0 0-2 2v4"/><path d="M2 7h20"/><path d="M22 7v3a2 2 0 0 1-2 2v0a2.7 2.7 0 0 1-1.59-.63.7.7 0 0 0-.82 0A2.7 2.7 0 0 1 16 12a2.7 2.7 0 0 1-1.59-.63.7.7 0 0 0-.82 0A2.7 2.7 0 0 1 12 12a2.7 2.7 0 0 1-1.59-.63.7.7 0 0 0-.82 0A2.7 2.7 0 0 1 8 12a2.7 2.7 0 0 1-1.59-.63.7.7 0 0 0-.82 0A2.7 2.7 0 0 1 4 12v0a2 2 0 0 1-2-2V7"/>',
    grid: '<rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/>'
  };

  function svgI(name, attrs) {
    attrs = attrs || '';
    return '<svg class="i" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" ' + attrs + '>' + (ICONS[name] || '') + '</svg>';
  }

  function hydrate(root) {
    root = root || document;
    root.querySelectorAll('svg[data-i]').forEach(function (s) {
      s.setAttribute('viewBox', '0 0 24 24');
      s.setAttribute('fill', 'none');
      s.setAttribute('stroke', 'currentColor');
      s.setAttribute('stroke-width', '1.8');
      s.setAttribute('stroke-linecap', 'round');
      s.setAttribute('stroke-linejoin', 'round');
      s.innerHTML = ICONS[s.dataset.i] || '';
      s.removeAttribute('data-i');
    });
  }

  var esc = function (s) {
    return String(s || '').replace(/[&<>"']/g, function (c) {
      return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
    });
  };

  var inr0 = new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 });
  var n0 = new Intl.NumberFormat('en-IN');

  function inrShort(v) {
    var a = Math.abs(v);
    if (a >= 1e7) return '₹' + (v / 1e7).toFixed(2) + 'Cr';
    if (a >= 1e5) return '₹' + (v / 1e5).toFixed(2) + 'L';
    if (a >= 1e3) return '₹' + (v / 1e3).toFixed(1) + 'K';
    return '₹' + Math.round(v);
  }

  function inrAxis(v) {
    var a = Math.abs(v);
    if (a >= 1e7) return '₹' + (v / 1e7).toFixed(1) + 'Cr';
    if (a >= 1e5) return '₹' + (v / 1e5).toFixed(1) + 'L';
    if (a >= 1e3) return '₹' + Math.round(v / 1e3) + 'K';
    return '₹' + Math.round(v);
  }

  function tipFor(host) {
    var t = host.querySelector('.dash-tip');
    if (!t) {
      t = document.createElement('div');
      t.className = 'dash-tip';
      host.appendChild(t);
    }
    return t;
  }

  function niceStep(v) {
    if (v <= 0) return 1;
    var p = Math.pow(10, Math.floor(Math.log10(v)));
    var n = v / p;
    return (n <= 1 ? 1 : n <= 2 ? 2 : n <= 2.5 ? 2.5 : n <= 5 ? 5 : 10) * p;
  }

  function niceMax(v) {
    var st = niceStep(v / 4);
    return Math.max(1, st * 4);
  }

  function axes(W, H, P, max, ticks, fmt) {
    var s = '';
    for (var i = 0; i <= ticks; i++) {
      var y = P.t + (H - P.t - P.b) * (1 - i / ticks);
      s += '<line x1="' + P.l + '" x2="' + (W - P.r) + '" y1="' + y + '" y2="' + y + '" stroke="var(--sc-chart-grid)"/>' +
           '<text x="' + (P.l - 8) + '" y="' + (y + 4) + '" text-anchor="end" font-size="11" fill="var(--sc-chart-muted)">' + fmt(max * i / ticks) + '</text>';
    }
    return s;
  }

  function xLabels(labels, xs, H, P) {
    var n = labels.length;
    if (n === 0) return '';
    var fit = Math.max(2, Math.floor(Math.abs(xs(n - 1) - xs(0)) / 62) + 1);
    var step = Math.ceil(n / Math.min(8, fit));
    var idx = [];
    for (var i = 0; i < n; i += step) idx.push(i);
    if (idx[idx.length - 1] !== n - 1) {
      if (Math.abs(xs(n - 1) - xs(idx[idx.length - 1])) < 58) idx.pop();
      idx.push(n - 1);
    }
    return idx.map(function (i) {
      var anchor = (i === n - 1 && n > 8) ? 'end' : 'middle';
      return '<text x="' + xs(i) + '" y="' + (H - P.b + 18) + '" text-anchor="' + anchor + '" font-size="11" fill="var(--sc-chart-muted)">' + esc(labels[i]) + '</text>';
    }).join('');
  }

  function areaChart(el, labels, cur, prev) {
    el.innerHTML = '';
    var W = el.clientWidth || 600;
    var H = el.clientHeight || 240;
    var P = { l: 52, r: 12, t: 12, b: 28 };
    var maxVal = Math.max(1, Math.max.apply(null, cur.concat(prev || [])));
    var max = niceMax(maxVal * 1.05);

    var xs = function (i) {
      return P.l + (labels.length <= 1 ? 0.5 : i / (labels.length - 1)) * (W - P.l - P.r);
    };
    var ys = function (v) {
      return P.t + (H - P.t - P.b) * (1 - v / max);
    };

    var line = function (a) {
      return a.map(function (v, i) {
        return (i ? 'L' : 'M') + xs(i).toFixed(1) + ',' + ys(v).toFixed(1);
      }).join('');
    };

    var area = cur.length > 0 ? (line(cur) + 'L' + xs(cur.length - 1) + ',' + ys(0) + 'L' + xs(0) + ',' + ys(0) + 'Z') : '';
    var prevLine = prev && prev.length ? '<path d="' + line(prev) + '" fill="none" stroke="var(--sc-chart-muted)" stroke-width="1.6" stroke-dasharray="5 4"/>' : '';

    var dots = (cur.length <= 31) ? cur.map(function (v, i) {
      return '<circle cx="' + xs(i) + '" cy="' + ys(v) + '" r="' + (cur.length > 14 ? 2.2 : 3.4) + '" fill="var(--sc-panel-bg)" stroke="var(--bs-primary)" stroke-width="2"/>';
    }).join('') : '';

    el.innerHTML = '<svg width="' + W + '" height="' + H + '" role="img" aria-label="Sales trend chart">' +
      '<defs><linearGradient id="ga" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="var(--bs-primary)" stop-opacity=".22"/><stop offset="1" stop-color="var(--bs-primary)" stop-opacity="0"/></linearGradient></defs>' +
      axes(W, H, P, max, 4, inrAxis) +
      xLabels(labels, xs, H, P) +
      prevLine +
      '<path d="' + area + '" fill="url(#ga)"/>' +
      '<path d="' + line(cur) + '" fill="none" stroke="var(--bs-primary)" stroke-width="2.4"/>' +
      dots +
      '<line id="hl" x1="0" x2="0" y1="' + P.t + '" y2="' + (H - P.b) + '" stroke="var(--sc-chart-grid)" opacity="0"/>' +
      '</svg>';

    var svg = el.querySelector('svg');
    var hl = svg.querySelector('#hl');
    var tip = tipFor(el);

    svg.addEventListener('mousemove', function (e) {
      var r = svg.getBoundingClientRect();
      var x = e.clientX - r.left;
      var i = Math.round((x - P.l) / (W - P.l - P.r) * (labels.length - 1));
      i = Math.max(0, Math.min(labels.length - 1, i));
      hl.setAttribute('x1', xs(i));
      hl.setAttribute('x2', xs(i));
      hl.setAttribute('opacity', '1');

      var prevV = prev && prev[i] ? prev[i] : 0;
      var d = prevV ? ((cur[i] - prevV) / prevV * 100) : 0;
      var deltaText = prevV ? '<br><span>Prev: ' + inr0.format(prevV) + ' (' + (d >= 0 ? '+' : '') + d.toFixed(1) + '%)</span>' : '';
      tip.innerHTML = '<b>' + esc(labels[i]) + '</b>' + inr0.format(cur[i]) + deltaText;
      tip.style.left = xs(i) + 'px';
      tip.style.top = ys(cur[i]) + 'px';
      tip.style.opacity = '1';
    });

    svg.addEventListener('mouseleave', function () {
      tip.style.opacity = '0';
      hl.setAttribute('opacity', '0');
    });
  }

  function columnChart(el, labels, seriesArr, colors, fmt, tipFmt, names) {
    fmt = fmt || inrAxis;
    tipFmt = tipFmt || inr0.format;
    names = names || [];
    el.innerHTML = '';
    var W = el.clientWidth || 600;
    var H = el.clientHeight || 220;
    var P = { l: 52, r: 8, t: 12, b: 28 };

    var flat = [];
    seriesArr.forEach(function (s) { flat = flat.concat(s); });
    var maxVal = Math.max(1, Math.max.apply(null, flat));
    var max = niceMax(maxVal * 1.05);

    var n = labels.length;
    var band = n > 0 ? (W - P.l - P.r) / n : 1;
    var gw = band * 0.7;
    var bw = gw / (seriesArr.length || 1);

    var xs = function (i) { return P.l + band * i + band / 2; };
    var ys = function (v) { return P.t + (H - P.t - P.b) * (1 - v / max); };

    var bars = '';
    labels.forEach(function (l, i) {
      seriesArr.forEach(function (s, k) {
        var x = P.l + band * i + (band - gw) / 2 + bw * k;
        var y = ys(s[i] || 0);
        bars += '<rect data-i="' + i + '" x="' + x.toFixed(1) + '" y="' + y.toFixed(1) + '" width="' + Math.max(2, bw - 2).toFixed(1) + '" height="' + (ys(0) - y).toFixed(1) + '" rx="3" fill="' + colors[k] + '"/>';
      });
    });

    el.innerHTML = '<svg width="' + W + '" height="' + H + '" role="img" aria-label="Column chart">' +
      axes(W, H, P, max, 4, fmt) +
      xLabels(labels, xs, H, P) +
      bars +
      '</svg>';

    var svg = el.querySelector('svg');
    var tip = tipFor(el);

    svg.addEventListener('mousemove', function (e) {
      var r = svg.getBoundingClientRect();
      var i = Math.floor((e.clientX - r.left - P.l) / band);
      if (i < 0 || i >= n) {
        tip.style.opacity = '0';
        return;
      }
      var lines = seriesArr.map(function (s, k) {
        return (names[k] ? esc(names[k]) + ': ' : '') + tipFmt(s[i] || 0);
      }).join('<br>');
      tip.innerHTML = '<b>' + esc(labels[i]) + '</b>' + lines;
      tip.style.left = xs(i) + 'px';
      var highestY = Math.max.apply(null, seriesArr.map(function (s) { return s[i] || 0; }));
      tip.style.top = ys(highestY) + 'px';
      tip.style.opacity = '1';
    });

    svg.addEventListener('mouseleave', function () {
      tip.style.opacity = '0';
    });
  }

  function donut(el, items, centerTop, centerBottom) {
    var total = items.reduce(function (a, b) { return a + b.v; }, 0);
    var R = 58;
    var C = 2 * Math.PI * R;
    var off = 0;

    var arcs = items.map(function (it) {
      var len = total ? (it.v / total * C) : 0;
      var titleText = esc(it.l) + ': ' + (it.fmt ? it.fmt(it.v) : inr0.format(it.v));
      var s = '<circle r="' + R + '" cx="80" cy="80" fill="none" stroke="' + it.c + '" stroke-width="22" stroke-dasharray="' + len + ' ' + (C - len) + '" stroke-dashoffset="' + (-off) + '" transform="rotate(-90 80 80)"><title>' + titleText + '</title></circle>';
      off += len;
      return s;
    }).join('');

    el.innerHTML = '<div class="donut-wrap"><svg width="160" height="160" viewBox="0 0 160 160" role="img" aria-label="Donut chart">' + arcs +
      '<text x="80" y="76" text-anchor="middle" font-size="16" font-weight="700" fill="var(--sc-text-primary)">' + esc(centerTop) + '</text>' +
      '<text x="80" y="96" text-anchor="middle" font-size="11" fill="var(--sc-text-secondary)">' + esc(centerBottom) + '</text></svg>' +
      '<div class="donut-legend">' + items.map(function (it) {
        var pct = total ? Math.round(it.v / total * 100) : 0;
        var displayVal = it.fmt ? it.fmt(it.v) : inrShort(it.v);
        return '<div><i class="sw" style="background:' + it.c + '"></i>' + esc(it.l) + '<b>' + esc(displayVal) + '</b><small>' + pct + '%</small></div>';
      }).join('') + '</div></div>';
  }

  function hbars(el, items, fmt, color) {
    fmt = fmt || inrShort;
    color = color || 'var(--c1)';
    var max = Math.max.apply(null, items.map(function (i) { return i.v; }).concat([1]));

    el.innerHTML = '<div class="hbars">' + items.map(function (it) {
      var pct = (it.v / max * 100).toFixed(1);
      return '<div class="hb"><div class="top"><span>' + esc(it.l) + '</span><b>' + esc(fmt(it.v)) + '</b></div>' +
             '<div class="track"><div class="fill" style="width:' + pct + '%;background:' + (it.c || color) + '"></div></div></div>';
    }).join('') + '</div>';
  }

  function spark(arr, color) {
    if (!arr || !arr.length) return '';
    var W = 140;
    var H = 30;
    var max = Math.max.apply(null, arr);
    var min = Math.min.apply(null, arr);
    var xs = function (i) { return i / (arr.length - 1) * W; };
    var ys = function (v) { return H - 2 - (max === min ? 0.5 : (v - min) / (max - min)) * (H - 4); };

    var d = arr.map(function (v, i) {
      return (i ? 'L' : 'M') + xs(i).toFixed(1) + ',' + ys(v).toFixed(1);
    }).join('');

    return '<svg class="spark" width="100%" height="' + H + '" viewBox="0 0 ' + W + ' ' + H + '" preserveAspectRatio="none">' +
      '<path d="' + d + '" fill="none" stroke="' + color + '" stroke-width="1.8" vector-effect="non-scaling-stroke"/></svg>';
  }

  function emptyState(msg) {
    return '<div class="empty">' + svgI('inbox') + esc(msg) + '</div>';
  }

  function deltaPill(p, invert) {
    var num = Number(p) || 0;
    var good = invert ? num < 0 : num >= 0;
    return '<span class="delta ' + (good ? 'up' : 'down') + '">' + (num >= 0 ? '▲' : '▼') + ' ' + Math.abs(num).toFixed(1) + '%</span>';
  }

  window.DashCharts = {
    ICONS: ICONS,
    svgI: svgI,
    hydrate: hydrate,
    esc: esc,
    inr0: inr0,
    n0: n0,
    inrShort: inrShort,
    inrAxis: inrAxis,
    areaChart: areaChart,
    columnChart: columnChart,
    donut: donut,
    hbars: hbars,
    spark: spark,
    emptyState: emptyState,
    deltaPill: deltaPill
  };
})(window);

