$(document).ready(function () {
    let TotalRows = 1;
    let PageStart = 1;
    let PageWindow = 5;
    let isInitializing = true;
    let sortColumn = 'billId';
    let sortDirection = 'DESC';

    $('#SearchName').select2({
        ajax: {
            url: '/Customer/CustomerSearch',
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return { term: params.term };
            },
            processResults: function (data) {
                return { results: data };
            }
        },
        minimumInputLength: 2,
        placeholder: 'Search by name...',
        allowClear: true,
        width: '100%'
    }).on('select2:select', function (e) {
        var d = e.params.data;
        $('#SearchTerm').val(d.text);
        PageStart = 1;
        $('#PageNumber').val(1);
        FetchData();
    }).on('select2:clear', function () {
        $('#SearchTerm').val('');
        PageStart = 1;
        $('#PageNumber').val(1);
        FetchData();
    });

    $('#SearchPhone').select2({
        ajax: {
            url: '/Customer/CustomerSearchByPhone',
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return { term: params.term };
            },
            processResults: function (data) {
                return { results: data };
            }
        },
        minimumInputLength: 2,
        placeholder: 'Search by phone...',
        allowClear: true,
        width: '100%'
    }).on('select2:select', function (e) {
        var d = e.params.data;
        $('#PhoneFilter').val(d.id);
        PageStart = 1;
        $('#PageNumber').val(1);
        FetchData();
    }).on('select2:clear', function () {
        $('#PhoneFilter').val('');
        PageStart = 1;
        $('#PageNumber').val(1);
        FetchData();
    });

    RestoreFilterState();
    isInitializing = false;
    $('#SortColumn').val(sortColumn);
    $('#SortDirection').val(sortDirection);
    FetchData();

    function FetchData() {
        $.ajax({
            url: '/Bill/BillList',
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
                    start + ' to ' + end + ' out of ' + TotalRows
                );

                SaveFilterState();
            },
            error: function (xhr, status, error) {
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

        for (let i = PageStart; i <= TotalPages && i <= PageStart + (PageWindow - 1); i++) {
            if (i == page) {
                buttonlist.append(
                    '<button type="button" class="page-btn btn-success" data-page="' + i + '">' + i + '</button>'
                );
            } else {
                buttonlist.append(
                    '<button type="button" class="page-btn btn-primary" data-page="' + i + '">' + i + '</button>'
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
            $('#SearchName').val(null).trigger('change');
            $('#SearchPhone').val(null).trigger('change');
            $('#SearchTerm').val('');
            $('#PhoneFilter').val('');
            $('#PageSizeList').val(10);
            $('#DateFrom').val('');
            $('#DateTo').val('');
            PageStart = 1;
            $('#PageNumber').val(1);
            sortColumn = 'billId';
            sortDirection = 'DESC';
            $('#SortColumn').val(sortColumn);
            $('#SortDirection').val(sortDirection);
            updateSortIcons();
            FetchData();
        }, 0);
    });

    $('#DateFrom, #DateTo').on('change', function () {
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

    function updateSortIcons() {
        $('.sortable .sort-icon').each(function () {
            let col = $(this).closest('.sortable').data('column');
            if (col === sortColumn) {
                $(this).text(sortDirection === 'ASC' ? ' \u25B2' : ' \u25BC');
            } else {
                $(this).text(' \u2195');
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

    $(document).on('click', '.btn-action-view', function () {
        let billId = Number($(this).data('id'));

        $('#billDetailsSpinner').show();
        $('#billDetailsContainer').hide();

        let isDesktop = window.innerWidth >= 1200;
        let offcanvas = new bootstrap.Offcanvas(document.getElementById('billDetailsOffcanvas'), {
            backdrop: isDesktop ? false : true,
            scroll: isDesktop ? true : false
        });
        offcanvas.show();

        if (isDesktop) {
            $('.layout-wrapper').addClass('drawer-open');
        }

        $.ajax({
            url: '/Bill/BillGet',
            type: 'GET',
            data: { id: billId },
            success: function (data) {
                if (!data.success) {
                    $('#billDetailsSpinner').hide();
                    showToast(data.message || 'Failed to load bill details.', 'error');
                    offcanvas.hide();
                    return;
                }
                $('#billDetailsSpinner').hide();
                $('#billDetailsContent').html(
                    '<dt class="col-sm-5">Bill ID</dt><dd class="col-sm-7">' + data.billId + '</dd>' +
                    '<dt class="col-sm-5">Customer</dt><dd class="col-sm-7">' + escapeHtml(data.userFullName) + '</dd>' +
                    '<dt class="col-sm-5">Phone</dt><dd class="col-sm-7">' + escapeHtml(data.userPhone) + '</dd>' +
                    '<dt class="col-sm-5">Date of Sale</dt><dd class="col-sm-7">' + data.dateOfSale + '</dd>' +
                    '<dt class="col-sm-5">Total Amount</dt><dd class="col-sm-7">' + parseFloat(data.totalAmount).toFixed(2) + '</dd>' +
                    '<dt class="col-sm-5">Paid Amount</dt><dd class="col-sm-7">' + parseFloat(data.paidAmount).toFixed(2) + '</dd>' +
                    '<dt class="col-sm-5">Due Amount</dt><dd class="col-sm-7">' + parseFloat(data.dueAmount).toFixed(2) + '</dd>'
                );
                $('#billDetailsContainer').show();
            },
            error: function () {
                $('#billDetailsSpinner').hide();
                showToast('Failed to load bill details.', 'error');
                offcanvas.hide();
            }
        });
    });

    $('#billDetailsOffcanvas').on('hidden.bs.offcanvas', function () {
        $('.layout-wrapper').removeClass('drawer-open');
    });

    $(document).on('click', '.btn-action-update', function () {
        var billId = Number($(this).data('id'));
        window.location.href = '/Bill/BillEdit?id=' + billId;
    });

    $(document).on('click', '.btn-action-delete', function () {
        var billId = Number($(this).data('id'));
        showConfirm('Delete Bill', 'Are you sure you want to delete bill #' + billId + '?', 'Delete').then(function (confirmed) {
            if (!confirmed) return;
            $.ajax({
                url: '/Bill/BillDelete',
                type: 'POST',
                data: { id: billId },
                success: function () {
                    showToast('Bill deleted successfully', 'success');
                    FetchData();
                },
                error: function () {
                    showToast('Delete failed', 'error');
                }
            });
        });
    });

    function escapeHtml(str) {
        if (!str) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function SaveFilterState() {
        let state = {
            pageNumber: $('#PageNumber').val(),
            pageSize: $('#PageSizeList').val(),
            searchTerm: $('#SearchTerm').val().trim(),
            phone: $('#PhoneFilter').val().trim(),
            dateFrom: $('#DateFrom').val(),
            dateTo: $('#DateTo').val(),
            sortColumn: sortColumn,
            sortDirection: sortDirection
        };
        sessionStorage.setItem('BillListFilters', JSON.stringify(state));
    }

    function RestoreFilterState() {
        let saved = sessionStorage.getItem('BillListFilters');
        if (saved) {
            try {
                var state = JSON.parse(saved);
            } catch (e) {
                sessionStorage.removeItem('BillListFilters');
                return;
            }
            if (state.pageNumber) $('#PageNumber').val(state.pageNumber);
            if (state.pageSize) $('#PageSizeList').val(state.pageSize);
            if (state.searchTerm) {
                $('#SearchTerm').val(state.searchTerm);
                var searchOption = new Option(state.searchTerm, state.searchTerm, true, true);
                $('#SearchName').append(searchOption).trigger('change');
            }
            if (state.phone) {
                $('#PhoneFilter').val(state.phone);
                var phoneOption = new Option(state.phone, state.phone, true, true);
                $('#SearchPhone').append(phoneOption).trigger('change');
            }
            if (state.dateFrom) $('#DateFrom').val(state.dateFrom);
            if (state.dateTo) $('#DateTo').val(state.dateTo);
            if (state.sortColumn) sortColumn = state.sortColumn;
            if (state.sortDirection) sortDirection = state.sortDirection;
        }
        updateSortIcons();
    }
});
