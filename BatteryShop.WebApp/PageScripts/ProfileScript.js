document.getElementById('profileImageFile').addEventListener('change', function (e) {
    var file = e.target.files[0];
    if (!file) return;
    if (!file.type.match('image/jpeg') && !file.type.match('image/png')) {
        showToast('Only JPG and PNG images are allowed.', 'error');
        return;
    }
    if (file.size > 5 * 1024 * 1024) {
        showToast('Image must be under 5 MB.', 'error');
        return;
    }
    var reader = new FileReader();
    reader.onload = function (ev) {
        var preview = document.getElementById('avatarPreview');
        var existing = document.getElementById('avatarImg');
        if (existing) existing.remove();
        var img = document.createElement('img');
        img.src = ev.target.result;
        img.alt = 'Profile';
        img.className = 'profile-avatar-preview';
        img.id = 'avatarImg';
        preview.appendChild(img);
    };
    reader.readAsDataURL(file);
});

document.getElementById('removePhotoBtn').addEventListener('click', function () {
    var btn = this;
    var removeUrl = document.querySelector('[data-remove-url]').getAttribute('data-remove-url');
    $.post(removeUrl, { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() })
        .done(function (res) {
            if (res.success) {
                var preview = document.getElementById('avatarPreview');
                var existing = document.getElementById('avatarImg');
                if (existing) existing.remove();
                var fallback = document.createElement('div');
                fallback.className = 'profile-avatar-fallback';
                fallback.id = 'avatarImg';
                fallback.textContent = '?';
                preview.appendChild(fallback);
                btn.classList.add('hidden');
                document.getElementById('profileImageHidden').value = '';
                showToast('Profile photo removed.', 'success');
            } else {
                showToast(res.message || 'Failed to remove photo.', 'error');
            }
        })
        .fail(function () {
            showToast('An error occurred.', 'error');
        });
});
