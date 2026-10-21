/**
 * Company profile: AJAX save of the InCompanyUpdateView form.
 * Relies on window.SmartCartAjax (sc-ajax.js) and jQuery from the layout. No Bootstrap JS required.
 *
 * Hooks
 *   #companyForm   posts identity, tax, contact and address; field errors use the input names
 */
(function (window, $) {
  'use strict';

  function initCompanyForm() {
    var $form = $('#companyForm');
    if (!$form.length || !window.SmartCartAjax) return;

    $form.on('submit', function (e) {
      e.preventDefault();
      window.SmartCartAjax.submitForm($form);
    });
  }

  $(initCompanyForm);
})(window, window.jQuery);
