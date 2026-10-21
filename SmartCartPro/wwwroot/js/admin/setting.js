/**
 * Settings page: AJAX save of the known keys for the company or the signed-in store.
 * Relies on window.SmartCartAjax (sc-ajax.js) and jQuery from the layout. No Bootstrap JS required.
 *
 * Hooks
 *   #settingsForm              posts Scope + Values[<setting.key>]; server errors use the same field names
 *   [data-sc-setting]          one setting row (highlighted while its value differs from the saved one)
 *   [data-sc-dirty-note]       "Unsaved changes" hint next to the save button
 */
(function (window, $) {
  'use strict';

  if (!$) return;
  var ajax = window.SmartCartAjax;

  function initSettingsForm() {
    var $form = $('#settingsForm');
    if (!$form.length || !ajax) return;

    var $fields = $form.find('[name^="Values["]');
    var $note = $form.find('[data-sc-dirty-note]');
    var initial = {};
    var dirty = false;

    $fields.each(function () {
      initial[this.name] = $(this).val();
    });

    function refreshDirty() {
      dirty = false;
      $fields.each(function () {
        var changed = $(this).val() !== initial[this.name];
        $(this).closest('[data-sc-setting]').toggleClass('bg-light', changed);
        if (changed) dirty = true;
      });
      $note.toggleClass('d-none', !dirty);
    }

    $fields.on('input change', refreshDirty);

    // Warn before leaving the page with edits that were not saved.
    window.addEventListener('beforeunload', function (e) {
      if (!dirty) return;
      e.preventDefault();
      e.returnValue = '';
    });

    $form.on('submit', function (e) {
      e.preventDefault();
      ajax.submitForm($form, {
        onSuccess: function () {
          // The helper reloads the page right after this callback; do not prompt for the edits just saved.
          dirty = false;
        }
      });
    });
  }

  $(initSettingsForm);
})(window, window.jQuery);
