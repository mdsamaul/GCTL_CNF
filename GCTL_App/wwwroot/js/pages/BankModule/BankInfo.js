let selectedBanks = [];
let currentPage = 1;
let pageSize = 10;
let currentSortColumn = 'BankID';
let currentSortDirection = 'desc';
console.log("bank page")
console.log(`Global Call: ${selectedBanks}`);

function loadNextBankId() {
    $.ajax({
        url: '/next-bank-id',
        method: 'GET',
        success: function (response) {
            $('#bankID').val(response);

        },
        error: function (xhr, status, error) {
            console.error("Failed to load next Bank ID:", error);
        }
    });
}

//Details population
$(document).on('click', '.btn-transfer', function () {
    const data = {
        autoID: $(this).data('autoid'),
        bankID: $(this).data('bankid'),
        bankName: $(this).data('bankname'),
        shortName: $(this).data('shortname'),
        lDate: $(this).data('ldate'),
        modifyDate: $(this).data('modifydate')
    };
    console.log("Populate data: ", data);
    populateBankForm(data);
    selectedBanks = [parseInt(data.autoID)];
    console.log("Selected Banks after populate:", selectedBanks);
});


// Select all checkbox
$(document).on('click', '#selectAll', function () {
    const isChecked = $(this).prop('checked');
    $('.row-checkbox:visible').prop('checked', isChecked);

    if (isChecked) {
        $('.row-checkbox:visible').each(function () {
            const id = parseFloat($(this).data('autoid'));
            if (!selectedBanks.includes(id)) {
                selectedBanks.push(id);
            }
        });
    } else {
        $('.row-checkbox:visible').each(function () {
            const id = parseFloat($(this).data('autoid'));
            const index = selectedBanks.indexOf(id);
            if (index !== -1) {
                selectedBanks.splice(index, 1);
            }
        });
    }
    console.log("Selected Banks:", selectedBanks);
});

// Save Banks
function saveBank() {
    console.log("Save Bank called");

    let autoID = $('#autoID').val();
    let bankData = {
        autoID: autoID || 0, 
        bankID: $("#bankID").val(),
        bankName: $("#bankName").val(),
        shortName: $("#shortName").val()
    };

    //validation
    if (!bankData.bankName) {
        toastr.error("Please enter Bank Name");
        return false;
    }


    //Check for duplicate
    $.ajax({
        url: '/bank/check-duplicate',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(bankData),
        success: function (dupResponse) {
            if (dupResponse.isDuplicate) {
                toastr.error(dupResponse.message);
                return;
            }

            //Decide if POST or PUT
            let url = '/bank-info';
            let method = 'POST';
            let successMessage = 'Data Saved Successfully';
            if (autoID) {
                url = `/bank-info/${autoID}`;
                method = 'PUT';
                successMessage = 'Data Updated Successfully';
            }

            // Step 3: Save or Update
            $.ajax({
                url: url,
                type: method,
                contentType: 'application/json',
                data: JSON.stringify(bankData),
                success: function (response) {
                    if (response.success) {
                        toastr.success(successMessage);

                        // Trigger a custom event with the new bank ID
                        const bankIdToUse = response.newBankID || bankData.bankID;
                        const event = new CustomEvent('bankUpdated', { detail: { newBankID: bankIdToUse } });

                        // Main window এ dispatch (modal/iframe  parent)
                        if (window.parent) {
                            window.parent.dispatchEvent(event);
                        } else {
                            window.dispatchEvent(event);
                        }

                        clearForm();
                        selectedBanks = [];
                        loadBanks(1, 10, currentSortColumn, currentSortDirection); 
                    } else {
                        toastr.error(response.message || "Operation failed!");
                        console.error('Error saving Bank:', response);
                    }
                },
                error: function (xhr, status, error) {
                    toastr.error("Server error occurred!");
                    console.error('Error saving Bank:', error);
                }
            });
        },
        error: function (xhr, status, error) {
            toastr.error("Server error occurred during duplicate check!");
            console.error('Error checking duplicate:', error);
        }
    });
}
function clearForm() {
    $("#autoID").val("");
    $("#bankID").val("");
    $("#bankName").val("");
    $("#shortName").val("");
    $(".form-control").removeClass("is-invalid is-valid");
    $(".invalid-feedback, .valid-feedback").remove();
    $('#modifyDateContainer').hide();
    $('#lDateContainer').hide();
    $(".row-checkbox").prop("checked", false);
    $("#selectAll").prop("checked", false);
    $('#searchInput').val('');
    loadNextBankId(); 
    selectedBanks = [];
    currentPage = 1;
    const emptySearch = '';
    loadBanks(1, pageSize, currentSortColumn, currentSortDirection, emptySearch);
}
function loadBanks(page = 1, pageSize = 10, sortColumn = currentSortColumn, sortDirection = currentSortDirection, searchTerm = "") {

    $.ajax({
        url: '/bank-list',
        type: 'GET',
        data: {
            pageNumber: page,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortDirection
        },
        success: function (response) {
            console.log("Bank Info Data:", response);
            let rows = '';
            response.data.forEach(function (item) {
                rows += `<tr>
                    <td class="text-center">
                        <input type="checkbox" class="form-check-input form-check-input-sm row-checkbox"
                            data-autoid="${item.autoID}" style="width:12px; height:12px;">
                    </td>
                    <td>
                            <button class="btn btn-transfer"
                                data-autoid="${item.autoID}"
                                data-bankid="${item.bankID}"
                                data-bankname="${item.bankName}"
                                data-shortname="${item.shortName || ''}"
                                data-ldate="${item.lDate || ''}"
                                data-modifydate="${item.modifyDate || ''}">
                                ${item.bankID}
                            </button>
                    </td>
                    <td>${item.bankName || ''}</td>
                    <td>${item.shortName || ''}</td>
                 </tr>`;
            });
            $('#tblBody').html(rows);

            // Pagination Info
            let page = response.paginationInfo.currentPage;
            let totalPages = response.paginationInfo.totalPages;
            let totalRecords = response.totalCount;
            let startIndex = response.paginationInfo.startItem;
            let endIndex = response.paginationInfo.endItem;

            $('#paginationSummary').text(`Showing ${startIndex} to ${endIndex} of ${totalRecords} entries`);
            $('#paginationInfo').text(`Page ${page} of ${Math.max(1, totalPages)}`);
            $('#totalRecordsInfo').text(`Total Records: ${totalRecords}`);

            currentPage = page;
            generatePageButtons(currentPage, totalPages);
            updateCheckboxState();
            updateSortIndicators();
        },
        error: function (xhr, status, error) {
            console.error("Error loading data: ", error);

        }
    });
}
function updateSortIndicators() {
    $('.sortable').removeClass('sort-asc sort-desc');
    $('.sortable .sort-icon').removeClass('fa-sort-up fa-sort-down').addClass('fa-sort');

    const header = $(`.sortable[data-column="${currentSortColumn}"]`);
    header.addClass(currentSortDirection === 'asc' ? 'sort-asc' : 'sort-desc');

    const icon = header.find('.sort-icon');
    icon.removeClass('fa-sort fa-sort-up fa-sort-down');
    icon.addClass(currentSortDirection === 'asc' ? 'fa-sort-up' : 'fa-sort-down');
}
function generatePageButtons(currentPage, totalPages) {
    const navigationDiv = $('#pageNavigation');
    navigationDiv.empty();

    function createButton(page) {
        const btn = $('<button>')
            .addClass('btn btn-sm mx-1')
            .text(page)
            .click(() => loadBanks(page, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim()));

        if (page === currentPage) {
            btn.removeClass('btn-outline-primary').addClass('btn-primary');
        } else {
            btn.addClass('btn-outline-primary');
        }
        return btn;
    }

    // Always show first page
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
function updateCheckboxState() {
    $('.row-checkbox').each(function () {
        const bankID = $(this).data('autoid');
        $(this).prop('checked', selectedBanks.includes(bankID));
    });
    const allChecked = $('.row-checkbox:visible').length > 0 &&
        $('.row-checkbox:visible:not(:checked)').length === 0;
    $('#selectAll').prop('checked', allChecked);
}
function debounce(func, delay) {
    let timer;
    return function () {
        const context = this, args = arguments;
        clearTimeout(timer);
        timer = setTimeout(() => func.apply(context, args), delay);
    };
}


function populateBankForm(data) {
    $('#bankForm input[name="bankID"]').val(data.bankID);
    $('#bankForm input[name="bankName"]').val(data.bankName);
    $('#bankForm input[name="shortName"]').val(data.shortName);
    $('#bankForm input[name="autoID"]').val(data.autoID);
    if (data.lDate) {
        const formattedLDate = new Date(data.lDate).toLocaleDateString('en-GB');
        $('#displayLDate').text(formattedLDate);
        $('#lDateContainer').show(); 
    } else {
        $('#lDateContainer').hide(); 
    }
    if (data.modifyDate) {
        const formattedModifyDate = new Date(data.modifyDate).toLocaleDateString('en-GB');
        $('#displayModifyDate').text(formattedModifyDate);
        $('#modifyDateContainer').show(); 
    } else {
        $('#modifyDateContainer').hide(); 
    }
}
function bindCheckboxHandlers() {
    $(document).off('click', '.row-checkbox'); 
    $(document).on('click', '.row-checkbox', function () {
        const id = parseInt($(this).data('autoid'));
        const isChecked = $(this).prop('checked');

        if (isChecked) {
            if (!selectedBanks.includes(id)) {
                selectedBanks.push(id);
            }
        } else {
            const index = selectedBanks.indexOf(id);
            if (index !== -1) {
                selectedBanks.splice(index, 1);
            }
        }

        const allChecked = $('.row-checkbox:visible').length > 0 &&
            $('.row-checkbox:visible:not(:checked)').length === 0;
        $('#selectAll').prop('checked', allChecked);

        console.log("Selected expense heads:", selectedBanks);
    });
}
function deleteBankList() {
    let idsToDelete = [...selectedBanks];
    let ids = JSON.stringify(idsToDelete);
    console.log(`IDs to be deleted: ${ids}`);

    $.ajax({
        url: '/bank-info-list',
        type: 'DELETE',
        data: ids,
        contentType: 'application/json',
        success: function (response) {

            const event = new CustomEvent('bankDeleted');
            window.dispatchEvent(event);

            selectedBanks = [];
            clearForm();
            loadBanks(currentPage, pageSize, currentSortColumn, currentSortDirection);
     
            console.log(`call form delete method:${selectedBanks}`);

            if (response.isSuccess) {
                toastr.success(response.message || "Data deleted successfully!");
            } else {
                toastr.error(response.message || "Failed to delete banks!");
            }
        },
        error: function (error) {
            isProcessingDelete = false;

            let errorMessage = "An error occurred while deleting Bank!";
            if (error.responseJSON && error.responseJSON.message) {
                errorMessage = error.responseJSON.message;
            }
            toastr.error(errorMessage);
            console.error('Error deleting Banks:', error);
        }
    });
}


$(document).ready(function () {
    $('#bankFormView').load('/BankInfo/Form', function () {
        loadNextBankId();
    });


    $(document).on("click", ".deleteBtn", function (e) {
        e.preventDefault();
        deleteBankList();
    });

    $(document).on("click", ".resetBtn", function () {
        console.log("Clerar Button Clicked")
        clearForm();
    });

    // Handle print button
    $(".printBtn").on("click", function (e) {
        e.preventDefault();
        printForm();
    });

    // Handle favorite button
    $(".favBtn").on("click", function () {
        toggleFavorite();
    });



    $(document).on("submit", "#bankForm", function (e) {
        e.preventDefault();
        saveBank();
    });

    $('#bankListView').load('/BankInfo/List', function () {
        loadBanks(currentPage, pageSize, currentSortColumn, currentSortDirection);
    });


    $(document).on('input', '.searchInput', debounce(function () {
        const searchValue = $('.searchInput').val();
        loadBanks(1, pageSize, currentSortColumn, currentSortDirection, searchValue); 
    }, 500)); 

    $(document).on('click', '#firstPage', function () {
        currentPage = 1;
        loadBanks(currentPage, pageSize, currentSortColumn, currentSortDirection);
    });

    $(document).on('click', '#prevPage', function () {
        console.log("previous page called on bank page ");
        if (currentPage > 1) {
            currentPage--;
            loadBanks(currentPage, pageSize, currentSortColumn, currentSortDirection);
        }
    });

    $(document).on('click', '#nextPage', function () {
        currentPage++;
        loadBanks(currentPage, pageSize, currentSortColumn, currentSortDirection);
    });

    $(document).on('click', '#lastPage', function () {
        let lastPage = $('#pageNavigation button').last().text();
        lastPage = parseInt(lastPage);
        if (!isNaN(lastPage)) {
            currentPage = lastPage;
            loadBanks(currentPage, pageSize, currentSortColumn, currentSortDirection);
        }
    });

    $(document).on('change', '.pageSize', function () {
        pageSize = parseInt($(this).val());
        loadBanks(1, pageSize, currentSortColumn, currentSortDirection); 
    });

    $(document).on('click', '.sortable', function () {
        const column = $(this).data('column');

        if (column === currentSortColumn) {
            currentSortDirection = (currentSortDirection === 'desc') ? 'asc' : 'desc';
        } else {
            currentSortColumn = column;
            currentSortDirection = 'desc';
        }

        loadBanks(1, pageSize, currentSortColumn, currentSortDirection, $('#searchInput').val().trim());
    });

    bindCheckboxHandlers();

});

