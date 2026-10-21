/* Audit log list + JSON details modal.
 * window.bootstrap is not global on Metis pages, so the modal is shown with Bootstrap CSS classes. */
(function (window, $) {
  'use strict';

  var A = window.SmartCartAjax;

  function prettyJson(raw) {
    if (raw == null || raw === '') return '';
    var text = String(raw);
    try {
      return JSON.stringify(JSON.parse(text), null, 2);
    } catch (e) {
      return text;
    }
  }

  function initList() {
    var $page = $('#auditListPage');
    if (!$page.length) return;

    $page.find('form[method="get"]').on('submit', function () {
      $page.addClass('opacity-50').attr('aria-busy', 'true');
    });
    $(window).on('pageshow', function () {
      $page.removeClass('opacity-50').removeAttr('aria-busy');
    });

    var detailsUrl = $page.data('details-url');
    var $modal = $('#auditDetailsModal');
    var $backdrop = $('[data-audit-backdrop]');
    var $pre = $modal.find('[data-audit-json]');
    var $meta = $modal.find('[data-audit-meta]');

    function closeModal() {
      $modal.removeClass('show').css('display', 'none').attr('aria-hidden', 'true');
      $backdrop.addClass('d-none').removeClass('show');
      $(document.body).removeClass('modal-open');
    }

    function openModal() {
      $modal.addClass('show').css('display', 'block').attr('aria-hidden', 'false');
      $backdrop.removeClass('d-none').addClass('show');
      $(document.body).addClass('modal-open');
    }

    function fillMeta(item) {
      $meta.empty();
      function row(label, value) {
        if (value == null || value === '') return;
        $('<dt class="col-sm-3 fw-normal text-muted"></dt>').text(label).appendTo($meta);
        $('<dd class="col-sm-9"></dd>').text(String(value)).appendTo($meta);
      }
      row('When', item.createdAt ? (A && A.fmt ? A.fmt.dateTime(item.createdAt) : item.createdAt) : '');
      row('User', item.userName);
      row('Store', item.storeName);
      row('Entity', item.entityName + (item.entityId != null ? ' #' + item.entityId : ''));
      row('Action', item.action);
    }

    $page.on('click', '[data-sc-audit-details]', function () {
      var id = $(this).data('id');
      if (!id || !A || !detailsUrl) return;
      $pre.text('Loading…');
      $meta.empty();
      openModal();
      A.getJson(detailsUrl, { id: id }, {
        silent: true,
        onSuccess: function (res) {
          var item = res && res.data ? res.data : {};
          fillMeta(item);
          $pre.text(prettyJson(item.details));
        },
        onError: function (res) {
          $pre.text((res && res.message) || 'Unable to load this audit entry.');
        }
      });
    });

    $(document).on('click', '[data-audit-close], [data-audit-backdrop]', closeModal);
    $(document).on('keydown', function (e) {
      if (e.key === 'Escape' && $modal.hasClass('show')) closeModal();
    });
  }

  $(function () {
    initList();
  });
})(window, window.jQuery);
