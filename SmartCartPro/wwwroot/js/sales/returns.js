/* Sales returns: list formatting, return-qty form, and details amounts. */
(function (window, $) {
  'use strict';

  var A = window.SmartCartAjax;

  function localizeMoney() {
    if (!A || !A.fmt) return;
    $('[data-sc-money]').each(function () {
      var raw = this.getAttribute('data-sc-money');
      if (raw !== null && raw !== '') this.textContent = A.fmt.money(raw);
    });
  }

  function initList() {
    var $page = $('#returnListPage');
    if (!$page.length) return;

    $page.find('form[method="get"]').on('submit', function () {
      $page.addClass('opacity-50').attr('aria-busy', 'true');
    });
    $(window).on('pageshow', function () {
      $page.removeClass('opacity-50').removeAttr('aria-busy');
    });
  }

  function initForm() {
    var $form = $('#saleReturnForm');
    if (!$form.length || !A) return;

    var $submit = $('#saleReturnSubmit');
    var $qty = $form.find('[data-return-qty]');

    function num(value) {
      var n = parseFloat(value);
      return isFinite(n) ? n : 0;
    }

    function updateSubmit() {
      var total = 0;
      $qty.each(function () { total += num(this.value); });
      $submit.prop('disabled', total <= 0);
    }

    $qty.on('input change', function () {
      var $input = $(this);
      var remaining = num($input.closest('tr').attr('data-remaining'));
      var qty = num($input.val());
      if (qty > remaining) $input.addClass('is-invalid');
      else $input.removeClass('is-invalid');
      updateSubmit();
    });

    $form.on('submit', function (e) {
      e.preventDefault();
      if ($submit.prop('disabled')) {
        A.toast('Enter a return quantity for at least one line.', 'warning');
        return;
      }
      A.submitForm($form);
    });

    updateSubmit();
  }

  $(function () {
    localizeMoney();
    initList();
    initForm();
  });
})(window, window.jQuery);
