/**
 * Product screens: list filters (/Product) and the create / edit form (/Product/Create, /Product/Edit).
 * Relies on window.SmartCartAjax (sc-ajax.js). Sub-categories come from Catalog/SubCategoryLookup (GET, JSON array).
 */
(function (window, $) {
  'use strict';

  var Ajax = window.SmartCartAjax;

  /** First non-null property of obj among names (tolerates PascalCase / camelCase JSON). */
  function pick(obj, names) {
    if (!obj) return null;
    for (var i = 0; i < names.length; i++) {
      if (obj[names[i]] != null) return obj[names[i]];
    }
    return null;
  }

  function initProductList() {
    var $page = $('#productListPage');
    if (!$page.length || !Ajax) return;

    // A sub-category belongs to one category: changing the category clears the sub-category before the auto-submit.
    $page.on('change', '#productCategory', function () {
      $('#productSubCategory').val('');
    });

    $page.find('form[method="get"]').on('submit', function () {
      $page.addClass('opacity-50').attr('aria-busy', 'true');
    });
  }

  function initProductForm() {
    var $form = $('#productForm');
    if (!$form.length || !Ajax) return;

    var $category = $form.find('[data-product-category]');
    var $subCategory = $form.find('[data-product-subcategory]');
    var lookupUrl = $form.attr('data-subcategory-url');
    var requestSeq = 0;

    function syncSubCategoryState() {
      // Without a category there is nothing to choose; disabled controls are not posted, which means "no sub-category".
      $subCategory.prop('disabled', !$category.val());
    }

    $category.on('change', function () {
      var categoryId = $category.val();
      var mine = ++requestSeq;

      Ajax.fillSelect($subCategory, [], { placeholder: '— None —' });
      syncSubCategoryState();
      if (!categoryId || !lookupUrl) return;

      $subCategory.prop('disabled', true).attr('aria-busy', 'true');
      Ajax.getJson(lookupUrl, { fkCategory: categoryId })
        .done(function (res) {
          if (mine !== requestSeq) return;
          var rows = $.isArray(res) ? res : (res && $.isArray(res.data) ? res.data : []);
          var items = [];
          $.each(rows, function (_, row) {
            var id = pick(row, ['idSubCategory', 'idSubcategory', 'id']);
            var name = pick(row, ['subCategoryName', 'name']);
            if (id != null && name != null) items.push({ id: id, name: name });
          });
          Ajax.fillSelect($subCategory, items, { valueField: 'id', textField: 'name', placeholder: '— None —' });
        })
        .always(function () {
          if (mine !== requestSeq) return;
          $subCategory.removeAttr('aria-busy');
          syncSubCategoryState();
        });
    });
    syncSubCategoryState();

    // Money fields: tidy plain numbers to two decimals; anything else is left for the server to validate.
    $form.on('blur', '[data-decimal]', function () {
      var $el = $(this);
      var value = $.trim($el.val());
      if (!/^\d+(\.\d+)?$/.test(value)) return;
      $el.val(parseFloat(value).toFixed(parseInt($el.attr('data-decimal'), 10) || 2));
    });

    // Barcode scanners end with Enter; that must not submit the form half-filled.
    $form.on('keydown', '[data-no-enter-submit]', function (e) {
      if (e.key === 'Enter') e.preventDefault();
    });

    $form.on('submit', function (e) {
      e.preventDefault();
      // A disabled sub-category select is not serialised; that is the intended "none".
      Ajax.submitForm($form);
    });
  }

  $(function () {
    initProductList();
    initProductForm();
  });
})(window, window.jQuery);
