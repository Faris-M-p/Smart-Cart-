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
    var $panel = $('#companyEditPanel');
    var $profile = $('#companyProfile');
    var $edit = $('#companyEditButton');
    var $cancel = $('#companyCancelButton');
    var submitting = false;

    if ($edit.length) {
      $edit.on('click', function () {
        if (!$form.length || submitting) return;
        $profile.addClass('d-none');
        $panel.removeClass('d-none');
        $edit.attr('aria-expanded', 'true');
        var firstField = $form.find('input:not([type="hidden"]), select, textarea').first();
        if (firstField.length) firstField.trigger('focus');
      });
    }

    if ($cancel.length) {
      $cancel.on('click', function () {
        if (submitting) return;
        $form[0].reset();
        if (window.SmartCartAjax) window.SmartCartAjax.clearFieldErrors($form);
        $form.find('[data-sc-form-summary]').addClass('d-none').empty();
        $panel.addClass('d-none');
        $profile.removeClass('d-none');
        $edit.attr('aria-expanded', 'false').trigger('focus');
      });
    }

    if (!$form.length || !window.SmartCartAjax) return;

    $form.on('submit', function (e) {
      e.preventDefault();
      if (submitting) return;
      submitting = true;
      window.SmartCartAjax.submitForm($form).always(function () {
        submitting = false;
      });
    });
  }

  $(initCompanyForm);
})(window, window.jQuery);
