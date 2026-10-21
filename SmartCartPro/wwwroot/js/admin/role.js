/**
 * Roles & permissions pages: list and the create / edit form with the permission matrix.
 * Relies on window.SmartCartAjax (sc-ajax.js) and jQuery from the layout. No Bootstrap JS required.
 *
 * Pages / hooks
 *   #roleListPage          list with a GET filter form (form[data-sc-list-filter]); status / delete use data-sc-post
 *   #roleForm              create / edit form (AJAX submit, redirect with flash toast)
 *   [data-sc-permission-matrix]  permission matrix: module toggles, counters, select / clear all
 */
(function (window, $) {
  'use strict';

  if (!$) return;
  var ajax = window.SmartCartAjax;

  /** "products.manage" -> "products.view" (null when the code is already a view permission). */
  function viewCodeFor(code) {
    var parts = String(code || '').split('.');
    if (parts.length < 2 || parts[parts.length - 1] === 'view') return null;
    return parts[0] + '.view';
  }

  function initMatrix() {
    $('[data-sc-permission-matrix]').each(function () {
      var $matrix = $(this);

      function refresh() {
        var selectedTotal = 0;
        $matrix.find('[data-sc-perm-module]').each(function () {
          var $module = $(this);
          var $perms = $module.find('[data-sc-perm]');
          var on = $perms.filter(':checked').length;
          var toggle = $module.find('[data-sc-perm-module-toggle]')[0];
          if (toggle) {
            toggle.checked = on > 0 && on === $perms.length;
            toggle.indeterminate = on > 0 && on < $perms.length;
          }
          $module.find('[data-sc-perm-count]').text(on + '/' + $perms.length);
          selectedTotal += on;
        });
        $matrix.find('[data-sc-perm-total]').text(selectedTotal);
      }

      $matrix.on('change', '[data-sc-perm-module-toggle]', function () {
        var checked = this.checked;
        $(this).closest('[data-sc-perm-module]').find('[data-sc-perm]').not(':disabled').prop('checked', checked);
        refresh();
      });

      $matrix.on('change', '[data-sc-perm]', function () {
        if (this.checked) {
          // An action permission is of little use without the module's view permission.
          var viewCode = viewCodeFor($(this).attr('data-perm-code'));
          if (viewCode) {
            $matrix.find('[data-sc-perm]').filter(function () {
              return $(this).attr('data-perm-code') === viewCode;
            }).not(':disabled').prop('checked', true);
          }
        }
        refresh();
      });

      $matrix.on('click', '[data-sc-perm-all]', function () {
        $matrix.find('[data-sc-perm]').not(':disabled').prop('checked', true);
        refresh();
      });

      $matrix.on('click', '[data-sc-perm-none]', function () {
        $matrix.find('[data-sc-perm]').not(':disabled').prop('checked', false);
        refresh();
      });

      refresh();
    });
  }

  function initForm() {
    var $form = $('#roleForm');
    if (!$form.length || !ajax) return;

    // Role codes are stored upper-case; show them that way as soon as the field is left.
    $form.find('input[name="RoleCode"]').on('blur', function () {
      this.value = $.trim(this.value).toUpperCase();
    });

    $form.on('submit', function (e) {
      e.preventDefault();
      ajax.submitForm($form);
    });
  }

  function initList() {
    var $page = $('#roleListPage');
    if (!$page.length) return;

    $page.find('form[data-sc-list-filter]').on('submit', function () {
      $page.addClass('opacity-50').attr('aria-busy', 'true');
    });
  }

  $(function () {
    initMatrix();
    initForm();
    initList();
  });
})(window, window.jQuery);
