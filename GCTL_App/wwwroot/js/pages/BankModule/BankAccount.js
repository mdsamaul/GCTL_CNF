let selectedAccounts = [];
let accountCurrentPage = 1;
let accountPageSize = 10;
let accountCurrentSortColumn = 'AccInfoID';
let accountCurrentSortDirection = 'desc';
let accountTotalPages = 1;
console.log("account page")
console.log(`Global Call: ${selectedAccounts}`);


//Getting Next Bank Account ID
function loadNextAccountId() {
    $.ajax({
        url: '/next-account-id', 
        method: 'GET',
        success: function (response) {
            console.log("Next Account ID:", response);
            $('#accountID').val(response);

        },
        error: function (xhr, status, error) {
            console.error("Failed to load next accoun ID:", error);
        }
    });
}
// Load Bank Dropdown with callback
function LoadBankDropDown(selectedId = null, callback = null) {
    $.ajax({
        url: '/bank-info-dropdown-for-account',
        type: 'GET',
        success: function (data) {
            console.log("Bank dropdown data:", data);

            var select = $("#accountBank");
            select.empty();
            select.append('<option value=""></option>');
            $.each(data, function (index, item) {
                const isSelected = selectedId != null && selectedId.toString() === item.bankID.toString() ? 'selected' : '';
                select.append('<option value="' + item.bankID + '" ' + isSelected + '>' + item.bankName + '</option>');
            });
            if (callback && typeof callback === "function") callback();
        }
    });
}

// Load Branch Dropdown by Bank
function LoadBranchDropDown(bankId, selectedId = null) {
    if (!bankId) {
        $("#branch").empty().append('<option value=""></option>');
        return;
    }
    $.ajax({
        url: '/branches-with-banks?id=' + encodeURIComponent(bankId),
        type: 'GET',
        success: function (data) {
            var select = $("#branch");
            select.empty();
            select.append('<option value=""></option>');
            $.each(data, function (index, item) {
                const isSelected = selectedId != null && selectedId.toString() === item.bankBranchID.toString() ? 'selected' : '';
                select.append('<option value="' + item.bankBranchID + '" ' + isSelected + '>' + item.bankBranchName + '</option>');
            });
        }
    });
}


// Save branch
function saveAccount() {
    let autoID = $('#accountAutoID').val();
    let isUpdate = !!autoID;

    let accountData = {
        autoID: $('#accountAutoID').val() ? parseFloat($('#accountAutoID').val()) : null,
        accInfoID: $("#accountID").val(),
        accountName: $("#accountName").val(),
        accountNO: $("#accountNumber").val(),
        bankID: $("#accountBank").val(),       
        branchID: $("#branch").val(),          
        companyCode: $("#companyCode").val(),
        userInfoEmployeeID: $("#userInfoEmployeeID").val(),
    };

    //Validation
    if (!accountData.accountName) {
        toastr.warning("Account Name is required!");
        return; 
    }


    if (!accountData.accountNO) {
        toastr.warning("Account No is required!");
        return;
    }

    //Duplicate Check
    $.ajax({
        url: '/account/check-duplicate',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(accountData),
        success: function (dupResponse) {
            console.log("Duplicate Check Data:", accountData);

            if (dupResponse.isDuplicate && !isUpdate) {
                toastr.warning(dupResponse.message);
                return;
            }

            //Save or Update
            $.ajax({
                url: isUpdate ? `/account-info/${autoID}` : '/account-info',
                type: isUpdate ? 'PUT' : 'POST',
                contentType: 'application/json',
                data: JSON.stringify(accountData),
                success: function (response) {
                    if (response.success) {
                        toastr.success(isUpdate ? "Data Updated Successfully." : "Data Saved Successfully.");
                        clearAccountForm();
                        loadAccounts(1, 10, accountCurrentSortColumn, accountCurrentSortDirection);
                    } else {
                        toastr.error(isUpdate ? " Update Failed." : "Insertion Failed.");
                    }
                },
                error: function () {
                    toastr.error(isUpdate ? "Error occurred while Updating Account!" : "Error occurred while Saving Account!");
                }
            });
        },
        error: function () {
            toastr.error("Error checking duplicate data!");
        }
    });
}

// Clear form 
function clearAccountForm() {
    $("#accountAutoID").val("");
    $("#accountID").val("");
    $("#branch").val("");
    $("#accountBank").val("");
    $("#accountNumber").val("");
    $("#accountName").val("");
    $("#companyCode").val("");
    $("#userInfoEmployeeID").val("");
    $(".form-control").removeClass("is-invalid is-valid");
    $(".invalid-feedback, .valid-feedback").remove();
    $('#accountLDateContainer').hide();
    $('#accountModifyDateContainer').hide();
    $("#branch").next(".error-message").remove();
    $("#accountSearchInput").val("");
    $(".accountrow-checkbox").prop("checked", false);
    $("#accountSelectAll").prop("checked", false);
    loadNextAccountId(); 
    LoadBranchDropDown();
    LoadBankDropDown();
    selectedAccounts = [];
    currentPage = 1;
    loadAccounts(accountCurrentPage, accountPageSize, accountCurrentSortColumn, accountCurrentSortDirection);
}

function populateAccountIDs(IDs) {
    let IDvalue = parseFloat(IDs);
    selectedAccounts.push(IDvalue);
    console.log(`IDvalue: ${IDvalue}`);
    console.log(`Account ID Seelected: ${selectedAccounts}`);
}

// Populate Account Form
function populateAccountForm(data) {
    $('#accountForm input[name="accountAutoID"]').val(data.autoID);
    $('#accountForm input[name="accountID"]').val(data.accInfoID);
    $('#accountForm input[name="accountName"]').val(data.accountName);
    $('#accountForm input[name="accountNumber"]').val(data.accountNO);

    // First load Bank and select correct bank
    LoadBankDropDown(data.bankID, function () {
        // After bank is loaded, load Branch and select correct branch
        LoadBranchDropDown(data.bankID, data.branchID);
    });

    if (data.lDate) {
        const formattedLDate = new Date(data.lDate).toLocaleDateString('en-GB');
        $('#displayAccountLDate').text(formattedLDate);
        $('#accountLDateContainer').show();
    } else {
        $('#accountLDateContainer').hide();
    }

    if (data.modifyDate) {
        const formattedModifyDate = new Date(data.modifyDate).toLocaleDateString('en-GB');
        $('#displayAccountModifyDate').text(formattedModifyDate);
        $('#accountModifyDateContainer').show();
    } else {
        $('#accountModifyDateContainer').hide();
    }
}

$(document).on('click', '.accountBtn-transfer', function () {
    console.log("Account Transfer Button Called!!");

    $("#accountAutoID").val("");
    $("#accountID").val("");
    $("#branch").val("");
    $("#accountBank").val("");
    $("#accountNumber").val("");
    $("#accountName").val("");
    $("#companyCode").val("");
    $("#userInfoEmployeeID").val("");



    const data = {
        autoID: $(this).data('accountautoid'),
        accInfoID: $(this).data('accountid'),
        accountNO: $(this).data('accountnumber'),
        accountName: $(this).data('accountname'),
        bankID: $(this).data('accountbankid'),
        branchID: $(this).data('accountbranchid'),
        lDate: $(this).data('accountldate'),
        modifyDate: $(this).data('accountmodifydate'),

    };
    console.log("Account Form Populated Data", data);
    populateAccountForm(data);
    selectedAccounts = [];
    populateAccountIDs(data.autoID);
});

// Select all checkbox
$(document).on('click', '#accountSelectAll', function () {
    console.log("Account Select All Button Clicked");
    const isChecked = $(this).prop('checked');
    $('.accountrow-checkbox:visible').prop('checked', isChecked);
    console.log('is checked:', isChecked);
    if (isChecked) {
        $('.accountrow-checkbox:visible').each(function () {
            const id = parseFloat($(this).data('autoidofaccount'));
            if (!selectedAccounts.includes(id)) {
                selectedAccounts.push(id);
            }
        });
    } else {
        $('.accountrow-checkbox:visible').each(function () {
            const id = parseFloat($(this).data('autoidofaccount'));
            const index = selectedAccounts.indexOf(id);
            if (index !== -1) {
                selectedAccounts.splice(index, 1);
            }
        });
    }
    console.log("Selected accounts:", selectedAccounts);
});

// Individual checkbox click
function bindAccountCheckboxHandlers() {
    $(document).off('click', '.accountrow-checkbox'); // Unbind previous handlers
    $(document).on('click', '.accountrow-checkbox', function () {
        const id = parseFloat($(this).data('autoidofaccount'));
        const isChecked = $(this).prop('checked');
        console.log("clicked from account row check box", isChecked);
        if (isChecked) {

            if (!selectedAccounts.includes(id)) {
                selectedAccounts.push(id);
            }
        } else {
            const index = selectedAccounts.indexOf(id);
            if (index !== -1) {
                selectedAccounts.splice(index, 1);
            }
        }

        const allChecked = $('.accountrow-checkbox:visible').length > 0 &&
            $('.accountrow-checkbox:visible:not(:checked)').length === 0;
        $('#accountSelectAll').prop('checked', allChecked);

        console.log("Selected accounts from branch:", selectedAccounts);
    });
}

//Loading All Account Info
function loadAccounts(page = 1, pageSize = 10, sortColumn = accountCurrentSortColumn, sortDirection = accountCurrentSortDirection, searchTerm = "", filters = {}) {
    accountCurrentPage = page; 

    let queryParams = {
        pageNumber: page,
        pageSize: pageSize,
        searchTerm: searchTerm,
        sortColumn: sortColumn,
        sortOrder: sortDirection
    };

    if (filters) {
        for (const key in filters) {
            if (filters[key]) {
                queryParams[`filter.${key}`] = filters[key];
            }
        }
    }

    $.ajax({
        url: '/account-list',
        type: 'GET',
        data: queryParams,
        success: function (response) {
            let rows = '';
            response.data.forEach(item => {
                rows += `<tr>
                    <td class="text-center">
                        <input type="checkbox" class="form-check-input form-check-input-sm accountrow-checkbox" style="width: 12px; height: 12px;" data-autoidofaccount="${item.autoID}">
                    </td>
                    <td>
                        <button class="btn accountBtn-transfer"
                            data-accountautoid="${item.autoID}"
                            data-accountbranchid="${item.branchID}"  
                            data-accountnumber="${item.accountNO}"
                            data-accountid="${item.accInfoID}"
                            data-accountname="${item.accountName}"
                            data-accountbankid="${item.bankID}"
                            data-accountbankname="${item.bankName}"
                            data-accountldate="${item.lDate}"
                            data-accountmodifydate="${item.modifyDate}">
                            ${item.accInfoID}
                        </button>
                    </td>
                    <td>${item.accountName || ''}</td>
                    <td>${item.accountNO || ''}</td>
                    <td>${item.bankName || ''}</td>
                    <td>${item.bankBranchName || ''}</td>
                </tr>`;
            });
            $('#accountTblBody').html(rows);

            const totalRecords = response.totalRecords || response.totalCount || response.length;
            const totalPages = Math.ceil(totalRecords / accountPageSize);

            accountCurrentPage = page;
            accountTotalPages = totalPages;

            // Update pagination summary
            const startIndex = (page - 1) * accountPageSize + 1;
            const endIndex = Math.min(page * accountPageSize, totalRecords);
            $('#accountPaginationSummary').text(`Showing ${startIndex} to ${endIndex} of ${totalRecords} entries`);
            $('#accountPaginationInfo').text(`Page ${page} of ${totalPages}`);
            $('#accountTotalRecordsInfo').text(`Total Records: ${totalRecords}`);

            // Generate page buttons
            generateAccountPageButtons(page, totalPages);
        },
        error: function (xhr, status, error) {
            console.error("Error loading data: ", error);
        }
    });
}

function generateAccountPageButtons(currentPage, totalPages) {
    const navigationDiv = $('#accountPageNavigation');
    navigationDiv.empty();

    function createButton(page) {
        const btn = $('<button>')
            .addClass('btn btn-sm mx-1')
            .text(page)
            .click(() => loadAccounts(page, accountPageSize, accountCurrentSortColumn, accountCurrentSortDirection));

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

//delete account list
function deleteAccountList() {
    if (!selectedAccounts || selectedAccounts.length === 0) {
        toastr.warning("No data selected to delete!");
        return;
    }

    let idsToDelete = [...selectedAccounts];
    let ids = JSON.stringify(idsToDelete);
    console.log(`IDs to be deleted: ${ids}`);

    $.ajax({
        url: '/account-info-list', 
        type: 'DELETE',
        data: ids,
        contentType: 'application/json',
        success: function (response) {
            if (response.isSuccess) {
                toastr.success(response.message); 
            } else {
                toastr.error(response.message);   
            }
            clearAccountForm();
            loadAccounts(accountCurrentPage, accountPageSize, accountCurrentSortColumn, accountCurrentSortDirection);
            selectedAccounts = [];
            console.log(`Selected accounts after deletion: ${selectedAccounts}`);
        },
        error: function (xhr, status, error) {
            let errorMessage = "An error occurred while deleting accounts!";
            if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            }
            toastr.error(errorMessage);
            console.error('Error deleting accounts:', errorMessage);
        }
    });
}

// Debounce function to delay the execution of a given function
function accountDebounce(func, delay) {
    let timer;
    return function () {
        const context = this, args = arguments;
        clearTimeout(timer);
        timer = setTimeout(() => func.apply(context, args), delay);
    };
}

// Update sort icons for Account table
function updateAccountSortIndicators() {
    $(".accountSortable").each(function () {
        let column = $(this).data("column");
        let icon = $(this).find(".sort-icon");

        icon.removeClass("fa-sort-up fa-sort-down").addClass("fa-sort");
        if (column === accountCurrentSortColumn) {
            icon.removeClass("fa-sort");
            icon.addClass(accountCurrentSortDirection === "asc" ? "fa-sort-up" : "fa-sort-down");
        }
    });
}

// Handle column header click for Account sorting
$(document).on('click', '.sortable', function () {
    const column = $(this).data('column');

    if (column === accountCurrentSortColumn) {
        accountCurrentSortDirection = accountCurrentSortDirection === 'asc' ? 'desc' : 'asc';
    } else {
        accountCurrentSortColumn = column;
        accountCurrentSortDirection = 'asc';
    }

    accountCurrentPage = 1; 

    loadAccounts(accountCurrentPage, accountPageSize, accountCurrentSortColumn, accountCurrentSortDirection);
    updateAccountSortIndicators();
});

function initAccountBankEvents() {
    // Bank Updated listener
    window.addEventListener('bankUpdated', function (e) {
        console.log("Bank Updated Event Received for Account Page", e.detail);
        LoadBankDropDown(); 
    });

    // Bank Deleted listener
    window.addEventListener('bankDeleted', function () {
        console.log("Bank Deleted Event Received for Account Page");
        LoadBankDropDown(); 
    });
}




$(document).ready(function () {
    initAccountBankEvents();
    //$('#accountFormView').load('/BankAccountInfo/Form');
    $('#accountFormView').load('/BankAccountInfo/Form', function () {
        loadNextAccountId();  
        LoadBankDropDown();
        LoadBranchDropDown();
    });


    bindAccountCheckboxHandlers();
    //loadNextAccountId();

    $(document).on('change', '#accountBank', function () {
        LoadBranchDropDown($('#accountBank').val());
        console.log("Called funtion", $('#accountBank').val())
    });

    $(document).on("focus", "#branch", function () {
        let bankSelectValue = $('#accountBank').val();

        // Remove previous error message
        $(this).next(".error-message").remove();

        if (!bankSelectValue || bankSelectValue.trim() === "") {
            $(this).after('<span class="error-message" style="color: red;">Please Select a Bank before Branch!</span>');
        }


    });
    // Remove error message when user selects a bank
    $(document).on("change", "#accountBank", function () {
        $("#branch").next(".error-message").remove();
    });


    // Handle delete button click
    $(document).on("click", ".accountDeleteBtn", function (e) {
        e.preventDefault();
        deleteAccountList();
    });

    // Handle clear button
    $(document).on("click", ".accountResetBtn", function () {
        console.log("Clerar Button Clicked")
        clearAccountForm();
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

    $(document).on('click', "#accountSaveBtn", function (e) {
        e.preventDefault();
        saveAccount();
    });


    $(document).on("submit", "#accountForm", function (e) {
        e.preventDefault();
        saveAccount();
    });


    $('#accountListView').load('/BankAccountInfo/List');

    loadAccounts(accountCurrentPage, accountPageSize, accountCurrentSortColumn, accountCurrentSortDirection);

    $(document).on('input', '#accountSearchInput', accountDebounce(function () {
        const searchValue = $('#accountSearchInput').val();
        loadAccounts(1, accountPageSize, accountCurrentSortColumn, accountCurrentSortDirection, searchValue); 
    }, 500)); 


    $(document).on('change', '#accountBank, #branch', function () {
        let bankIdValue = $('#accountBank').val();
        let branchValue = $('#branch').val();
        console.log("Call from account to get branch and bank :", bankIdValue);

        loadAccounts(accountCurrentPage = 1, accountPageSize = 10, accountCurrentSortColumn = accountCurrentSortColumn, accountCurrentSortDirection = accountCurrentSortDirection, searchTerm = "", filters = { bankID: bankIdValue, bankBranchID: branchValue });

    });

    // First Page
    $(document).on('click', '#accountFirstPage', function () {
        if (accountCurrentPage !== 1) {
            loadAccounts(1, accountPageSize, accountCurrentSortColumn, accountCurrentSortDirection);
        }
    });

    // Previous Page
    $(document).on('click', '#accountPrevPage', function () {
        if (accountCurrentPage > 1) {
            loadAccounts(accountCurrentPage - 1, accountPageSize, accountCurrentSortColumn, accountCurrentSortDirection);
        }
    });

    // Next Page
    $(document).on('click', '#accountNextPage', function () {
        if (accountCurrentPage < accountTotalPages) {
            loadAccounts(accountCurrentPage + 1, accountPageSize, accountCurrentSortColumn, accountCurrentSortDirection);
        }
    });

    // Last Page
    $(document).on('click', '#accountLastPage', function () {
        if (accountCurrentPage !== accountTotalPages) {
            loadAccounts(accountTotalPages, accountPageSize, accountCurrentSortColumn, accountCurrentSortDirection);
        }
    });

    // Change page size
    $(document).on('change', '#accountPageSize', function () {
        accountPageSize = parseInt($(this).val());
        loadAccounts(1, accountPageSize, accountCurrentSortColumn, accountCurrentSortDirection); // Reset to the first page on size change
    });

});


