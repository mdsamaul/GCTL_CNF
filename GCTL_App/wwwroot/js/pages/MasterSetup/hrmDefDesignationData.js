let currentpageDesig = 1;
let pageSizeDesig = 10;
let totalPageDesig = 0;
let editIdDesig;

let currentSortColumnDesig = 'DesignationCode';
let currentSortOrderDesig = 'desc';

$(document).ready(() => {
    setupDesigEventListeners();
    loadPaginatedDesig();
    loadDesigId();
    showFooterDesig();
    updatePaginationDesig();
    updateSortingIndicatorDesig();
    setupActionButtonsDesig();
})

function setupDesigEventListeners() {
    $('.designation-dropdown-item').on('click', function () {
        var selectedSize = $(this).data("size");
        if (!selectedSize)
            return
        

        pageSizeDesig = parseInt(selectedSize, 10);
        var displayText = selectedSize == -1 ? "All" : selectedSize;

        $('#designation-selectedPageSize').text(displayText);
        loadPaginatedDesig();
    })


    $('th.designation-sort').on('click', function () {
        const column = $(this).data('sort');
        $('.designation-sort').removeClass('sort-asc sort-desc');

        if (currentSortColumnDesig === column) {
            currentSortOrderDesig = currentSortOrderDesig === 'asc' ? 'desc' : 'asc';
        } else {
            currentSortColumnDesig = column;
            currentSortOrderDesig = 'desc';
        }

        $(this).addClass('sort-' + currentSortOrderDesig);

        loadPaginatedDesig(currentSortColumnDesig, currentSortOrderDesig);
        updateSortingIndicatorDesig(column, currentSortOrderDesig);
    });

    $("#designation-btnBulkDelete").on('click', bulkDeleteDesig);
    $("#designation-btnDelete").on('click', singleDeleteDesig);

    $("#designation-resetBtn").on('click', resetFormDesig);
    $("#designation-btnSave").on('click', saveDesig);

    const debouncedSearch = debounce(() => {
        currentpageDesig = 1;
        loadPaginatedDesig();
    }, 300);

    $("#designation-searchInput").on("input", debouncedSearch);


    $("#designation-prevPageBtn").on('click', function () {
        if (currentpageDesig > 1) {
            currentpageDesig--;
            loadPaginatedDesig();
        }
    });

    $("#designation-nextPageBtn").on('click', function () {
        if (currentpageDesig < totalPageDesig) {
            currentpageDesig++;
            loadPaginatedDesig();
        }
    });


    $('#designation-check-all').on('change', function () {
        $('.designation-selectItem').prop('checked', $(this).prop('checked'));
        toggleBulkActionsDesig();
    })

    $(document).on('change', '.designation-selectItem', toggleBulkActionsDesig);
}

function debounce(func, delay) {
    let timer;
    return function (...args) {
        clearTimeout(timer);
        timer = setTimeout(() => {
            func.apply(this, args);
        }, delay);
    };
}

function updateSortingIndicatorDesig() {
    $('th.designation-sort').find('.sort-icon').remove();

    const th = $(`th.designation-sort[data-sort="${currentSortColumnDesig}"]`);
    const icon = currentSortOrderDesig === 'asc' ? 'fa-sort-up' : 'fa-sort-down';

    th.append(`<span class="sort-icon ms-2"><i class="fas ${icon}"></i></span>`);
}

function updatePaginationDesig(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#designation-paginationLinks");
    paginationLinks.empty();
    const windowSize = 1;


    const createPageButton = (page) => {
        return $('<li>')
            .addClass('page-item')
            .toggleClass('active', page === currentPage)
            .append(
                $('<button>')
                    .addClass('page-link')
                    .text(page)
                    .on('click', () => goToPageDesig(page))
            );
    };

    const addEllipsis = () => {
        return $('<li>')
            .addClass('page-item disabled')
            .append(
                $('<span>')
                    .addClass('page-link')
                    .text('...')
            );
    };

    if (currentPage > windowSize + 1) {
        paginationLinks.append(createPageButton(1), addEllipsis());
    }

    const startPage = Math.max(1, currentPage - windowSize);
    const endPage = Math.min(totalPages, currentPage + windowSize);
    for (let i = startPage; i <= endPage; i++) {
        paginationLinks.append(createPageButton(i));
    }

    if (currentPage < totalPages - windowSize) {
        paginationLinks.append(addEllipsis(), createPageButton(totalPages));
    }

    $("#designation-prevPageBtn").prop('disabled', currentPage === 1);
    $("#designation-nextPageBtn").prop('disabled', currentPage === totalPages);
}

function goToPageDesig(page) {
    currentpageDesig = page;
    loadPaginatedDesig();
}

function showFooterDesig() {
    $("#designation-footer").toggleClass("d-none", !editIdDesig);
}

function toggleBulkActionsDesig() {
    console.log("toggle");
    const anyChecked = $('.designation-selectItem:checked').length > 0;
    console.log(anyChecked);
    $('#designation-btnBulkDelete').toggleClass('d-none', !anyChecked);
    $('#designation-btnDelete').toggleClass('d-none', anyChecked);
}

function loadPaginatedDesig(sortColumn = 'DesignationCode', sortOrder = 'desc') {

    const searchTerm = $("#designation-searchInput").val();
    console.log(searchTerm);


    $.ajax({
        url: '/HrmDefDesignation/GetAllPaginated',
        method: 'GET',
        data: { pageNumber: currentpageDesig, pageSize: pageSizeDesig, searchTerm: searchTerm, sortColumn: sortColumn, sortOrder: sortOrder },

        success: function (response) {
            console.log(response);
            renderDesigTable(response);
            setupActionButtonsDesig();
        },
        error: function (error) {
            console.error("Error loading data: ", error);
            $("#designation-table-body").html('<tr><td colspan="8" class="text-center">Failed to load data</td></tr>');
            $("#designation-paginationInfo").text('');
            $("#designation-paginationLinks").empty();
        }

    });
}

function renderDesigTable(response) {
    const tableBody = $("#designation-table-body");
    tableBody.empty();

    if (response.data && response.data.length > 0) {
        $.each(response.data, (_, item) => {
            tableBody.append(`
                <tr class="position-static ptype-row" data-id="${item.autoId}">
                    <td style="width:5%" class="text-center text-middle align-middle">
                        <input type="checkbox" class="designation-selectItem" data-id="${item.autoId}" />
                    </td>
                    <td style="padding: 5px 10px; font-size: 14px;" class="align-middle white-space-nowrap ps-0">${item.designationCode}</td>
                    <td style="padding: 5px 10px;font-size: 14px;" class="align-middle ps-0">${item.designationName}</td>
                    <td style="padding: 5px 10px; font-size: 14px;" class="align-middle ps-0">${item.designationShortName}</td>
                    <td style="padding: 5px 10px; font-size: 14px;" class="align-middle ps-0">${item.banglaDesignation}</td>
                    <td style="padding: 5px 10px; font-size: 14px;" class="align-middle ps-0">${item.banglaShortName}</td>
                    <td style="padding: 5px 10px; font-size: 14px;" class="align-middle ps-0">${item.gradeCode}</td>
                    <td style="padding: 5px 10px; font-size: 14px;" class="align-middle ps-0">${item.stepNoId}</td>
                </tr>
            `);
        });
    } else {
        tableBody.html('<tr><td colspan="8" class="text-center">No Data Available</td></tr>');

        $("#designation-paginationInfo").text('');
        $("#designation-paginationLinks").empty();
    }

    // Pagination info update
    if (response.paginationInfo) {
        const paginationInfo = response.paginationInfo;
        $("#designation-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
        updatePaginationDesig(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        totalPageDesig = paginationInfo.totalPages;
    } else {
        $("#designation-paginationInfo").text('');
        $("#designation-paginationLinks").empty();
        totalPageDesig = 0;
    }
}

function setupActionButtonsDesig() {
    $(".ptype-row").find("select, input").click(e => e.stopPropagation());


    $(".ptype-row").on("click", function () {
        const ptypeCode = $(this).data("id");

        if (!ptypeCode) return;

        $.ajax({
            url: `/HrmDefDesignation/Details/${ptypeCode}`,
            method: 'GET',
            success: populateFormDesig,
            error: () => toastr.error("Failed to load delivery location details.")
        });
    });
}

function populateFormDesig(response) {
    console.log(response);
    editIdDesig = response.autoId;
    showFooterDesig();

    const KeyToIdMap = {
        autoId: "AutoId",
        designationCode: "DesignationCode",
        designationName: "DesignationName",
        designationShortName: "DesignationShortName",
        gradeCode: "GradeCode",
        stepNoId: "StepNoId",
        banglaDesignation: "BanglaDesignation",
        banglaShortName: "BanglaShortName"
    };

    $.each(response, (key, value) => {
        const fieldId = KeyToIdMap[key] || key;
        const $field = $(`#${fieldId}`);

        if ($field.length) {
            $field.val(value ?? '');
        } else {
            console.warn(`No matching field for key: ${key}`);
        }
    });

    // Bangladeshi date format - dd/MM/yyyy
    function formatBDDate(dateValue) {
        return dateValue ? new Date(dateValue).toLocaleDateString('en-GB') : '';
    }

    $("#entryDate").text(`Entry Date: ${formatBDDate(response.ldate)}`);
    $("#lastUpdateDate").text(`Last Updated: ${formatBDDate(response.modifyDate)}`);
}


function checkForDuplicateDesig(formData, callback) {
    $.ajax({
        url: '/HrmDefDesignation/CheckDuplicate',
        method: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.isDuplicate) {
                toastr.error('Data Already Exists!');
                callback(false);
            } else {
                callback(true);
            }
        },
        error: function (error) {
            console.error("Error checking for duplicate: ", error);
            toastr.error('Error checking for duplicate Designation');
            callback(false);
        }
    })
}

function saveDesig(e) {
    e.preventDefault();

    const designation = $("#DesignationName").val().trim();

    if (!designation || designation === "") {
        toastr.error('Please enter Designation Name.');
        $('#DesignationName').addClass('error-border').focus();
        return false;
    }
    //that is changes from me and that is checking duplicate
    $("#AutoId").val(editIdDesig ?? 0);
    const formData = new FormData($('#designation-Form')[0]);

    checkForDuplicateDesig(formData, function (isUnique) {
        if (isUnique) {
            const url = editIdDesig ? `/HrmDefDesignation/Edit/${editIdDesig}` : `/HrmDefDesignation/Create`;
            const method = editIdDesig ? 'PUT' : 'POST';

            $.ajax({
                url,
                type: method,
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    toastr.success(editIdDesig ? 'Data Updated Successfully' : 'Data Saved Successfully');
                    editIdDesig = null;
                    resetFormDesig();
                    loadDesigId();
                    loadPaginatedDesig();
                },
                error: function (error) {
                    handleSaveErrorDesig(error);
                }
            });
        } else {
            console.log("Duplicate Designation found.")
        }
    })
}

function handleSaveErrorDesig(error) {
    if (error.responseJSON && error.responseJSON.errors) {

        let errorMessage = [];

        $.each(error.responseJSON.error, (_, messages) => {
            $.each(message, (_, message) => {
                errorMessage.push(`<li>${message}</li>`);
            });
        });

        errorMessage += "</ul>";
        toastr.error('Validation Error:\n<ul>' + errorMessage.join('') + '</ul>');;
    } else {
        toastr.error('Error saving customer');
    }
    console.error("Error saving customer:", error);
}

function singleDeleteDesig() {
    if (!editIdDesig) {
        toastr.warning("Please select a Designation.");
        return;
    }
    console.log(editIdDesig);
    Swal.fire({
        title: "Are you sure?",
        text: "Once deleted, you won't be able to recover this Designation!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Delete",
        cancelButtonText: "Cancel",
        confirmButtonColor: "#d33"
    }).then((result) => {
        if (result.isConfirmed) {
            deleteDesig(editIdDesig);
            editIdDesig = null;
        }
    });
}
function deleteDesig(ptypeCode) {
    $.ajax({
        url: `/HrmDefDesignation/Delete/${ptypeCode}`,
        type: 'DELETE',
        success: function (response) {
            if (response.success) {
                toastr.success(response.message);
            }
            resetFormDesig();
            loadDesigId();
            loadPaginatedDesig();
        },
        error: function (xhr, status, error) {
            console.error("Error deleting Designation:", error);
            toastr.error("Failed to delete Designation. Please try again.");
        }
    });
};

function bulkDeleteDesig() {
    const selectedIds = [];

    $(".designation-selectItem:checked").each(function () {
        selectedIds.push($(this).data('id'));
    });

    console.log(selectedIds);

    if (selectedIds.length === 0) {
        Swal.fire('No selection', 'Please select at least one item to delete', 'info');
        return;
    }

    Swal.fire({
        title: 'Are you sure?',
        text: `You are about to delete ${selectedIds.length} Designations. This action cannot be undone.`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, delete them!',
        cancelButtonText: 'Cancel'
    }).then((result) => {

        if (result.isConfirmed) {
            $.ajax({
                url: '/HrmDefDesignation/BulkDelete',
                method: 'POST',
                data: { ids: selectedIds },
                traditional: true,
                success: function (response) {
                    if (true) {
                        Swal.fire('Deleted!', response.message, 'success');
                        resetFormDesig();
                        loadPaginatedDesig();
                    } else {
                        Swal.fire('Failed!', response.message, 'error');
                    }
                },
                error: function () {
                    Swal.fire('Error!', 'An error occurred while deleting.', 'error');
                }
            });
        }
    });
}

function loadDesigId() {
    $.ajax({
        type: "GET",
        url: `/HrmDefDesignation/GenerateNewIdAsync`,
        success: function (response) {
            if (response) {
                console.log(response);
                $('#DesignationCode').val(response);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error fetching CustomerId:", error);
        }
    });
}

function resetFormDesig() {
    const form = document.getElementById('designation-Form');
    if (form) {
        form.reset();
    }
    //That is added from me and this is use for reset Searchbox
    $("#designation-searchInput").val('');
    currentpageDesig = 1;
    editIdDesig = null;
    loadDesigId();
    toggleBulkActionsDesig();
    showFooterDesig();
    loadPaginatedDesig();
    $("#designation-check-all").prop('checked', false);
}

function formatDate(dateStr) {
    if (!dateStr) return "--";
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-US', {
        year: 'numeric', month: 'short', day: 'numeric',
        hour: '2-digit', minute: '2-digit'
    });
}