let currentpageCountry = 1;
let pageSizeCountry = 10;
let totalPageCountry = 0;
let editIdCountry;

let currentSortColumnCountry = 'CountryCode';
let currentSortOrderCountry = 'desc';

$(document).ready(() => {
    setupCountryEventListeners();
    loadPaginatedCountry();
    loadCountryId();
    showFooterCountry();
    updatePaginationCountry();
    updateSortingIndicatorCountry();
    setupActionButtonsCountry();
});

function setupCountryEventListeners() {
    $('.country-dropdown-item').on('click', function () {
        var selectedSize = $(this).data("size");
        if (!selectedSize)
            return

        pageSizeCountry = parseInt(selectedSize, 10);
        var displaytext = (pageSizeCountry === -1) ? "All" : selectedSize;

        $('#country-selectedPageSize').text(displaytext);
        loadPaginatedCountry();
    })


    $('th.country-sort').on('click', function () {
        const column = $(this).data('sort');
        $('.country-sort').removeClass('sort-asc sort-desc');

        if (currentSortColumnCountry === column) {
            currentSortOrderCountry = currentSortOrderCountry === 'asc' ? 'desc' : 'asc';
        } else {
            currentSortColumnCountry = column;
            currentSortOrderCountry = 'asc';
        }

        $(this).addClass('sort-' + currentSortOrderCountry);

        loadPaginatedCountry(currentSortColumnCountry, currentSortOrderCountry);
        updateSortingIndicatorCountry(column, currentSortOrderCountry);
    });

    $("#country-btnBulkDelete").on('click', bulkDeleteCountry);
    $("#country-btnDelete").on('click', singleDeleteCountry);

    $("#country-resetBtn").on('click', resetFormCountry);
    $("#country-btnSave").on('click', saveCountry);

    const debouncedSearch = debounce(() => {
        currentpageCountry = 1;
        loadPaginatedCountry();
    }, 300);

    $("#country-searchInput").on("input", debouncedSearch);


    $("#country-prevPageBtn").on('click', function () {
        if (currentpageCountry > 1) {
            currentpageCountry--;
            loadPaginatedCountry();
        }
    });

    $("#country-nextPageBtn").on('click', function () {
        if (currentpageCountry < totalPageCountry) {
            currentpageCountry++;
            loadPaginatedCountry();
        }
    });

    $('#country-check-all').on('change', function () {
        $('.country-selectItem').prop('checked', $(this).prop('checked'));
        toggleBulkActionsCountry();
    })


    $(document).on('change', '.country-selectItem', toggleBulkActionsCountry);
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
function updateSortingIndicatorCountry() {
    $('th.country-sort').find('.sort-icon').remove();

    const th = $(`th.country-sort[data-sort="${currentSortColumnCountry}"]`);
    const icon = currentSortOrderCountry === 'asc' ? 'fa-sort-up' : 'fa-sort-down';

    th.append(`<span class="sort-icon ms-2"><i class="fas ${icon}"></i></span>`);
}
function updatePaginationCountry(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#country-paginationLinks");
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
                    .on('click', () => goToPageCountry(page))
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

    $("#country-prevPageBtn").prop('disabled', currentPage === 1);
    $("#country-nextPageBtn").prop('disabled', currentPage === totalPages);
}
function goToPageCountry(page) {
    currentpageCountry = page;
    loadPaginatedCountry();
}
function showFooterCountry() {
    $("#country-footer").toggleClass("d-none", !editIdCountry);
}
function toggleBulkActionsCountry() {
    console.log("toggle");
    const anyChecked = $('.country-selectItem:checked').length > 0;
    console.log(anyChecked);
    $('#country-btnBulkDelete').toggleClass('d-none', !anyChecked);
    $('#country-btnDelete').toggleClass('d-none', anyChecked);
}
function loadPaginatedCountry(sortColumn = 'CountryCode', sortOrder = 'desc') {
    const searchTerm = $("#country-searchInput").val();
    console.log(searchTerm);

    $.ajax({
        url: '/CoreCountry/GetAllPaginated',
        method: 'GET',
        data: { pageNumber: currentpageCountry, pageSize: pageSizeCountry, searchTerm: searchTerm, sortColumn: sortColumn, sortOrder: sortOrder },

        success: function (response) {
            console.log(response);
            renderCountryTable(response);
            setupActionButtonsCountry();
        },
        error: function (error) {
            console.error("Error loading data: ", error);
            $("#deliveryTableBody").html('<tr><td colspan="6" class="text-center">No ERROR Data Found</td></tr>');
        }
    });
}
function renderCountryTable(response) {
    const tableBody = $("#country-table-body");
    tableBody.empty();

    if (response.data.length > 0) {
        console.log(response.data);
        $("no-data").hide();

        $.each(response.data, (_, item) => {
            tableBody.append(`
                     <tr class="position-static country-row" data-id="${item.countryCode}">
                    <td style="width:5%" class="text-center text-middle align-middle ps-2">
                        <input type="checkbox" class="country-selectItem" data-id="${item.countryCode}" />
                    </td>
                    <td style="padding: 5px 10px; font-size: 14px;" class="align-middle white-space-nowrap ps-0">   ${item.countryId}    </  td>
                    <td style="padding: 5px 10px; font-size: 10px;" class="align-middle ps-0">${item.countryName}</td>
                    <td style="padding: 5px 10px; font-size: 10px;" class="align-middle ps-0">${item.ioccode}</td>
                    <td style="padding: 5px 10px; font-size: 10px;" class="align-middle ps-0">${item.isocode}</td>
                </tr>
            `)
        });
    } else {
        tableBody.html('<tr><td colspan="6" class="text-center">No data available</td></tr>');
        $("#country-paginationInfo").text('');
        $("#country-paginationLinks").empty();
    }


    const paginationInfo = response.paginationInfo;
    $("#country-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
    $("#country-paginationLinks").text(`(${paginationInfo.totalItems})`);

    updatePaginationCountry(paginationInfo.pageNumber, paginationInfo.currentPage, paginationInfo.totalPages);
    totalPageCountry = paginationInfo.totalPages;
}
function setupActionButtonsCountry() {
    $(".country-row").find("select, input").click(e => e.stopPropagation());


    $(".country-row").on("click", function () {
        const ptypeCode = $(this).data("id");

        if (!ptypeCode) return;

        $.ajax({
            url: `/CoreCountry/Details/${ptypeCode}`,
            method: 'GET',
            success: populateFormCountry,
            error: () => toastr.error("Failed to load country details.")
        });
    });
}
function populateFormCountry(response) {
    console.log(response);
    // Set edit ID 
    editIdCountry = response.countryCode || response.CountryCode;

    showFooterCountry();

    // Map lowercase keys from response to exact HTML IDs
    const KeyToIdMap = {
        "countrycode": "CountryCode",
        "countryid": "CountryId",
        "countryname": "CountryName",
        "ioccode": "Ioccode",
        "isocode": "Isocode"
    };

    $.each(response, (key, value) => {
        const lowerKey = key.toLowerCase();

        // Skip audit fields if you want, or add those IDs if you want to show them
        if (["ldate", "luser", "lip", "lmac", "modifydate"].includes(lowerKey)) {
            return; // ignore these keys for now
        }

        const fieldId = KeyToIdMap[lowerKey];
        if (!fieldId) {
            console.warn(`No matching field for key: ${key}`);
            return;
        }

        const $field = $(`#${fieldId}`);
        if ($field.length) {
            $field.val(value ?? '');
        } else {
            console.warn(`No matching element with id: ${fieldId}`);
        }
    });

    // Bangladeshi date format: dd/MM/yyyy
    function formatBDDate(dateValue) {
        return dateValue ? new Date(dateValue).toLocaleDateString('en-GB') : '--';
    }

    $("#entryDate").text(`Entry Date: ${formatBDDate(response.ldate)}`);
    $("#lastUpdateDate").text(`Last Updated: ${formatBDDate(response.modifyDate)}`);
}

function checkForDuplicateCountry(formData, callback) {
    $.ajax({
        url: '/CoreCountry/CheckDuplicate',
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
            toastr.error('Error checking for duplicate Country');
            callback(false);
        }
    })
}
function saveCountry(e) {
    e.preventDefault();

    const country = $("#CountryName").val().trim();

    if (!country || country === "") {
        toastr.error('Please enter Country Name.');
        $('#CountryName').addClass('error-border').focus();
        return false;
    }

    const formData = new FormData($('#country-Form')[0]);

    checkForDuplicateCountry(formData, function (isUnique) {
        if (isUnique) {
  
            const url = editIdCountry ? `/CoreCountry/Edit/${editIdCountry}` : `/CoreCountry/Create`;
            method = editIdCountry ? 'PUT' : 'POST';

            $.ajax({
                url,
                type: method,
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    toastr.success(editIdCountry ? 'Data Updated Successfully' : 'Data Saved Successfully');
                    editIdCountry = null;
                    resetFormCountry();
                    loadCountryId();
                    loadPaginatedCountry();
                },
                error: function (error) {
                    handleSaveErrorCountry(error);
                }
            });
        } else {
            console.log("Duplicate Country found.")
        }
    })
}
function handleSaveErrorCountry(error) {
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
function singleDeleteCountry() {
    if (!editIdCountry) {
        toastr.warning("Please select a Country.");
        return;
    }
    console.log(editIdCountry);
    Swal.fire({
        title: "Are you sure?",
        text: "Once deleted, you won't be able to recover this Country!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Delete",
        cancelButtonText: "Cancel",
        confirmButtonColor: "#d33"
    }).then((result) => {
        if (result.isConfirmed) {
            deleteCountry(editIdCountry);
            editIdCountry = null;
        }
    });
}
function deleteCountry(ptypeCode) {
    $.ajax({
        url: `/CoreCountry/Delete/${ptypeCode}`,
        type: 'DELETE',
        success: function (response) {
            if (response.success) {
                toastr.success(response.message);
            }
            resetFormCountry();
            loadCountryId();
            loadPaginatedCountry();
        },
        error: function (xhr, status, error) {
            console.error("Error deleting Country:", error);
            toastr.error("Failed to delete Country. Please try again.");
        }
    });
};
function bulkDeleteCountry() {
    const selectedIds = [];

    $(".country-selectItem:checked").each(function () {
        selectedIds.push($(this).data('id'));
    });

    console.log(selectedIds);

    if (selectedIds.length === 0) {
        Swal.fire('No selection', 'Please select at least one item to delete', 'info');
        return;
    }

    Swal.fire({
        title: 'Are you sure?',
        text: `You are about to delete ${selectedIds.length} Countries. This action cannot be undone.`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, delete them!',
        cancelButtonText: 'Cancel'
    }).then((result) => {

        if (result.isConfirmed) {
            $.ajax({
                url: '/CoreCountry/BulkDelete',
                method: 'POST',
                data: { ids: selectedIds },
                traditional: true,
                success: function (response) {
                    if (true) {
                        Swal.fire('Deleted!', response.message, 'success');
                        resetFormCountry();
                        loadPaginatedCountry();
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
function loadCountryId() {
    $.ajax({
        type: "GET",
        url: `/CoreCountry/GenerateNewIdAsync`,
        success: function (response) {
            if (response) {
                console.log(response);
                $('#CountryId').val(response);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error fetching CustomerId:", error);
        }
    });
}
function resetFormCountry() {
    const form = document.getElementById('country-Form');
    if (form) {
        form.reset();
    }

    currentpageCountry = 1;
    editIdCountry = null;
    loadCountryId();
    toggleBulkActionsCountry();
    showFooterCountry();
    $("#country-searchInput").val('').trigger('input');
    $("#country-check-all").prop('checked', false);
    $(".country-selectItem").prop('checked', false);
}

