function handleToggleClick(e, track) {
    var opt = e.target;
    if (!opt.classList.contains('toggle-opt') || opt.classList.contains('active')) return;

    var row = $(track).closest('tr');
    var ownerId = row.data('owner-id');
    var value = opt.getAttribute('data-value');

    var thumb = track.querySelector('.toggle-opt.active');
    var label = track.parentElement.querySelector('.toggle-label');

    var leftPx, newClass, statusText;
    if (value === '0') {
        leftPx = 3; newClass = 'rejected'; statusText = 'Rejected';
    } else if (value === '1') {
        leftPx = 51; newClass = 'accepted'; statusText = 'Accepted';
    } else {
        leftPx = 27; newClass = 'awaiting'; statusText = 'Awaiting';
    }

    thumb.style.left = leftPx + 'px';
    thumb.className = 'toggle-opt active ' + newClass;
    thumb.setAttribute('data-value', value);
    label.className = 'toggle-label ' + newClass;
    label.textContent = statusText;

    var actionUrl = document.querySelector('.owners-card').getAttribute('data-action-url');
    var postData = { id: ownerId };
    if (value !== '') postData.status = value;
    $.post(actionUrl, postData)
        .done(function (res) {
            if (res.success) {
                showToast('Owner ' + statusText.toLowerCase() + '.', 'success');
            } else {
                showToast(res.message || 'Failed to update status.', 'error');
            }
        })
        .fail(function () {
            showToast('An error occurred.', 'error');
        });
}
