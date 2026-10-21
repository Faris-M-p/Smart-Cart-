/**
 * Suggested identifiers for admin create/edit forms.
 * Values are suggestions only. Saving still uses the existing server checks.
 * Sale, return, goods-receipt, purchase and online-order numbers are not generated here.
 * Variant names are free text. "Size 8" is not a separate attribute.
 */
(function (window, $) {
  'use strict';

  if (!$) return;
  if (window.SmartCartAutoCode) return;

  var delegated = false;

  function normalize(value, max) {
    var upper = String(value || '').trim().toUpperCase();
    var chars = '';
    var pending = false;
    var i;
    var c;

    for (i = 0; i < upper.length; i++) {
      c = upper.charAt(i);
      if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')) {
        if (pending && chars.length) chars += '-';
        pending = false;
        chars += c;
      } else {
        pending = true;
      }
    }

    if (chars.length > max) chars = chars.slice(0, max).replace(/-+$/, '');
    return chars;
  }

  function clip(text, max) {
    if (text.length <= max) return text;
    return text.slice(0, max).replace(/-+$/, '');
  }

  function withNumericSuffix(text, suffix, max) {
    var tail = '-' + suffix;
    var room = Math.max(1, max - tail.length);
    var head = text.length <= room ? text : text.slice(0, room).replace(/-+$/, '');
    if (!head) head = 'X';
    return clip(head + tail, max);
  }

  function compose(prefix, body, suffix, max) {
    var head = normalize(prefix, max);
    var text = body || '';

    if (!text && !head) return '';

    if (head) {
      if (!text) text = head;
      else if (text !== head && text.indexOf(head + '-') !== 0) text = head + '-' + text;
    }

    if (suffix && text) {
      var extra = suffix.charAt(0) === '-' ? suffix : '-' + suffix;
      var room = Math.max(1, max - extra.length);
      if (text.length > room) text = text.slice(0, room).replace(/-+$/, '');
      if (text && text.slice(-extra.length) !== extra) text += extra;
    }

    return clip(text, max);
  }

  function maxOf($field) {
    var configured = parseInt($field.attr('data-auto-max'), 10);
    var limit = parseInt($field.attr('maxlength'), 10);
    var max = configured > 0 ? configured : (limit > 0 ? limit : 50);
    if (limit > 0 && limit < max) max = limit;
    return max;
  }

  function sourceValue(key) {
    var $source = $('[data-auto-source="' + key + '"]').first();
    if ($source.length) return $source.val() || '';
    var $byId = $('#' + key);
    return $byId.length ? ($byId.val() || '') : '';
  }

  function stateOf($field) {
    var state = ($field.attr('data-auto-state') || 'auto').toLowerCase();
    if (state === 'auto' || state === 'manual' || state === 'persisted' || state === 'blank') return state;
    return 'manual';
  }

  function setState($field, state) {
    $field.attr('data-auto-state', state);
    $field.attr('data-auto-generated', state === 'auto' ? 'true' : 'false');
  }

  function suggestion($field) {
    var max = maxOf($field);
    var from = $field.attr('data-auto-from');
    var body = from ? normalize(sourceValue(from), max) : '';
    var text = compose($field.attr('data-auto-prefix') || '', body, $field.attr('data-auto-suffix') || '', max);
    var attempt = $field.data('autoAttempt') || 0;
    if (attempt > 1 && text) text = withNumericSuffix(text, attempt, max);
    return text;
  }

  function writeValue($field, value) {
    $field.data('autoWriting', true);
    $field.val(value);
    $field.data('autoSuggestion', value);
    $field.data('autoWriting', false);
  }

  function writeSuggestion($field) {
    writeValue($field, suggestion($field));
    var ownSource = $field.attr('data-auto-source');
    if (ownSource) refreshFrom(ownSource);
  }

  function refreshFrom(sourceKey) {
    $('[data-auto-from="' + sourceKey + '"]').each(function () {
      var $target = $(this);
      if (stateOf($target) !== 'auto') return;
      $target.data('autoAttempt', 0);
      writeSuggestion($target);
    });
  }

  function checkDigit(twelve) {
    var sum = 0;
    var i;
    for (i = 0; i < 12; i++) {
      var digit = twelve.charCodeAt(i) - 48;
      sum += (i % 2 === 0) ? digit : digit * 3;
    }
    return String((10 - (sum % 10)) % 10);
  }

  function random9() {
    var n;
    if (window.crypto && window.crypto.getRandomValues) {
      var bytes = new Uint32Array(1);
      window.crypto.getRandomValues(bytes);
      n = bytes[0] % 1000000000;
    } else {
      n = Math.floor(Math.random() * 1000000000);
    }
    return String(n).padStart(9, '0');
  }

  /** Internal EAN-13 suggestion. A new call returns a different code. It is not a GS1 assignment. */
  function internalEan() {
    var body = '200' + random9();
    return body + checkDigit(body);
  }

  function writeBarcode($field) {
    var current = $field.val() || '';
    var next = internalEan();
    var guard = 0;
    while (next === current && guard < 5) {
      next = internalEan();
      guard += 1;
    }
    setState($field, 'auto');
    writeValue($field, next);
  }

  function bindOnce() {
    if (delegated) return;
    delegated = true;

    $(document).on('input.autoCode', '[data-auto-source]', function () {
      refreshFrom($(this).attr('data-auto-source'));
    });

    $(document).on('input.autoCode', '[data-auto-target]', function () {
      var $field = $(this);
      if ($field.data('autoWriting')) return;
      if (stateOf($field) !== 'auto') return;
      if (($field.val() || '') !== ($field.data('autoSuggestion') || '')) setState($field, 'manual');
    });

    $(document).on('click.autoCode', '[data-auto-regenerate]', function (event) {
      event.preventDefault();
      var $field = $('[data-auto-target="' + $(this).attr('data-auto-regenerate') + '"]').first();
      if (!$field.length) return;
      $field.data('autoAttempt', ($field.data('autoAttempt') || 0) + 1);
      setState($field, 'auto');
      writeSuggestion($field);
    });

    $(document).on('click.autoCode', '[data-auto-barcode]', function (event) {
      event.preventDefault();
      var $field = $('[data-auto-target="' + $(this).attr('data-auto-barcode') + '"]').first();
      if (!$field.length) return;
      writeBarcode($field);
    });
  }

  function init(root) {
    bindOnce();
    $(root || document).find('[data-auto-target]').each(function () {
      var $field = $(this);
      if ($field.data('autoReady')) return;
      $field.data('autoReady', true);
      setState($field, stateOf($field));
      $field.data('autoSuggestion', $field.val() || '');
      if ($field.attr('data-auto-barcode-default') === 'true' && !$.trim($field.val() || '')) {
        writeBarcode($field);
      }
    });
  }

  window.SmartCartAutoCode = { init: init };
  $(function () { init(document); });
}(window, window.jQuery));
