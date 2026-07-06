let SelectPaymentMode = [];
let currentPage = 1;
let pageSize = 10;
let currentSortColumn = 'PaymentModeID';
let currentSortDirection = 'desc';
let totalPages = 1;
console.log("PaymentMode Page Called");

// Load Next Payment Mode ID
function LoadNextPaymentModeID() {
    $.ajax({
        url: '/next-payment-mode-id',
        method: 'GET',
        success: function (response) {
            $("#PaymentModeID").val(response);
            console.log("Next Payment Called");
        },
        error: function (xhr, status, error) {
            console.log("Failed to load next PaymentMode ID");
        }
    });
}

// Save or Update
function SaveOrUpdate() {
    let hiddenID = parseInt($("#hiddenID").val()) || 0;
    let isEdit = hiddenID > 0; 

    let modeData = {
        AutoId: hiddenID, 
        PaymentModeID: $("#PaymentModeID").val(),
        PaymentModeName: $("#PaymentModeName").val().trim(),
        PaymentModeShortName: $("#PaymentModeShortName").val().trim(),
    };

    // Validation
    if (!modeData.PaymentModeName) {
        toastr.error("Please enter Payment Mode");
        $("#PaymentModeName").addClass('error-border').focus();
        return;
    } else {
        $("#PaymentModeName").removeClass('error-border');
    }

    // Duplicate check
    $.ajax({
        url: '/payment-mode/check-duplicate',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(modeData),
        success: function (response) {
            if (response.isDuplicate) {
                toastr.error(response.message);
                return;
            }

            // Save or Update
            let url = isEdit ? '/payment-mode/' + hiddenID : '/payment-mode';
            let type = isEdit ? 'PUT' : 'POST';

            $.ajax({
                url: url,
                type: type,
                contentType: 'application/json',
                data: JSON.stringify(modeData),
                success: function (res) {
                    if (res.success) {
                        toastr.success(res.message);
                        ResetForm();
                        LoadPaymentModeList();
                    } else {
                        toastr.error(res.message);
                    }
                },
                error: function (xhr) {
                    toastr.error(xhr.responseJSON?.message || (isEdit ? "Update failed." : "Save failed."));
                }
            });
        },
        error: function (xhr) {
            toastr.error(xhr.responseJSON?.message || "Duplicate check failed.");
        }
    });
}

// Load Payment Mode List with Pagination & Sorting
function LoadPaymentModeList(page = 1, pageSizeVal = pageSize, sortColumn = currentSortColumn, sortDirection = currentSortDirection, searchTerm = "") {
    currentPage = page;
    pageSize = pageSizeVal;
    currentSortColumn = sortColumn;
    currentSortDirection = sortDirection;

    $.ajax({
        url: '/payment-mode-list',
        type: 'GET',
        data: {
            pageNumber: page,
            pageSize: pageSizeVal,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortDirection
        },
        success: function (response) {
            console.log("PaymentMode Response:", response);

            let rows = '';
            if (!response.data || response.data.length === 0) {
                rows = `<tr class="no-data">
                            <td colspan="4" class="text-center">No Data Available</td>
                        </tr>`;
            } else {
                response.data.forEach(function (item) {
                    rows += `<tr>
                        <td class="text-center align-middle">
                            <input type="checkbox" class="row-checkbox" data-id="${item.autoId}">
                        </td>
                        <td class="align-middle">
                            <button type="button" class="btn btn-link edit-payment-mode"
                                data-item='${JSON.stringify(item)}'>
                                ${item.paymentModeID}
                            </button>
                        </td>
                        <td class="align-middle">${item.paymentModeName || ''}</td>
                        <td class="align-middle">${item.paymentModeShortName || ''}</td>
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
        error: function (xhr, status, error) {
            console.error("Error loading data: ", error);
            $('#tblBody').html(`<tr class="no-data"><td colspan="4" class="text-center">Failed to load data</td></tr>`);
        }
    });
}

// Populate Payment Mode Form for Edit
function populatePaymentModeForm(data) {
    console.log("Populating Payment Mode Form, data:", data);

    let autoId = data.AutoId || data.autoId || 0;
    $('#PaymentModeForm input[name="hiddenID"]').val(autoId);
    $('#PaymentModeForm input[name="PaymentModeID"]').val(data.paymentModeID);
    $('#PaymentModeForm input[name="PaymentModeName"]').val(data.paymentModeName);
    $('#PaymentModeForm input[name="PaymentModeShortName"]').val(data.paymentModeShortName || "");

    // LDate
    if (data.lDate) {
        const lDate = new Date(data.lDate).toLocaleDateString('en-GB'); 
        $('#displayLDate').text(lDate);
        $('#lDateContainer').show();
    } else {
        $('#lDateContainer').hide(); 
    }

        //ModifyDate
    if (data.modifyDate) {
        const modifyDate = new Date(data.modifyDate).toLocaleDateString('en-GB'); 
        $('#displayModifyDate').text(modifyDate);
        $('#modifyDateContainer').show();
    } else {
        $('#modifyDateContainer').hide();
    }
    // Reset Selected branch to only this one ID
    SelectPaymentMode = [parseInt(data.autoId)];
    console.log("Selected Payment Mode after populate:", SelectPaymentMode);
}


// Edit button
$(document).on('click', '.edit-payment-mode', function () {
    const data = $(this).data('item');
    populatePaymentModeForm(data);
});

// Pagination buttons
function generatePageButtons(currentPage, totalPages) {
    const navigationDiv = $('#pageNavigation');
    navigationDiv.empty();

    function createButton(page) {
        const btn = $('<button>')
            .addClass('btn btn-sm mx-1')
            .text(page)
            .click(() => LoadPaymentModeList(page, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim()));

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

// Reset form
function ResetForm() {
    $("#PaymentModeForm")[0].reset();
    $("#hiddenID").val("0");
    $("#PaymentModeID").val("");
    $("#PaymentModeName").val("");
    $("#PaymentModeShortName").val("");
    $("#PaymentModeName").removeClass('error-border');
    $('#searchInput').val('');
    $(".form-control").removeClass("is-invalid is-valid");
    $(".invalid-feedback, .valid-feedback").remove();
    $('#lDateContainer').hide();
    $('#modifyDateContainer').hide();
    $(".row-checkbox").prop("checked", false);
    $("#selectAll").prop("checked", false);

    SelectPaymentMode = [];
    currentPage = 1;
    LoadNextPaymentModeID();
    const emptySearch = '';
    LoadPaymentModeList(1, pageSize, currentSortColumn, currentSortDirection, emptySearch);
}

// Sorting
$(document).on('click', '.sortable', function () {
    let column = $(this).data('column');

    if (currentSortColumn === column) {
        currentSortDirection = currentSortDirection === 'asc' ? 'desc' : 'asc';
    } else {
        currentSortColumn = column;
        currentSortDirection = 'asc';
    }

    LoadPaymentModeList(1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
});

// Delete selected Payment Modes
function DeleteSelectedPaymentModes() {
    let selectedIds = [...SelectPaymentMode];
    $(".row-checkbox:checked").each(function () {
        selectedIds.push(parseInt($(this).data("id"))); 
    });

    if (selectedIds.length === 0) {
        toastr.warning("Please select at least one record to delete.");
        return;
    }
    $.ajax({
        url: '/payment-mode-list', 
        type: 'DELETE',
        contentType: 'application/json',
        data: JSON.stringify(selectedIds),
        success: function (res) {
            if (res.isSuccess) {
                toastr.success(res.message);
                ResetForm();
                LoadPaymentModeList(); 
            } else {
                toastr.error(res.message);
            }
        },
        error: function (xhr) {
            toastr.error(xhr.responseJSON?.message || "Delete failed.");
        }
    });
}




// Document Ready
$(document).ready(function () {
    LoadNextPaymentModeID();
    $(document).on("submit", "#PaymentModeForm", function (e) {
        e.preventDefault();
        SaveOrUpdate(e);
    });
    LoadPaymentModeList();
    //delete button 
    $("#deleteBtn").on("click", function () {
        DeleteSelectedPaymentModes();
    });
    // Search on keyup
    $('#searchInput').on('keyup', function () {
        let searchTerm = $(this).val().trim();
        LoadPaymentModeList(1, pageSize, currentSortColumn, currentSortDirection, searchTerm);
    });

    $('#firstPage').off('click').on('click', function () {
        if (currentPage !== 1) {
            LoadPaymentModeList(1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });

    $('#prevPage').off('click').on('click', function () {
        if (currentPage > 1) {
            LoadPaymentModeList(currentPage - 1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });

    $('#nextPage').off('click').on('click', function () {
        if (currentPage < totalPages) {
            LoadPaymentModeList(currentPage + 1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });

    $('#lastPage').off('click').on('click', function () {
        if (currentPage !== totalPages) {
            LoadPaymentModeList(totalPages, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });
    $(document).on("click", ".resetBtn", function () {
        console.log("Clear Button Clicked");
        ResetForm();
    });
    $('#pageSize').on('change', function () {
        let newSize = parseInt($(this).val());
        pageSize = newSize;
        LoadPaymentModeList(1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
    });

});
