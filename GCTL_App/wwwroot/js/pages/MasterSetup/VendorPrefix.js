let selectedVendorPrefixes = [];
let currentPage = 1;
let pageSize = 10;
let currentSortColumn = 'VendorPrifixId';
let currentSortDirection = 'desc';
let totalPages = 1;
console.log("Vendor Prefix page");
console.log(`Global Call: ${selectedVendorPrefixes}`);

// Functions Section
function loadNextVendorPrefixId() {
    $.ajax({
        url: '/next-vendor-prefix-id',
        method: 'GET',
        success: function (response) {
            $('#vendorPrefixID').val(response);
        },
        error: function (xhr, status, error) {
            console.error("Failed to load next vendor prefix ID:", error);
        }
    });
}
function populateVendorPrefixForm(data) {
    console.log("From Vendor Prefix Form Population Function, data are:", data);
    $('#vendorPrefixForm input[name="hiddenID"]').val(data.tc);
    $('#vendorPrefixForm input[name="vendorPrefixID"]').val(data.vendorPrifixID);
    $('#vendorPrefixForm input[name="prefixName"]').val(data.prifixName);
    $('#vendorPrefixForm input[name="shortName"]').val(data.shortName);

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
function clearForm() {
    $("#vendorPrefixForm")[0].reset();
    $("#hiddenID").val("");
    $("#vendorPrefixID").val("");
    $("#prefixName").val("");
    $("#shortName").val("");
    $("#searchInput").val("");  
    $(".form-control").removeClass("is-invalid is-valid");
    $(".invalid-feedback, .valid-feedback").remove();
    $('#modifyDateContainer').hide();
    $('#lDateContainer').hide();

    selectedVendorPrefixes = [];
    $('.row-checkbox').prop('checked', false);
    $('#selectAll').prop('checked', false);

    loadNextVendorPrefixId();
    currentPage = 1;
    const emptySearch = '';
    loadVendorPrefixes(1, pageSize, currentSortColumn, currentSortDirection, emptySearch);
}


function loadVendorPrefixes(page = 1, pageSize = 10, sortColumn = currentSortColumn, sortDirection = currentSortDirection, searchTerm = "") {
    currentPage = page;  

    $.ajax({
        url: '/vendor-prefix-list',
        type: 'GET',
        data: {
            pageNumber: page,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortDirection
        },
        success: function (response) {
            console.log('Vendor Prefix Table', response);
            let rows = '';

            if (!response.data || response.data.length === 0) {
                rows = `<tr class="no-data">
                            <td colspan="4" class="text-center">No Data Available</td>
                        </tr>`;
            }
            else {
                response.data.forEach(function (item) {
                    rows += `<tr>
                                <td class="text-center">
                                    <input type="checkbox" class="form-check-input form-check-input-sm row-checkbox" style="width: 12px; height: 12px;" data-tcidcheck="${item.tc}">
                                </td>
                                <td>
                                    <button class="btn btn-transfer"
                                        data-tcidbtn="${item.tc}"
                                        data-vendorprifixid="${item.vendorPrifixID}"
                                        data-prifixname="${item.prifixName}"
                                        data-shortname="${item.shortName}"
                                        data-ldate="${item.lDate}"
                                        data-modifydate="${item.modifyDate}">
                                        ${item.vendorPrifixID}
                                    </button>
                                </td>
                                <td>${item.prifixName || ''}</td>
                                <td>${item.shortName || ''}</td>
                            </tr>`;
                });
            } 

            $('#tblBody').html(rows);
            // pagination info 
            const startIndex = response.paginationInfo?.startItem || 0;
            const endIndex = response.paginationInfo?.endItem || 0;
            const totalRecords = response.paginationInfo?.totalItems || 0;
            totalPages = pageSize === -1 ? 1 : Math.ceil(totalRecords / pageSize);

            $('#paginationInfo').text(`Page ${currentPage} of ${totalPages}`);
            $('#paginationSummary').text(`Showing ${startIndex} to ${endIndex} Items of ${totalRecords}`);
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

function updateSortIndicators() {
    $('.sortable').removeClass('sort-asc sort-desc');
    $('.sortable .sort-icon').removeClass('fa-sort-up fa-sort-down').addClass('fa-sort');
    $(`.sortable[data-column="${currentSortColumn}"]`).addClass(`sort-${currentSortDirection}`);
    if (currentSortDirection === 'asc') {
        $(`.sortable[data-column="${currentSortColumn}"] .sort-icon`).removeClass('fa-sort fa-sort-down').addClass('fa-sort-up');
    } else {
        $(`.sortable[data-column="${currentSortColumn}"] .sort-icon`).removeClass('fa-sort fa-sort-up').addClass('fa-sort-down');
    }
}

function generatePageButtons(currentPage, totalPages) {
    const navigationDiv = $('#pageNavigation');
    navigationDiv.empty();

    function createButton(page) {
        const btn = $('<button>')
            .addClass('btn btn-sm mx-1')
            .text(page)
            .click(() => loadVendorPrefixes(page, pageSize, currentSortColumn, currentSortDirection));

        if (page === currentPage) {
            btn.removeClass('btn-outline-primary').addClass('btn-primary');
        } else {
            btn.addClass('btn-outline-primary');
        }
        return btn;
    }

    // Always show first page
    navigationDiv.append(createButton(1));

    // Show "..." if currentPage is not near start
    if (currentPage > 2) {
        navigationDiv.append($('<span>').text('...').css('padding', '0 5px'));
    }

    // Show middle (current page), only if it's not 1 or last
    if (currentPage !== 1 && currentPage !== totalPages) {
        navigationDiv.append(createButton(currentPage));
    }

    // Show "..." if currentPage is not near end
    if (currentPage < totalPages - 1) {
        navigationDiv.append($('<span>').text('...').css('padding', '0 5px'));
    }

    // Always show last page if more than 1
    if (totalPages > 1) {
        navigationDiv.append(createButton(totalPages));
    }

 
}

function updateCheckboxState() {
    $('.row-checkbox').each(function () {
        const vendorPrefixID = $(this).data('tcidcheck');
        $(this).prop('checked', selectedVendorPrefixes.includes(vendorPrefixID));
    });

    const allChecked = $('.row-checkbox:visible').length > 0 && $('.row-checkbox:visible:not(:checked)').length === 0;
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

function populateVendorPrefixIDs(IDs) {
    let IDvalue = IDs;
    selectedVendorPrefixes.push(IDvalue);
    console.log(`IDvalue: ${IDvalue}`);
    console.log(`Call From ${selectedVendorPrefixes}`);
}

function bindCheckboxHandlers() {
    $(document).off('click', '.row-checkbox');
    $(document).on('click', '.row-checkbox', function () {
        console.log("Checkbox HTML:", $(this).prop('outerHTML'));

        const id = parseInt($(this).data('tcidcheck'));
        const isChecked = $(this).prop('checked');

        if (isChecked) {
            if (!selectedVendorPrefixes.includes(id)) {
                selectedVendorPrefixes.push(id);
            }
        } else {
            const index = selectedVendorPrefixes.indexOf(id);
            if (index !== -1) {
                selectedVendorPrefixes.splice(index, 1);
            }
        }

        const allChecked = $('.row-checkbox:visible').length > 0 && $('.row-checkbox:visible:not(:checked)').length === 0;
        $('#selectAll').prop('checked', allChecked);
        console.log("Selected vendor prefixes:", selectedVendorPrefixes);
    });
}

function deleteVendorPrefixList() {
    let idsToDelete = [...selectedVendorPrefixes];
    let ids = JSON.stringify(idsToDelete);
    console.log(`IDs to be deleted: ${ids}`);

    $.ajax({
        url: '/vendor-prefix-list',
        type: 'DELETE',
        data: JSON.stringify(idsToDelete),
        processData: false,
        contentType: 'application/json',
        success: function (response) {
            clearForm();
            loadVendorPrefixes(currentPage, pageSize, currentSortColumn, currentSortDirection);
            selectedVendorPrefixes = [];
            console.log(`call from delete method: ${selectedVendorPrefixes}`);
            if (response && response.isSuccess) {
                toastr.success(response.message);
            }
            else {
                toastr.error(response.message || "Deleted Failed");
            }
        },
        error: function (error) {
            let errorMessage = "An error occurred while deleting vendor prefix!";
            if (error.responseJSON && error.responseJSON.message) {
                errorMessage = error.responseJSON.message;
            }
            console.error('Error deleting vendor prefixes:', error);
        }
    });
}

function checkForDuplicateVendorPrefix(vendorPrefixData, callback) {
    $.ajax({
        url: '/VendorPrefix/CheckDuplicate',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(vendorPrefixData),
        success: function (response) {
            if (response.isDuplicate) {
                toastr.error('Data already exists!');
                callback(true);
            } else {
                callback(false);
            }
        },
        error: function (err) {
            toastr.error('Error checking for duplicate Vendor Prefix');
            callback(true);
        }
    });
}

function saveVendorPrefix() {
    console.log("Save VendorPrefix called");

    let hiddenID = $("#hiddenID").val();
    let isEdit = hiddenID && hiddenID !== "0";

    let vendorPrefixData = {
        TC: isEdit ? parseInt(hiddenID) : 0,
        VendorPrifixID: $("#vendorPrefixID").val(),
        PrifixName: $("#prefixName").val().trim(),
        ShortName: $("#shortName").val()
    };
    if (!vendorPrefixData.PrifixName) {
        toastr.error('Please enter Prefix Name.');
        $('#prefixName').addClass('error-border').focus();
        return;
    }

    //Duplicate Check
    checkForDuplicateVendorPrefix(vendorPrefixData, function (isDuplicate) {
        if (isDuplicate) {
            return;
        }
        //  Proceed to Save or Update
        let url = '/vendor-prefix';
        let method = 'POST';
        let successMessage = 'Data Saved Successfully';

        if (isEdit) {
            url = `/vendor-prefix/${hiddenID}`;
            method = 'PUT';
            successMessage = 'Data Updated Successfully';
        }

        $.ajax({
            url: url,
            type: method,
            contentType: 'application/json',
            data: JSON.stringify(vendorPrefixData),
            success: function (response) {
                if (response.success) {
                    toastr.success(successMessage);
                    clearForm();
                    loadVendorPrefixes(1, 10, currentSortColumn, currentSortDirection);
                } else {
                    toastr.error(response.message || 'Insertion Failed.');
                }
            },
            error: function (xhr, status, error) {
                console.error('Error saving vendor prefix:', error);
                toastr.error('Failed to save Vendor Prefix.');
            }
        });
    });
}

// Document Ready
$(document).ready(function () {
    loadNextVendorPrefixId();
    loadVendorPrefixes(currentPage, pageSize, currentSortColumn, currentSortDirection);

    $(document).on("submit", "#vendorPrefixForm", function (e) {
        e.preventDefault();
        saveVendorPrefix(e);
    });

    $(document).on("click", ".deleteBtn", function (e) {
        e.preventDefault();
        deleteVendorPrefixList();
    });

    $(document).on("click", ".resetBtn", function () {
        console.log("Clear Button Clicked");
        clearForm();
    });

    $(document).on('click', '.btn-transfer', function () {
        const data = {
            tc: $(this).data('tcidbtn'),
            vendorPrifixID: $(this).data('vendorprifixid'),
            prifixName: $(this).data('prifixname'),
            shortName: $(this).data('shortname'),
            lDate: $(this).data('ldate'),
            modifyDate: $(this).data('modifydate')
        };
        console.log("Populated Data .......:", data);
        populateVendorPrefixForm(data);
        selectedVendorPrefixes = [];
        populateVendorPrefixIDs(data.tc);
    });

    $(document).on('click', '#selectAll', function () {
        const isChecked = $(this).prop('checked');
        $('.row-checkbox:visible').prop('checked', isChecked);

        if (isChecked) {
            $('.row-checkbox:visible').each(function () {
                const id = $(this).data('tcidcheck');
                if (!selectedVendorPrefixes.includes(id)) {
                    selectedVendorPrefixes.push(id);
                }
            });
        } else {
            $('.row-checkbox:visible').each(function () {
                const id = parseFloat($(this).data('tcidcheck'));
                const index = selectedVendorPrefixes.indexOf(id);
                if (index !== -1) {
                    selectedVendorPrefixes.splice(index, 1);
                }
            });
        }
        console.log("Selected Vendor Prefixes:", selectedVendorPrefixes);
    });

    $(document).on('input', '.searchInput', debounce(function () {
        const searchValue = $(this).val();
        loadVendorPrefixes(1, pageSize, currentSortColumn, currentSortDirection, searchValue);
    }, 500));

    $(document).on('click', '#firstPage', function () {
        console.log("First page Called");
        currentPage = 1;
        loadVendorPrefixes(currentPage, pageSize, currentSortColumn, currentSortDirection);
    });

    $(document).on('click', '#prevPage', function () {
        console.log("previous page Called");
        if (currentPage > 1) {
            currentPage--;
            loadVendorPrefixes(currentPage, pageSize, currentSortColumn, currentSortDirection);
        }
    });

    $(document).on('click', '#nextPage', function () {
        console.log("Next page button clicked");
        console.log("Current Page before increment:", currentPage);
        console.log("Total Pages:", totalPages);

        if (currentPage < totalPages) {
            currentPage++;
            console.log("Loading page:", currentPage);
            loadVendorPrefixes(currentPage, pageSize, currentSortColumn, currentSortDirection);
        } else {
            console.log("Already on last page");
        }
    });

    $(document).on('click', '#lastPage', function () {
        console.log("last page Called");
        if (currentPage !== totalPages) {  
            currentPage = totalPages;
            loadVendorPrefixes(currentPage, pageSize, currentSortColumn, currentSortDirection);
        }
    });

    $(document).on('change', '.pageSize', function () {
        pageSize = parseInt($(this).val());
        loadVendorPrefixes(1, pageSize, currentSortColumn, currentSortDirection);
    });

    $(document).on('click', '.dropdown-item-size', function (e) {
        e.preventDefault();
        pageSize = parseInt($(this).data('size'));
        $('#pageSize').text(pageSize === -1 ? 'All' : pageSize);
        loadVendorPrefixes(1, pageSize, currentSortColumn, currentSortDirection);
    });

    $(document).on('click', '.sortable', function () {
        const column = $(this).data('column');
        if (column === currentSortColumn) {
            currentSortDirection = currentSortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            currentSortColumn = column;
            currentSortDirection = 'asc';
        }
        loadVendorPrefixes(1, pageSize, currentSortColumn, currentSortDirection);
    });

    bindCheckboxHandlers();


});


//$(document).on('change', '#prefixName', function () {
//    let prefixNameValue = $('#prefixName').val();
//    $('#searchInput').val(prefixNameValue);
//    console.log(`Search Value: ${$('#searchInput').val()}`);
//    loadVendorPrefixes(page = 1, pageSize = 10, sortColumn = currentSortColumn, sortDirection = currentSortDirection, searchTerm = prefixNameValue);
//});
//$(document).on('click', '#prevPageBtn', function () {
//    if (currentPage > 1) {
//        currentPage--;
//        loadVendorPrefixes(currentPage, pageSize, currentSortColumn, currentSortDirection);
//    }
//});

//$(document).on('click', '#nextPageBtn', function () {
//    if (currentPage < totalPages) {
//        currentPage++;
//        loadVendorPrefixes(currentPage, pageSize, currentSortColumn, currentSortDirection);
//    }
//});