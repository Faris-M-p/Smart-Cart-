/**
 * Modules page: feature tree, presets and save.
 * Relies on window.SmartCartAjax (sc-ajax.js) and jQuery from the layout.
 */
(function (window, $) {
  'use strict';

  var ICONS = {
    DASHBOARD: 'bi-speedometer2',
    PRODUCTS: 'bi-box-seam',
    INVENTORY: 'bi-boxes',
    PURCHASES: 'bi-truck',
    POS: 'bi-cash-register',
    CUSTOMERS: 'bi-people',
    REPORTS: 'bi-graph-up',
    SETTINGS: 'bi-gear',
    USERS: 'bi-person-badge'
  };

  function init() {
    var page = document.getElementById('moduleFeaturePage');
    var dataEl = document.getElementById('moduleFeatureData');
    if (!page || !dataEl || !window.SmartCartAjax) return;

    var payload = JSON.parse(dataEl.textContent || '{}');
    var modules = payload.modules || [];
    var presets = payload.presets || [];
    var expanded = new Set();
    var query = '';
    var current = enforce(new Set(payload.enabledFeatureCodes || []));
    var saved = new Set(current);
    var presetCode = matchPreset(current);
    var focusId = null;

    var treeEl = document.getElementById('moduleFeatureTree');
    var cardsEl = document.getElementById('moduleFeatureCards');
    var search = document.getElementById('moduleFeatureSearch');
    var clearBtn = document.getElementById('moduleFeatureClear');
    var toggleBtn = document.getElementById('moduleFeatureToggle');
    var statusEl = document.getElementById('moduleFeatureStatus');
    var saveBtn = document.getElementById('moduleFeatureSave');
    var discardBtn = document.getElementById('moduleFeatureDiscard');

    function leavesOf(node) {
      if (!node) return [];
      if (node.features) return node.features.flatMap(leavesOf);
      if (node.children && node.children.length) return node.children.flatMap(leavesOf);
      if (node.isGroup) return [];
      return [node];
    }

    function allLeaves() {
      return modules.flatMap(leavesOf);
    }

    function enforce(set) {
      var next = new Set();
      modules.forEach(function (mod) {
        var leaves = leavesOf(mod);
        var any = leaves.some(function (leaf) { return set.has(leaf.featureCode); });
        if (!mod.isCore && !any) return;
        leaves.forEach(function (leaf) {
          if (leaf.isCore || set.has(leaf.featureCode)) next.add(leaf.featureCode);
        });
      });
      return next;
    }

    function sameSet(a, b) {
      if (a.size !== b.size) return false;
      var ok = true;
      a.forEach(function (code) { if (!b.has(code)) ok = false; });
      return ok;
    }

    function matchPreset(set) {
      var found = 'CUSTOM';
      presets.forEach(function (preset) {
        var codes = new Set(enforce(new Set(preset.featureCodes || [])));
        if (sameSet(set, codes)) found = (preset.presetCode || '').toUpperCase();
      });
      return found;
    }

    function stats(node) {
      var leaves = leavesOf(node);
      var on = leaves.filter(function (leaf) { return current.has(leaf.featureCode); }).length;
      var optional = leaves.filter(function (leaf) { return !leaf.isCore; });
      return {
        total: leaves.length,
        on: on,
        optionalOn: optional.filter(function (leaf) { return current.has(leaf.featureCode); }).length,
        optionalTotal: optional.length
      };
    }

    function esc(text) {
      return String(text == null ? '' : text).replace(/[&<>"']/g, function (ch) {
        return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[ch];
      });
    }

    function highlight(text) {
      var safe = esc(text);
      if (!query) return safe;
      var re = new RegExp('(' + query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&') + ')', 'ig');
      return safe.replace(re, '<mark>$1</mark>');
    }

    function selfMatch(node) {
      if (!query) return false;
      var name = (node.featureName || node.moduleName || '').toLowerCase();
      var desc = (node.description || '').toLowerCase();
      return name.indexOf(query) >= 0 || desc.indexOf(query) >= 0;
    }

    function subMatch(node) {
      if (selfMatch(node)) return true;
      var kids = node.features || node.children || [];
      return kids.some(subMatch);
    }

    function renderNode(node, level, ancestorMatched) {
      var kids = node.features || node.children || [];
      var leaf = !kids.length;
      var kidsMatch = query && kids.some(subMatch);
      if (query && !ancestorMatched && !selfMatch(node) && !kidsMatch) return '';

      var st = stats(node);
      var id = node.featureCode || node.moduleCode;
      var open = !leaf && (query ? (kidsMatch || expanded.has(id)) : expanded.has(id));
      var checked = st.total > 0 && st.on === st.total;
      var indet = st.on > 0 && !checked;
      var locked = leaf ? !!node.isCore : (st.optionalTotal === 0 && (node.isCore || level > 1));
      var state = st.on === 0 ? 'state-off' : (checked ? 'state-full' : 'state-part');
      var name = node.featureName || node.moduleName || '';
      var typeBadge = (node.isCore && (leaf || level === 1))
        ? '<span class="badge text-bg-primary">Core</span>'
        : '<span class="badge text-bg-light border">Optional</span>';
      var count = '';
      if (!leaf) {
        var pct = st.total ? Math.round(st.on / st.total * 100) : 0;
        count = '<div class="sc-feature-count"><b>' + st.on + '</b> of ' + st.total + ' enabled' +
          (level === 1 ? '<div class="sc-feature-bar"><i style="width:' + pct + '%"></i></div>' : '') +
          '</div>';
      }
      var caret = leaf
        ? '<span class="sc-feature-caret-sp"></span>'
        : '<button type="button" class="sc-feature-caret" data-act="caret" aria-expanded="' + open + '" aria-label="' + (open ? 'Collapse ' : 'Expand ') + esc(name) + '"><i class="bi bi-chevron-right"></i></button>';
      var icon = level === 1
        ? '<span class="sc-feature-ic"><i class="bi ' + (ICONS[node.moduleCode] || 'bi-grid') + '"></i></span>'
        : '';
      var lock = locked ? ' <i class="bi bi-lock text-secondary" title="Core — always enabled"></i>' : '';
      var pass = ancestorMatched || selfMatch(node);
      var kidsHtml = leaf ? '' : '<div class="sc-feature-kids">' + kids.map(function (child) {
        return renderNode(child, level + 1, pass);
      }).join('') + '</div>';

      return '<div class="sc-feature-node l' + level + ' ' + (leaf ? 'leaf' : '') + ' ' + (open ? 'open' : '') + ' ' + state + '" data-id="' + esc(id) + '">' +
        '<div class="sc-feature-row">' +
          '<div class="sc-feature-main">' + caret +
            '<input type="checkbox" class="form-check-input m-0" data-act="cb" id="feat-' + esc(id) + '"' +
              (checked ? ' checked' : '') + (locked ? ' disabled' : '') + ' data-indet="' + indet + '" aria-label="' + esc(name) + '">' +
            icon +
            '<div class="min-w-0">' +
              '<div class="sc-feature-name">' + highlight(name) + lock + '</div>' +
              (node.description ? '<div class="sc-feature-desc">' + highlight(node.description) + '</div>' : '') +
            '</div>' +
          '</div>' +
          '<div class="sc-feature-type">' + typeBadge + '</div>' +
          '<div>' + count + '</div>' +
        '</div>' + kidsHtml + '</div>';
    }

    function renderCards() {
      var leaves = allLeaves();
      var on = leaves.filter(function (leaf) { return current.has(leaf.featureCode); }).length;
      var core = leaves.filter(function (leaf) { return leaf.isCore; }).length;
      var cards = [
        ['Total features', leaves.length, 'Across ' + modules.length + ' modules', 'bi-grid', 'text-primary'],
        ['Enabled features', on, (leaves.length ? Math.round(on / leaves.length * 100) : 0) + '% of all features', 'bi-check-circle', 'text-success'],
        ['Disabled features', leaves.length - on, 'Optional items switched off', 'bi-toggle-off', 'text-secondary'],
        ['Core features', core, 'Always enabled while the module is on', 'bi-lock', 'text-primary']
      ];
      cardsEl.innerHTML = cards.map(function (card) {
        return '<div class="col-6 col-xl-3"><div class="card border-0 shadow-sm h-100"><div class="card-body d-flex align-items-center gap-3">' +
          '<i class="bi ' + card[3] + ' fs-3 ' + card[4] + '"></i><div><div class="text-muted small">' + esc(card[0]) +
          '</div><div class="fs-4 fw-semibold">' + card[1] + '</div><div class="small text-muted">' + esc(card[2]) +
          '</div></div></div></div></div>';
      }).join('');
    }

    function renderPresets() {
      page.querySelectorAll('[data-preset]').forEach(function (btn) {
        var active = btn.getAttribute('data-preset') === presetCode;
        btn.classList.toggle('btn-primary', active && btn.getAttribute('data-preset') !== 'CUSTOM');
        btn.classList.toggle('btn-outline-primary', !active && btn.getAttribute('data-preset') !== 'CUSTOM');
        btn.classList.toggle('btn-secondary', active && btn.getAttribute('data-preset') === 'CUSTOM');
        btn.classList.toggle('btn-outline-secondary', !active && btn.getAttribute('data-preset') === 'CUSTOM');
        btn.setAttribute('aria-pressed', active ? 'true' : 'false');
      });
    }

    function renderToggle() {
      var optional = allLeaves().filter(function (leaf) { return !leaf.isCore; });
      var allOn = optional.every(function (leaf) { return current.has(leaf.featureCode); });
      toggleBtn.textContent = allOn ? 'Clear Optional' : 'Enable All Optional';
      toggleBtn.setAttribute('aria-pressed', allOn ? 'true' : 'false');
    }

    function renderSave() {
      var dirty = !sameSet(current, saved);
      statusEl.classList.toggle('dirty', dirty);
      statusEl.querySelector('[data-status-text]').textContent = dirty ? 'Unsaved changes' : 'All changes saved';
      saveBtn.disabled = !dirty;
      discardBtn.disabled = !dirty;
    }

    function renderTree() {
      var html = modules.map(function (mod) { return renderNode(mod, 1, false); }).join('');
      treeEl.innerHTML = html || '<div class="text-center text-muted py-5"><i class="bi bi-search d-block fs-3 mb-2"></i><b>No modules or features match this search.</b></div>';
      treeEl.querySelectorAll('input[data-act="cb"]').forEach(function (box) {
        box.indeterminate = box.getAttribute('data-indet') === 'true';
      });
      if (focusId) {
        var el = document.getElementById(focusId);
        if (el) el.focus();
        focusId = null;
      }
    }

    function renderAll() {
      renderTree();
      renderCards();
      renderPresets();
      renderToggle();
      renderSave();
    }

    function findNode(id, list) {
      list = list || modules;
      for (var i = 0; i < list.length; i++) {
        var node = list[i];
        if ((node.featureCode || node.moduleCode) === id) return node;
        var kids = node.features || node.children || [];
        var found = findNode(id, kids);
        if (found) return found;
      }
      return null;
    }

    function applySet(set, nextPreset) {
      current = enforce(set);
      presetCode = nextPreset || matchPreset(current);
      renderAll();
    }

    treeEl.addEventListener('click', function (e) {
      var act = e.target.closest('[data-act="caret"], .sc-feature-name');
      if (!act || act.closest('input')) return;
      var nodeEl = act.closest('.sc-feature-node');
      if (!nodeEl || nodeEl.classList.contains('leaf')) return;
      var id = nodeEl.getAttribute('data-id');
      if (expanded.has(id)) expanded.delete(id); else expanded.add(id);
      renderTree();
    });

    treeEl.addEventListener('change', function (e) {
      var box = e.target;
      if (!box.matches('input[data-act="cb"]')) return;
      var nodeEl = box.closest('.sc-feature-node');
      var node = findNode(nodeEl.getAttribute('data-id'));
      if (!node) return;
      focusId = box.id;
      var next = new Set(current);
      var leaves = leavesOf(node);
      if (!node.features && !(node.children && node.children.length)) {
        if (node.isCore) {
          box.checked = true;
          return;
        }
        if (box.checked) next.add(node.featureCode); else next.delete(node.featureCode);
      } else if (node.moduleCode && !node.featureCode) {
        var anyOn = leaves.some(function (leaf) { return current.has(leaf.featureCode); });
        leaves.forEach(function (leaf) { next.delete(leaf.featureCode); });
        if (node.isCore) {
          leaves.forEach(function (leaf) { if (leaf.isCore) next.add(leaf.featureCode); });
          var optional = leaves.filter(function (leaf) { return !leaf.isCore; });
          var allOpt = optional.every(function (leaf) { return current.has(leaf.featureCode); });
          if (!allOpt) optional.forEach(function (leaf) { next.add(leaf.featureCode); });
        } else if (!anyOn) {
          leaves.forEach(function (leaf) { if (leaf.isCore) next.add(leaf.featureCode); });
        }
      } else {
        var optionalKids = leaves.filter(function (leaf) { return !leaf.isCore; });
        var allKids = optionalKids.every(function (leaf) { return current.has(leaf.featureCode); });
        optionalKids.forEach(function (leaf) {
          if (allKids) next.delete(leaf.featureCode); else next.add(leaf.featureCode);
        });
      }
      applySet(next, 'CUSTOM');
      presetCode = matchPreset(current);
      renderPresets();
    });

    page.querySelectorAll('[data-preset]').forEach(function (btn) {
      btn.addEventListener('click', function () {
        var code = btn.getAttribute('data-preset');
        if (code === 'CUSTOM') {
          presetCode = 'CUSTOM';
          renderPresets();
          return;
        }
        var preset = presets.find(function (item) {
          return (item.presetCode || '').toUpperCase() === code;
        });
        if (!preset) return;
        current = enforce(new Set(preset.featureCodes || []));
        presetCode = matchPreset(current);
        renderAll();
      });
    });

    toggleBtn.addEventListener('click', function () {
      var optional = allLeaves().filter(function (leaf) { return !leaf.isCore; });
      var allOn = optional.every(function (leaf) { return current.has(leaf.featureCode); });
      var next = new Set(current);
      optional.forEach(function (leaf) {
        if (allOn) next.delete(leaf.featureCode); else next.add(leaf.featureCode);
      });
      applySet(next, 'CUSTOM');
      presetCode = matchPreset(current);
      renderPresets();
    });

    document.getElementById('moduleFeatureExpand').addEventListener('click', function () {
      function walk(list) {
        (list || []).forEach(function (node) {
          var kids = node.features || node.children || [];
          if (kids.length) expanded.add(node.featureCode || node.moduleCode);
          walk(kids);
        });
      }
      walk(modules);
      renderTree();
    });

    document.getElementById('moduleFeatureCollapse').addEventListener('click', function () {
      expanded.clear();
      renderTree();
    });

    discardBtn.addEventListener('click', function () {
      current = new Set(saved);
      presetCode = matchPreset(current);
      renderAll();
    });

    saveBtn.addEventListener('click', function () {
      window.SmartCartAjax.postJson(page.getAttribute('data-save-url'), {
        enabledFeatureCodes: Array.from(current)
      }, {
        onSuccess: function (res) {
          var codes = res && res.data && res.data.enabledFeatureCodes;
          saved = enforce(new Set(codes || Array.from(current)));
          current = new Set(saved);
          presetCode = matchPreset(current);
          renderAll();
        }
      });
    });

    search.addEventListener('input', function () {
      query = search.value.trim().toLowerCase();
      clearBtn.classList.toggle('d-none', !search.value);
      renderTree();
    });
    clearBtn.addEventListener('click', function () {
      search.value = '';
      query = '';
      clearBtn.classList.add('d-none');
      renderTree();
      search.focus();
    });
    search.addEventListener('keydown', function (e) {
      if (e.key === 'Escape') clearBtn.click();
    });

    window.addEventListener('beforeunload', function (e) {
      if (sameSet(current, saved)) return;
      e.preventDefault();
      e.returnValue = '';
    });

    renderAll();
  }

  $(init);
})(window, window.jQuery);
