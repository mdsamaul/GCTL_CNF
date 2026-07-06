let selectedBranches = [];
let branchCurrentPage = 1;
let branchPageSize = 10;
let branchCurrentSortColumn = 'BankBranchID';
let branchCurrentSortDirection = 'desc';
console.log("branch page")
console.log(`Global Call: ${selectedBranches}`);


//BAnkIDs Getting
function loadNextBranchId() {
    $.ajax({
        url: '/next-branch-id',
        method: 'GET',
        success: function (response) {
            console.log("Branch ID:", response);
            $('#branchID').val(response);

        },
        error: function (xhr, status, error) {
            console.error("Failed to load next Branch ID:", error);
        }
    });
}

//Save and Update with BranchName Duplicate Checking
async function saveBranch() {
    console.log("Save branch called");

    const autoID = $('#branchAutoID').val();
    const branchName = $("#branchName").val().trim();
    const bankID = $('#bank').val();
    const shortName = $("#branchShortName").val().trim();
    const swiftCode = $("#swiftCode").val().trim();
    const address = $("#branchAddress").val().trim();
    const phone = $("#branchPhone").val().trim();

    if (!bankID) return toastr.warning("Please select a Bank");
    if (!branchName) return toastr.warning("Please Enter Branch Name");

    const branchData = {
        AutoID: autoID ? parseInt(autoID) : 0,
        BankBranchName: branchName,
        BankID: bankID
    };

    try {
        //Duplicate check
        const dupResponse = await $.ajax({
            url: '/bank-branch/check-duplicate',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(branchData)
        });

        if (dupResponse.isDuplicate) {
            toastr.error("Data already exists for this bank!");
            return;
        }

        //Save or update
        const url = autoID ? `/branch/${autoID}` : '/branch';
        const method = autoID ? 'PUT' : 'POST';
        const successMessage = autoID ? 'Data Updated Successfully' : 'Data Saved Successfully';
        const errorMessage = autoID ? 'Update Failed' : 'Insertion Failed';

        const saveData = {
            AutoID: autoID ? parseInt(autoID) : 0,
            BankBranchID: $("#branchID").val(),
            BankBranchName: branchName,
            ShortName: shortName,
            SWIFTCode: swiftCode,
            Address: address,
            Phone: phone,
            BankID: bankID
        };

        const saveResponse = await $.ajax({
            url: url,
            type: method,
            contentType: 'application/json',
            data: JSON.stringify(saveData)
        });

        if (saveResponse.success) {
            LoadBranchDropDown(saveData.bankID, saveData.newBranchID);
            selectedBranches = [];
            clearBranchForm();
            loadBranches(branchCurrentPage, branchPageSize, branchCurrentSortColumn, branchCurrentSortDirection);
            toastr.success(successMessage);
        } else {
            toastr.error(saveResponse.message || errorMessage);
        }

    } catch (error) {
        console.error('Error in saveBranch:', error);
        const autoIDPresent = autoID ? true : false;
        toastr.error(autoIDPresent ? "An error occurred while updating branch" : "An error occurred while saving branch");
    }
}

// Clear form fields
function clearBranchForm() {
    $("#branchAutoID").val("");
    $("#branchPhone").val("");
    $("#branchAddress").val("");
    $("#branchShortName").val("");
    $("#branchName").val("");
    $("#bank").val("");
    $("#swiftCode").val("");
    $(".form-control").removeClass("is-invalid is-valid");
    $(".invalid-feedback, .valid-feedback").remove();
    $('#branchModifyDateContainer').hide();
    $('#branchLDateContainer').hide();
    $(".row-checkbox").prop("checked", false);
    $("#branchSelectAll").prop("checked", false);
    $('#branchSearchInput').val('');

    loadNextBranchId();
    loadBankDropdown();

    loadNextBankId();
    selectedBranches = [];
    currentPage = 1;
    loadBranches(branchCurrentPage, branchPageSize, branchCurrentSortColumn, branchCurrentSortDirection, "", "");

}


function populateBranchForm(data) {
    $('#branchForm input[name="branchID"]').val(data.bankBranchID);
    $('#branchForm input[name="branchName"]').val(data.bankBranchName);
    $('#branchForm input[name="branchShortName"]').val(data.shortName);
    $('#branchForm input[name="swiftCode"]').val(data.sWIFTCode);
    $('#branchForm input[name="branchAddress"]').val(data.address);
    $('#branchForm input[name="branchPhone"]').val(data.phone);
    $('#branchForm input[name="branchAutoID"]').val(data.autoID);
    loadBankDropdown(data.bankID);

    if (data.lDate) {
        const formattedLDate = new Date(data.lDate).toLocaleDateString('en-GB');
        $('#displayBranchLDate').text(formattedLDate);
        $('#branchLDateContainer').show();
    } else {
        $('#branchLDateContainer').hide();
    }

    if (data.modifyDate) {
        const formattedModifyDate = new Date(data.modifyDate).toLocaleDateString('en-GB');
        $('#displayBranchModifyDate').text(formattedModifyDate);
        $('#branchModifyDateContainer').show();
    } else {
        $('#branchModifyDateContainer').hide();
    }
}

//Details population
$(document).on('click', '.branchBtn-transfer', function () {

    const data = {
        autoID: $(this).data('branchautoid'),
        bankBranchID: $(this).data('branchid'),
        bankID: $(this).data('branchbankid'),
        bankBranchName: $(this).data('branchname'),
        shortName: $(this).data('shortname'),
        swiftCode: $(this).data('swiftcode'),
        address: $(this).data('address'),
        phone: $(this).data('phone'),
        lDate: $(this).data('branchldate'),
        modifyDate: $(this).data('branchmodifydate')
    };
    selectedBranches = [];
    populateBranchForm(data);

    selectedBranches = [parseInt(data.autoID)];


    console.log("Selected Branch (last populate only):", selectedBranches);
});

// Select all checkbox
$(document).on('click', '#branchSelectAll', function () {
    const isChecked = $(this).prop('checked');
    $('.branchrow-checkbox:visible').prop('checked', isChecked);

    if (isChecked) {
        $('.branchrow-checkbox:visible').each(function () {
            const id = parseFloat($(this).data('checkautoid'));
            if (!selectedBranches.includes(id)) {
                selectedBranches.push(id);
            }
        });
    } else {
        $('.branchrow-checkbox:visible').each(function () {
            const id = parseFloat($(this).data('checkautoid'));
            const index = selectedBranches.indexOf(id);
            if (index !== -1) {
                selectedBranches.splice(index, 1);
            }
        });
    }
    console.log("Selected Branches:", selectedBranches);
});

// Individual checkbox click
function bindBranchCheckboxHandlers() {
    $(document).off('click', '.branchrow-checkbox');
    $(document).on('click', '.branchrow-checkbox', function () {
        const id = parseInt($(this).data('checkautoid'));
        const isChecked = $(this).prop('checked');

        if (isChecked) {
            if (!selectedBranches.includes(id)) selectedBranches.push(id);
        } else {
            selectedBranches = selectedBranches.filter(item => item !== id);
        }

        // Update select all checkbox
        const allChecked = $('.branchrow-checkbox:visible').length > 0 &&
            $('.branchrow-checkbox:visible:not(:checked)').length === 0;
        $('#branchSelectAll').prop('checked', allChecked);

        console.log("Selected Branches:", selectedBranches);
    });
}

function loadBranches(page = 1, pageSize = 10, sortColumn = branchCurrentSortColumn, sortDirection = branchCurrentSortDirection, searchTerm = "", bankID = "") {
    $.ajax({
        url: '/branch-list',
        type: 'GET',
        data: {
            pageNumber: page,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortDirection,
            bankID: bankID
        },
        success: function (response) {
            const data = response.data || [];
            const paginationInfo = response.paginationInfo || {};
            const totalRecords = paginationInfo.totalItems || 0;
            const totalPages = paginationInfo.totalPages || 1;
            const pageNum = paginationInfo.currentPage || 1;

            let rows = '';
            if (data.length > 0) {
                data.forEach(function (item) {
                    rows += `<tr>
                        <td class="text-center">
                            <input type="checkbox" class="form-check-input form-check-input-sm branchrow-checkbox" 
                                   data-checkautoid="${item.autoID}" />
                        </td>
                        <td>
                            <button class="btn branchBtn-transfer"
                                data-branchautoid="${item.autoID}"
                                data-branchbankid="${item.bankID}"
                                data-branchid="${item.bankBranchID}"
                                data-branchname="${item.bankBranchName}"
                                data-shortname="${item.shortName}"
                                data-swiftcode="${item.swiftCode}"
                                data-address="${item.address}"
                                data-phone="${item.phone}"
                                data-branchldate="${item.lDate}"
                                data-branchmodifydate="${item.modifyDate}">
                                ${item.bankBranchID}
                            </button>
                        </td>
                        <td>${item.bankBranchName || ''}</td>
                        <td>${item.shortName || ''}</td>
                        <td>${item.swiftCode || ''}</td>
                        <td>${item.address || ''}</td>
                        <td>${item.phone || ''}</td>
                        <td>${item.bankName || ''}</td>
                    </tr>`;
                });
            } else {
                rows = `<tr><td colspan="8" class="text-center">No Data Found</td></tr>`;
            }
            //$('#branchTblBody').html(generateRows(data));
            $('#branchTblBody').html(rows);
            $('#branchPaginationInfo').text(`Page ${pageNum} of ${totalPages}`);
            $('#branchPaginationSummary').text(`Showing ${data.length} of ${totalRecords} records`);
            $('#branchTotalRecordsInfo').text(`Total Records: ${totalRecords}`);

            generateBranchPageButtons(pageNum, totalPages);

            branchCurrentPage = pageNum;

            bindBranchCheckboxHandlers();
            updateBranchSortIndicators();
        },
        error: function (xhr, status, error) {
            console.error("Error loading branches:", error);
        }
    });
}

//Bank Dropdown Filter
$(document).on('change', '#bank', function () {
    const selectedBankID = $('#bank').val();
    console.log("Selected BankID from dropdown:", selectedBankID);
    branchCurrentPage = 1;
    loadBranches(branchCurrentPage, branchPageSize, branchCurrentSortColumn, branchCurrentSortDirection, "", selectedBankID);
});

function updateBranchSortIndicators() {
    $(".branchSortable").each(function () {
        let column = $(this).data("column");
        let icon = $(this).find(".sort-icon");

        icon.removeClass("fa-sort-up fa-sort-down").addClass("fa-sort");

        if (column === branchCurrentSortColumn) {
            icon.removeClass("fa-sort");
            icon.addClass(branchCurrentSortDirection === "asc" ? "fa-sort-up" : "fa-sort-down");
        }
    });
}

function generateBranchPageButtons(currentPage, totalPages) {
    const navigationDiv = $('#branchPageNavigation');
    navigationDiv.empty();

    function createButton(page) {
        const btn = $('<button>')
            .addClass('btn btn-sm mx-1')
            .text(page)
            .click(() => loadBranches(page, branchPageSize, branchCurrentSortColumn, branchCurrentSortDirection, $('#branchSearchInput').val().trim(), $('#bank').val()));

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
    if (currentPage > 1 && currentPage < totalPages) {
        navigationDiv.append(createButton(currentPage));
    }
    if (currentPage < totalPages - 1) {
        navigationDiv.append($('<span>').text('...').css('padding', '0 5px'));
    }
    if (totalPages > 1) {
        navigationDiv.append(createButton(totalPages));
    }
}

function deleteBranchList() {
    if (selectedBranches.length === 0) {
        toastr.warning("No branches selected to delete!");
        return;
    }

    const idsToDelete = [...selectedBranches];
    console.log("IDs to delete:", idsToDelete);

    $.ajax({
        url: '/branch-list',
        type: 'DELETE',
        data: JSON.stringify(idsToDelete),
        contentType: 'application/json',
        success: function (response) {
            if (response.isSuccess) {
                toastr.success(response.message || "Branch deleted successfully!");
            } else {
                toastr.error(response.message || "Failed to delete branch!");
            }

            selectedBranches = [];
            clearBranchForm();
            loadBranches(branchCurrentPage, branchPageSize, branchCurrentSortColumn, branchCurrentSortDirection);
        },
        error: function (error) {
            toastr.error("An error occurred while deleting branch!");
            console.error('Error deleting branches:', error);
        }
    });
}


function branchDebounce(func, delay) {
    let timer;
    return function () {
        const context = this, args = arguments;
        clearTimeout(timer);
        timer = setTimeout(() => func.apply(context, args), delay);
    };
}

// Load Banks into dropdown
function loadBankDropdown(selectedId = null) {
    $.ajax({
        url: '/bank-info-dropdown',
        type: 'GET',
        success: function (data) {
            let $bankDropdown = $("#bank");
            $bankDropdown.empty();
            $bankDropdown.append('<option value=""></option>');

            $.each(data, function (index, item) {
                let isSelected = selectedId && selectedId == item.bankID ? "selected" : "";
                $bankDropdown.append(
                    `<option value="${item.bankID}" ${isSelected}>${item.bankName}</option>`
                );
            });
        },
        error: function (xhr, status, error) {
            console.error("Error loading banks:", error);
        }
    });
}

// Branch Page
window.addEventListener('bankUpdated', function (e) {
    console.log("Bank Updated Event Received for Branch Page");

    // Reload bank dropdown and select the new bank
    loadBankDropdown();
});

window.addEventListener('bankDeleted', function () {
    console.log("Bank deleted, refresh dropdown");

    // Reload bank dropdown with nothing selected
    loadBankDropdown();
});




$(document).ready(function () {
    //$('#bankBranchFormView').load('/BankBranchInfo/Form');

    $('#bankBranchFormView').load('/BankBranchInfo/Form', function () {
        loadNextBranchId();
        loadBankDropdown();
    });





    // Handle delete button click
    $(document).on("click", ".branchDeleteBtn", function (e) {
        console.log("Clicke Delete button for branch");
        e.preventDefault();
        deleteBranchList();
    });

    // Handle clear button
    $(document).on("click", ".branchResetBtn", function () {
        console.log("Clerar Button Clicked")
        clearBranchForm();
    });

    // Handle print button
    $(".branchPrintBtn").on("click", function (e) {
        e.preventDefault();
        printForm();
    });

    // Handle favorite button
    $(".branchFavBtn").on("click", function () {
        toggleFavorite();
    });


    $(document).on("submit", "#branchForm", function (e) {
        e.preventDefault();
        saveBranch();
    });

    $('#bankBranchListView').load('/BankBranchInfo/List');
    loadBranches(branchCurrentPage, branchPageSize, branchCurrentSortColumn, branchCurrentSortDirection, "", "");

    $(document).on('input', '#branchSearchInput', branchDebounce(function () {
        const searchValue = $('#branchSearchInput').val();
        loadBranches(1, branchPageSize, branchCurrentSortColumn, branchCurrentSortDirection, searchValue);
    }, 500));


    $(document).on('click', '#branchFirstPage', function () {
        branchCurrentPage = 1;
        loadBranches(branchCurrentPage, branchPageSize, branchCurrentSortColumn, branchCurrentSortDirection);
    });

    $(document).on('click', '#branchPrevPage', function () {
        console.log("Clicked on branch page on prev button");
        if (branchCurrentPage > 1) {
            branchCurrentPage--;
            loadBranches(branchCurrentPage, branchPageSize, branchCurrentSortColumn, branchCurrentSortDirection);
        }
    });

    $(document).on('click', '#branchNextPage', function () {
        branchCurrentPage++;
        loadBranches(branchCurrentPage, branchPageSize, branchCurrentSortColumn, branchCurrentSortDirection);
    });

    $(document).on('click', '#branchLastPage', function () {
        const totalPages = parseInt($('#branchPaginationInfo').text().split(' ')[3]) || 1;
        branchCurrentPage = totalPages; 
        loadBranches(branchCurrentPage, branchPageSize, branchCurrentSortColumn, branchCurrentSortDirection, $('#branchSearchInput').val().trim(), $('#bank').val());
    });


    // Change page size
    $(document).on('change', '#branchPageSize', function () {
        branchPageSize = parseInt($(this).val());
        loadBranches(1, branchPageSize, branchCurrentSortColumn, branchCurrentSortDirection); // Reset to the first page on size change
    });

    // Sortable columns click handler
    $(document).on('click', '.branchSortable', function () {
        const column = $(this).data('column');

        if (column === branchCurrentSortColumn) {
            branchCurrentSortDirection = branchCurrentSortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            branchCurrentSortColumn = column;
            branchCurrentSortDirection = 'asc'; 
        }

        branchCurrentPage = 1; 
        loadBranches(branchCurrentPage, branchPageSize, branchCurrentSortColumn, branchCurrentSortDirection, $('#branchSearchInput').val().trim(), $('#bank').val());
    });

    bindBranchCheckboxHandlers();

});

