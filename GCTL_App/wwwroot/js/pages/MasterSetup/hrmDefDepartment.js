let SelectedDepartment = [];
let currentPage = 1;
let pageSize = 10;
let currentSortColumn = 'DepartmentCode';
let currentSortDirection = 'desc';
let totalPages = 1;
console.log("Department page Called");

// Load Next Department ID
function LoadNextDepartmentID() {
    $.ajax({
        url: '/next-department-id',
        method: 'GET',
        success: function (response) {
            $("#DepartmentCode").val(response);
            console.log("Next DepartmentCode Called", response);
        },
        error: function (xhr, status, error) {
            console.log("Failed to load next DepartmentCode");
        }
    });
}

// Save or Update
function SaveOrUpdateDepartment() {

    let hiddenID = parseInt($("#hiddenID").val()) || 0;
    let isEdit = hiddenID > 0;

    let VMData = {        
        autoId: hiddenID,
        DepartmentCode: $("#DepartmentCode").val(),
        DepartmentName: $("#DepartmentName").val().trim(),
        DepartmentShortName: $("#DepartmentShortName").val().trim(),
        BanglaDepartment: $("#BanglaDepartment").val().trim(),
        BanglaShortName: $("#BanglaShortName").val().trim(),
    };
    console.log("VMData", VMData);

    // Validation
    if (!VMData.DepartmentName) {
        toastr.error("Please enter Department");
        $("#DepartmentName").addClass('error-border').focus();
        return;
    } else {
        $("#DepartmentName").removeClass('error-border');
    }

    // Duplicate check
    $.ajax({
        url: '/department/check-duplicate',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(VMData),
        success: function (response) {
            if (response.isDuplicate) {
                toastr.error(response.message);
                return;
            }

            // Save or Update
            let url = isEdit ? `/department/${hiddenID}` : '/department';
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
                        LoadDepartmentList();
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
            toastr.error(xhr.responseJSON?.message || "Duplicate check failed.");
        }
    });
}

// Load Department List with Pagination & Sorting
function LoadDepartmentList(page = 1, pageSizeVal = pageSize, sortColumn = currentSortColumn, sortDirection = currentSortDirection, searchTerm = "") {
    currentPage = page;
    pageSize = pageSizeVal;
    currentSortColumn = sortColumn;
    currentSortDirection = sortDirection;

    $.ajax({
        url: '/department-list',
        type: 'GET',
        data: {
            pageNumber: page,
            pageSize: pageSizeVal,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortDirection
        },
        success: function (response) {
            console.log("Department Response:", response);
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
                            <button type="button" class="btn btn-link edit-department px-0"
                                data-item='${JSON.stringify(item)}'>
                                ${item.departmentCode}
                            </button>
                        </td>
                        <td class="align-middle">${item.departmentName || ''}</td>
                        <td class="align-middle">${item.departmentShortName || ''}</td>
                        <td class="align-middle">${item.banglaDepartment || ''}</td>
                        <td class="align-middle">${item.BanglaShortName || ''}</td>
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

// Populate Departmente Form for Edit
function populateDepartmentForm(data) {
    console.log("Populating Department Form, data:", data);

    let autoId = data.autoId || data.autoId || 0;
    $('#DepartmentForm input[name="hiddenID"]').val(autoId);
    $('#DepartmentForm input[name="DepartmentCode"]').val(data.departmentCode);
    $('#DepartmentForm input[name="DepartmentName"]').val(data.departmentName);
    $('#DepartmentForm input[name="DepartmentShortName"]').val(data.departmentShortName || "");
    $('#DepartmentForm input[name="BanglaDepartment"]').val(data.banglaDepartment);
    $('#DepartmentForm input[name="BanglaShortName"]').val(data.banglaShortName || "");

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
    // Reset selectedBanks to only this one ID
    SelectedDepartment = [parseInt(data.autoId)];
    console.log("Selected Department after populate:", SelectedDepartment);
}

// Edit button
$(document).on('click', '.edit-department', function () {
    const data = $(this).data('item');
    populateDepartmentForm(data);
});

// Pagination buttons
function generatePageButtons(currentPage, totalPages) {
    const navigationDiv = $('#pageNavigation');
    navigationDiv.empty();

    function createButton(page) {
        const btn = $('<button>')
            .addClass('btn btn-sm mx-1')
            .text(page)
            .click(() => LoadDepartmentList(page, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim()));

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
    $("#DepartmentForm")[0].reset();
    $("#hiddenID").val(0);
    $('#searchInput').val('');
    $(".form-control").removeClass("is-invalid is-valid");
    $(".invalid-feedback, .valid-feedback").remove();
    $('#lDateContainer').hide();
    $('#modifyDateContainer').hide();
    $(".row-checkbox").prop("checked", false);
    $("#selectAll").prop("checked", false);

    SelectedDepartment = [];
    currentPage = 1;
    LoadNextDepartmentID();
    const emptySearch = '';
    LoadDepartmentList(1, pageSize, currentSortColumn, currentSortDirection, emptySearch);
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

    LoadDepartmentList(1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
});

// Delete selected Payment Modes
function DeleteSelectedDepartment() {
    let selectedIds = [...SelectedDepartment];
    $(".row-checkbox:checked").each(function () {
        selectedIds.push(parseInt($(this).data("id")));
    });

    if (selectedIds.length === 0) {
        toastr.warning("Please select at least one record to delete.");
        return;
    }

    $.ajax({
        url: '/department-list',
        type: 'DELETE',
        contentType: 'application/json',
        data: JSON.stringify(selectedIds),
        success: function (res) {
            if (res.isSuccess) {
                toastr.success(res.message);
                ResetForm();
                LoadDepartmentList();
            } else {
                toastr.error(res.message);
            }
        },
        error: function (xhr) {
            toastr.error(xhr.responseJSON?.message || "Delete Failed.");
        }
    });
}




// Document Ready
$(document).ready(function () {
    LoadNextDepartmentID();
    $(document).on("submit", "#DepartmentForm", function (e) {
        e.preventDefault();
        SaveOrUpdateDepartment(e);
    });
    LoadDepartmentList();
    //delete button 
    $("#deleteBtn").on("click", function () {
        DeleteSelectedDepartment();
    });
    // Search on keyup
    $('#searchInput').on('keyup', function () {
        let searchTerm = $(this).val().trim();
        LoadDepartmentList(1, pageSize, currentSortColumn, currentSortDirection, searchTerm);
    });

    $('#firstPage').off('click').on('click', function () {
        if (currentPage !== 1) {
            LoadDepartmentList(1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });

    $('#prevPage').off('click').on('click', function () {
        if (currentPage > 1) {
            LoadDepartmentList(currentPage - 1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });

    $('#nextPage').off('click').on('click', function () {
        if (currentPage < totalPages) {
            LoadDepartmentList(currentPage + 1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });

    $('#lastPage').off('click').on('click', function () {
        if (currentPage !== totalPages) {
            LoadDepartmentList(totalPages, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
        }
    });
    $(document).on("click", ".resetBtn", function () {
        console.log("Clear Button Clicked");
        ResetForm();
    });
    $('#pageSize').on('change', function () {
        let newSize = parseInt($(this).val());
        pageSize = newSize;
        LoadDepartmentList(1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
    });

});
