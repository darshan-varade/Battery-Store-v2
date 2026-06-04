$(document).ready(function () {
    let TotalRows = 1;
    let PageStart = 1;
    let PageWindow = 5;

    FetchData();

    function FetchData() {
        console.log('FetchData called');

        $.ajax({
            url: '/Item/ItemList',
            type: 'POST',
            data: $('#searchForm').serialize(),
            success: function (result) {
                console.log('AJAX success, result:', result);

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
        window.location.href = '/Item/ItemAdd';
    });

    $(document).on('click', '.btn-action-delete', function () {
        let itemId = $(this).data('id');

        $.ajax({
            url: '/Item/ItemDelete',
            type: 'POST',
            data: { id: itemId },
            success: function () {
                alert('Item deleted successfully');
                FetchData();
            },
            error: function (xhr, status, error) {
                console.log(xhr.responseText);
                alert('Delete failed');
            }
        });
    });
});