$(document).ready(function () {
    let TotalRows = 1;
    let PageStart = 1;
    let PageWindow = 5;
    let isInitializing = true;
    let sortColumn = 'userId';
    let sortDirection = 'ASC';

    $('#CitySelect').select2({
        placeholder: 'Filter by city...',
        allowClear: true,
        width: '100%'
    });

    $('#updCity').select2({
        tags: true,
        placeholder: 'Select or type city...',
        width: '100%',
        dropdownParent: $('#updateModal')
    });

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
        var term = d.text;
        $('#SearchTerm').val(term);
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

    $('#updName').select2({
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
        tags: true,
        placeholder: 'Search or type new name...',
        width: '100%',
        dropdownParent: $('#updateModal')
    }).on('select2:select', function (e) {
        var d = e.params.data;
        if (d.id && d.id != d.text) {
            if (updMode === 'update') {
                $('#updPhone').val(d.phone);
                if (d.cityName) {
                    if ($('#updCity').find('option[value="' + d.cityName + '"]').length) {
                        $('#updCity').val(d.cityName).trigger('change');
                    } else {
                        var newOption = new Option(d.cityName, d.cityName, true, true);
                        $('#updCity').append(newOption).trigger('change');
                    }
                }
            }
        } else {
            var newName = d.text;
            if (newName && newName.trim() !== '') {
                $('#updPhone').val('');
            }
        }
    });

    RestoreFilterState();
    isInitializing = false;
    $('#SortColumn').val(sortColumn);
    $('#SortDirection').val(sortDirection);
    FetchData();

    function FetchData() {
        $.ajax({
            url: '/Customer/CustomerList',
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
            $('#CitySelect').val(null).trigger('change');
            $('#SearchName').val(null).trigger('change');
            $('#SearchPhone').val(null).trigger('change');
            $('#SearchTerm').val('');
            $('#PhoneFilter').val('');
            $('#PageSizeList').val(10);
            PageStart = 1;
            $('#PageNumber').val(1);
            sortColumn = 'userId';
            sortDirection = 'ASC';
            $('#SortColumn').val(sortColumn);
            $('#SortDirection').val(sortDirection);
            updateSortIcons();
            FetchData();
        }, 0);
    });

    $('#CitySelect').on('change', function () {
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

    let updMode = 'update';

    $('#addCustomerBtn').click(function () {
        updMode = 'add';

        $('#updUserId').val('');
        $('#updName').val(null).trigger('change');
        $('#updPhone').val('');
        $('#updCity').val(null).trigger('change');

        $('#updateModalTitle').html('<i class="bi bi-plus-circle"></i> Add Customer');
        $('#updateModal .modal-footer .btn-success').html('<i class="bi bi-plus-circle"></i> Add Customer');

        $('#updBalanceRow').hide();
        $('#updSpinner').hide();
        $('#updFormContainer').show();

        let modal = new bootstrap.Modal(document.getElementById('updateModal'));
        modal.show();
    });

    $('#updateModal').on('hidden.bs.modal', function () {
        $('#updUserId').val('');
        $('#updName').val(null).trigger('change');
        $('#updPhone').val('');
        $('#updCity').val(null).trigger('change');
        $('#updBalance').val('');
    });

    $(document).on('click', '.btn-action-delete', function () {
        let userId = $(this).data('id');
        showConfirm('Delete Customer', 'Are you sure you want to delete customer #' + userId + '?', 'Delete').then(function (confirmed) {
            if (!confirmed) return;
            $.ajax({
                url: '/Customer/CustomerDelete',
                type: 'POST',
                data: { id: userId },
                success: function () {
                    showToast('Customer deleted successfully', 'success');
                    FetchData();
                },
                error: function () {
                    showToast('Delete failed', 'error');
                }
            });
        });
    });

    $(document).on('click', '.btn-action-update', function () {
        updMode = 'update';

        $('#updateModalTitle').html('<i class="bi bi-pencil-square"></i> Update Customer');
        $('#updateModal .modal-footer .btn-success').html('<i class="bi bi-check-circle"></i> Update');

        let userId = Number($(this).data('id'));

        $('#updBalanceRow').show();
        $('#updFormContainer').hide();
        $('#updSpinner').show();

        let modal = new bootstrap.Modal(document.getElementById('updateModal'));
        modal.show();

        $.ajax({
            url: '/Customer/CustomerGet',
            type: 'GET',
            data: { id: userId },
            success: function (data) {
                if (!data.success) {
                    $('#updSpinner').hide();
                    showToast(data.message || 'Failed to load customer data.', 'error');
                    modal.hide();
                    return;
                }
                $('#updSpinner').hide();
                $('#updFormContainer').show();
                $('#updUserId').val(data.userId);

                if ($('#updName').find('option[value="' + data.userId + '"]').length) {
                    $('#updName').val(data.userId).trigger('change');
                } else {
                    var newOption = new Option(data.userFullName, data.userId, true, true);
                    $('#updName').append(newOption).trigger('change');
                }

                $('#updPhone').val(data.userPhone);

                $('#updBalance').val(parseFloat(data.userBalance).toFixed(2));

                if ($('#updCity').find('option[value="' + data.cityName + '"]').length) {
                    $('#updCity').val(data.cityName).trigger('change');
                } else {
                    var newCityOption = new Option(data.cityName, data.cityName, true, true);
                    $('#updCity').append(newCityOption).trigger('change');
                }

            },
            error: function () {
                $('#updSpinner').hide();
                showToast('Failed to load customer data.', 'error');
                modal.hide();
            }
        });
    });

    $(document).on('click', '.btn-action-view', function () {
        let userId = Number($(this).data('id'));

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
            url: '/Customer/CustomerGet',
            type: 'GET',
            data: { id: userId },
            success: function (data) {
                if (!data.success) {
                    $('#detailsSpinner').hide();
                    showToast(data.message || 'Failed to load customer details.', 'error');
                    offcanvas.hide();
                    return;
                }
                $('#detailsSpinner').hide();
                $('#detailsContent').html(
                    '<dt class="col-sm-5">Customer ID</dt><dd class="col-sm-7">' + data.userId + '</dd>' +
                    '<dt class="col-sm-5">Name</dt><dd class="col-sm-7">' + escapeHtml(data.userFullName) + '</dd>' +
                    '<dt class="col-sm-5">Phone</dt><dd class="col-sm-7">' + escapeHtml(data.userPhone) + '</dd>' +
                    '<dt class="col-sm-5">City</dt><dd class="col-sm-7">' + escapeHtml(data.cityName) + '</dd>' +
                    '<dt class="col-sm-5">Balance</dt><dd class="col-sm-7">' + parseFloat(data.userBalance).toFixed(2) + '</dd>'
                );
                $('#detailsContainer').show();
            },
            error: function () {
                $('#detailsSpinner').hide();
                showToast('Failed to load customer details.', 'error');
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

    $('#updateForm').submit(function (e) {
        e.preventDefault();

        var data = {
            UserFullName: '',
            UserPhone: $('#updPhone').val().trim(),
            CityName: $('#updCity').val()
        };

        var nameSelect = $('#updName').select2('data');
        if (nameSelect && nameSelect.length > 0) {
            var fullText = nameSelect[0].text || '';
            data.UserFullName = fullText.trim() || fullText;
        }

        if (updMode === 'update') {
            data.UserId = parseInt($('#updUserId').val());
        }

        if (!data.UserFullName) { showToast('Customer name is required.', 'warning'); return; }
        if (!data.UserPhone) { showToast('Phone number is required.', 'warning'); return; }
        if (!data.CityName) { showToast('City is required.', 'warning'); return; }

        var url = updMode === 'update' ? '/Customer/CustomerUpdate' : '/Customer/CustomerAdd';

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
            cityId: $('#CitySelect').val(),
            searchTerm: $('#SearchTerm').val().trim(),
            phone: $('#PhoneFilter').val().trim(),
            sortColumn: sortColumn,
            sortDirection: sortDirection
        };
        sessionStorage.setItem('CustomerListFilters', JSON.stringify(state));
    }

    function RestoreFilterState() {
        let saved = sessionStorage.getItem('CustomerListFilters');
        if (saved) {
            try {
                var state = JSON.parse(saved);
            } catch (e) {
                sessionStorage.removeItem('CustomerListFilters');
                return;
            }
            if (state.pageNumber) $('#PageNumber').val(state.pageNumber);
            if (state.pageSize) $('#PageSizeList').val(state.pageSize);
            if (state.cityId) $('#CitySelect').val(state.cityId).trigger('change');
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
            if (state.sortColumn) sortColumn = state.sortColumn;
            if (state.sortDirection) sortDirection = state.sortDirection;
        }
        updateSortIcons();
    }
});
