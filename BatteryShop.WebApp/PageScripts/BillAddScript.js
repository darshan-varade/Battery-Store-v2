$(document).ready(function () {
    var batchCount = 0;
    var customerConfirmed = false;
    var pendingVehicleInfos = {};

    $('#custName').select2({
        ajax: {
            url: '/Customer/CustomerSearch',
            dataType: 'json',
            delay: 250,
            data: function (params) { return { term: params.term }; },
            processResults: function (data) { return { results: data }; }
        },
        minimumInputLength: 2,
        tags: true,
        placeholder: 'Search or type new name...',
        width: '100%'
    }).on('select2:select', function (e) {
        var d = e.params.data;
        if (d.id && d.id != d.text) {
            $('#custUserId').val(d.id);
            $('#custPhone').val(d.phone);
            if (d.cityName) {
                if ($('#custCity').find('option[value="' + d.cityName + '"]').length) {
                    $('#custCity').val(d.cityName).trigger('change');
                } else {
                    $('#custCity').append(new Option(d.cityName, d.cityName, true, true)).trigger('change');
                }
            }
        } else {
            $('#custUserId').val('');
            $('#custPhone').val('');
        }
    });

    $('#custCity').select2({
        tags: true,
        placeholder: 'Select or type city...',
        width: '100%'
    });

    $('#confirmCustBtn').click(function () {
        var sel = $('#custName').select2('data');
        var name = sel && sel.length > 0 ? (sel[0].text || '') : '';
        var phone = $('#custPhone').val().trim();
        var city = $('#custCity').val();

        if (!name) { showToast('Customer name is required.', 'warning'); return; }
        if (!phone) { showToast('Phone number is required.', 'warning'); return; }
        if (!city) { showToast('City is required.', 'warning'); return; }

        if (_editMode) {
            showToast('Customer info validated.', 'success');
            return;
        }

        afterCustomerConfirm();
    });

    function afterCustomerConfirm() {
        customerConfirmed = true;
        $('#custName, #custPhone, #custCity, #confirmCustBtn').prop('disabled', true);
        $('#dateSection, #itemsSection, #paymentSection, #saveBillBtn').removeClass('hidden-section');

        var today = new Date().toISOString().split('T')[0];
        $('#billDate').val(today);

        if (batchCount === 0) addBatch();
    }

    var today = new Date().toISOString().split('T')[0];
    $('#billDate').val(today);

    function addBatch() {
        batchCount++;
        var id = batchCount;
        var html =
            '<div class="batch-card" data-batch="' + id + '">' +
            '  <div class="d-flex justify-content-between align-items-center mb-2">' +
            '    <strong><i class="bi bi-box-seam text-primary me-2"></i>Item Entry</strong>' +
            '    <button type="button" class="btn btn-sm btn-outline-danger remove-batch" data-batch="' + id + '"><i class="bi bi-trash me-1"></i>Remove</button>' +
            '  </div>' +
            '  <div class="row g-2 mb-2">' +
            '    <div class="col-md-4">' +
            '      <label class="form-label">Brand</label>' +
            '      <select id="brand-' + id + '" class="form-control batch-brand"></select>' +
            '    </div>' +
            '    <div class="col-md-4">' +
            '      <label class="form-label">Type</label>' +
            '      <select id="type-' + id + '" class="form-control batch-type" disabled><option value="">--Select Brand First--</option></select>' +
            '    </div>' +
            '    <div class="col-md-2">' +
            '      <label class="form-label">Qty</label>' +
            '      <input type="number" id="qty-' + id + '" class="form-control batch-qty" value="1" min="1" />' +
            '    </div>' +
            '    <div class="col-md-2">' +
            '      <label class="form-label d-none d-md-block">&nbsp;</label>' +
            '      <button type="button" class="btn btn-primary w-100 fetch-serials" data-batch="' + id + '"><i class="bi bi-search me-1"></i>Fetch</button>' +
            '    </div>' +
            '  </div>' +
            '  <div class="batch-price-info d-none" id="priceInfo-' + id + '">' +
            '    <span>Price: <strong class="batch-unit-price">₹0</strong>' +
            '    &nbsp;×&nbsp; Qty: <span class="batch-qty-label">1</span>' +
            '    &nbsp;=&nbsp; <strong class="batch-total-price text-primary">₹0</strong></span>' +
            '  </div>' +
            '  <div class="serials-container" id="serials-' + id + '"></div>' +
            '</div>';

        $('#batchesContainer').append(html);
        populateBrandDropdown(id);
        setupBatchEvents(id);
    }

    function populateBrandDropdown(id) {
        var sel = $('#brand-' + id);
        sel.append('<option value="">--Select Brand--</option>');
        _modelBrandList.forEach(function (b) {
            sel.append('<option value="' + b.BrandId + '">' + b.BrandName + '</option>');
        });
    }

    function setupBatchEvents(id) {
        $('#brand-' + id).change(function () {
            var brandId = Number($(this).val());
            var typeSel = $('#type-' + id);
            typeSel.empty().append('<option value="">--Select Type--</option>').prop('disabled', !brandId);
            if (brandId) {
                _modelTypeList.forEach(function (t) {
                    if (t.BrandId === brandId) {
                        typeSel.append('<option value="' + t.TypeId + '" data-price="' + (t.itemPrice || 0) + '" data-oldprice="' + (t.oldItemPrice || 0) + '">' + t.TypeName + '</option>');
                    }
                });
            }
        });

        $('#type-' + id).change(function () {
            var sel = $(this);
            var price = Number(sel.find('option:selected').data('price') || 0);
            $('#priceInfo-' + id).removeClass('d-none').find('.batch-unit-price').text('₹' + price.toFixed(2));
            updateBatchPrice(id);
            clearSerials(id);

            var brandId = Number($('#brand-' + id).val());
            var typeId = Number(sel.val());
            if (brandId && typeId) {
                $.getJSON('/Bill/GetAvailableCount', { brandId: brandId, typeId: typeId })
                    .done(function (data) {
                        $('#priceInfo-' + id + ' .batch-count').remove();
                        var ct = data.availableCount || 0;
                        var cls = ct > 0 ? 'text-success' : 'text-danger';
                        $('#priceInfo-' + id + ' span').append(' <span class="batch-count">| In Stock: <strong class="' + cls + '">' + ct + '</strong></span>');
                    })
                    .fail(function () {
                        $('#priceInfo-' + id + ' .batch-count').remove();
                        $('#priceInfo-' + id + ' span').append(' <span class="batch-count text-muted">| In Stock: <strong>—</strong></span>');
                    });
            }
        });

        $('#qty-' + id).change(function () {
            updateBatchPrice(id);
            clearSerials(id);
        });

        $('.remove-batch[data-batch="' + id + '"]').click(function () {
            if ($('.batch-card').length > 1) {
                $(this).closest('.batch-card').remove();
                recalcPayment();
            } else {
                showToast('At least one item is required.', 'warning');
            }
        });

        $('.fetch-serials[data-batch="' + id + '"]').click(function () {
            fetchSerials(id);
        });
    }

    function updateBatchPrice(id) {
        var price = Number($('#type-' + id).find('option:selected').data('price') || 0);
        var qty = Number($('#qty-' + id).val() || 0);
        $('#priceInfo-' + id).find('.batch-qty-label').text(qty);
        $('#priceInfo-' + id).find('.batch-total-price').text('₹' + (price * qty).toFixed(2));
    }

    function clearSerials(id) {
        $('#serials-' + id).empty();
        recalcPayment();
    }

    function fetchSerials(batchId) {
        var brandId = Number($('#brand-' + batchId).val());
        var typeId = Number($('#type-' + batchId).val());
        var qty = Number($('#qty-' + batchId).val());

        if (!brandId) { showToast('Select a brand.', 'warning'); return; }
        if (!typeId) { showToast('Select a type.', 'warning'); return; }
        if (!qty || qty < 1) { showToast('Enter valid quantity.', 'warning'); return; }

        $.ajax({
            url: '/Bill/GetAvailableSerials',
            type: 'GET',
            data: { brandId: brandId, typeId: typeId, count: qty },
            success: function (data) {
                if (!data || data.length === 0) {
                    showToast('No in-stock items found for this brand/type.', 'warning');
                    return;
                }
                renderSerials(batchId, data);
            },
            error: function () {
                showToast('Failed to fetch serials.', 'error');
            }
        });
    }

    function renderSerials(batchId, serials) {
        var container = $('#serials-' + batchId);
        container.empty();

        serials.forEach(function (s, idx) {
            var sid = batchId + '-' + idx;
            var formattedPrice = Number(s.itemPrice || 0).toFixed(2);
            var row =
                '<div class="serial-row" data-serial-id="' + sid + '" data-item-id="' + s.itemId + '">' +
                '  <div class="serial-header-bar">' +
                '    <div class="d-flex align-items-center gap-2">' +
                '      <span class="serial-tag-badge"><i class="bi bi-upc-scan me-2"></i>Item #' + (idx + 1) + '&nbsp;&bull;&nbsp;<span class="font-monospace text-primary ms-1">' + escapeHtml(s.itemSerialNumber) + '</span></span>' +
                '      <span class="serial-price-badge">₹' + formattedPrice + '</span>' +
                '    </div>' +
                '    <button type="button" class="btn btn-sm btn-outline-danger border-0 remove-serial px-2" title="Remove Item"><i class="bi bi-trash3 me-1"></i>Remove</button>' +
                '  </div>' +
                '  <div class="row g-3 align-items-end">' +
                '    <div class="col-md-4 col-sm-6">' +
                '      <label class="serial-label">Exchange / Return Status</label>' +
                '      <select class="form-control old-status"></select>' +
                '    </div>' +
                '    <div class="col-md-4 col-sm-6 old-date-col hidden-section">' +
                '      <label class="serial-label">Old Sale Date</label>' +
                '      <input type="date" class="form-control old-date" />' +
                '    </div>' +
                '    <div class="col-md-4 col-sm-6 old-serial-col hidden-section">' +
                '      <label class="serial-label">Old Serial Number</label>' +
                '      <input type="text" class="form-control old-serial" placeholder="Enter old serial #" />' +
                '    </div>' +
                '    <div class="col-md-4 col-sm-6 discount-col hidden-section">' +
                '      <label class="serial-label">Warranty Discount %</label>' +
                '      <input type="text" class="form-control discount-pct fw-bold text-success" readonly value="0" />' +
                '    </div>' +
                '    <div class="col-md-4 col-sm-6">' +
                '      <label class="serial-label">Vehicle Mapping</label>' +
                '      <button type="button" class="btn btn-outline-primary w-100 veh-btn btn-veh-custom" data-serial-id="' + sid + '"><i class="bi bi-car-front me-1"></i>Link Vehicle</button>' +
                '    </div>' +
                '  </div>' +
                '  <input type="text" class="serial-display d-none" value="' + escapeHtml(s.itemSerialNumber) + '" readonly />' +
                '  <input type="hidden" class="old-item-price" value="0" />' +
                '  <input type="hidden" class="item-price" value="' + s.itemPrice + '" />' +
                '  <input type="hidden" class="vehicle-info-id" value="" />' +
                '</div>';
            container.append(row);
            populateOldStatus(sid);
        });

        recalcPayment();
        updateBatchPrice(batchId);
    }

    function populateOldStatus(sid) {
        var sel = $('[data-serial-id="' + sid + '"] .old-status');
        sel.empty().append('<option value="">-- No Return (New Sale) --</option>');
        _modelOldStatusList.forEach(function (os) {
            sel.append('<option value="' + os.OldItemStatusId + '">' + os.OldItemStatusName + '</option>');
        });
    }

    $(document).on('click', '.remove-serial', function () {
        var row = $(this).closest('.serial-row');
        var batchCard = row.closest('.batch-card');
        var batchId = batchCard.data('batch');
        row.remove();
        var remaining = batchCard.find('.serial-row').length;
        $('#qty-' + batchId).val(remaining);
        recalcPayment();
    });

    $(document).on('change', '.old-status', function () {
        var row = $(this).closest('.serial-row');
        var sid = row.data('serial-id');
        var statusId = Number($(this).val());

        var oldDateCol = row.find('.old-date-col');
        var oldSerialCol = row.find('.old-serial-col');
        var discountCol = row.find('.discount-col');
        var oldPriceInput = row.find('.old-item-price');

        if (statusId === 1 || statusId === 2) {
            oldDateCol.removeClass('hidden-section');
            oldSerialCol.removeClass('hidden-section');
            discountCol.removeClass('hidden-section');
            oldPriceInput.val(0);
        } else if (statusId >= 3) {
            oldDateCol.addClass('hidden-section');
            oldSerialCol.addClass('hidden-section');
            discountCol.addClass('hidden-section');
            row.find('.old-date').val('');
            row.find('.discount-pct').val(0);

            var typeId = Number($('#type-' + row.closest('.batch-card').data('batch')).val());
            var typeObj = _modelTypeList.find(function (t) { return t.TypeId === typeId; });
            if (typeObj) {
                oldPriceInput.val(typeObj.oldItemPrice || 0);
            }
        } else {
            oldDateCol.addClass('hidden-section');
            oldSerialCol.addClass('hidden-section');
            discountCol.addClass('hidden-section');
            oldPriceInput.val(0);
        }
        recalcPayment();
    });

    $(document).on('change', '.old-date', function () {
        var row = $(this).closest('.serial-row');
        var typeId = Number($('#type-' + row.closest('.batch-card').data('batch')).val());
        var oldDate = $(this).val();

        if (typeId && oldDate) {
            $.ajax({
                url: '/Bill/GetDiscount',
                type: 'GET',
                data: { itemTypeId: typeId, oldItemDateOfSale: oldDate },
                success: function (data) {
                    var pct = data.discountPercent || 0;
                    row.find('.discount-pct').val(pct);
                    recalcPayment();
                }
            });
        }
    });

    $(document).on('click', '.veh-btn', function () {
        var sid = $(this).data('serial-id');
        $('#vehTargetSerialId').val(sid);
        $('#vehBrand').val('').trigger('change');
        $('#vehModel').empty().append('<option value="">--Select Brand First--</option>').prop('disabled', true);
        $('#vehRegNumber').val('');
        var modal = new bootstrap.Modal(document.getElementById('vehicleInfoModal'));
        modal.show();
    });

    $('#vehBrand').change(function () {
        var brandId = Number($(this).val());
        var modelSel = $('#vehModel');
        modelSel.empty().append('<option value="">--Select Model--</option>').prop('disabled', !brandId);
        if (!brandId) return;

        $.ajax({
            url: '/Bill/GetVehicleModels',
            type: 'GET',
            data: { brandId: brandId },
            success: function (data) {
                if (data && data.length) {
                    data.forEach(function (m) {
                        modelSel.append('<option value="' + m.VehicleModelId + '">' + m.VehicleModelName + '</option>');
                    });
                }
            }
        });
    });

    $('#vehSaveBtn').click(function () {
        var modelId = Number($('#vehModel').val());
        var reg = $('#vehRegNumber').val().trim();
        var sid = $('#vehTargetSerialId').val();

        if (!modelId) { showToast('Select vehicle brand and model.', 'warning'); return; }
        if (!reg) { showToast('Enter registration number.', 'warning'); return; }

        pendingVehicleInfos[sid] = { modelId: modelId, regNumber: reg };

        var brandText = $('#vehBrand option:selected').text();
        var modelText = $('#vehModel option:selected').text();
        $('[data-serial-id="' + sid + '"] .veh-btn').html('<i class="bi bi-car-front-fill me-1"></i> ' + escapeHtml(brandText) + ' ' + escapeHtml(modelText) + ' (' + escapeHtml(reg) + ')').removeClass('btn-outline-primary btn-outline-info').addClass('btn-success');
        $('[data-serial-id="' + sid + '"] .vehicle-info-id').val('-1');

        var modal = bootstrap.Modal.getInstance(document.getElementById('vehicleInfoModal'));
        if (modal) modal.hide();
    });

    function recalcPayment() {
        var subtotal = 0;
        var tradein = 0;
        var discount = 0;

        $('.serial-row').each(function () {
            var itemPrice = Number($(this).find('.item-price').val() || 0);
            var oldPrice = Number($(this).find('.old-item-price').val() || 0);
            var discPct = Number($(this).find('.discount-pct').val() || 0);
            var statusId = Number($(this).find('.old-status').val() || 0);

            if (statusId === 1 || statusId === 2) {
                discount += itemPrice * discPct / 100;
            } else if (statusId >= 3) {
                tradein += oldPrice;
            }

            subtotal += itemPrice;
        });

        var total = subtotal - tradein - discount;
        if (total < 0) total = 0;

        $('#paySubtotal').text(subtotal.toFixed(2));
        $('#payTradein').text(tradein.toFixed(2));
        $('#payDiscount').text(discount.toFixed(2));
        $('#payTotal').text(total.toFixed(2));

        $('#payPaid').attr('max', total);
        calcDue();
        updateSaveButton();
    }

    function calcDue() {
        var total = parseFloat($('#payTotal').text()) || 0;
        var paid = parseFloat($('#payPaid').val()) || 0;
        var due = total - paid;
        if (due < 0) due = total;
        $('#payDue').text(due.toFixed(2));
        updateSaveButton();
    }

    $('#payPaid').on('input', function () {
        calcDue();
    });

    function updateSaveButton() {
        var total = parseFloat($('#payTotal').text()) || 0;
        var paid = parseFloat($('#payPaid').val()) || 0;
        var enabled = customerConfirmed && total > 0 && paid >= 0 && paid <= total;
        $('#saveBillBtn').prop('disabled', !enabled);
    }

    $('#billAddForm').submit(function (e) {
        e.preventDefault();
        saveBill();
    });

    function saveBill() {
        var sel = $('#custName').select2('data');
        var name = sel && sel.length > 0 ? (sel[0].text || '') : '';
        var phone = $('#custPhone').val().trim();
        var city = $('#custCity').val();

        if (!name || !phone || !city) {
            showToast('Customer information is incomplete.', 'warning');
            return;
        }

        var dateOfSale = $('#billDate').val();
        if (!dateOfSale) { showToast('Bill date is required.', 'warning'); return; }

        var items = [];

        $('.serial-row').each(function () {
            var row = $(this);
            var statusId = Number(row.find('.old-status').val() || 0);
            var oldItemDate = row.find('.old-date').val() || null;
            var oldItemSerial = row.find('.old-serial').val() || null;
            var discountPct = Number(row.find('.discount-pct').val() || 0);
            var itemPrice = Number(row.find('.item-price').val() || 0);
            var oldPrice = Number(row.find('.old-item-price').val() || 0);
            var itemId = Number(row.data('item-id'));
            var sid = row.data('serial-id');

            var finalPrice;
            if (statusId === 1 || statusId === 2) {
                finalPrice = itemPrice * (100 - discountPct) / 100;
            } else if (statusId >= 3) {
                finalPrice = itemPrice - oldPrice;
            } else {
                finalPrice = itemPrice;
            }

            var billItemId = row.data('bill-item-id') || null;
            var item = {
                billItemId: billItemId,
                itemId: itemId,
                oldItemId: null,
                oldItemDateOfSale: oldItemDate,
                oldItemSerialNumber: oldItemSerial,
                itemPrice: itemPrice,
                oldItemPrice: statusId >= 3 ? oldPrice : 0,
                discountPercentage: discountPct,
                itemFinalPrice: Math.round(finalPrice * 100) / 100,
                vehicleModelId: (pendingVehicleInfos[sid] || {}).modelId || null,
                vehicleRegNumber: (pendingVehicleInfos[sid] || {}).regNumber || null
            };

            items.push(item);
        });

        if (items.length === 0) { showToast('No items to save.', 'warning'); return; }

        var totalAmount = parseFloat($('#payTotal').text()) || 0;
        var paidAmount = parseFloat($('#payPaid').val()) || 0;

        if (_editMode) {
            var origPhone = _editCustomer ? _editCustomer.userPhone : '';
            if (phone !== origPhone) {
                $.ajax({
                    url: '/Bill/CheckCustomerPhone',
                    type: 'GET',
                    data: { phone: phone, excludeUserId: (_editCustomer ? _editCustomer.userId : 0) },
                    success: function (data) {
                        if (data.exists) {
                            showConfirm('Duplicate Phone', 'This phone belongs to <strong>' + escapeHtml(data.userName) + '</strong>. The bill will be assigned to this customer. Continue?', 'Assign to ' + escapeHtml(data.userName)).then(function (confirmed) {
                                if (confirmed) {
                                    doSaveBill(name, phone, city, dateOfSale, items, totalAmount, paidAmount);
                                }
                            });
                        } else {
                            doSaveBill(name, phone, city, dateOfSale, items, totalAmount, paidAmount);
                        }
                    },
                    error: function () {
                        doSaveBill(name, phone, city, dateOfSale, items, totalAmount, paidAmount);
                    }
                });
                return;
            }
        }

        doSaveBill(name, phone, city, dateOfSale, items, totalAmount, paidAmount);
    }

    function doSaveBill(name, phone, city, dateOfSale, items, totalAmount, paidAmount) {
        var request = {
            billId: _editMode ? _editBillId : 0,
            dateOfSale: dateOfSale,
            totalAmount: totalAmount,
            paidAmount: paidAmount,
            itemsJson: JSON.stringify(items)
        };

        request.customerId = getCustomerUserId();
        request.customerName = name;
        request.customerPhone = phone;
        request.customerCity = city;

        $('#saveBillBtn').prop('disabled', true).text('Saving bill...');

        var saveUrl = _editMode ? '/Bill/BillEdit' : '/Bill/BillAdd';
        var saveLabel = _editMode ? 'Update Bill' : 'Save Bill';

        $.ajax({
            url: saveUrl,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(request),
            success: function (data) {
                if (data.success) {
                    var msg = _editMode ? 'Bill #' + data.billId + ' updated successfully!' : 'Bill #' + data.billId + ' created successfully!';
                    showToast(msg, 'success');
                    setTimeout(function () {
                        window.location.href = '/Bill/BillList';
                    }, 1000);
                } else {
                    showToast(data.message || 'Failed to save bill.', 'error');
                    $('#saveBillBtn').prop('disabled', false).text(saveLabel);
                }
            },
            error: function () {
                showToast('Failed to save bill.', 'error');
                $('#saveBillBtn').prop('disabled', false).text(saveLabel);
            }
        });
    }

    function getCustomerUserId() {
        var uid = $('#custUserId').val();
        if (uid) return Number(uid);
        var sel = $('#custName').select2('data');
        if (sel && sel.length > 0 && sel[0].id && sel[0].id != sel[0].text) {
            return Number(sel[0].id);
        }
        return null;
    }

    $('#addBatchBtn').click(function () {
        addBatch();
    });

    $(document).on('change', '.batch-qty, .old-date, .old-serial, .discount-pct', function () {
        recalcPayment();
    });

    $(document).on('keyup', '.old-serial, .discount-pct', function () {
        recalcPayment();
    });

    function renderEditSerials(batchId, items) {
        var container = $('#serials-' + batchId);

        items.forEach(function (item, idx) {
            var sid = 'edit-' + batchId + '-' + idx;
            var formattedPrice = Number(item.itemPrice || 0).toFixed(2);
            var hasWarranty = item.discountPercentage > 0;
            var hasExchange = Number(item.oldItemPrice || 0) > 0;
            var hasOldItem = item.oldItemId !== null && item.oldItemId !== undefined;

            var rowHtml =
                '<div class="serial-row" data-serial-id="' + sid + '" data-item-id="' + item.itemId + '" data-bill-item-id="' + (item.billItemId || '') + '">' +
                '  <div class="serial-header-bar">' +
                '    <div class="d-flex align-items-center gap-2">' +
                '      <span class="serial-tag-badge"><i class="bi bi-upc-scan me-2"></i>Item #' + (idx + 1) + '&nbsp;&bull;&nbsp;<span class="font-monospace text-primary ms-1">' + escapeHtml(item.itemSerialNumber) + '</span></span>' +
                '      <span class="serial-price-badge">₹' + formattedPrice + '</span>' +
                '    </div>' +
                '    <button type="button" class="btn btn-sm btn-outline-danger border-0 remove-serial px-2" title="Remove Item"><i class="bi bi-trash3 me-1"></i>Remove</button>' +
                '  </div>' +
                '  <div class="row g-3 align-items-end">' +
                '    <div class="col-md-4 col-sm-6">' +
                '      <label class="serial-label">Exchange / Return Status</label>' +
                '      <select class="form-control old-status"></select>' +
                '    </div>' +
                '    <div class="col-md-4 col-sm-6 old-date-col' + (hasOldItem ? '' : ' hidden-section') + '">' +
                '      <label class="serial-label">Old Sale Date</label>' +
                '      <input type="date" class="form-control old-date" value="' + (item.oldItemDateOfSale || '') + '" />' +
                '    </div>' +
                '    <div class="col-md-4 col-sm-6 old-serial-col' + (hasOldItem ? '' : ' hidden-section') + '">' +
                '      <label class="serial-label">Old Serial Number</label>' +
                '      <input type="text" class="form-control old-serial" value="' + escapeHtml(item.oldItemSerialNumber || '') + '" placeholder="Enter old serial #" />' +
                '    </div>' +
                '    <div class="col-md-4 col-sm-6 discount-col' + (hasWarranty ? '' : ' hidden-section') + '">' +
                '      <label class="serial-label">Warranty Discount %</label>' +
                '      <input type="text" class="form-control discount-pct fw-bold text-success" readonly value="' + (item.discountPercentage || 0) + '" />' +
                '    </div>' +
                '    <div class="col-md-4 col-sm-6">' +
                '      <label class="serial-label">Vehicle Mapping</label>' +
                '      <button type="button" class="btn btn-outline-primary w-100 veh-btn btn-veh-custom" data-serial-id="' + sid + '"><i class="bi bi-car-front me-1"></i>Link Vehicle</button>' +
                '    </div>' +
                '  </div>' +
                '  <input type="text" class="serial-display d-none" value="' + escapeHtml(item.itemSerialNumber) + '" readonly />' +
                '  <input type="hidden" class="old-item-price" value="' + (Number(item.oldItemPrice) || 0) + '" />' +
                '  <input type="hidden" class="item-price" value="' + (Number(item.itemPrice) || 0) + '" />' +
                '  <input type="hidden" class="vehicle-info-id" value="" />' +
                '</div>';

            container.append(rowHtml);
            populateOldStatus(sid);

            if (hasOldItem) {
                var statusSel = $('[data-serial-id="' + sid + '"] .old-status');
                if (hasWarranty) {
                    statusSel.val(1);
                } else if (hasExchange) {
                    statusSel.val(3);
                }
            }

            if (item.vehicleRegNumber) {
                pendingVehicleInfos[sid] = { modelId: item.vehicleModelId, regNumber: item.vehicleRegNumber };
                $('[data-serial-id="' + sid + '"] .veh-btn').html('<i class="bi bi-car-front-fill me-1"></i> ' + escapeHtml(item.vehicleRegNumber)).removeClass('btn-outline-primary').addClass('btn-success');
            }
        });

        recalcPayment();
        updateBatchPrice(batchId);
    }

    function initEditMode() {
        customerConfirmed = true;

        if (_editCustomer && _editCustomer.userFullName) {
            var opt = new Option(_editCustomer.userFullName, _editCustomer.userId || _editCustomer.userFullName, true, true);
            $('#custName').append(opt).trigger('change');
            $('#custPhone').val(_editCustomer.userPhone || '');
            $('#custUserId').val(_editCustomer.userId || '');
            if (_editCustomer.cityName) {
                if ($('#custCity').find('option[value="' + _editCustomer.cityName + '"]').length) {
                    $('#custCity').val(_editCustomer.cityName).trigger('change');
                } else {
                    $('#custCity').append(new Option(_editCustomer.cityName, _editCustomer.cityName, true, true)).trigger('change');
                }
            }
        }

        $('#dateSection, #itemsSection, #paymentSection, #saveBillBtn').removeClass('hidden-section');

        var groups = {};
        _editItems.forEach(function (item) {
            var key = item.BrandId + '-' + item.itemTypeId;
            if (!groups[key]) groups[key] = { brandId: item.BrandId, typeId: item.itemTypeId, items: [] };
            groups[key].items.push(item);
        });

        $('#batchesContainer').empty();
        batchCount = 0;

        var groupKeys = Object.keys(groups);
        groupKeys.forEach(function (key, gIdx) {
            var group = groups[key];
            addBatch();
            var id = batchCount;
            $('#brand-' + id).val(group.brandId).trigger('change');
            setTimeout(function () {
                $('#type-' + id).val(group.typeId).trigger('change');
                setTimeout(function () {
                    $('#serials-' + id).empty();
                    renderEditSerials(id, group.items);
                    $('#brand-' + id).prop('disabled', true);
                    $('#type-' + id).prop('disabled', true);
                    $('#qty-' + id).prop('readonly', true);
                }, 100);
            }, 100);
        });

        if (_editDateOfSale) $('#billDate').val(_editDateOfSale);
        if (_editPaidAmount > 0) $('#payPaid').val(_editPaidAmount);

        setTimeout(function () { recalcPayment(); }, 500);
    }

    function escapeHtml(str) {
        if (!str) return '';
        return String(str).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
    }

    if (_editMode) initEditMode();
});