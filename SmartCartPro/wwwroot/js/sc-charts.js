/**
 * SmartCart Pro SVG charts (window.SmartCharts).
 * Metis ApexCharts is an ES module without a global — do not depend on it.
 * Uses Bootstrap CSS variables, SVG viewBox, title tooltips, role=img,
 * and a visually-hidden table fallback.
 */
(function (window) {
  'use strict';

  var NS = 'http://www.w3.org/2000/svg';

  function cssVar(name, fallback) {
    var value = '';
    try { value = getComputedStyle(document.documentElement).getPropertyValue(name).trim(); } catch (e) { value = ''; }
    return value || fallback;
  }

  function palette() {
    return [
      cssVar('--bs-primary', '#0d6efd'),
      cssVar('--bs-success', '#198754'),
      cssVar('--bs-info', '#0dcaf0'),
      cssVar('--bs-warning', '#ffc107'),
      cssVar('--bs-danger', '#dc3545'),
      cssVar('--bs-secondary', '#6c757d'),
      cssVar('--bs-purple', '#6f42c1'),
      cssVar('--bs-teal', '#20c997')
    ];
  }

  function readData(el, data) {
    if (Array.isArray(data)) return data;
    var raw = el.getAttribute('data-sc-chart-json');
    if (!raw) return [];
    try {
      var parsed = JSON.parse(raw);
      return Array.isArray(parsed) ? parsed : [];
    } catch (e) {
      return [];
    }
  }

  function points(data) {
    return (data || []).map(function (item) {
      if (item == null) return { label: '', value: 0 };
      if (typeof item === 'number') return { label: String(item), value: Number(item) || 0 };
      return {
        label: item.label != null ? String(item.label) : (item.Label != null ? String(item.Label) : ''),
        value: Number(item.value != null ? item.value : item.Value) || 0
      };
    });
  }

  function clear(el) {
    while (el.firstChild) el.removeChild(el.firstChild);
  }

  function svgEl(name, attrs) {
    var node = document.createElementNS(NS, name);
    Object.keys(attrs || {}).forEach(function (key) {
      node.setAttribute(key, attrs[key]);
    });
    return node;
  }

  function title(text) {
    var node = svgEl('title');
    node.textContent = text;
    return node;
  }

  function fallbackTable(el, rows, caption) {
    var table = document.createElement('table');
    table.className = 'visually-hidden';
    var cap = document.createElement('caption');
    cap.textContent = caption || 'Chart data';
    table.appendChild(cap);
    var thead = document.createElement('thead');
    var hr = document.createElement('tr');
    ['Label', 'Value'].forEach(function (h) {
      var th = document.createElement('th');
      th.textContent = h;
      hr.appendChild(th);
    });
    thead.appendChild(hr);
    table.appendChild(thead);
    var tbody = document.createElement('tbody');
    rows.forEach(function (row) {
      var tr = document.createElement('tr');
      var td1 = document.createElement('td');
      td1.textContent = row.label;
      var td2 = document.createElement('td');
      td2.textContent = String(row.value);
      tr.appendChild(td1);
      tr.appendChild(td2);
      tbody.appendChild(tr);
    });
    table.appendChild(tbody);
    el.appendChild(table);
  }

  function emptyMessage(svg, message) {
    var text = svgEl('text', {
      x: '200',
      y: '120',
      'text-anchor': 'middle',
      fill: cssVar('--bs-secondary-color', '#6c757d'),
      'font-size': '14'
    });
    text.textContent = message || 'No data';
    svg.appendChild(text);
  }

  function renderFrame(el, caption) {
    el.classList.add('sc-chart');
    var svg = svgEl('svg', {
      viewBox: '0 0 400 240',
      role: 'img',
      'aria-label': caption || 'Chart',
      focusable: 'false'
    });
    el.appendChild(svg);
    return svg;
  }

  function bars(el, data, opts) {
    opts = opts || {};
    var rows = points(readData(el, data));
    var caption = opts.title || el.getAttribute('data-sc-chart-title') || 'Bar chart';
    clear(el);
    fallbackTable(el, rows, caption);
    var svg = renderFrame(el, caption);
    if (!rows.length || rows.every(function (r) { return r.value === 0; })) {
      emptyMessage(svg, opts.emptyText);
      return;
    }
    var max = Math.max.apply(null, rows.map(function (r) { return Math.abs(r.value); })) || 1;
    var colors = palette();
    var left = 36;
    var right = 12;
    var top = 16;
    var bottom = 48;
    var width = 400 - left - right;
    var height = 240 - top - bottom;
    var gap = 6;
    var barW = Math.max(8, (width / rows.length) - gap);

    rows.forEach(function (row, i) {
      var h = Math.max(1, (Math.abs(row.value) / max) * height);
      var x = left + i * (barW + gap);
      var y = top + height - h;
      var rect = svgEl('rect', {
        x: String(x),
        y: String(y),
        width: String(barW),
        height: String(h),
        rx: '3',
        fill: colors[i % colors.length]
      });
      rect.appendChild(title(row.label + ': ' + row.value));
      svg.appendChild(rect);

      var label = svgEl('text', {
        x: String(x + barW / 2),
        y: '228',
        'text-anchor': 'middle',
        fill: cssVar('--bs-body-color', '#212529'),
        'font-size': '9'
      });
      label.textContent = row.label.length > 10 ? row.label.slice(0, 9) + '…' : row.label;
      svg.appendChild(label);
    });
  }

  function line(el, data, opts) {
    opts = opts || {};
    var rows = points(readData(el, data));
    var caption = opts.title || el.getAttribute('data-sc-chart-title') || 'Line chart';
    clear(el);
    fallbackTable(el, rows, caption);
    var svg = renderFrame(el, caption);
    if (!rows.length) {
      emptyMessage(svg, opts.emptyText);
      return;
    }
    var max = Math.max.apply(null, rows.map(function (r) { return r.value; }));
    if (max <= 0) max = 1;
    var left = 36;
    var right = 12;
    var top = 16;
    var bottom = 40;
    var width = 400 - left - right;
    var height = 240 - top - bottom;
    var step = rows.length === 1 ? 0 : width / (rows.length - 1);
    var color = palette()[0];
    var coords = rows.map(function (row, i) {
      return {
        x: left + i * step,
        y: top + height - (row.value / max) * height,
        row: row
      };
    });

    var axis = svgEl('line', {
      x1: String(left),
      y1: String(top + height),
      x2: String(left + width),
      y2: String(top + height),
      stroke: cssVar('--bs-border-color', '#dee2e6'),
      'stroke-width': '1'
    });
    svg.appendChild(axis);

    if (coords.length === 1) {
      var single = svgEl('circle', {
        cx: String(coords[0].x),
        cy: String(coords[0].y),
        r: '4',
        fill: color
      });
      single.appendChild(title(coords[0].row.label + ': ' + coords[0].row.value));
      svg.appendChild(single);
    } else {
      var d = coords.map(function (c, i) {
        return (i === 0 ? 'M' : 'L') + c.x + ' ' + c.y;
      }).join(' ');
      var path = svgEl('path', {
        d: d,
        fill: 'none',
        stroke: color,
        'stroke-width': '2.5',
        'stroke-linejoin': 'round',
        'stroke-linecap': 'round'
      });
      svg.appendChild(path);
      coords.forEach(function (c) {
        var dot = svgEl('circle', { cx: String(c.x), cy: String(c.y), r: '3.5', fill: color });
        dot.appendChild(title(c.row.label + ': ' + c.row.value));
        svg.appendChild(dot);
      });
    }

    rows.forEach(function (row, i) {
      if (rows.length > 10 && i % Math.ceil(rows.length / 8) !== 0) return;
      var label = svgEl('text', {
        x: String(left + i * step),
        y: '228',
        'text-anchor': 'middle',
        fill: cssVar('--bs-body-color', '#212529'),
        'font-size': '9'
      });
      label.textContent = row.label.length > 8 ? row.label.slice(0, 7) + '…' : row.label;
      svg.appendChild(label);
    });
  }

  function polar(cx, cy, r, angle) {
    return {
      x: cx + r * Math.cos(angle),
      y: cy + r * Math.sin(angle)
    };
  }

  function donut(el, data, opts) {
    opts = opts || {};
    var rows = points(readData(el, data)).filter(function (r) { return r.value > 0; });
    var caption = opts.title || el.getAttribute('data-sc-chart-title') || 'Donut chart';
    clear(el);
    fallbackTable(el, rows, caption);
    var svg = renderFrame(el, caption);
    var total = rows.reduce(function (sum, r) { return sum + r.value; }, 0);
    if (!rows.length || total <= 0) {
      emptyMessage(svg, opts.emptyText);
      return;
    }

    var colors = palette();
    var cx = 130;
    var cy = 120;
    var outer = 78;
    var inner = 44;
    var angle = -Math.PI / 2;

    rows.forEach(function (row, i) {
      var slice = (row.value / total) * Math.PI * 2;
      var start = angle;
      var end = angle + slice;
      angle = end;
      var large = slice > Math.PI ? 1 : 0;
      var p1 = polar(cx, cy, outer, start);
      var p2 = polar(cx, cy, outer, end);
      var p3 = polar(cx, cy, inner, end);
      var p4 = polar(cx, cy, inner, start);
      var d = [
        'M', p1.x, p1.y,
        'A', outer, outer, 0, large, 1, p2.x, p2.y,
        'L', p3.x, p3.y,
        'A', inner, inner, 0, large, 0, p4.x, p4.y,
        'Z'
      ].join(' ');
      var path = svgEl('path', { d: d, fill: colors[i % colors.length] });
      path.appendChild(title(row.label + ': ' + row.value));
      svg.appendChild(path);
    });

    rows.forEach(function (row, i) {
      var y = 28 + i * 18;
      if (y > 220) return;
      var sw = svgEl('rect', {
        x: '230',
        y: String(y - 8),
        width: '10',
        height: '10',
        rx: '2',
        fill: colors[i % colors.length]
      });
      svg.appendChild(sw);
      var text = svgEl('text', {
        x: '246',
        y: String(y),
        fill: cssVar('--bs-body-color', '#212529'),
        'font-size': '11'
      });
      text.textContent = row.label;
      svg.appendChild(text);
    });
  }

  function mount(root) {
    var scope = root || document;
    var nodes = scope.querySelectorAll ? scope.querySelectorAll('[data-sc-chart]') : [];
    Array.prototype.forEach.call(nodes, function (el) {
      var kind = (el.getAttribute('data-sc-chart') || 'bars').toLowerCase();
      if (kind === 'line') line(el);
      else if (kind === 'donut') donut(el);
      else bars(el);
    });
  }

  window.SmartCharts = {
    bars: bars,
    line: line,
    donut: donut,
    mount: mount
  };

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function () { mount(document); });
  } else {
    mount(document);
  }
})(window);
