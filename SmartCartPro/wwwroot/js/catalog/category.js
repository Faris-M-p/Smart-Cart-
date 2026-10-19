(function (window, $) {
  'use strict';

  function initCategoryForm() {
    var $form = $('#categoryForm');
    if (!$form.length || !window.SmartCartAjax) return;

    $form.on('submit', function (e) {
      e.preventDefault();
      window.SmartCartAjax.submitForm($form);
    });
  }

  function initCategoryList() {
    var $page = $('#categoryListPage');
    if (!$page.length || !window.SmartCartAjax) return;

    var setActiveUrl = $page.data('set-active-url');

    $page.find('form[method="get"]').on('submit', function () {
      $page.addClass('opacity-50').attr('aria-busy', 'true');
    });

    $page.on('click', '[data-sc-toggle-active]', function () {
      var $btn = $(this);
      var id = $btn.data('id');
      var nextActive = $btn.data('next-active') === true || $btn.data('next-active') === 'True' || $btn.data('next-active') === 'true';
      var name = $btn.data('name') || 'this category';
      var actionLabel = nextActive ? 'activate' : 'deactivate';

      window.SmartCartAjax.confirm({
        title: (nextActive ? 'Activate' : 'Deactivate') + ' category',
        body: 'Are you sure you want to ' + actionLabel + ' "' + name + '"?',
        confirmText: nextActive ? 'Activate' : 'Deactivate',
        confirmClass: nextActive ? 'btn-primary' : 'btn-warning',
        onConfirm: function () {
          window.SmartCartAjax.post(setActiveUrl, { id: id, isActive: nextActive }, {
            onSuccess: function (res) {
              var isActive = res && res.data && res.data.isActive;
              var $row = $page.find('tr[data-category-id="' + id + '"]');
              if (!$row.length) {
                window.location.reload();
                return;
              }
              var $badge = $row.find('[data-sc-status-badge]');
              if (isActive) {
                $badge.removeClass('text-bg-secondary').addClass('text-bg-success').text('Active');
                $btn.text('Deactivate').data('next-active', false).attr('data-next-active', 'false');
              } else {
                $badge.removeClass('text-bg-success').addClass('text-bg-secondary').text('Inactive');
                $btn.text('Activate').data('next-active', true).attr('data-next-active', 'true');
              }
            }
          });
        }
      });
    });
  }

  $(function () {
    initCategoryForm();
    initCategoryList();
  });
})(window, window.jQuery);
