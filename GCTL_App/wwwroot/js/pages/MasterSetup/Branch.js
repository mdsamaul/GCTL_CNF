let SelectedBranch = [];
let currentPage = 1;
let pageSize = 10;
let currentSortColumn = 'BranchCode';
let currentSortDirection = 'desc';
let totalPages = 1;
console.log("Branch page Called"); 


//#region Load Core Company Dropdown
function LoadCoreCompanyDropdown() {
    $.ajax({
        url: '/corebranch-core-company-dropdown', 
        method: 'GET',
        success: function (response) {
            console.log("Company Data Loaded", response.data);

            var $company = $('#CompanyCode'); 
            $company.empty().append('<option value="">Select Company</option>');

            $.each(response.data, function (index, company) {
                $company.append(
                    '<option value="' + company.id + '">' + company.name + '</option>'
                );
            });

            // Initialize Select2 to make it searchable
            $company.select2({
                placeholder: "Select Company",
                width: '100%',
                allowClear: true,
            });
        },
        error: function (xhr, status, error) {
            console.error('Failed to load Company Dropdown:', error);
        }
    });
}

//#endregion


//#region Load Next Branch ID
function LoadNextBranchID() {
    $.ajax({
        url: '/next-core-branch-id',
        method: 'GET',
        success: function (response) {
            $("#BranchCode").val(response);
            console.log("Next BranchCode Called", response);
        },
        error: function (xhr, status, error) {
            console.log("Failed to load next BranchCode");
        }
    });
}

//#endregion


//#region Save or Update
function SaveOrUpdateBranch() {

    let hiddenID = parseInt($("#hiddenID").val()) || 0;
    let isEdit = hiddenID > 0;

    let VMData = {        
        autoId: hiddenID,
        BranchCode: $("#BranchCode").val(),
        BranchName: $("#BranchName").val().trim(),
        Address: $("#Address").val().trim(),
        Phone: $("#Phone").val().trim(),
        Email: $("#Email").val().trim(),
        CompanyCode: $("#CompanyCode").val().trim(),
        BanglaBranch: $("#BanglaBranch").val().trim(),
        AddressBangla: $("#AddressBangla").val().trim(),
        Fax: $("#Fax").val().trim(),
    };
    console.log("VMData", VMData);

    // Validation
    if (!VMData.BranchName) {
        toastr.error("Please enter Branch");
        $("#BranchName").addClass('error-border').focus();
        return;
    } else {
        $("#BranchName").removeClass('error-border');
    }

    // Duplicate check
    $.ajax({
        url: '/branch/check-duplicate',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(VMData),
        success: function (response) {
            if (response.isDuplicate) {
                toastr.error(response.message);
                return;
            }

            // Save or Update
            let url = isEdit ? `/core-branch/${hiddenID}` : '/core-branch';
            let type = isEdit ? 'PUT' : 'POST';

            $.ajax({
                url: url,
                type: type,
                contentType: 'application/json',
                data: JSON.stringify(VMData),
                success: function (res) {
                    if (res.success) {
                        toastr.success(res.message);
                        ResetForm();
                        LoadBranchList();
                    } else {
                        toastr.error(res.message);
                    }
                },
                error: function (xhr) {
                    toastr.error(xhr.responseJSON?.message || (isEdit ? "Update Failed." : "Insertion Failed."));
                }
            });
        },
        error: function (xhr) {
            console.log("Error response:", xhr.responseJSON);
            toastr.error(xhr.responseJSON?.message || "Duplicate check failed.");
        }
    });
}

//#endregion


//#region Load Branch List with Pagination & Sorting
function LoadBranchList(page = 1, pageSizeVal = pageSize, sortColumn = currentSortColumn, sortDirection = currentSortDirection, searchTerm = "") {
    currentPage = page;
    pageSize = pageSizeVal;
    currentSortColumn = sortColumn;
    currentSortDirection = sortDirection;

    $.ajax({
        url: '/core-branch-list',
        type: 'GET',
        data: {
            pageNumber: page,
            pageSize: pageSizeVal,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortDirection
        },
        success: function (response) {
            console.log("Branch Response:", response);
       
            let rows = '';
            if (!response.data || response.data.length === 0) {
                rows = `<tr class="no-data">
                            <td colspan="6" class="text-center">No Data Available</td>
                        </tr>`;
            } else {
                response.data.forEach(function (item) {
                    rows += `<tr>
                        <td class="text-center align-middle">
                            <input type="checkbox" class="row-checkbox" data-id="${item.autoId}">
                        </td>
                        <td class="text-center align-middle">
                            <button type="button" class="btn btn-link edit-branch px-0"
                                data-item='${JSON.stringify(item)}'>
                                ${item.branchCode}
                            </button>
                        </td>
                        <td class="text-middle">${item.branchName || ''}</td>    
                        <td class="text-middle">${item.address || ''}</td>
                        <td class="text-center text-middle">${item.phone || ''}</td>
                        <td class="text-center text-middle">${item.email || ''}</td>
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
            $('#tblBody').html(`<tr class="no-data"><td colspan="6" class="text-center">Failed to load data</td></tr>`);
        }
    });
}

//#endregion


//#region Edit Details Data
function populateBranchForm(data) {
    console.log("Populating Branch Form, data:", data);

    let autoId = data.autoId || 0;
    $('#BranchForm input[name="hiddenID"]').val(autoId);
    $('#BranchForm input[name="BranchCode"]').val(data.branchCode);
    //$('#BranchForm input[name="CompanyCode"]').val(data.companyCode);
    $('#BranchForm select[name="CompanyCode"]').val(data.companyCode).trigger('change');
    $('#BranchForm input[name="BranchName"]').val(data.branchName);
    $('#BranchForm input[name="Address"]').val(data.address || "");
    $('#BranchForm input[name="Phone"]').val(data.phone);
    $('#BranchForm input[name="Email"]').val(data.email || "");


    if (data.lDate) {
        let entryDate = new Date(data.lDate).toLocaleString('en-GB', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: 'numeric',
            minute: '2-digit',
            hour12: true
        }).replace(',', '');
        $('#displaylDate').text(entryDate.toUpperCase());
    } else {
        $('#displaylDate').text('');
    }

    if (data.modifyDate) {
        let updateDate = new Date(data.modifyDate).toLocaleString('en-GB', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: 'numeric',
            minute: '2-digit',
            hour12: true
        }).replace(',', '');
        $('#displayModifyDate').text(updateDate.toUpperCase());
    } else {
        $('#displayModifyDate').text('');
    }
  

    // Reset Selected branch to only this one ID
    SelectedBranch = [parseInt(data.autoId)];
    console.log("Selected Core Branch after populate:", SelectedBranch);

}

$(document).on('click', '.edit-branch', function () {
    const data = $(this).data('item');
    populateBranchForm(data);

});

//#endregion


//#region Pagination buttons
function generatePageButtons(currentPage, totalPages) {
    const navigationDiv = $('#pageNavigation');
    navigationDiv.empty();

    function createButton(page) {
        const btn = $('<button>')
            .addClass('btn btn-sm mx-1')
            .text(page)
            .click(() => LoadBranchList(page, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim()));

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

//#endregion


//#region SelectAll Checkbox control
function updateCheckboxState() {
    $(".row-checkbox").off("change").on("change", function () {
        $("#selectAll").prop("checked", $(".row-checkbox:checked").length === $(".row-checkbox").length);
    });

    $("#selectAll").off("change").on("change", function () {
        $(".row-checkbox").prop("checked", this.checked);
    });
}

//#endregion


//#region Sorting indicators
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

//#endregion


//#region Reset form
function ResetForm() {
    $("#BranchForm")[0].reset();
    $("#hiddenID").val(0);
    $('#searchInput').val('');
    $(".form-control").removeClass("is-invalid is-valid");
    $(".invalid-feedback, .valid-feedback").remove();
    $('#displaylDate').text('');
    $('#displayModifyDate').text('');
    $(".row-checkbox").prop("checked", false);
    $("#selectAll").prop("checked", false);
    $('#CompanyCode').val('').trigger('change');


    // Set Entry Date
    $('#displaylDate').text(getBangladeshDateTime());

    SelectedBranch = [];
    currentPage = 1;
    LoadNextBranchID();
    const emptySearch = '';
    LoadBranchList(1, pageSize, currentSortColumn, currentSortDirection, emptySearch);
}

//#endregion


//#region Sorting

$(document).on('click', '.sortable', function () {
    let column = $(this).data('column');

    if (currentSortColumn === column) {
        currentSortDirection = currentSortDirection === 'asc' ? 'desc' : 'asc';
    } else {
        currentSortColumn = column;
        currentSortDirection = 'asc';
    }

    LoadBranchList(1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
});

//#endregion


//#region Delete selected Branch
function DeleteSelectedBranch() {
    let selectedIds = [...SelectedBranch];
    $(".row-checkbox:checked").each(function () {
        selectedIds.push(parseInt($(this).data("id")));
    });

    if (selectedIds.length === 0) {
        toastr.warning("Please select at least one record to delete.");
        return;
    }

    $.ajax({
        url: '/core-branch-list',
        type: 'DELETE',
        contentType: 'application/json',
        data: JSON.stringify(selectedIds),
        success: function (res) {
            if (res.isSuccess) {
                toastr.success(res.message);
                ResetForm();
                LoadBranchList();
            } else if (res.refError){
                toastr.warning(res.message);
            } else {
                toastr.error(res.message);
            }
        },
        error: function (xhr) {
            toastr.error(xhr.responseJSON?.message || "Delete Failed.");
        }
    });
}

//#endregion




//#region Document Ready

$(document).ready(function () {
    LoadNextBranchID();
    LoadCoreCompanyDropdown();


    // Set Entry Date
    $('#displaylDate').text(getBangladeshDateTime());

    $(document).on("submit", "#BranchForm", function (e) {
        e.preventDefault();
        SaveOrUpdateBranch(e);
    });
    LoadBranchList();
    //delete button 
    $("#deleteBtn").on("click", function () {
        DeleteSelectedBranch();
    });
    // Search on keyup
    $('#searchInput').on('keyup', function () {
        let searchTerm = $(this).val().trim();
        LoadBranchList(1, pageSize, currentSortColumn, currentSortDirection, searchTerm);
    });

    $('#firstPage').off('click').on('click', function () {
        if (currentPage !== 1) {
            LoadBranchList(1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });

    $('#prevPage').off('click').on('click', function () {
        if (currentPage > 1) {
            LoadBranchList(currentPage - 1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });

    $('#nextPage').off('click').on('click', function () {
        if (currentPage < totalPages) {
            LoadBranchList(currentPage + 1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });

    $('#lastPage').off('click').on('click', function () {
        if (currentPage !== totalPages) {
            LoadBranchList(totalPages, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });
    $(document).on("click", ".resetBtn", function () {
        console.log("Clear Button Clicked");
        ResetForm();
    });
    $('#pageSize').on('change', function () {
        let newSize = parseInt($(this).val());
        pageSize = newSize;
        LoadBranchList(1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
    });

});

//#endregion


//#region Function to get current date in Bangladesh time
function getBangladeshDateTime() {
    // BD timezone offset is +6:00 from UTC
    let now = new Date();
    let utc = now.getTime() + (now.getTimezoneOffset() * 60000);
    let bdOffset = 6 * 60 * 60 * 1000;
    let bdTime = new Date(utc + bdOffset);

    // Format DD/MM/YYYY hh:MM AM/PM
    let day = bdTime.getDate().toString().padStart(2, '0');
    let month = (bdTime.getMonth() + 1).toString().padStart(2, '0');
    let year = bdTime.getFullYear();

    let hours = bdTime.getHours();
    let minutes = bdTime.getMinutes().toString().padStart(2, '0');

    let ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12;
    hours = hours ? hours : 12; // 0 => 12

    return `${day}/${month}/${year} ${hours}:${minutes} ${ampm}`;
}

//#endregion