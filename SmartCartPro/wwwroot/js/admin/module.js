/**
 * Module assignment: save toggles. Basic/Premium presets use declarative data-sc-post on the page.
 * Relies on window.SmartCartAjax (sc-ajax.js) and jQuery from the layout. No Bootstrap JS required.
 *
 * Hooks
 *   #moduleForm   posts EnabledModuleIds[]; core modules are hidden+checked so they always save
 */
(function (window, $) {
  'use strict';

  function initModuleForm() {
    var $form = $('#moduleForm');
    if (!$form.length || !window.SmartCartAjax) return;

    $form.on('submit', function (e) {
      e.preventDefault();
      window.SmartCartAjax.submitForm($form);
    });
  }

  $(initModuleForm);
})(window, window.jQuery);
