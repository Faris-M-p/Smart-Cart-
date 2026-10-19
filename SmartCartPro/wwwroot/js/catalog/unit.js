/**
 * Unit of measure master pages (list + create/edit form).
 * Needs jQuery and window.SmartCartAjax (sc-ajax.js). Activate / deactivate buttons on the list are
 * declarative (data-sc-post + data-sc-confirm in Units.cshtml), so only the form and list state live here.
 */
(function (window, $) {
  'use strict';

  function initUnitForm() {
    var $form = $('#unitForm');
    if (!$form.length || !window.SmartCartAjax) return;

    $form.on('submit', function (e) {
      e.preventDefault();
      window.SmartCartAjax.submitForm($form);
    });
  }

  function initUnitList() {
    var $page = $('#unitListPage');
    if (!$page.length) return;

    function setLoading(loading) {
      $page.toggleClass('opacity-50', loading);
      if (loading) {
        $page.attr('aria-busy', 'true');
      } else {
        $page.removeAttr('aria-busy');
      }
    }

    // Search / status / page-size changes reload the page: dim the list while the server responds.
    $page.find('form[method="get"]').on('submit', function () {
      setLoading(true);
    });

    // Returning with the browser back button must not show a dimmed list.
    $(window).on('pageshow', function () {
      setLoading(false);
    });
  }

  $(function () {
    initUnitForm();
    initUnitList();
  });
})(window, window.jQuery);
