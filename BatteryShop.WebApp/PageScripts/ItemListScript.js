$(document).ready(function () {
    let TotalRows = 1;
    let PageStart = 1;
    let PageWindow = 5;
    let isInitializing = true;
    let sortColumn = 'itemId';
    let sortDirection = 'ASC';

    // Initialize Select2 first so it is ready to receive restored state
    $('#BrandSelect').select2({
        placeholder: 'Filter by brands...',
        allowClear: true,
        width: '100%'
    });

    $('#StatusSelect').select2({
        placeholder: 'Filter by status...',
        allowClear: true,
        width: '100%'
    });

    $('#BrandSelect').on('select2:open select2:close', function () {
        updateBrandSelectionDisplay();
    });

    $(window).on('resize', function () {
        updateBrandSelectionDisplay();
    });

    RestoreFilterState();
    updateBrandSelectionDisplay();

    isInitializing = false;
    $('#SortColumn').val(sortColumn);
    $('#SortDirection').val(sortDirection);
    FetchData();

    function FetchData() {
        console.log('FetchData called');

        $.ajax({
            url: '/Item/ItemList',
            type: 'POST',
            data: $('#searchForm').serialize(),
            success: function (result) {

                $('#resultContainer').html(result);
                updateSortIcons();

                TotalRows = parseInt($('#TotalRows').val(), 10);

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
            // Reset Select2 selection
            $('#BrandSelect').val(null).trigger('change');
            $('#StatusSelect').val(null).trigger('change');
            PageStart = 1;
            $('#PageNumber').val(1);
            sortColumn = 'itemId';
            sortDirection = 'ASC';
            $('#SortColumn').val(sortColumn);
            $('#SortDirection').val(sortDirection);
            updateSortIcons();
        }, 0);
    });

    // ===== Select2 Multi-Select Brand Filter Display Update =====
    function updateBrandSelectionDisplay() {
        setTimeout(function () {
            var $select = $('#BrandSelect');
            var $container = $select.next('.select2-container');
            if (!$container.length) return;

            var $rendered = $container.find('.select2-selection__rendered');
            
            // Remove existing badge
            $rendered.find('.select2-selection__badge').remove();
            
            var $choices = $rendered.find('.select2-selection__choice');
            var count = $choices.length;
            
            // Show only the first selected option
            $choices.each(function (index) {
                if (index < 1) {
                    $(this).show();
                } else {
                    $(this).hide();
                }
            });
            
            // If more than 1 option is selected, show + (count - 1)
            if (count > 1) {
                var badgeHtml = '<li class="select2-selection__choice select2-selection__badge" ' +
                                'title="' + count + ' brands selected">' +
                                '+' + (count - 1) +
                                '</li>';
                
                var $search = $rendered.find('.select2-search');
                if ($search.length) {
                    $search.before(badgeHtml);
                } else {
                    $rendered.append(badgeHtml);
                }
            }
        }, 0);
    }

    $('#BrandSelect').on('change', function () {
        $('#PageNumber').val(1);
        PageStart = 1;
        updateBrandSelectionDisplay();
        if (!isInitializing) {
            FetchData();
        }
    });

    $('#StatusSelect').on('change', function () {
        $('#PageNumber').val(1);
        PageStart = 1;
        if (!isInitializing) {
            FetchData();
        }
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

    // ===== Sortable Column Headers =====
    function updateSortIcons() {
        $('.sortable .sort-icon').each(function () {
            let col = $(this).closest('.sortable').data('column');
            if (col === sortColumn) {
                $(this).text(sortDirection === 'ASC' ? ' ▲' : ' ▼');
            } else {
                $(this).text(' ↕');
            }
        });
    }

    $(document).on('click', '.sortable', function () {
        let col = $(this).data('column');
        if (sortColumn === col) {
            sortDirection = sortDirection === 'ASC' ? 'DESC' : 'ASC';
        } else {
            sortColumn = col;
            sortDirection = 'ASC';
        }
        updateSortIcons();
        $('#SortColumn').val(sortColumn);
        $('#SortDirection').val(sortDirection);
        $('#PageNumber').val(1);
        PageStart = 1;
        if (!isInitializing) FetchData();
    });

    // ===== Mode: "add" or "update" for modal =====
    let updMode = 'update';

    $('#addItemBtn').click(function () {
        updMode = 'add';

        $('#updItemId').val('');
        $('#updSerialNumber').val('');
        $('#updTransactionId').val('');
        $('#updBrandFilter').val('');
        $('#updTypeId').val('');
        $('#updStatusId').val('');

        $('#updateModalTitle').html('<i class="bi bi-plus-circle"></i> Add Item');
        $('#updateModal .modal-footer .btn-success').html('<i class="bi bi-plus-circle"></i> Add Item');

        $('#updSpinner').hide();
        $('#updFormContainer').show();

        let modal = new bootstrap.Modal(document.getElementById('updateModal'));
        modal.show();
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
        updMode = 'update';

        $('#updateModalTitle').html('<i class="bi bi-pencil-square"></i> Update Item');
        $('#updateModal .modal-footer .btn-success').html('<i class="bi bi-check-circle"></i> Update');

        let itemId = Number($(this).data('id'));

        $('#updFormContainer').hide();
        $('#updSpinner').show();

        let modal = new bootstrap.Modal(document.getElementById('updateModal'));
        modal.show();

        $.ajax({
            url: '/Item/ItemGet',
            type: 'GET',
            data: { id: itemId },
            success: function (data) {
                if (!data.success) {
                    $('#updSpinner').hide();
                    showToast(data.message || 'Failed to load item data.', 'error');
                    modal.hide();
                    return;
                }
                $('#updSpinner').hide();
                $('#updFormContainer').show();
                $('#updItemId').val(data.itemId);
                $('#updSerialNumber').val(data.serialNumber);
                $('#updTransactionId').val(data.transactionId);

                $('#updBrandFilter').val(data.brandId);
                populateTypeDropdown(data.brandId);
                $('#updTypeId').val(data.typeId);
                $('#updStatusId').val(data.itemStatusId);
            },
            error: function () {
                $('#updSpinner').hide();
                showToast('Failed to load item data.', 'error');
                modal.hide();
            }
        });
    });

    $(document).on('click', '.btn-action-view', function () {
        let itemId = Number($(this).data('id'));

        $('#detailsSpinner').show();
        $('#detailsContainer').hide();

        let isDesktop = window.innerWidth >= 1200;
        let offcanvas = new bootstrap.Offcanvas(document.getElementById('detailsOffcanvas'), {
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
                if (!data.success) {
                    $('#detailsSpinner').hide();
                    showToast(data.message || 'Failed to load item details.', 'error');
                    offcanvas.hide();
                    return;
                }
                $('#detailsSpinner').hide();
                $('#detailsContent').html(
                    '<dt class="col-sm-5">Item ID</dt><dd class="col-sm-7">' + data.itemId + '</dd>' +
                    '<dt class="col-sm-5">Serial Number</dt><dd class="col-sm-7">' + escapeHtml(data.serialNumber) + '</dd>' +
                    '<dt class="col-sm-5">Brand</dt><dd class="col-sm-7">' + escapeHtml(data.brandName) + '</dd>' +
                    '<dt class="col-sm-5">Type</dt><dd class="col-sm-7">' + escapeHtml(data.typeName) + '</dd>' +
                    '<dt class="col-sm-5">Transaction ID</dt><dd class="col-sm-7">' + data.transactionId + '</dd>' +
                    '<dt class="col-sm-5">Status</dt><dd class="col-sm-7">' + getStatusName(data.itemStatusId) + '</dd>'
                );
                $('#detailsContainer').show();
            },
            error: function () {
                $('#detailsSpinner').hide();
                showToast('Failed to load item details.', 'error');
                offcanvas.hide();
            }
        });
    });

    $('#detailsOffcanvas').on('hidden.bs.offcanvas', function () {
        $('.layout-wrapper').removeClass('drawer-open');
    });

    function escapeHtml(str) {
        if (!str) return '';
        return $('<span>').text(str).html();
    }

    function getTypeName(typeId) {
        var type = (_modelTypeList || []).find(function(t) { return t.TypeId === typeId; });
        return type ? escapeHtml(type.TypeName) : typeId;
    }

    function getStatusName(statusId) {
        var statusMap = { 1: 'In Stock', 2: 'Sold', 3: 'Replacement Given', 4: 'Damaged', 5: 'Scrapped', 6: 'Received Back' };
        return statusMap[statusId] || 'Unknown';
    }

    function populateTypeDropdown(brandId) {
        var $typeSelect = $('#updTypeId');
        $typeSelect.empty().append('<option value="">--Select Type--</option>');
        (_modelTypeList || []).forEach(function (type) {
            if (!brandId || type.BrandId === brandId) {
                $typeSelect.append('<option value="' + type.TypeId + '" data-brand-id="' + type.BrandId + '">' + type.TypeName + '</option>');
            }
        });
        $typeSelect.prop('disabled', false);
    }

    $('#updBrandFilter').change(function () {
        populateTypeDropdown(parseInt($(this).val()) || 0);
        $('#updTypeId').val('');
    });



    $('#updateForm').submit(function (e) {
        e.preventDefault();

        var data = {
            SerialNumber: $('#updSerialNumber').val().trim(),
            TypeId: parseInt($('#updTypeId').val()),
            TransactionId: parseInt($('#updTransactionId').val()),
            ItemStatusId: parseInt($('#updStatusId').val()) || 1
        };

        if (updMode === 'update') {
            data.ItemId = parseInt($('#updItemId').val());
        }

        if (!data.SerialNumber) { showToast('Serial Number is required.', 'warning'); return; }
        if (isNaN(data.TypeId)) { showToast('Please select a Type.', 'warning'); return; }
        if (isNaN(data.TransactionId)) { showToast('Transaction ID must be numeric.', 'warning'); return; }

        var url = updMode === 'update' ? '/Item/ItemUpdate' : '/Item/ItemAddOne';

        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function (response) {
                if (response.success) {
                    showToast(response.message, 'success');
                    var modal = bootstrap.Modal.getInstance(document.getElementById('updateModal'));
                    if (modal) modal.hide();
                    FetchData();
                } else {
                    showToast(response.message, 'error');
                }
            },
            error: function (xhr) {
                var msg = 'Operation failed.';
                if (xhr.responseJSON && xhr.responseJSON.message) msg = xhr.responseJSON.message;
                showToast(msg, 'error');
            }
        });
    });

    function SaveFilterState() {
        let state = {
            pageNumber: $('#PageNumber').val(),
            pageSize: $('#PageSizeList').val(),
            brandIds: $('#BrandSelect').val(),
            statusIds: $('#StatusSelect').val(),
            serialNumber: $('#SerialNumber').val().trim(),
            sortColumn: sortColumn,
            sortDirection: sortDirection
        };
        sessionStorage.setItem('ItemListFilters', JSON.stringify(state));
    }

    function RestoreFilterState() {
        let saved = sessionStorage.getItem('ItemListFilters');
        if (saved) {
            try {
                var state = JSON.parse(saved);
            } catch (e) {
                sessionStorage.removeItem('ItemListFilters');
                return;
            }
            if (state.pageNumber) $('#PageNumber').val(state.pageNumber);
            if (state.pageSize) $('#PageSizeList').val(state.pageSize);
            if (state.brandIds) $('#BrandSelect').val(state.brandIds).trigger('change');
            if (state.statusIds) $('#StatusSelect').val(state.statusIds).trigger('change');
            if (state.serialNumber) $('#SerialNumber').val(state.serialNumber);
            if (state.sortColumn) sortColumn = state.sortColumn;
            if (state.sortDirection) sortDirection = state.sortDirection;
        }
        updateSortIcons();
    }
});