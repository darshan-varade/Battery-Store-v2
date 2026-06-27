$(document).ready(function () {
    let batchCount = 0;

    addBatch();

    $('#addBatchBtn').click(function () {
        addBatch();
    });

    function addBatch() {
        batchCount++;
        const batchId = `batch-${batchCount}`;
        
        const batchHTML = `
                <div class="batch-card" data-batch-id="${batchId}">
                    <div class="batch-header">
                        ${batchCount > 1 ? `<button type="button" class="btn-remove-batch" title="Remove this batch"><i class="bi bi-trash"></i></button>` : ''}
                        
                    </div>
                    <div class="batch-filters">
                        <div class="filter-group">
                            <label for="brand-${batchId}" class="form-label">Brand *</label>
                            <select id="brand-${batchId}" class="form-control brand-select" data-batch-id="${batchId}">
                                <option value="">--Select Brand--</option>
                            </select>
                        </div>
                        <div class="filter-group">
                            <label for="type-${batchId}" class="form-label">Type *</label>
                            <select id="type-${batchId}" class="form-control type-select" data-batch-id="${batchId}" disabled>
                                <option value="">--Select Type--</option>
                            </select>
                        </div>
                    </div>
                    <div class="serial-numbers-container" data-batch-id="${batchId}" style="display: none;">
                        <div class="serial-row serial-row-header">
                            <div class="serial-col">Serial Number</div>
                            <div class="serial-col-action">Action</div>
                        </div>
                        <div class="serial-items-list" id="serial-list-${batchId}"></div>
                        <button type="button" class="btn btn-sm btn-primary add-serial-btn" data-batch-id="${batchId}">
                            <i class="bi bi-plus"></i> Add Serial Number
                        </button>
                    </div>
                </div>
            `;

        $('#batchesContainer').append(batchHTML);

        populateBrandDropdown(batchId);

        $(`#brand-${batchId}`).change(function () {
            onBrandChange(batchId);
        });
        $(`#type-${batchId}`).change(function () {
            onTypeChange(batchId);
        });

        addSerialNumberInput(batchId);
    }

    function populateBrandDropdown(batchId) {
        const select = $(`#brand-${batchId}`);
        (_modelBrandList || []).forEach(function (brand) {
            select.append(`<option value="${brand.BrandId}">${brand.BrandName}</option>`);
        });
    }

    function onBrandChange(batchId) {
        const brandId = $(`#brand-${batchId}`).val();
        const typeSelect = $(`#type-${batchId}`);

        if (!brandId) {
            typeSelect.prop('disabled', true).html('<option value="">--Select Type--</option>');
            $(`.serial-numbers-container[data-batch-id="${batchId}"]`).hide();
            return;
        }

        typeSelect.prop('disabled', false);
        typeSelect.html('<option value="">--Select Type--</option>');

        (_modelTypeList || []).forEach(function (type) {
            typeSelect.append(`<option value="${type.TypeId}">${type.TypeName}</option>`);
        });

        $(`.serial-numbers-container[data-batch-id="${batchId}"]`).hide();
    }

    function onTypeChange(batchId) {
        const typeValue = $(`#type-${batchId}`).val();
        if (typeValue) {
            $(`.serial-numbers-container[data-batch-id="${batchId}"]`).show();
        } else {
            $(`.serial-numbers-container[data-batch-id="${batchId}"]`).hide();
            $(`#serial-list-${batchId}`).html('');
            addSerialNumberInput(batchId);
        }
    }

    function addSerialNumberInput(batchId) {
        const serialList = $(`#serial-list-${batchId}`);
        const serialCount = serialList.children().length + 1;
        const serialInputId = `${batchId}-serial-${serialCount}`;

        const serialHTML = `
                <div class="serial-row serial-row-data" data-serial-id="${serialInputId}">
                    <div class="serial-col">
                        <input type="text" id="${serialInputId}" class="form-control serial-input" placeholder="Enter serial number" />
                    </div>
                    <div class="serial-col-action">
                        <button type="button" class="btn btn-sm btn-outline-danger remove-serial-btn" title="Remove"><i class="bi bi-trash"></i></button>
                    </div>
                </div>
            `;

        serialList.append(serialHTML);
    }

    function removeBatch(batchId) {
        showConfirm('Remove Batch', 'Are you sure you want to remove this batch?', 'Remove').then(function (confirmed) {
            if (confirmed) {
                $(`.batch-card[data-batch-id="${batchId}"]`).remove();
            }
        });
    }

    $('#itemAddForm').submit(function (e) {
        e.preventDefault();
        collectFormData();
    });

    function collectFormData() {
        const batchesData = {
            transactionId: $('#transactionId').val(),
            batches: []
        };

        $('.batch-card').each(function () {
            const batchId = $(this).data('batch-id');
            const brand = $(`#brand-${batchId}`).val();
            const type = $(`#type-${batchId}`).val();
            const serials = [];

            $(`#serial-list-${batchId} .serial-input`).each(function () {
                const serial = $(this).val().trim();
                if (serial) {
                    serials.push(serial);
                }
            });

            if (brand && type && serials.length > 0) {
                batchesData.batches.push({
                    brand: brand,
                    type: type,
                    serials: serials
                });
            }
        });

        if (batchesData.batches.length === 0) {
            showToast('Please add at least one item with brand, type, and serial number.', 'warning');
            return;
        }

        if (!batchesData.transactionId) {
            showToast('Please enter Transaction ID.', 'warning');
            return;
        }

        const transactionId = parseInt(batchesData.transactionId);

        if (isNaN(transactionId)) {
            showToast('Transaction ID must be numeric.', 'warning');
            return;
        }

        const flattenedData = flattenBatches(batchesData);

        if (!flattenedData) {
            return;
        }

        submitItemsToBackend(flattenedData);
    }

    function flattenBatches(batchesData) {
        const items = [];
        const allSerials = new Set();
        let duplicateFound = false;

        batchesData.batches.forEach(batch => {
            batch.serials.forEach(serial => {
                if (allSerials.has(serial)) {
                    duplicateFound = true;
                    return;
                }
                allSerials.add(serial);
                items.push({
                    serialNumber: serial,
                    brandId: parseInt(batch.brand),
                    typeId: parseInt(batch.type)
                });
            });
        });

        if (duplicateFound) {
            showToast('Duplicate serial number found. Each serial number must be unique.', 'warning');
            return null;
        }

        if (items.length === 0) {
            showToast('No valid items to submit.', 'warning');
            return null;
        }

        return {
            transactionId: parseInt(batchesData.transactionId),
            items: items
        };
    }

    function submitItemsToBackend(data) {
        $.ajax({
            url: _urlItemAdd,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                if (response.success) {
                    showToast(response.message || 'Items added successfully!', 'success');
                    setTimeout(function () {
                        let returnUrl = sessionStorage.getItem('ItemListReturnUrl');
                        sessionStorage.removeItem('ItemListReturnUrl');
                        window.location.href = returnUrl || _urlItemList;
                    }, 1000);
                } else {
                    showToast(response.message || 'Failed to add items.', 'error');
                }
            },
            error: function (xhr, status, error) {
                let errorMessage = 'Error adding items: ' + error;
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                } else if (xhr.responseText) {
                    errorMessage = xhr.responseText;
                }
                showToast(errorMessage, 'error');
            }
        });
    }

    $(document).on('click', '.add-serial-btn', function () {
        const batchId = $(this).data('batch-id');
        addSerialNumberInput(batchId);
    });

    $(document).on('click', '.remove-serial-btn', function () {
        const serialList = $(this).closest('.serial-items-list');
        if (serialList.find('.serial-row-data').length <= 1) {
            showToast('At least one serial number field is required.', 'warning');
            return;
        }
        $(this).closest('.serial-row-data').remove();
    });

    $(document).on('click', '.btn-remove-batch', function () {
        const batchCard = $(this).closest('.batch-card');
        const batchId = batchCard.data('batch-id');
        removeBatch(batchId);
    });

    $(document).on('click', '#cancelBtn', function (e) {
        e.preventDefault();
        let returnUrl = sessionStorage.getItem('ItemListReturnUrl');
        sessionStorage.removeItem('ItemListReturnUrl');
        window.location.href = returnUrl || _urlItemList;
    });
});