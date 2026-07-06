let currentPage = 1;
let pageSize = 10;
let currentSortColumn = "CompanyCode";
let currentSortDirection = "desc";
let searchTerm = "";
let totalPages = 0;

//Load Next Company ID 
function loadNextCompanyId() {
    $.ajax({
        url: '/next-Company-id', 
        method: 'GET',
        success: function (response) {
            $('#CompanyCode').val(response);
        },
        error: function (xhr, status, error) {
            console.error("Failed to load next Company ID:", error);
        }
    });
}
// Load Coutriews into the dropdown
function LoadCountriesDropdown() {
    $.ajax({
        url: '/core-country-dropdown',
        method: 'GET',
        success: function (countries) {
            var $country = $('#Country');
            $country.empty().append('<option value=""></option>');

            $.each(countries, function (index, country) {
                $country.append(
                    '<option value="' + country.countryId + '">' + country.countryName + '</option>'
                );
            });

            $country.select2({
                placeholder: 'Country<span style="color:red">*</span>',
               escapeMarkup: function (markup) { return markup; },
                width: '100%'
            });
        },
        error: function (xhr, status, error) {
            console.error('Failed to load countries:', error);
        }
    });
}

// Load currencies into the dropdown
function LoadCurrenciesDropdown() {
    $.ajax({
        url: '/currency-type-dropdown',
        method: 'GET',
        success: function (currencies) {
            var $currency = $('#BaseCurrency');
            $currency.empty().append('<option value=""></option>'); 

            $.each(currencies, function (index, currency) {
                $currency.append(
                    '<option value="' + currency.currencyId + '">' + currency.currencyName + '</option>'
                );
            });

            // Initialize Select2 to make it searchable
            $currency.select2({
                placeholder: "Base Currency",
                width: '100%' 
            });
        },
        error: function (xhr, status, error) {
            console.error('Failed to load currencies:', error);
        }
    });
}

function validateCompanyForm() {
    let isValid = true;

    $("#CompanyName, #Address1, #Country").removeClass("error-border");

    // Company Name
    const companyName = $("#CompanyName").val().trim();
    if (!companyName) {
        toastr.error("Please enter Company Name");
        $("#CompanyName").addClass("error-border").focus();
        isValid = false;
        return false; 
    }

    // Address1
    const address1 = $("#Address1").val().trim();
    if (!address1) {
        toastr.error("Please enter Address1");
        $("#Address1").addClass("error-border").focus();
        isValid = false;
        return false;
    }

    // Country
    const country = $("#Country").val();
    if (!country) {
        toastr.error("Please select Country");
        $("#Country").next(".select2").find(".select2-selection").addClass("error-border"); 
        $("#Country").focus();
        isValid = false;
        return false;
    } else {
        $("#Country").next(".select2").find(".select2-selection").removeClass("error-border");
    }

    return isValid;
}


async function saveOrUpdateCompany() {

    //Validate 
    if (!validateCompanyForm()) return;

    const companyData = {
        AutoID: $("#autoID").val() ? parseInt($("#autoID").val()) : 0,
        CompanyCode: $("#CompanyCode").val(),
        CompanyName: $("#CompanyName").val().trim(),
        CompanyShortName: $("#CompanyShortName").val().trim(),
        Address1: $("#Address1").val().trim(),
        Address2: $("#Address2").val().trim(),
        State: $("#State").val().trim(),
        City: $("#City").val().trim(),
        ZipCode: $("#ZipCode").val().trim(),
        Country: $("#Country").val(),  
        Phone1: $("#Phone1").val().trim(),
        Phone2: $("#Phone2").val().trim(),
        BIN: $("#BIN").val().trim(),
        HotLine: $("#HotLine").val().trim(),
        Email: $("#Email").val().trim(),
        URL: $("#URL").val().trim(),
        RegNo: $("#RegNo").val().trim(),
        TIN: $("#TIN").val().trim(),
        BaseCurrency: $("#BaseCurrency").val()  
    };

    console.log("Company Data:", companyData);

    try {
        //Duplicate check
        const dupResponse = await $.ajax({
            url: '/company/check-duplicate',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(companyData)
        });

        if (dupResponse.isDuplicate) {
            toastr.error(dupResponse.message || "Data already exists!");
            return;
        }

        //Save or Update
        const isUpdate = companyData.AutoID > 0;
        const saveResponse = await $.ajax({
            url: isUpdate ? `/core-company/${companyData.AutoID}` : '/core-company',
            type: isUpdate ? 'PUT' : 'POST',
            contentType: 'application/json',
            data: JSON.stringify(companyData)
        });
        console.log("Company response:", companyData);
        if (saveResponse.success) {
            
            clearCompanyForm();
            LoadCountriesDropdown();
            LoadCurrenciesDropdown();
            LoadAllCompany(currentPage, pageSize, currentSortColumn, currentSortDirection, searchTerm);
            toastr.success(isUpdate ? "Data Updated Successfully" : "Data Saved Successfully");
        } else {
            toastr.error(saveResponse.message || (isUpdate ? "Update Failed" : "Insertion Failed"));
        }

    } catch (err) {
        console.error(err);
        toastr.error(companyData.AutoID ? "Error updating company" : "Error saving company");
    }
}

function LoadAllCompany(page = 1, pageSizeVal = pageSize, sortColumn = currentSortColumn, sortDirection = currentSortDirection, search = searchTerm) {
    currentPage = page;
    pageSize = pageSizeVal;
    currentSortColumn = sortColumn;
    currentSortDirection = sortDirection;
    searchTerm = search;

    $.ajax({
        url: '/core-company-list',
        type: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSizeVal,
            searchTerm: searchTerm,
            sortColumn: currentSortColumn,
            sortOrder: currentSortDirection
        },
        success: function (response) {
            console.log("Company Response:", response);

            let rows = '';
            if (!response.data || response.data.length === 0) {
                rows = `<tr class="no-data">
                            <td colspan="5" class="text-center">No Data Available</td>
                        </tr>`;
            } else {
                response.data.forEach(function (item) {
                    rows += `<tr>
                        <td class="text-center align-middle">
                            <input type="checkbox" class="row-checkbox" data-id="${item.autoID}">
                        </td>
                        <td class="text-center align-middle">
                          <button type="button" class="btn btn-link edit-company px-0" data-id="${item.autoID}">
                            ${item.companyCode}
                           </button>

                        </td>
                        <td class="align-middle">${item.companyName || ''}</td>
                        <td class="align-middle">${item.companyShortName || ''}</td>
                        <td class="align-middle">${item.address1 || ''}</td>
                    </tr>`;
                });
            }

            $('#tblBody').html(rows);

            // Pagination info
            totalPages = Math.ceil((response.paginationInfo?.totalItems || 0) / pageSize);
            $('#paginationInfo').text(`Page ${currentPage} of ${totalPages}`);
            $('#paginationSummary').text(`Showing ${response.paginationInfo?.startItem || 0} to ${response.paginationInfo?.endItem || 0} of ${response.paginationInfo?.totalItems || 0} entries`);
            $('#totalRecordsInfo').text(`Total Records: ${response.paginationInfo?.totalItems || 0}`);

            generateCompanyPageButtons(currentPage, totalPages);
            $('#selectAll').prop('checked', false);
            updateCheckboxState();
            updateSortIndicators();
        },
        error: function (xhr, status, error) {
            console.error("Error loading company data: ", error);
            $('#tblBody').html(`<tr class="no-data"><td colspan="5" class="text-center">Failed to load data</td></tr>`);
        }
    });
}

function generateCompanyPageButtons(currentPage, totalPages) {
    const navigationDiv = $('#pageNavigation');
    navigationDiv.empty();

    function createButton(page) {
        const btn = $('<button>')
            .addClass('btn btn-sm mx-1')
            .text(page)
            .click(() => LoadAllCompany(page, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim()));

        if (page === currentPage) {
            btn.removeClass('btn-outline-primary').addClass('btn-primary');
        } else {
            btn.addClass('btn-outline-primary');
        }
        return btn;
    }

    if (totalPages === 0) return; 

    navigationDiv.append(createButton(1));

    if (currentPage > 3) {
        navigationDiv.append($('<span>').text('...').css('padding', '0 5px'));
    }

    if (currentPage !== 1 && currentPage !== totalPages) {
        navigationDiv.append(createButton(currentPage));
    }

    if (currentPage < totalPages - 2) {
        navigationDiv.append($('<span>').text('...').css('padding', '0 5px'));
    }

    if (totalPages > 1) {
        navigationDiv.append(createButton(totalPages));
    }
}
function updateCheckboxState() {
    const $allCheckboxes = $('.row-checkbox');
    const $selectAll = $('#selectAll');

    if ($allCheckboxes.length === 0) {
        $selectAll.prop('checked', false);
        $selectAll.prop('indeterminate', false);
        return;
    }

    const checkedCount = $allCheckboxes.filter(':checked').length;

    if (checkedCount === 0) {
        $selectAll.prop('checked', false);
        $selectAll.prop('indeterminate', false);
    } else if (checkedCount === $allCheckboxes.length) {
        $selectAll.prop('checked', true);
        $selectAll.prop('indeterminate', false);
    } else {
        $selectAll.prop('checked', false);
        $selectAll.prop('indeterminate', true);
    }
}

// Call this when user clicks the "Select All" checkbox
$(document).on('change', '#selectAll', function () {
    const isChecked = $(this).is(':checked');
    $('.row-checkbox').prop('checked', isChecked);
});

// Update sort indicators on table headers
function updateSortIndicators() {
    $('.sortable').each(function () {
        const column = $(this).data('column');
        const $icon = $(this).find('.sort-icon');

        $icon.removeClass('fa-sort-up fa-sort-down').addClass('fa-sort'); // reset

        if (column === currentSortColumn) {
            if (currentSortDirection === 'asc') {
                $icon.removeClass('fa-sort').addClass('fa-sort-up');
            } else if (currentSortDirection === 'desc') {
                $icon.removeClass('fa-sort').addClass('fa-sort-down');
            }
        }
    });
}

// Change sort direction when header is clicked
$(document).on('click', '.sortable', function () {
    const column = $(this).data('column');

    if (currentSortColumn === column) {
        currentSortDirection = currentSortDirection === 'asc' ? 'desc' : 'asc';
    } else {
        currentSortColumn = column;
        currentSortDirection = 'asc';
    }

    LoadAllCompany(currentPage, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
});

function populateCompanyForm(data) {
    $('#CompanyForm input[name="autoID"]').val(data.autoID);
    $('#CompanyForm input[name="CompanyCode"]').val(data.companyCode);
    $('#CompanyForm input[name="CompanyName"]').val(data.companyName);
    $('#CompanyForm input[name="CompanyShortName"]').val(data.companyShortName || "");
    $('#CompanyForm input[name="Address1"]').val(data.address1 || "");
    $('#CompanyForm input[name="Address2"]').val(data.address2 || "");
    $('#CompanyForm input[name="State"]').val(data.state || "");
    $('#CompanyForm input[name="City"]').val(data.city || "");
    $('#CompanyForm input[name="ZipCode"]').val(data.zipCode || "");

    // Country & BaseCurrency populated here
    $('#CompanyForm select[name="Country"]').val(data.country).trigger('change');
    $('#CompanyForm select[name="BaseCurrency"]').val(data.baseCurrency).trigger('change');

    $('#CompanyForm input[name="Phone1"]').val(data.phone1 || "");
    $('#CompanyForm input[name="Phone2"]').val(data.phone2 || "");
    $('#CompanyForm input[name="BIN"]').val(data.bIN || "");
    $('#CompanyForm input[name="HotLine"]').val(data.hotLine || "");
    $('#CompanyForm input[name="Email"]').val(data.email || "");
    $('#CompanyForm input[name="URL"]').val(data.uRL || "");
    $('#CompanyForm input[name="RegNo"]').val(data.regNo || "");
    $('#CompanyForm input[name="TIN"]').val(data.tIN || "");

    // Entry Date
    if (data.lDate) {
        let entryDate = new Date(data.lDate).toLocaleString('en-GB', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: 'numeric',
            minute: '2-digit',
            hour12: true
        }).replace(',', '');
        $('#lDateContainer').text(entryDate.toUpperCase());
    } else {
        $('#lDateContainer').text('');
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
        $('#modifyDateContainer').text(updateDate.toUpperCase());
    } else {
        $('#modifyDateContainer').text('');
    }
}

//Populate data with clicked edit button

$(document).on('click', '.edit-company', function () {
    const id = $(this).data('id');

    $.ajax({
        url: `/core-company-details?id=${id}`,
        type: 'GET',
        success: function (data) {
            console.log("Fetched Company details:", data);
            populateCompanyForm(data);
        },
        error: function (err) {
            console.error('Failed to fetch company details:', err);
        }
    });
});

//Reset form function
function clearCompanyForm() {
    $("#CompanyForm")[0].reset();
    $("#autoID").val(0);
    $("#Country, #BaseCurrency").val("").trigger("change");
    loadNextCompanyId();
    $('#lDateContainer').text('');
    $('#modifyDateContainer').text('');
    $(".row-checkbox").prop("checked", false);
    $("#selectAll").prop("checked", false);
    $('#searchInput').val('');

    // Set Entry Date
    $('#lDateContainer').text(getBangladeshDateTime());

    LoadAllCompany(1, pageSize, currentSortColumn, currentSortDirection, '');
}


//Select/Deselect all checkboxes
$(document).on('change', '#selectAll', function () {
    const isChecked = $(this).is(':checked');
    $('.row-checkbox').prop('checked', isChecked);
});

//Delete Single Company and Bulk Delete

$(document).on("click", "#deleteBtn", function () {
    const id = $("#autoID").val(); 
    const selectedIds = $(".row-checkbox:checked")
        .map(function () { return parseInt($(this).data("id")); })
        .get();

    if (id && id > 0) {
        //Single Delete 

        $.ajax({
            url: `/core-company/${id}`,
            type: 'DELETE',
            success: function (response) {
                toastr.success(response.message || "Data Deleted Successfully");
                clearCompanyForm();
                LoadAllCompany();
                loadNextCompanyId();
            },
            error: function () {
                toastr.error("Error deleting company");
            }
        });

    }
    else if(selectedIds.length > 0) {

        $.ajax({
            url: '/core-company-list',
            type: 'DELETE',
            contentType: 'application/json',
            data: JSON.stringify(selectedIds),
            success: function (response) {
                if (response.isSuccess) {
                    toastr.success(response.message);
                    loadNextCompanyId();
                    LoadAllCompany();
                    $("#selectAll").prop("checked", false);
                } else {
                    toastr.error(response.message || "Bulk delete failed");
                }
            },
            error: function () {
                toastr.error("Error deleting companies");
            }
        });

    } else {
        toastr.warning("Please select a data to delete");
    }
});

// Search on typing
$(document).on("input", "#searchInput", function () {
    const searchValue = $(this).val().trim();
    LoadAllCompany(1, pageSize, currentSortColumn, currentSortDirection, searchValue);
});


$(document).ready(function () {
    LoadAllCompany();
    loadNextCompanyId();
    LoadCountriesDropdown();
    LoadCurrenciesDropdown();

    // Set Entry Date
    $('#lDateContainer').text(getBangladeshDateTime());

    $(document).on("submit", "#CompanyForm", function (e) {
        e.preventDefault();
        saveOrUpdateCompany(e);
    });
    $(document).on("click", ".resetBtn", function () {
        console.log("Clear Button Clicked");
        clearCompanyForm();
    });
    $('#firstPage').off('click').on('click', function () {
        if (currentPage > 1) LoadAllCompany(1, pageSize, currentSortColumn, currentSortDirection, searchTerm);
    });
    $('#prevPage').off('click').on('click', function () {
        if (currentPage > 1) LoadAllCompany(currentPage - 1, pageSize, currentSortColumn, currentSortDirection, searchTerm);
    });
    $('#nextPage').off('click').on('click', function () {
        if (currentPage < totalPages) LoadAllCompany(currentPage + 1, pageSize, currentSortColumn, currentSortDirection, searchTerm);
    });
    $('#lastPage').off('click').on('click', function () {
        if (currentPage < totalPages) LoadAllCompany(totalPages, pageSize, currentSortColumn, currentSortDirection, searchTerm);
    });
    $('#pageSize').on('change', function () {
        let newSize = parseInt($(this).val());
        pageSize = newSize;
        LoadAllCompany(1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
    });
});


// Function to get current date in Bangladesh time

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