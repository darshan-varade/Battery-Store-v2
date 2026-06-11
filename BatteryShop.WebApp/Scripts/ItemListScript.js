$(document).ready(function () {
    let TotalRows = 1;
    let PageStart = 1;
    let PageWindow = 5;

    RestoreFilterState();
    FetchData();

    function FetchData() {
        console.log('FetchData called');

        $.ajax({
            url: '/Item/ItemList',
            type: 'POST',
            data: $('#searchForm').serialize(),
            success: function (result) {

                $('#resultContainer').html(result);

                TotalRows = parseInt($('#TotalRows').val());

                LoadButton();

                $('#PageItems').empty();

                let page = Number($('#PageNumber').val());
                let size = Number($('#PageSizeList').val());

                let start = (page - 1) * size + 1;
                let end = (page - 1) * size + size;

                if (end > TotalRows || end <= 0)
                    end = TotalRows;

                $('#PageItems').append(
                    `${start} to ${end} out of ${TotalRows}`
                );

                SaveFilterState();
            },
            error: function (xhr, status, error) {
                console.error('AJAX error:', status, error);

                $('#resultContainer').html(
                    '<div class="alert alert-danger">Error loading data: ' + error + '</div>'
                );
            }
        });
    }

    function LoadButton() {
        let buttonlist = $('#buttonlist');
        let page = Number($('#PageNumber').val());

        buttonlist.empty();

        let PageSize = $('#PageSizeList').val();

        if (PageSize <= 0)
            PageSize = Number.MAX_VALUE;

        let TotalPages = Math.ceil(TotalRows / PageSize);

        if (page > 1) {
            $('#prevButton').prop('disabled', false);
            $('#prevButton').removeClass('disabled');
        } else {
            $('#prevButton').prop('disabled', true);
            $('#prevButton').addClass('disabled');
        }

        for (let i = PageStart; i <= TotalPages && i <= PageStart + (PageWindow - 1) ; i++) {

            if (i == page) {
                buttonlist.append(
                    `<button type="button" class="page-btn btn-success" data-page="${i}">${i}</button>`
                );
            } else {
                buttonlist.append(
                    `<button type="button" class="page-btn btn-primary" data-page="${i}">${i}</button>`
                );
            }
        }

        if (page < TotalPages) {
            $('#nextButton').prop('disabled', false);
            $('#nextButton').removeClass('disabled');
        } else {
            $('#nextButton').prop('disabled', true);
            $('#nextButton').addClass('disabled');
        }
    }

    $('#searchForm').submit(function (e) {
        e.preventDefault();
        FetchData();
    });

    $('#searchBtn').click(function () {
        PageStart = 1;
        $('#PageNumber').val(1);
        FetchData();
    });

    $('#searchForm').on('reset', function () {
        setTimeout(function () {
            PageStart = 1;
            $('#PageNumber').val(1);
            FetchData();
        }, 0);
    });

    $('#BrandSelect').change(function () {
        $('#PageNumber').val(1);
        PageStart = 1;
        FetchData();
    });

    $('#PageSizeList').change(function () {
        $('#PageNumber').val(1);
        PageStart = 1;
        FetchData();
    });

    $('#buttonlist').on('click', '.page-btn', function () {
        let page = Number($(this).data('page'));
        $('#PageNumber').val(page);
        FetchData();
    });

    $('#prevButton').click(function () {
        let num = Number($('#PageNumber').val());

        if (num % PageWindow == 1) {
            PageStart = PageStart - PageWindow;
            LoadButton();
        }

        $('#PageNumber').val(num - 1);
        FetchData();
    });

    $('#nextButton').click(function () {
        let num = Number($('#PageNumber').val());

        if (num % PageWindow == 0) {
            PageStart = PageStart + PageWindow;
            LoadButton();
        }

        $('#PageNumber').val(num + 1);
        FetchData();
    });

    $('#addItemBtn').click(function () {
        sessionStorage.setItem('ItemListReturnUrl', window.location.href);
        window.location.href = '/Item/ItemAdd';
    });

    $(document).on('click', '.btn-action-delete', function () {
        let itemId = $(this).data('id');
        showConfirm('Delete Item', `Are you sure you want to delete item #${itemId}?`, 'Delete').then(function (confirmed) {
            if (!confirmed) return;
            $.ajax({
                url: '/Item/ItemDelete',
                type: 'POST',
                data: { id: itemId },
                success: function () {
                    showToast('Item deleted successfully', 'success');
                    FetchData();
                },
                error: function (xhr, status, error) {
                    console.log(xhr.responseText);
                    showToast('Delete failed', 'error');
                }
            });
        });
    });
    $(document).on('click', '.btn-action-update', function () {
        let itemId = Number($(this).data('id'));
        let isDesktop = window.innerWidth >= 1200;

        $('#updFormContainer').hide();
        $('#updSpinner').show();

        var offcanvas = new bootstrap.Offcanvas(document.getElementById('updateOffcanvas'), {
            backdrop: isDesktop ? false : true,
            scroll: isDesktop ? true : false
        });
        offcanvas.show();

        if (isDesktop) {
            $('.layout-wrapper').addClass('drawer-open');
        }

        $.ajax({
            url: '/Item/ItemGet',
            type: 'GET',
            data: { id: itemId },
            success: function (data) {
                $('#updSpinner').hide();
                $('#updFormContainer').show();
                $('#updItemId').val(data.itemId);
                $('#updSerialNumber').val(data.serialNumber);
                $('#updTransactionId').val(data.transactionId);
                $('#updBrandId').val(data.brandId);

                populateTypeDropdown(data.brandId);
                $('#updTypeId').val(data.typeId);
            },
            error: function () {
                $('#updSpinner').hide();
                showToast('Failed to load item data.', 'error');
                offcanvas.hide();
            }
        });
    });

    $('#updateOffcanvas').on('hidden.bs.offcanvas', function () {
        $('.layout-wrapper').removeClass('drawer-open');
    });

    function populateTypeDropdown(brandId) {
        var $typeSelect = $('#updTypeId');
        $typeSelect.empty().append('<option value="">--Select Type--</option>');
        (_modelTypeList || []).forEach(function (type) {
            $typeSelect.append('<option value="' + type.TypeId + '">' + type.TypeName + '</option>');
        });
        $typeSelect.prop('disabled', !brandId);
    }

    $('#updBrandId').change(function () {
        populateTypeDropdown($(this).val());
        if (!$(this).val()) $('#updTypeId').val('');
    });

    $('#updateForm').submit(function (e) {
        e.preventDefault();

        var data = {
            ItemId: parseInt($('#updItemId').val()),
            SerialNumber: $('#updSerialNumber').val().trim(),
            BrandId: parseInt($('#updBrandId').val()),
            TypeId: parseInt($('#updTypeId').val()),
            TransactionId: parseInt($('#updTransactionId').val())
        };

        if (!data.SerialNumber) { showToast('Serial Number is required.', 'warning'); return; }
        if (isNaN(data.BrandId)) { showToast('Please select a Brand.', 'warning'); return; }
        if (isNaN(data.TypeId)) { showToast('Please select a Type.', 'warning'); return; }
        if (isNaN(data.TransactionId)) { showToast('Transaction ID must be numeric.', 'warning'); return; }

        $.ajax({
            url: '/Item/ItemUpdate',
            type: 'POST',
            data: data,
            success: function (response) {
                if (response.success) {
                    showToast(response.message, 'success');
                    bootstrap.Offcanvas.getInstance(document.getElementById('updateOffcanvas')).hide();
                    FetchData();
                } else {
                    showToast(response.message, 'error');
                }
            },
            error: function (xhr) {
                var msg = 'Update failed.';
                if (xhr.responseJSON && xhr.responseJSON.message) msg = xhr.responseJSON.message;
                showToast(msg, 'error');
            }
        });
    });

    function SaveFilterState() {
        let state = {
            pageNumber: $('#PageNumber').val(),
            pageSize: $('#PageSizeList').val(),
            brandId: $('#BrandSelect').val(),
            serialNumber: $('#SerialNumber').val().trim()
        };
        sessionStorage.setItem('ItemListFilters', JSON.stringify(state));
    }

    function RestoreFilterState() {
        let saved = sessionStorage.getItem('ItemListFilters');
        if (saved) {
            let state = JSON.parse(saved);
            if (state.pageNumber) $('#PageNumber').val(state.pageNumber);
            if (state.pageSize) $('#PageSizeList').val(state.pageSize);
            if (state.brandId) $('#BrandSelect').val(state.brandId);
            if (state.serialNumber) $('#SerialNumber').val(state.serialNumber);
        }
    }
});