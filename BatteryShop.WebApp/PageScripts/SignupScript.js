$(function () {
    var inputs = $('#otpContainer .otp-digit');
    var hidden = $('#otpCodeHidden');
    var timerEl = $('#resendTimer');
    var btn = $('#resendBtn');
    var countdown = 60;
    var timer;

    function updateHidden() {
        var val = '';
        inputs.each(function () { val += ($(this).val() || ''); });
        hidden.val(val);
        if (val.length === 6) {
            hidden.closest('form').submit();
        }
    }

    inputs.on('keydown', function (e) {
        var idx = parseInt($(this).data('index'));

        if (e.key === 'Backspace') {
            if ($(this).val() === '' && idx > 0) {
                inputs.eq(idx - 1).focus().val('');
            } else if ($(this).val() !== '') {
                $(this).val('');
            }
            updateHidden();
            e.preventDefault();
        }
    });

    inputs.on('keyup', function (e) {
        var val = $(this).val();
        var idx = parseInt($(this).data('index'));

        if (/^[0-9]$/.test(val)) {
            if (idx < 5) {
                inputs.eq(idx + 1).focus();
            }
        } else if (val.length > 1) {
            $(this).val(val.charAt(0));
            if (idx < 5) {
                inputs.eq(idx + 1).focus();
            }
        } else {
            $(this).val('');
        }
        updateHidden();
    });

    inputs.on('input', function () {
        var val = $(this).val();
        if (val.length > 1) {
            var digits = val.replace(/\D/g, '').split('');
            inputs.each(function (i) {
                $(this).val(digits[i] || '');
            });
            if (digits.length === 6) {
                updateHidden();
            }
        }
    });

    inputs.on('paste', function (e) {
        e.preventDefault();
        var text = (e.originalEvent.clipboardData || window.clipboardData).getData('text');
        var digits = text.replace(/\D/g, '').split('').slice(0, 6);
        inputs.each(function (i) {
            $(this).val(digits[i] || '');
        });
        updateHidden();
    });

    function startTimer() {
        countdown = 60;
        timerEl.show();
        btn.prop('disabled', true);
        timer = setInterval(function () {
            countdown--;
            timerEl.text('Resend in ' + countdown + 's');
            if (countdown <= 0) {
                clearInterval(timer);
                timerEl.hide();
                btn.prop('disabled', false);
            }
        }, 1000);
    }

    btn.on('click', function () {
        var token = $('input[name="__RequestVerificationToken"]').val();
        var email = $('input[name="OtpEmail"]').val();

        btn.prop('disabled', true);
        timerEl.text('Sending...').show();

        $.post('/Auth/ResendOtp', {
            email: email,
            __RequestVerificationToken: token
        }).done(function (res) {
            if (res.success) {
                startTimer();
            } else {
                timerEl.text(res.error || 'Failed to resend.');
                btn.prop('disabled', false);
            }
        }).fail(function () {
            timerEl.text('Network error. Try again.');
            btn.prop('disabled', false);
        });
    });

    inputs.first().focus();
    startTimer();
});