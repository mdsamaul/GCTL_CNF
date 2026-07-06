let SelectPaymentType = [];
let currentPage = 1;
let pageSize = 10;
let currentSortColumn = 'PaymentTypeID';
let currentSortDirection = 'desc';
let totalPages = 1;
console.log("PaymentType Page Called");

// Load Next Payment Type ID
function LoadNextPaymentTypeID() {
    $.ajax({
        url: '/next-payment-type-id',
        method: 'GET',
        success: function (response) {
            $("#PaymentTypeID").val(response);
            console.log("Next PaymentTypeID Loaded");
        },
        error: function () {
            console.log("Failed to load next PaymentTypeID");
        }
    });
}

// Save or Update
function SaveOrUpdate() {
    let hiddenID = parseInt($("#hiddenID").val()) || 0;
    let isEdit = hiddenID > 0;

    let TypeData = {
        TC: hiddenID,
        PaymentTypeID: $("#PaymentTypeID").val(),
        PaymentType: $("#PaymentType").val().trim(),
        ShortName: $("#ShortName").val().trim(),
    };

    // Validation
    if (!TypeData.PaymentType) {
        toastr.error("Please enter Payment Type");
        $("#PaymentType").addClass('error-border').focus();
        return;
    } else {
        $("#PaymentType").removeClass('error-border');
    }

    // Duplicate check
    $.ajax({
        url: '/payment-type/check-duplicate',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(TypeData),
        success: function (response) {
            if (response.isDuplicate) {
                toastr.error(response.message);
                return;
            }

            // Save or Update
            let url = isEdit ? `/payment-type/${hiddenID}` : '/payment-type';
            let type = isEdit ? 'PUT' : 'POST';

            $.ajax({
                url: url,
                type: type,
                contentType: 'application/json',
                data: JSON.stringify(TypeData),
                success: function (res) {
                    if (res.success) {
                        toastr.success(res.message);
                        ResetForm();
                        LoadPaymentTypeList();
                    } else {
                        toastr.error(res.message);
                    }
                },
                error: function (xhr) {
                    toastr.error(xhr.responseJSON?.message || (isEdit ? "Update failed." : "Save failed."));
                }
            });
        },
        error: function () {
            toastr.error("Duplicate check failed.");
        }
    });
}

// Load Payment Type List with Pagination & Sorting
function LoadPaymentTypeList(page = 1, pageSizeVal = pageSize, sortColumn = currentSortColumn, sortDirection = currentSortDirection, searchTerm = "") {
    currentPage = page;
    pageSize = pageSizeVal;
    currentSortColumn = sortColumn;
    currentSortDirection = sortDirection;

    $.ajax({
        url: '/payment-type-list',
        type: 'GET',
        data: {
            pageNumber: page,
            pageSize: pageSizeVal,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortDirection
        },
        success: function (response) {
            console.log("PaymentType Response:", response);
            let rows = '';
            if (!response.data || response.data.length === 0) {
                rows = `<tr class="no-data">
                            <td colspan="4" class="text-center">No Data Available</td>
                        </tr>`;
            } else {
                response.data.forEach(function (item) {
                    rows += `<tr>
                        <td class="text-center align-middle">
                            <input type="checkbox" class="row-checkbox" data-id="${item.tc}">
                        </td>
                        <td class="align-middle">
                            <button type="button" class="btn btn-link edit-payment-type"
                                data-item='${JSON.stringify(item)}'>
                                ${item.paymentTypeID}
                            </button>
                        </td>
                        <td class="align-middle">${item.paymentType || ''}</td>
                        <td class="align-middle">${item.shortName || ''}</td>
                    </tr>`;
                });
            }

            $('#tblBody').html(rows);

            // Pagination Info
            const startIndex = response.paginationInfo?.startItem || 0;
            const endIndex = response.paginationInfo?.endItem || 0;
            const totalRecords = response.paginationInfo?.totalItems || 0;
            totalPages = pageSizeVal === -1 ? 1 : Math.ceil(totalRecords / pageSizeVal);

            $('#paginationInfo').text(`Page ${currentPage} of ${totalPages}`);
            $('#paginationSummary').text(`Showing ${startIndex} to ${endIndex} Items of ${totalRecords} entries`);
            $('#totalRecordsInfo').text(`Total Records: ${totalRecords}`);

            generatePageButtons(currentPage, totalPages);
            $('#selectAll').prop('checked', false);
            updateCheckboxState();
            updateSortIndicators();
        },
        error: function () {
            $('#tblBody').html(`<tr class="no-data"><td colspan="4" class="text-center">Failed to load data</td></tr>`);
        }
    });
}

// Populate Form for Edit
function populatePaymentTypeForm(data) {
    console.log("Populating Payment Type Form:", data);

    $('#hiddenID').val(data.tc || 0);
    $('#PaymentTypeID').val(data.paymentTypeID || '');
    $('#PaymentType').val(data.paymentType || '');
    $('#ShortName').val(data.shortName || '');

    // LDate 
    if (data.lDate) {
        const lDate = new Date(data.lDate).toLocaleDateString('en-GB'); 
        $('#displayLDate').text(lDate);
        $('#lDateContainer').show();
    } else {
        $('#lDateContainer').hide();
    }

    // ModifyDate 
    if (data.modifyDate) {
        const modifyDate = new Date(data.modifyDate).toLocaleDateString('en-GB'); 
        $('#displayModifyDate').text(modifyDate);
        $('#modifyDateContainer').show();
    } else {
        $('#modifyDateContainer').hide();
    }
    // Reset Selected Payment Type to only this one ID
    SelectPaymentType = [parseInt(data.tc)];
    console.log("Selected Payment Type after populate:", SelectPaymentType);
}


// Edit button
$(document).on('click', '.edit-payment-type', function () {
    const data = $(this).data('item');
    populatePaymentTypeForm(data);
});

// Delete 
function DeleteSelectedPaymentTypes() {
    let selectedIds = [...SelectPaymentType];
    $(".row-checkbox:checked").each(function () {
        selectedIds.push(parseInt($(this).data("id")));
    });

    if (selectedIds.length === 0) {
        toastr.warning("Please select at least one record to delete.");
        return;
    }

    $.ajax({
        url: '/payment-type-list',
        type: 'DELETE',
        contentType: 'application/json',
        data: JSON.stringify(selectedIds),
        success: function (res) {
            if (res.isSuccess) {
                toastr.success(res.message);
                ResetForm();
                LoadPaymentTypeList();
            } else {
                toastr.error(res.message);
            }
        },
        error: function () {
            toastr.error("Delete failed.");
        }
    });
}

// Reset form
function ResetForm() {
    $("#PaymentTypeForm")[0].reset();
    $("#hiddenID").val("0");
    $('#lDateContainer, #modifyDateContainer').hide();
    $('#searchInput').val('');
    SelectPaymentType = [];
    currentPage = 1;
    LoadNextPaymentTypeID();
    LoadPaymentTypeList(1, pageSize, currentSortColumn, currentSortDirection, '');
}
function generatePageButtons(currentPage, totalPages) {
    const navigationDiv = $('#pageNavigation');
    navigationDiv.empty();

    function createButton(page) {
        const btn = $('<button>')
            .addClass('btn btn-sm mx-1')
            .text(page)
            .click(() => LoadPaymentTypeList(page, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim()));

        if (page === currentPage) {
            btn.removeClass('btn-outline-primary').addClass('btn-primary');
        } else {
            btn.addClass('btn-outline-primary');
        }
        return btn;
    }

    navigationDiv.append(createButton(1));
    if (currentPage > 2) {
        navigationDiv.append($('<span>').text('...').css('padding', '0 5px'));
    }

    if (currentPage !== 1 && currentPage !== totalPages) {
        navigationDiv.append(createButton(currentPage));
    }

    if (currentPage < totalPages - 1) {
        navigationDiv.append($('<span>').text('...').css('padding', '0 5px'));
    }
    if (totalPages > 1) {
        navigationDiv.append(createButton(totalPages));
    }
}

// SelectAll Checkbox control
function updateCheckboxState() {
    $(".row-checkbox").off("change").on("change", function () {
        $("#selectAll").prop("checked", $(".row-checkbox:checked").length === $(".row-checkbox").length);
    });

    $("#selectAll").off("change").on("change", function () {
        $(".row-checkbox").prop("checked", this.checked);
    });
}

// Sorting indicators
function updateSortIndicators() {
    $(".sortable").each(function () {
        let column = $(this).data("column");
        let icon = $(this).find(".sort-icon");

        icon.removeClass("fa-sort-up fa-sort-down").addClass("fa-sort");
        if (column === currentSortColumn) {
            icon.removeClass("fa-sort");
            icon.addClass(currentSortDirection === "asc" ? "fa-sort-up" : "fa-sort-down");
        }
    });
}

$(document).on('click', '.sortable', function () {
    const column = $(this).data('column');

    if (column === currentSortColumn) {
        currentSortDirection = (currentSortDirection === 'asc') ? 'desc' : 'asc';
    } else {
        currentSortColumn = column;
        currentSortDirection = 'asc';
    }

 
    const searchTerm = $('#searchInput').val().trim();
    LoadPaymentTypeList(1, pageSize, currentSortColumn, currentSortDirection, searchTerm);
});


// Document Ready
$(document).ready(function () {
    LoadNextPaymentTypeID();
    LoadPaymentTypeList();

    // Save
    $(document).on("submit", "#PaymentTypeForm", function (e) {
        e.preventDefault();
        SaveOrUpdate();
    });

    // Delete
    $("#deleteBtn").on("click", function () {
        DeleteSelectedPaymentTypes();
    });

    // Clear
    $(document).on("click", ".resetBtn", function () {
        ResetForm();
    });

    // Search
    $('#searchInput').on('keyup', function () {
        let searchTerm = $(this).val().trim();
        LoadPaymentTypeList(1, pageSize, currentSortColumn, currentSortDirection, searchTerm);
    });

    // Page size
    $('#pageSize').on('change', function () {
        let newSize = parseInt($(this).val());
        pageSize = newSize;
        LoadPaymentTypeList(1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
    });
    //Fast button
    $('#firstPage').off('click').on('click', function () {
        if (currentPage !== 1) {
            LoadPaymentTypeList(1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });
    //Previous button
    $('#prevPage').off('click').on('click', function () {
        if (currentPage > 1) {
            LoadPaymentTypeList(currentPage - 1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });
    //Next button
    $('#nextPage').off('click').on('click', function () {
        if (currentPage < totalPages) {
            LoadPaymentTypeList(currentPage + 1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });
    //Last button
    $('#lastPage').off('click').on('click', function () {
        if (currentPage !== totalPages) {
            LoadPaymentTypeList(totalPages, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });

});
