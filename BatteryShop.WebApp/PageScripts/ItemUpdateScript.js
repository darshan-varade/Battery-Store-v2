$(document).ready(function () {

    $('#itemUpdateForm').submit(function (e) {

        e.preventDefault();

        let data = {
            ItemId: parseInt($('#ItemId').val()),
            SerialNumber: $('#SerialNumber').val().trim(),
            BrandId: parseInt($('#BrandId').val()),
            TypeId: parseInt($('#TypeId').val()),
            TransactionId: parseInt($('#TransactionId').val())
        };

        if (!data.SerialNumber) {
            showToast('Serial Number is required.', 'warning');
            return;
        }

        if (isNaN(data.BrandId)) {
            showToast('Please select a Brand.', 'warning');
            return;
        }

        if (isNaN(data.TypeId)) {
            showToast('Please select a Type.', 'warning');
            return;
        }

        if (isNaN(data.TransactionId)) {
            showToast('Transaction ID must be numeric.', 'warning');
            return;
        }

        $.ajax({
            url: _urlItemUpdate,
            type: 'POST',
            data: data, 
            success: function (response) {
                if (response.success) {
                    showToast(response.message || 'Item updated successfully.', 'success');
                    setTimeout(function () {
                        let returnUrl = sessionStorage.getItem('ItemListReturnUrl');
                        sessionStorage.removeItem('ItemListReturnUrl');
                        window.location.href = returnUrl || _urlItemList;
                    }, 1000);
                }
                else {
                    showToast(response.message || 'Update failed.', 'error');
                }
            },
            error: function (xhr, status, error) {
                let errorMessage = 'Update failed: ' + error;
                if (xhr.responseJSON &&
                    xhr.responseJSON.message) {
                    errorMessage =
                        xhr.responseJSON.message;
                }
                showToast(errorMessage, 'error');
            }
        });

    });

    $('#cancelBtn').click(function (e) {
        e.preventDefault();
        let returnUrl = sessionStorage.getItem('ItemListReturnUrl');
        sessionStorage.removeItem('ItemListReturnUrl');
        window.location.href = returnUrl || _urlItemList;
    });
});