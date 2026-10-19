/**
 * Auth pages (login, OTP, change/forgot/reset password). Every form posts via SmartCartAjax and
 * follows the JSON envelope: data.redirectUrl on success, validationErrors/message on failure.
 */
(function (window, $) {
  'use strict';

  function bindRedirectingForm(selector) {
    var $form = $(selector);
    if (!$form.length) return;
    $form.on('submit', function (e) {
      e.preventDefault();
      window.SmartCartAjax.submitForm($form);
    });
  }

  function initForgotPassword() {
    var $form = $('#forgotForm');
    if (!$form.length) return;
    $form.on('submit', function (e) {
      e.preventDefault();
      window.SmartCartAjax.submitForm($form, {
        stayOnSuccess: true,
        onSuccess: function (res) {
          $('#forgotSent').removeClass('d-none').text((res && res.message) || 'Check your email for the reset link.');
          $form.addClass('d-none');
        }
      });
    });
  }

  function initResendOtp() {
    var $btn = $('#resendOtpBtn');
    var $form = $('#otpForm');
    if (!$btn.length || !$form.length) return;

    $btn.on('click', function () {
      var token = $form.find('[name="ChallengeToken"]').val();
      $btn.prop('disabled', true);
      window.SmartCartAjax.post($btn.data('resend-url'), { challengeToken: token }, {
        $form: $form,
        onSuccess: function (res) {
          if (res && res.data && res.data.challengeToken) {
            $form.find('[name="ChallengeToken"]').val(res.data.challengeToken);
          }
          $form.find('[name="Code"]').val('').trigger('focus');
        }
      }).always(function () {
        $btn.prop('disabled', false);
      });
    });
  }

  $(function () {
    if (!window.SmartCartAjax) return;
    bindRedirectingForm('#loginForm');
    bindRedirectingForm('#otpForm');
    bindRedirectingForm('#changePasswordForm');
    bindRedirectingForm('#resetForm');
    initForgotPassword();
    initResendOtp();
  });
})(window, window.jQuery);
