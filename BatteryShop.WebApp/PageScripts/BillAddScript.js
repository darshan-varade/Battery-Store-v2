$(document).ready(function () {
    var batchCount = 0;
    var customerConfirmed = false;
    var pendingVehicleInfos = {};

    // ====== Customer Select2 ======
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

    // ====== Confirm Customer (no DB call — deferred to bill submit) ======
    $('#confirmCustBtn').click(function () {
        var sel = $('#custName').select2('data');
        var name = sel && sel.length > 0 ? (sel[0].text || '') : '';
        var phone = $('#custPhone').val().trim();
        var city = $('#custCity').val();

        if (!name) { showToast('Customer name is required.', 'warning'); return; }
        if (!phone) { showToast('Phone number is required.', 'warning'); return; }
        if (!city) { showToast('City is required.', 'warning'); return; }

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

    // ====== Bill Date default ======
    var today = new Date().toISOString().split('T')[0];
    $('#billDate').val(today);

    // ====== Add Item Batch ======
    function addBatch() {
        batchCount++;
        var id = batchCount;
        var html =
            '<div class="batch-card" data-batch="' + id + '">' +
            '  <div class="d-flex justify-content-between align-items-center mb-2">' +
            '    <strong>Batch #' + id + '</strong>' +
            '    <button type="button" class="btn btn-sm btn-outline-danger remove-batch" data-batch="' + id + '">\u2715 Remove</button>' +
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
            '    <div class="col-md-2 d-flex align-items-end">' +
            '      <button type="button" class="btn btn-primary btn-sm w-100 fetch-serials" data-batch="' + id + '">Fetch</button>' +
            '    </div>' +
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
            clearSerials(id);
        });

        $('#qty-' + id).change(function () {
            clearSerials(id);
        });

        $('.remove-batch[data-batch="' + id + '"]').click(function () {
            if ($('.batch-card').length > 1) {
                $(this).closest('.batch-card').remove();
                recalcPayment();
            } else {
                showToast('At least one batch is required.', 'warning');
            }
        });

        $('.fetch-serials[data-batch="' + id + '"]').click(function () {
            fetchSerials(id);
        });
    }

    function clearSerials(id) {
        $('#serials-' + id).empty();
        recalcPayment();
    }

    // ====== Fetch Serials ======
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
            var row =
                '<div class="serial-row" data-serial-id="' + sid + '" data-item-id="' + s.itemId + '">' +
                '  <div class="row g-2">' +
                '    <div class="col-md-2"><label class="form-label">Serial</label><input type="text" class="form-control serial-display" value="' + escapeHtml(s.itemSerialNumber) + '" readonly /></div>' +
                '    <div class="col-md-2"><label class="form-label">Old Status</label><select class="form-control old-status"><option value="">--None--</option></select></div>' +
                '    <div class="col-md-2 old-date-col hidden-section"><label class="form-label">Old Date</label><input type="date" class="form-control old-date" /></div>' +
                '    <div class="col-md-2 old-serial-col hidden-section"><label class="form-label">Old Serial</label><input type="text" class="form-control old-serial" placeholder="Enter old serial" /></div>' +
                '    <div class="col-md-2 discount-col hidden-section"><label class="form-label">Discount %</label><input type="text" class="form-control discount-pct" readonly value="0" /></div>' +
                '    <div class="col-md-2"><label class="form-label">Vehicle</label><button type="button" class="btn btn-outline-info btn-sm w-100 veh-btn" data-serial-id="' + sid + '">Add Vehicle</button></div>' +
                '  </div>' +
                '  <input type="hidden" class="old-item-price" value="0" />' +
                '  <input type="hidden" class="item-price" value="' + s.itemPrice + '" />' +
                '  <input type="hidden" class="vehicle-info-id" value="" />' +
                '</div>';
            container.append(row);
            populateOldStatus(sid);
        });

        recalcPayment();
    }

    function populateOldStatus(sid) {
        var sel = $('[data-serial-id="' + sid + '"] .old-status');
        sel.append('<option value="">--None--</option>');
        _modelOldStatusList.forEach(function (os) {
            sel.append('<option value="' + os.OldItemStatusId + '">' + os.OldItemStatusName + '</option>');
        });
    }

    // ====== Old Status change ======
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

    // ====== Old Date change ======
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

    // ====== Vehicle Info Modal (local only, no DB save) ======
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
        $('[data-serial-id="' + sid + '"] .veh-btn').text(brandText + ' ' + modelText + ' ' + reg).removeClass('btn-outline-info').addClass('btn-info');
        $('[data-serial-id="' + sid + '"] .vehicle-info-id').val('-1');

        var modal = bootstrap.Modal.getInstance(document.getElementById('vehicleInfoModal'));
        if (modal) modal.hide();
    });

    // ====== Payment Calculation ======
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

    // ====== Save Bill ======
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

        // Build item list (vehicle info embedded directly)
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

            var item = {
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

        // Build request — all data sent together, SP handles customer + vehicle creation
        var request = {
            dateOfSale: dateOfSale,
            totalAmount: totalAmount,
            paidAmount: paidAmount,
            itemsJson: JSON.stringify(items)
        };

        var custId = getCustomerUserId();
        if (custId) {
            request.customerId = custId;
        } else {
            request.customerName = name;
            request.customerPhone = phone;
            request.customerCity = city;
        }

        $('#saveBillBtn').prop('disabled', true).text('Saving bill...');

        $.ajax({
            url: '/Bill/BillAdd',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(request),
            success: function (data) {
                if (data.success) {
                    showToast('Bill #' + data.billId + ' created successfully!', 'success');
                    setTimeout(function () {
                        window.location.href = '/Bill/BillList';
                    }, 1000);
                } else {
                    showToast(data.message || 'Failed to save bill.', 'error');
                    $('#saveBillBtn').prop('disabled', false).text('Save Bill');
                }
            },
            error: function () {
                showToast('Failed to save bill.', 'error');
                $('#saveBillBtn').prop('disabled', false).text('Save Bill');
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

    // ====== Add Batch button ======
    $('#addBatchBtn').click(function () {
        addBatch();
    });

    // ====== Key events to recalc ======
    $(document).on('change', '.batch-qty, .old-date, .old-serial, .discount-pct', function () {
        recalcPayment();
    });

    $(document).on('keyup', '.old-serial, .discount-pct', function () {
        recalcPayment();
    });

    function escapeHtml(str) {
        if (!str) return '';
        return String(str).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
    }
});
