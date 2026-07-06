let currentPage = 1;
let pageSize = 10;
let currentSortColumn = 'PaymentTermsCode';
let currentSortingDirection = 'desc';
let totalPages = 1;
let selectedPaymentTerm = [];

// Next PaymentTerms ID Load
function LoadNextPaymentTermsID() {
    $.ajax({
        url: '/next-payment-terms-id',
        method: 'GET',
        success: function (response) {
            $("#PaymentTermsCode").val(response);
            console.log("Next PaymentTermsID Called");
        },
        error: function () {
            console.log("Failed to load Next PaymentTermsID");
        }
    });
}

// Auto PaymentTermsName generate
function updatePaymentTermsName() {
    let type = $("#Type").val(); 
    let percentage = parseInt($("#Percentise").val()) || 0;
    let creditDays = parseInt($("#CreditDays").val()) || 0;

    let name = "";

    if (type === "Advance") {
        if (percentage === 100) {
            name = `100% Adv.`;
        } else {
            name = `${percentage}% Adv. + ${100 - percentage}% Cr. (${creditDays} Days)`;
        }
    } else if (type === "Credit") {
        if (percentage === 100) {
            name = `100% Cr. (${creditDays} Days)`;
        } else {
            name = `${percentage}% Cr. + ${100 - percentage}% Adv. (${creditDays} Days)`;
        }
    }

    $("#PaymentTermsName").val(name);
}
$("#Type, #Percentise, #CreditDays").on("input change", updatePaymentTermsName);

// Save or Update
function SaveOrUpdatePaymentTerms() {
    let hiddenID = parseInt($("#hiddenID").val()) || 0;
    let isEdit = hiddenID > 0;

    let TermsData = {
        TC: hiddenID,
        PaymentTermsCode: $("#PaymentTermsCode").val().trim(),
        PaymentTermsName: $("#PaymentTermsName").val().trim(),
        Type: $("#Type").val(),
        Percentise: $("#Percentise").val().trim() || "0",
        CreditDays: $("#CreditDays").val() ? parseInt($("#CreditDays").val()) : null
    };

    // Validation
    if (!TermsData.Type) {
        toastr.error("Please select Payment Type.");
        $("#Type").addClass('error-border').focus();
        return;
    } else {
        $("#Type").removeClass('error-border');
    }

    
    $.ajax({
        url: '/payment-terms/check-duplicate',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(TermsData),
        success: function (dupRes) {
            if (dupRes.isDuplicate) {
                toastr.error(dupRes.message);
                return; 
            } else {
               
                let url = isEdit ? `/payment-terms/${hiddenID}` : '/payment-terms';
                let type = isEdit ? 'PUT' : 'POST';

                $.ajax({
                    url: url,
                    type: type,
                    contentType: 'application/json',
                    data: JSON.stringify(TermsData),
                    success: function (res) {
                        if (res.success) {
                            toastr.success(res.message);
                            ResetTermForm();
                            LoadPaymentTermsList();
                        } else {
                            toastr.error(res.message);
                        }
                    },
                    error: function (xhr) {
                        toastr.error(xhr.responseJSON?.message || (isEdit ? "Update Failed." : "Insertion Failed."));
                    }
                });
            }
        },
        error: function () {
            toastr.error("Failed to check duplicate.");
        }
    });
}

// Bulk Delete
function BulkDeletePaymentTerms() {
    let selectedIds = [...selectedPaymentTerm];
    $(".row-checkbox:checked").each(function () {
        selectedIds.push(parseInt($(this).data("id")));
    });

    if (!selectedIds.length) {
        toastr.error("Please select at least one record to delete.");
        return;
    }

    $.ajax({
        url: '/payment-terms-list',
        type: 'DELETE',
        contentType: 'application/json',
        data: JSON.stringify(selectedIds),
        success: function (res) {
            if (res.isSuccess) {
                toastr.success(res.message);
                ResetTermForm();
                LoadPaymentTermsList();
            } else {
                toastr.error(res.message);
            }
        },
        error: function (xhr) {
            toastr.error(xhr.responseJSON?.message || "Delete failed.");
        }
    });
}

// Load All Data
function LoadPaymentTermsList(page = 1, pageSizeVal = pageSize, sortColumn = currentSortColumn, sortDirection = currentSortingDirection, searchTerm = "") {
    currentPage = page;
    pageSize = pageSizeVal;
    currentSortColumn = sortColumn;
    currentSortingDirection = sortDirection;

    $.ajax({
        url: '/payment-terms-list',
        method: "GET",
        data: {
            pageNumber: page,
            pageSize: pageSizeVal,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortDirection
        },
        success: function (response) {
            console.log("PaymentTerms :", response);
            let rows = '';
            if (!response.data || response.data.length === 0) {
                rows = `<tr class="no-data"><td colspan="6" class="text-center">No Data Available</td></tr>`;
            } else {
                response.data.forEach(function (item) {
                    rows += `<tr>
                        <td class="text-center align-middle">
                            <input type="checkbox" class="row-checkbox" data-id="${item.tc}">
                        </td>
                        <td class="text-center align-middle">
                            <button type="button" class="btn btn-link edit-payment-term p-0 m-0"
                                data-item='${JSON.stringify(item)}'>
                                ${item.paymentTermsCode}
                            </button>
                        </td>
                        <td class="align-middle">${item.paymentTermsName || ''}</td>
                        <td class="text-center align-middle">${item.type || ''}</td>
                        <td class="text-center align-middle">${item.percentise || ''}</td>
                        <td class="text-center align-middle">${item.creditDays || ''}</td>
                    </tr>`;
                });
            }

            $('#tblBody').html(rows);

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
            $('#tblBody').html(`<tr class="no-data"><td colspan="6" class="text-center">Failed to load data</td></tr>`);
        }
    });
}

// Edit Button
$(document).on("click", ".edit-payment-term", function () {
    let item = $(this).data("item");
    if (typeof item === "string") item = JSON.parse(item);

    $("#hiddenID").val(item.tc || "");
    $("#PaymentTermsCode").val(item.paymentTermsCode || "");
    $("#PaymentTermsName").val(item.paymentTermsName || "");
    $("#Type").val(item.type || "");
    $("#Percentise").val(item.percentise || "");
    $("#CreditDays").val(item.creditDays || "");

    // Reset Selected Payement Term to only this one ID
    selectedPaymentTerm = [item.tc];
    console.log("Selected Payement Term after populate:", selectedPaymentTerm);
});

// Checkbox Control
function updateCheckboxState() {
    $(".row-checkbox").off("change").on("change", function () {
        $("#selectAll").prop("checked", $(".row-checkbox:checked").length === $(".row-checkbox").length);
    });

    $("#selectAll").off("change").on("change", function () {
        $(".row-checkbox").prop("checked", this.checked);
    });
}

// Sorting Indicator
function updateSortIndicators() {
    $(".sortable").each(function () {
        let column = $(this).data("column");
        let icon = $(this).find(".sort-icon");

        icon.removeClass("fa-sort-up fa-sort-down").addClass("fa-sort");
        if (column === currentSortColumn) {
            icon.removeClass("fa-sort");
            icon.addClass(currentSortingDirection === "asc" ? "fa-sort-up" : "fa-sort-down");
        }
    });
}

// Click event for sorting
$(document).on('click', '.sortable', function () {
    let column = $(this).data('column');

    if (currentSortColumn === column) {
        currentSortingDirection = currentSortingDirection === 'asc' ? 'desc' : 'asc';
    } else {
        currentSortColumn = column;
        currentSortingDirection = 'asc';
    }
    // Reload data with sorting
    LoadPaymentTermsList(1, pageSize, currentSortColumn, currentSortingDirection, $('#searchInput').val().trim());
});

// Pagination
function generatePageButtons(currentPage, totalPages) {
    const navigationDiv = $('#pageNavigation');
    navigationDiv.empty();

    function createButton(page) {
        const btn = $('<button>')
            .addClass('btn btn-sm mx-1')
            .text(page)
            .click(() => LoadPaymentTermsList(page, pageSize, currentSortColumn, currentSortingDirection, $('#searchInput').val().trim()));

        if (page === currentPage) {
            btn.removeClass('btn-outline-primary').addClass('btn-primary');
        } else {
            btn.addClass('btn-outline-primary');
        }
        return btn;
    }

    navigationDiv.append(createButton(1));
    if (currentPage > 2) navigationDiv.append($('<span>').text('...').css('padding', '0 5px'));
    if (currentPage !== 1 && currentPage !== totalPages) navigationDiv.append(createButton(currentPage));
    if (currentPage < totalPages - 1) navigationDiv.append($('<span>').text('...').css('padding', '0 5px'));
    if (totalPages > 1) navigationDiv.append(createButton(totalPages));
}

// Reset Form
function ResetTermForm() {
    $("#PaymentTermsForm")[0].reset();
    $("#hiddenID").val("");
    $("#searchInput").val('');
    $("#selectAll").prop('checked', false);
    $(".row-checkbox").prop('checked', false);

    let selectedIds = [];
    LoadNextPaymentTermsID();
    LoadPaymentTermsList(1, pageSize, currentSortColumn, currentSortingDirection, '');
}

// Document Ready
$(document).ready(function () {
    LoadNextPaymentTermsID();
    LoadPaymentTermsList();

    $(document).on('click', ".resetBtn", ResetTermForm);
    $(document).on("submit", "#PaymentTermsForm", function (e) {
        e.preventDefault();
        SaveOrUpdatePaymentTerms();
    });
    $(document).on("click", "#deleteBtn", BulkDeletePaymentTerms);

    $('#searchInput').on('keyup', function () {
        let searchTerm = $(this).val().trim();
        LoadPaymentTermsList(1, pageSize, currentSortColumn, currentSortingDirection, searchTerm);
    });

    $('#pageSize').on('change', function () {
        pageSize = parseInt($(this).val());
        LoadPaymentTermsList(1, pageSize, currentSortColumn, currentSortingDirection, $('#searchInput').val().trim());
    });

    $('#firstPage').on('click', function () {
        if (currentPage !== 1) LoadPaymentTermsList(1, pageSize, currentSortColumn, currentSortingDirection, $('#searchInput').val().trim());
    });
    $('#prevPage').on('click', function () {
        if (currentPage > 1) LoadPaymentTermsList(currentPage - 1, pageSize, currentSortColumn, currentSortingDirection, $('#searchInput').val().trim());
    });
    $('#nextPage').on('click', function () {
        if (currentPage < totalPages) LoadPaymentTermsList(currentPage + 1, pageSize, currentSortColumn, currentSortingDirection, $('#searchInput').val().trim());
    });
    $('#lastPage').on('click', function () {
        if (currentPage !== totalPages) LoadPaymentTermsList(totalPages, pageSize, currentSortColumn, currentSortingDirection, $('#searchInput').val().trim());
    });
});
