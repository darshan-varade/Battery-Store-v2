function showToast(message, type) {
    var bgClass = 'bg-secondary text-white';
    if (type === 'success') bgClass = 'bg-success text-white';
    else if (type === 'error') bgClass = 'bg-danger text-white';
    else if (type === 'warning') bgClass = 'bg-warning';
    else if (type === 'info') bgClass = 'bg-info';

    var toastHtml = '<div class="toast ' + bgClass + '" role="alert">' +
        '<div class="d-flex">' +
        '<div class="toast-body">' + message + '</div>' +
        '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>' +
        '</div></div>';

    var $toast = $(toastHtml);
    $('#toastContainer').append($toast);
    var toast = new bootstrap.Toast($toast[0], { delay: 3000 });
    toast.show();
    $toast.on('hidden.bs.toast', function () { $(this).remove(); });
}

function showConfirm(title, message, confirmText) {
    return new Promise(function (resolve) {
        $('#confirmModalTitle').text(title);
        $('#confirmModalMessage').text(message);
        $('#confirmModalBtn').text(confirmText || 'Confirm');
        $('#confirmModal').modal('show');

        $('#confirmModalBtn').off('click').on('click', function () {
            $('#confirmModal').modal('hide');
            resolve(true);
        });
        $('#confirmModal').off('hidden.bs.modal').on('hidden.bs.modal', function () {
            resolve(false);
        });
    });
}
