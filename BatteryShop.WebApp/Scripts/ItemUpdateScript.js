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
            alert('Serial Number is required.');
            return;
        }

        if (isNaN(data.BrandId)) {
            alert('Please select a Brand.');
            return;
        }

        if (isNaN(data.TypeId)) {
            alert('Please select a Type.');
            return;
        }

        if (isNaN(data.TransactionId)) {
            alert('Transaction ID must be numeric.');
            return;
        }

        $.ajax({
            url: _urlItemUpdate,
            type: 'POST',
            data: data, 
            success: function (response) {
                if (response.success) {
                    alert(response.message || 'Item updated successfully.');
                    window.location.href = _urlItemList;
                }
                else {
                    alert(response.message || 'Update failed.');
                }
            },
            error: function (xhr, status, error) {
                let errorMessage = 'Update failed: ' + error;
                if (xhr.responseJSON &&
                    xhr.responseJSON.message) {
                    errorMessage =
                        xhr.responseJSON.message;
                }
                alert(errorMessage);
            }
        });

    });

});