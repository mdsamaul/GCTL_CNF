
// Example usage: Populate with sample data
$(document).ready(function () {

    showDev('loaded')

    //#region Select 2

    const initializeSelect = () => {
        // $('.searchableSelect').select2({
        //     width: '100%',
        //     allowClear: true,
        //     placeholder: 'Select an option',
        //     language: { noResults: () => 'No results found' },
        //     escapeMarkup: markup => markup
        // });
    };

    initializeSelect();

    //#endregion

    //#region Populate TopTable

    let currentPage = 1;
    let pageSize = 5;
    let filteredRecords = 0;


    $('#pageSizeSelect').val(pageSize);
    loadJobs();

    // Reload on filter change
    $('#customerFilter, #shipmentModeFilter, #dateFrom, #dateTo, #searchInput, #pageSizeSelect')
        .on('change keyup', function () {
            currentPage = 1;
            pageSize = parseInt($('#pageSizeSelect').val());
            loadJobs();
        });

    // Pagination clicks
    $(document).on('click', '#paginationPrev', function () {
        if (currentPage > 1) {
            currentPage--;
            loadJobs();
        }
    });

    $(document).on('click', '#paginationNext', function () {
        if (currentPage < Math.ceil(filteredRecords / pageSize)) {
            currentPage++;
            loadJobs();
        }
    });

    $(document).on('click', '#paginationNumbers .page-link', function (e) {
        e.preventDefault();
        currentPage = parseInt($(this).data('page'));
        loadJobs();
    });

    function loadJobs() {
        const filters = {
            page: currentPage,
            pageSize: pageSize,
            customerName: $('#customerFilter').val(),
            shipmentMode: $('#shipmentModeFilter').val(),
            dateFrom: $('#dateFrom').val(),
            dateTo: $('#dateTo').val(),
            search: $('#searchInput').val()
        };

        $.ajax({
            url: '/SepDocUpdate/GetJobs',
            type: 'GET',
            data: filters,
            success: function (response) {

                renderTable(response.data);
                filteredRecords = response.filteredRecords;

                $('#dataListInfo').text(
                    `Showing ${(currentPage - 1) * pageSize + 1} to ${Math.min(currentPage * pageSize, filteredRecords)} of ${filteredRecords} entries`
                );

                renderPagination(Math.ceil(filteredRecords / pageSize));
                populateDropdowns(response.data);
            },
            error: function () {
                alert('Error loading data');
            }
        });
    }

    function renderTable(data) {
        let rows = '';
        if (data.length === 0) {
            rows = '<tr><td colspan="6" class="text-center">No records found</td></tr>';
        } else {
            data.forEach(job => {
                rows += `<tr>
                            <td class="ps-0 py-2">
                                <a href="#" class="text-primary fw-bold job-detail-link" data-id="${job.id}">${job.jobNo}</a>
                            </td>
                            
                            <td class="ps-2  py-2 text-center">${job.shipmentMode}</td>
                            <td class="ps-2  py-2 text-center">${job.jobDate}</td>
                            <td class="ps-2  py-2 text-center">${job.customerName}</td>
                            <td class="ps-2  py-2 text-end">${job.docReceivedDate || '-'}</td>
                           
                        </tr>`;
            });
        }
        $('#job-settings-table').html(rows);
    }

    //function renderPagination(totalPages) {
    //    let pagesHtml = '';
    //    for (let i = 1; i <= totalPages; i++) {
    //        pagesHtml += `<li class="page-item ${i === currentPage ? 'active' : ''}">
    //                <a class="page-link" href="#" data-page="${i}">${i}</a>
    //            </li>`;
    //    }
    //    $('#paginationNumbers').html(pagesHtml || '<li class="page-item disabled"><a class="page-link">1</a></li>');

    //    $('#paginationPrev').prop('disabled', currentPage === 1);
    //    $('#paginationNext').prop('disabled', currentPage >= totalPages || totalPages === 0);
    //}

    function renderPagination(totalPages) {
        let pagesHtml = '';

        if (totalPages <= 7) {
            // ছোট সংখ্যক পেজ হলে সবগুলো দেখাও
            for (let i = 1; i <= totalPages; i++) {
                pagesHtml += `<li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#" data-page="${i}">${i}</a>
            </li>`;
            }
        } else {
            // সবসময় প্রথম পেজ দেখাও
            pagesHtml += `<li class="page-item ${currentPage === 1 ? 'active' : ''}">
            <a class="page-link" href="#" data-page="1">1</a>
        </li>`;

            // যদি currentPage > 4 হয় তবে ellipsis দেখাও
            if (currentPage > 4) {
                pagesHtml += `<li class="page-item disabled"><a class="page-link">...</a></li>`;
            }

            // currentPage এর চারপাশে 3টা পেজ দেখাও
            let start = Math.max(2, currentPage - 1);
            let end = Math.min(totalPages - 1, currentPage + 1);

            for (let i = start; i <= end; i++) {
                pagesHtml += `<li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#" data-page="${i}">${i}</a>
            </li>`;
            }

            // যদি currentPage < totalPages - 3 হয় তবে ellipsis দেখাও
            if (currentPage < totalPages - 3) {
                pagesHtml += `<li class="page-item disabled"><a class="page-link">...</a></li>`;
            }

            // সবসময় শেষ পেজ দেখাও
            pagesHtml += `<li class="page-item ${currentPage === totalPages ? 'active' : ''}">
            <a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a>
        </li>`;
        }

        $('#paginationNumbers').html(pagesHtml || '<li class="page-item disabled"><a class="page-link">1</a></li>');

        $('#paginationPrev').prop('disabled', currentPage === 1);
        $('#paginationNext').prop('disabled', currentPage >= totalPages || totalPages === 0);
    }

    function populateDropdowns(data) {
        const customers = [...new Set(data.map(x => x.customerName))].sort();
        const $cust = $('#customerFilter');
        if ($cust.children('option').length === 1) {
            customers.forEach(c => $cust.append(`<option value="${c}">${c}</option>`));
        }

        const modes = [...new Set(data.map(x => x.shipmentMode))].sort();
        const $mode = $('#shipmentModeFilter');
        if ($mode.children('option').length === 1) {
            modes.forEach(m => $mode.append(`<option value="${m}">${m}</option>`));
        }
    }

    //#endregion

    //#region On click populate Form

    // Click on Job No link or View button to load details
    $(document).on('click', '.job-detail-link, .btn-view-job', function (e) {
        e.preventDefault();
        const jobId = $(this).data('id');

        $.ajax({
            url: '/SepDocUpdate/GetJobDetail',
            type: 'GET',
            data: { id: jobId },
            success: function (data) {
                clearForm();
                populateForm(data);
                



            },
            error: function () {
                alert('Error loading job details');
            }
        });
    });

    //#endregion

    //#region nicher table
    let statusCurrentPage = 1;
    let statusPageSize = 10;
    let statusFilteredRecords = 0;


    $('#statusPageSizeSelect').val(statusPageSize);
    loadJobStatuses();

    // Search and page size change
    $('#statusSearchInput, #statusPageSizeSelect').on('keyup change', function () {
        statusCurrentPage = 1;
        statusPageSize = parseInt($('#statusPageSizeSelect').val());
        loadJobStatuses();
    });

    // Pagination
    $(document).on('click', '#statusPaginationPrev', function () {
        if (statusCurrentPage > 1) {
            statusCurrentPage--;
            loadJobStatuses();
        }
    });

    $(document).on('click', '#statusPaginationNext', function () {
        if (statusCurrentPage < Math.ceil(statusFilteredRecords / statusPageSize)) {
            statusCurrentPage++;
            loadJobStatuses();
        }
    });

    $(document).on('click', '#statusPaginationNumbers .page-link', function (e) {
        e.preventDefault();
        statusCurrentPage = parseInt($(this).data('page'));
        loadJobStatuses();
    });

    function loadJobStatuses() {
        const params = {
            page: statusCurrentPage,
            pageSize: statusPageSize,
            search: $('#statusSearchInput').val()
        };

        $.ajax({
            url: '/SepDocUpdate/GetJobStatuses', // Adjust controller name if needed
            type: 'GET',
            data: params,
            success: function (response) {
                renderStatusTable(response.data);
                statusFilteredRecords = response.filteredRecords;

                const from = (statusCurrentPage - 1) * statusPageSize + 1;
                const to = Math.min(statusCurrentPage * statusPageSize, statusFilteredRecords);
                $('#statusDataListInfo').text(`Showing ${from} to ${to} of ${statusFilteredRecords} entries`);

                renderStatusPagination(Math.ceil(statusFilteredRecords / statusPageSize));
            },
            error: function () {
                alert('Error loading job status history');
            }
        });
    }

    function renderStatusTable(data) {
        let rows = '';
        if (data.length === 0) {
            rows = '<tr><td colspan="6" class="text-center">No records found</td></tr>';
        } else {
            data.forEach(s => {
                const badgeClass = s.Status === "Success" ? "bg-success" :
                    s.Status === "Warning" ? "bg-warning" :
                        s.Status === "Danger" ? "bg-danger" : "bg-secondary";

                rows += `<tr>
                           <td class="ps-0">
                                <input type="checkbox"
                                       class="form-check-input"
                                       ${s.status === 'Approved' ? 'checked' : ''}
                                        />
                            </td>

                            <td class="ps-2">${s.jobUpdateCode}</td>
                            <td class="ps-2">
                                 <a href="#" class="job-no-link"
                                 data-jobno="${s.jobNo}"
                                  data-id="${s.id}" >
                                    <strong>${s.jobNo}</strong>
                                </a>

                            </td>
                            <td class="ps-2  ">${s.shipmentStatus}</td>
                            <td class="ps-2 text-end">${s.dateTime}</td>
                            
                        </tr>`;
            });
        }
        $('#jobStatusTableBody').html(rows);


        // Click event bind
        $('.job-no-link').off('click').on('click', function (e) {
            e.preventDefault();
            const jobNo = $(this).data('jobno');
            const id = $(this).data('id');


            $.ajax({
                url: '/SepDocUpdate/GetJobDetails', // কন্ট্রোলারের Action URL
                type: 'GET',
                data: { jobNo: jobNo, id: id },
                success: function (response) {


                    populateForm(response.jobTop)
                    populateBottomForm(response.jobBottom)

                    // Set as true (checked)
                    $('#isEditHidden').prop('checked', true);



                },
                error: function (xhr, status, error) {
                    console.error(error);
                }
            });
        });

    }

    function renderStatusPagination(totalPages) {
        let pagesHtml = '';
        for (let i = 1; i <= totalPages; i++) {
            pagesHtml += `<li class="page-item ${i === statusCurrentPage ? 'active' : ''}">
                        <a class="page-link" href="#" data-page="${i}">${i}</a>
                    </li>`;
        }
        $('#statusPaginationNumbers').html(pagesHtml || '<li class="page-item active"><a class="page-link">1</a></li>');

        $('#statusPaginationPrev').prop('disabled', statusCurrentPage === 1);
        $('#statusPaginationNext').prop('disabled', statusCurrentPage >= totalPages || totalPages === 0);
    }
    //#endregion

    //#region Submit



    $('#saveShipmentBtn').on('click', function (e) {
        e.preventDefault();

        // ফর্ম ডেটা সংগ্রহ
        var formData = $('#shipmentForm').serialize();

        $.ajax({
            url: $('#shipmentForm').attr('action'),   // asp-action="Create" অনুযায়ী URL নেবে
            type: $('#shipmentForm').attr('method'),  // method="post"
            data: formData,
            success: function (response) {
                if (response.success) {
                    clearForm();
                    loadJobStatuses();
                    toastr.success(response.message || "Shipment saved successfully!");
                }
                else {
                    toastr.warning(response.message || "Shipment not saved!");
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
                toastr.warning("Error occurred while saving shipment.");
            }
        });
    });





    //#endregion

    //#region SampleData

    var sampleData = {
        shipmentModeId: '1', // Assuming '1' is a valid option value
        isCustomJobNo: true,
        jobNo: 'JOB12345',
        jobDate: '2023-10-01',
        customerId: '2',
        customerDeliveryAddress: '123 Main St, City, Country',
        portId: '3',
        docsReceivedDate: '2023-09-15',
        lcExpNo: 'LC123456',
        lcValue: '100000',
        ipEpNo: 'IP789',
        ipDate: '2023-09-20',
        importerId: '4',
        invoiceNo: 'INV001',
        invoiceDate: '2023-09-25',
        blNo: 'BL987654',
        blDate: '2023-10-05',
        beNo: 'BE111222',
        beDate: '2023-10-10',
        containerNo: 'CONT123',
        containerSize: '20FT',
        lcaNo: 'LCA456',
        dischargeDate: '2023-10-15',
        materialDescription: 'Sample materials for import',
        quantity: 100,
        quantityUnitId: '5', // Assuming '5' is PKG unit
        weight: 5000,
        weightUnitId: '6', // Assuming '6' is KG unit
        forwarderId: '7',
        vesselRottNo: 'VESSEL001',
        remarks: 'Sample remarks'
    };

    //populateForm(sampleData);

    //#endregion


});

//#region Clr
function clearForm() {
    // Text inputs
    $('#jobNo').val('');
    $('#jobNoHidden').val('');
    $('#jobDate').val('');
    $('#customerDeliveryAddress').val('');
    $('#docsReceivedDate').val('');
    $('#lcExpNo').val('');
    $('#lcValue').val('');
    $('#ipEpNo').val('');
    $('#ipDate').val('');
    $('#invoiceNo').val('');
    $('#invoiceDate').val('');
    $('#blNo').val('');
    $('#blDate').val('');
    $('#beNo').val('');
    $('#beDate').val('');
    $('#containerNo').val('');
    $('#containerSize').val('');
    $('#lcaNo').val('');
    $('#dischargeDate').val('');
    $('#materialDescription').val('');
    $('#quantity').val('');
    $('#weight').val('');
    $('#vesselRottNo').val('');
    $('#remarks').val('');
    $('#creationDate').val('');
    $('#updatedDate').val('');

    // Date fields (Bottom form)
    $('#etaDeliveryDate').val('');
    $('#actualDeliveryDate').val('');
    $('#unstuffingDate').val('');
    $('#etdDate').val('');

    // Custom Dropdowns - instance.clear() call korte hobe, .val().trigger('change') na
    var dropdownIds = [
        '#shipmentModeId', '#customerId', '#portId', '#importerId',
        '#quantityUnitId', '#weightUnitId', '#forwarderId',
        '#placeOfLoadingId', '#shedYardId', '#freightChargeId'
    ];

    dropdownIds.forEach(function (id) {
        var instance = $(id).data('customDdInstance');
        if (instance) {
            instance.clear();
        } else {
            // customDropdown init na thakle fallback
            $(id).val('').trigger('change');
        }
    });

    // Textarea
    $('#shipmentStatus').val('');
    $('#autoId').val('');

    // Checkboxes
    $('#isCustomJobNo').prop('checked', false);
}
//#endregion

//#region Function to populate the form with data
function populateBottomForm(data) {

    showDev(data);

    // Date fields
    if (data.etaDeliveryDate) {
        $('#etaDeliveryDate').val(data.etaDeliveryDate.split('T')[0]);
    }
    if (data.actualDeliveryDate) {
        $('#actualDeliveryDate').val(data.actualDeliveryDate.split('T')[0]);
    }
    if (data.unstuffingDate) {
        $('#unstuffingDate').val(data.unstuffingDate.split('T')[0]);
    }
    if (data.etdDate) {
        $('#etdDate').val(data.etdDate.split('T')[0]);
    }

    // Dropdowns
    if (data.placeOfLoadingID) {
        $('#placeOfLoadingId').val(data.placeOfLoadingID).trigger('change');
    }
    if (data.shedYardID) {
        $('#shedYardId').val(data.shedYardID).trigger('change');
    }
    if (data.freightChargeID) {
        $('#freightChargeId').val(data.freightChargeID).trigger('change');
    }

    // Textarea
    if (data.shipmentStatus) {
        $('#shipmentStatus').val(data.shipmentStatus);
    }


    if (data.autoId) {
        $('#autoId').val(data.autoId);


    }
}

function populateForm(data) {

    showDev(data)

    setCustomDropdownValue('#shipmentModeId', data.shipmentModeId);
    $('#isCustomJobNo').prop('checked', data.isCustomJobNo || false);
    $('#jobNo').val(data.jobNo || '');
    $('#jobNoHidden').val(data.jobNo || '');
    $('#jobDate').val(data.jobDate || '');
    setCustomDropdownValue('#customerId', data.customerId);
    $('#customerDeliveryAddress').val(data.customerDeliveryAddress || '');
    setCustomDropdownValue('#portId', data.portId);
    $('#docsReceivedDate').val(data.docsReceivedDate || '');
    $('#lcExpNo').val(data.lcExpNo || '');
    $('#lcValue').val(data.lcValue || '');
    $('#ipEpNo').val(data.ipEpNo || '');
    $('#ipDate').val(data.ipDate || '');
    setCustomDropdownValue('#importerId', data.importerId);
    $('#invoiceNo').val(data.invoiceNo || '');
    $('#invoiceDate').val(data.invoiceDate || '');
    $('#blNo').val(data.blNo || '');
    $('#blDate').val(data.blDate || '');
    $('#beNo').val(data.beNo || '');
    $('#beDate').val(data.beDate || '');
    $('#containerNo').val(data.containerNo || '');
    $('#containerSize').val(data.containerSize || '');
    $('#lcaNo').val(data.lcaNo || '');
    $('#dischargeDate').val(data.dischargeDate || '');
    $('#materialDescription').val(data.materialDescription || '');
    $('#quantity').val(data.quantity || '');
    setCustomDropdownValue('#quantityUnitId', data.quantityUnitId);
    $('#weight').val(data.weight || '');
    setCustomDropdownValue('#weightUnitId', data.weightUnitId);
    setCustomDropdownValue('#forwarderId', data.forwarderId);
    $('#vesselRottNo').val(data.vesselRottNo || '');
    $('#remarks').val(data.remarks || '');
    $('#creationDate').val(data.createdAt || '');
    $('#updatedDate').val(data.updatedAt || '');
}


//#endregion
