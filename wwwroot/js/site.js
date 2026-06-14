// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Auto-initialize when DOM is ready
$(document).ready(function () {
    // Initialize product filters if elements exist on page
    if ($('#searchInput').length > 0 || $('#filterButton').length > 0) {
        initializeProductFilters();
    }

    // Initialize supplier filters if elements exist on page
    if ($('#searchInputSupplier').length > 0) {
        initializeSupplierFilters();
    }
    if ($("#selectSupplier").length) {
        $.ajax({
            url: '/Suppliers/GetAllSuppliers',
            type: 'GET',
            dataType: 'json',
            success: function (data) {

                let supplierDropDown = $('#selectSupplier');
               

                data.forEach(function (supplier) {
                    supplierDropDown.append(
                        `<option value="${supplier.id}">${supplier.supplierName}</option>`
                    );
                });
            },
            error: function () {
                console.error('Failed to fetch suppliers.');
            }
        });
    }


    let productIndex = 0;
    let allProducts = null; // cache products so we can filter selected items

    $('#addProductbtn').on('click', function () {
        const index = productIndex++;

        const newFields = `
            <div class="product-row" data-index="${index}">
                <div class="col-md-5 mt-3">
                    <label class="form-label fw-semibold">Product Name</label>
                    <select class="form-select productDropdown" name="PurchaseProducts[${index}].ProductId" data-index="${index}" required>
                        <option value="">Loading...</option>
                    </select>
                </div>
                <div class="col-md-2 mt-3">
                    <label class="form-label fw-semibold">Quantity</label>
                    <input type="number" class="form-control quantity-input" name="PurchaseProducts[${index}].Quantity" placeholder="0" min="1" data-index="${index}" required />
                </div>
                <div class="col-md-3 mt-3">
                    <label class="form-label fw-semibold">Unit Cost Price</label>
                    <div class="input-group">
                    <span class="input-group-text">Rs</span>
                    <input type="number" class="form-control costprice-input" name="PurchaseProducts[${index}].CostPrice" placeholder="0.00" min="0" step="0.01" data-index="${index}" required />
                
                    </div>
                </div>
                <div class="col-md-2 mt-3 d-flex align-items-end">
                    <button type="button" class="btn btn-danger w-100 remove-product-btn" data-index="${index}">
                        <i class="bi bi-trash"></i> Remove
                    </button>
                </div>
            </div>
        `;

        $("#productFields").append(newFields);
        $(".totalPaidField").show('fast');


        // Target ONLY the last added product dropdown
        let dropdown = $(".productDropdown").last();

        // Populate dropdown, caching products and excluding already-selected items
        if (!allProducts) {
            $.ajax({
                url: '/Products/GetAllProducts',
                type: 'GET',
                dataType: 'json',
                success: function (data) {
                    allProducts = data || [];
                    applyProductOptions(dropdown);
                    updateAllDropdownOptions();
                },
                error: function () {
                    console.error('Failed to fetch products.');
                }
            });
        } else 
        {
            applyProductOptions(dropdown);
            updateAllDropdownOptions();
        }
    });

    // Remove product row
    $(document).on('click', '.remove-product-btn', function () {
        $(this).closest('.product-row').remove();
        calculateTotal();
        updateAllDropdownOptions();
        
        // Hide total fields if no products
        if ($('.product-row').length === 0) 
    {
            $('.totalPaidField').hide('fast');
        }
    });

    // Recalculate options when a product selection changes
    $(document).on('change', '.productDropdown', function ()
    {
        updateAllDropdownOptions();

    });

    // Calculate total when quantity or cost price changes
    $(document).on('input', '.quantity-input, .costprice-input', function () {
        calculateTotal();
    });

    function calculateTotal() 
    {
        let total = 0;
        $('.product-row').each(function () {
            const quantity = parseFloat($(this).find('.quantity-input').val()) || 0;
            const costPrice = parseFloat($(this).find('.costprice-input').val()) || 0;
            total += quantity * costPrice;
        });
        $('#totalExpectedAmount').val(total.toFixed(2));
    }

    
    function applyProductOptions(dropdown) {
        if (!allProducts) 
            return;
        const currentVal = dropdown.val();
        const selected = getSelectedProductIds(dropdown);

        dropdown.empty().append('<option value="">Select a Product</option>');

        allProducts.forEach(function (product) {
            const id = product.id != null ? product.id.toString() : '';
            if (id === '') return;

            // Keep current selection even if already chosen elsewhere; others exclude it
            if (!selected.has(id) || id === currentVal) {
                const isSelected = currentVal === id ? 'selected' : '';
                dropdown.append(`<option value="${id}" ${isSelected}>${product.productName}</option>`);
            }
        });
    }

    // Get set of selected product IDs from all dropdowns except the one passed (to allow keeping its choice)
    function getSelectedProductIds(excludeDropdown) {
        const ids = new Set();
        $('.productDropdown').each(function () {
            if (excludeDropdown && this === excludeDropdown[0]) return;
            const val = $(this).val();
            if (val) ids.add(val.toString());
        });
        return ids;
    }

    // Apply option filtering to every dropdown
    function updateAllDropdownOptions() {
        $('.productDropdown').each(function () {
            applyProductOptions($(this));
        });
    }
    

});

// Escape HTML to prevent XSS attacks (global helper used by products and suppliers)
function escapeHtml(text) {
    if (text === null || text === undefined) return '';
    var map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.toString().replace(/[&<>"']/g, function (m) { return map[m]; });
}

// Product Filtering Functionality
function initializeProductFilters() {
    let currentPage = 1;
    let currentFilters = {
        searchTerm: '',
        category: '',
        minPrice: null,
        maxPrice: null
    };

    // Check if any filter is active
    function hasActiveFilters() {
        return currentFilters.searchTerm !== '' ||
            currentFilters.category !== '' ||
            currentFilters.minPrice !== null ||
            currentFilters.maxPrice !== null;
    }

    // Update clear button visibility
    function updateClearButtonVisibility() {
        if (hasActiveFilters()) {
            $('#clearFilters').show();
        } else {
            $('#clearFilters').hide();
        }
    }

    // Search button click (magnifying glass)
    $('#searchButton').on('click', function () {
        currentFilters.searchTerm = $('#searchInput').val().trim();
        currentPage = 1;
        updateClearButtonVisibility();
        loadProducts();
    });

    // Search on Enter key
    $('#searchInput').on('keypress', function (e) {
        if (e.which === 13) {
            $('#searchButton').click();
        }
    });

    // Filter button click - open modal
    $('#filterButton').on('click', function () {
        // Load current filter values into modal
        $('#categoryFilter').val(currentFilters.category);
        $('#minPrice').val(currentFilters.minPrice || '');
        $('#maxPrice').val(currentFilters.maxPrice || '');

        // Show the modal
        var filterModal = new bootstrap.Modal(document.getElementById('filterModal'));
        filterModal.show();
    });

    // Apply filters button click (inside modal)
    $('#applyFilters').on('click', function () {
        currentFilters.searchTerm = $('#searchInput').val().trim();
        currentFilters.category = $('#categoryFilter').val();
        currentFilters.minPrice = $('#minPrice').val() || null;
        currentFilters.maxPrice = $('#maxPrice').val() || null;
        currentPage = 1;
        updateClearButtonVisibility();
        loadProducts();

        // Close the modal
        var filterModal = bootstrap.Modal.getInstance(document.getElementById('filterModal'));
        if (filterModal) {
            filterModal.hide();
        }
    });

    // Clear filters button click
    $('#clearFilters').on('click', function () {
        $('#searchInput').val('');
        $('#categoryFilter').val('');
        $('#minPrice').val('');
        $('#maxPrice').val('');
        currentFilters = {
            searchTerm: '',
            category: '',
            minPrice: null,
            maxPrice: null
        };
        currentPage = 1;
        updateClearButtonVisibility();
        loadProducts();
    });

    // Load products via AJAX
    function loadProducts() {
        $.ajax({
            url: '/Products/FilterProducts',
            type: 'GET',
            data: {
                page: currentPage,
                searchTerm: currentFilters.searchTerm,
                category: currentFilters.category,
                minPrice: currentFilters.minPrice,
                maxPrice: currentFilters.maxPrice
            },
            beforeSend: function () {
                $('#productsTableBody').html('<tr><td colspan="7" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></td></tr>');
            },
            success: function (data) {
                updateProductsTable(data.products);
                updatePagination(data);
            },
            error: function () {
                $('#productsTableBody').html('<tr><td colspan="7" class="text-center text-danger">Error loading products</td></tr>');
            }
        });
    }

    // Update products table
    function updateProductsTable(products) {
        let html = '';
        if (products.length === 0) {
            html = '<tr><td colspan="7" class="text-center text-muted">No products found</td></tr>';
        } else {
            products.forEach(function (product) {
                html += `
                    <tr>
                        <td>
                            <div class="d-flex align-items-center">
                                <div class="bg-light rounded p-2 me-2">
                                    <i class="bi bi-box text-primary" style="font-size: 20px;"></i>
                                </div>
                                <span class="fw-bold">${escapeHtml(product.productName)}</span>
                            </div>
                        </td>
                        <td><span class="badge bg-light text-dark">${escapeHtml(product.category || 'N/A')}</span></td>
                        <td class="text-muted">Rs ${product.costPrice.toFixed(2)}</td>
                        <td class="fw-bold">Rs ${product.salePrice.toFixed(2)}</td>
                        <td class="py-3"><span class="badge text-muted text-dark px-3 py-2">${escapeHtml(product.description || 'N/A')}</span></td>
                        <td><code>${escapeHtml(product.barcode)}</code></td>
                        <td>
                            <div class="d-flex justify-content-center gap-1">
                                <a class="btn btn-sm btn-outline-info" title="View" href="/Products/ViewProduct/${product.id}">
                                    <i class="bi bi-eye"></i>
                                </a>
                                <a class="btn btn-sm btn-outline-primary" title="Edit" href="/Products/EditProduct/${product.id}">
                                    <i class="bi bi-pencil"></i>
                                </a>
                                <a class="btn btn-sm btn-outline-danger" title="Delete" href="/Products/DeleteProduct/${product.id}">
                                    <i class="bi bi-trash"></i>
                                </a>
                            </div>
                        </td>
                    </tr>
                `;
            });
        }
        $('#productsTableBody').html(html);
    }

    // (uses global escapeHtml)

    // Update pagination
    function updatePagination(data) {
        let start = (data.currentPage - 1) * data.pageSize + 1;
        let end = Math.min(data.currentPage * data.pageSize, data.totalEntries);

        $('#paginationInfo').html(`Showing ${start} to ${end} of <b>${data.totalEntries}</b>`);

        let paginationHtml = '';

        // Previous button
        paginationHtml += `
            <li class="page-item ${data.currentPage === 1 ? 'disabled' : ''}">
                <a class="page-link" data-page="${data.currentPage - 1}" style="cursor: pointer;">
                    <i class="bi bi-chevron-left"></i>
                </a>
            </li>
        `;

        // Page numbers
        let startPage = Math.max(1, data.currentPage - 2);
        let endPage = Math.min(data.totalPages, data.currentPage + 2);

        if (startPage > 1) {
            paginationHtml += `<li class="page-item"><a class="page-link" data-page="1" style="cursor: pointer;">1</a></li>`;
            if (startPage > 2) {
                paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            paginationHtml += `
                <li class="page-item ${i === data.currentPage ? 'active' : ''}">
                    <a class="page-link" data-page="${i}" style="cursor: pointer;">${i}</a>
                </li>
            `;
        }

        if (endPage < data.totalPages) {
            if (endPage < data.totalPages - 1) {
                paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
            paginationHtml += `<li class="page-item"><a class="page-link" data-page="${data.totalPages}" style="cursor: pointer;">${data.totalPages}</a></li>`;
        }

        // Next button
        paginationHtml += `
            <li class="page-item ${data.currentPage === data.totalPages ? 'disabled' : ''}">
                <a class="page-link" data-page="${data.currentPage + 1}" style="cursor: pointer;">
                    <i class="bi bi-chevron-right"></i>
                </a>
            </li>
        `;

        $('#paginationControls').html(paginationHtml);
    }

    // Pagination click handler
    $(document).on('click', '#paginationControls .page-link', function (e) {
        e.preventDefault();
        let page = $(this).data('page');

        // Handle asp-route links (initial page load)
        if (!page) {
            const href = $(this).attr('href');
            if (href && href !== '' && href.includes('page=')) {
                const urlParams = new URLSearchParams(href.split('?')[1]);
                page = urlParams.get('page');
            }
        }

        if (page && page > 0) {
            currentPage = parseInt(page);
            loadProducts();
        }
    });
}

// Supplier Filtering Functionality
function initializeSupplierFilters() {
    let currentPage = 1;
    let currentFilters = {
        searchTerm: ''
    };

    // Check if any filter is active
    function hasActiveFilters() {
        return currentFilters.searchTerm !== '';
    }

    // Update clear button visibility
    function updateClearButtonVisibility() {
        if (hasActiveFilters()) {
            $('#clearFiltersSupplier').show();
        } else {
            $('#clearFiltersSupplier').hide();
        }
    }

    // Search button click (magnifying glass)
    $('#searchButtonSupplier').on('click', function () {
        currentFilters.searchTerm = $('#searchInputSupplier').val().trim();
        currentPage = 1;
        updateClearButtonVisibility();
        loadSuppliers();
    });

    // Search on Enter key
    $('#searchInputSupplier').on('keypress', function (e) {
        if (e.which === 13) {
            $('#searchButtonSupplier').click();
        }
    });

    // Clear filters button click
    $('#clearFiltersSupplier').on('click', function () {
        $('#searchInputSupplier').val('');
        currentFilters = {
            searchTerm: ''
        };
        currentPage = 1;
        updateClearButtonVisibility();
        loadSuppliers();
    });

    // Load suppliers via AJAX
    function loadSuppliers() {
        $.ajax({
            url: '/Suppliers/FilterSuppliers',
            type: 'GET',
            data: {
                page: currentPage,
                searchTerm: currentFilters.searchTerm
            },
            beforeSend: function () {
                $('#suppliersTableBody').html('<tr><td colspan="5" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></td></tr>');
            },
            success: function (data) {
                updateSuppliersTable(data.suppliers);
                updateSuppliersPagination(data);
            },
            error: function () {
                $('#suppliersTableBody').html('<tr><td colspan="5" class="text-center text-danger">Error loading suppliers</td></tr>');
            }
        });
    }

    // Update suppliers table
    function updateSuppliersTable(suppliers) {
        let html = '';
        if (suppliers.length === 0) {
            html = '<tr><td colspan="5" class="text-center text-muted">No suppliers found</td></tr>';
        } else {
            suppliers.forEach(function (supplier) {
                html += `
                    <tr>
                        <td>
                            <div class="d-flex align-items-center">
                                <div class="bg-success bg-opacity-10 rounded-circle p-2 me-3">
                                    <i class="bi bi-building text-success" style="font-size: 20px;"></i>
                                </div>
                                <span class="fw-bold text-dark">${escapeHtml(supplier.supplierName)}</span>
                            </div>
                        </td>
                        <td>${escapeHtml(supplier.contactNumber)}</td>
                        <td>${escapeHtml(supplier.email || '-')}</td>
                        <td>
                            <div class="d-flex justify-content-center gap-1">
                                <a class="btn btn-sm btn-outline-info rounded-pill" title="View Products" href="/Suppliers/ViewProductsBySupplier/${supplier.id}">
                                    <i class="bi bi-box2"></i>
                                </a>
                            </div>
                        </td>
                        <td>
                            <div class="d-flex justify-content-center gap-2">
                                <a class="btn btn-sm btn-outline-success rounded-pill" title="View" href="/Suppliers/ViewSupplier/${supplier.id}">
                                    <i class="bi bi-eye"></i>
                                </a>
                                <a class="btn btn-sm btn-outline-primary rounded-pill" title="Edit" href="/Suppliers/EditSupplier/${supplier.id}">
                                    <i class="bi bi-pencil"></i>
                                </a>
                                <a class="btn btn-sm btn-outline-danger rounded-pill" title="Delete" href="/Suppliers/DeleteSupplier/${supplier.id}">
                                    <i class="bi bi-trash"></i>
                                </a>
                            </div>
                        </td>
                    </tr>
                `;
            });
        }
        $('#suppliersTableBody').html(html);
    }

    // Update pagination
    function updateSuppliersPagination(data) {
        let start = (data.currentPage - 1) * data.pageSize + 1;
        let end = Math.min(data.currentPage * data.pageSize, data.totalEntries);

        $('#paginationInfoSupplier').html(`Showing ${start} to ${end} of <b>${data.totalEntries}</b>`);

        let paginationHtml = '';

        // Previous button
        paginationHtml += `
            <li class="page-item ${data.currentPage === 1 ? 'disabled' : ''}">
                <a class="page-link" data-page="${data.currentPage - 1}" style="cursor: pointer;">
                    <i class="bi bi-chevron-left"></i>
                </a>
            </li>
        `;

        // Page numbers
        let startPage = Math.max(1, data.currentPage - 2);
        let endPage = Math.min(data.totalPages, data.currentPage + 2);

        if (startPage > 1) {
            paginationHtml += `<li class="page-item"><a class="page-link" data-page="1" style="cursor: pointer;">1</a></li>`;
            if (startPage > 2) {
                paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            paginationHtml += `
                <li class="page-item ${i === data.currentPage ? 'active' : ''}">
                    <a class="page-link" data-page="${i}" style="cursor: pointer;">${i}</a>
                </li>
            `;
        }

        if (endPage < data.totalPages) {
            if (endPage < data.totalPages - 1) {
                paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
            paginationHtml += `<li class="page-item"><a class="page-link" data-page="${data.totalPages}" style="cursor: pointer;">${data.totalPages}</a></li>`;
        }

        // Next button
        paginationHtml += `
            <li class="page-item ${data.currentPage === data.totalPages ? 'disabled' : ''}">
                <a class="page-link" data-page="${data.currentPage + 1}" style="cursor: pointer;">
                    <i class="bi bi-chevron-right"></i>
                </a>
            </li>
        `;

        $('#paginationControlsSupplier').html(paginationHtml);
    }

    // Pagination click handler
    $(document).on('click', '#paginationControlsSupplier .page-link', function (e) {
        e.preventDefault();
        let page = $(this).data('page');

        // Handle asp-route links (initial page load)
        if (!page) {
            const href = $(this).attr('href');
            if (href && href !== '' && href.includes('page=')) {
                const urlParams = new URLSearchParams(href.split('?')[1]);
                page = urlParams.get('page');
            }
        }

        if (page && page > 0) {
            currentPage = parseInt(page);
            loadSuppliers();
        }
    });
    var supplierDropDown = $('#selectSupplier');   

    $.ajax({
        url: '/Suppliers/GetAllSuppliers',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            supplierDropDown.empty(); 

            data.forEach(function (supplier) {
                var h=$('<option>', {
                    value: supplier.id,
                    text: supplier.supplierName
                });
                console.log(h);
                supplierDropDown.append(h
                    
                );
            });
        },
        error: function (xhr, status, error) {
            console.error('Failed to fetch suppliers:', error);
        }
    });


}
$(document).ready(function () {
    // Load cart on page load
    loadCart();

    // Function to load cart from server
    function loadCart() {
        $.ajax({
            url: '/Sales/GetCart',
            type: 'GET',
            dataType: 'json',
            success: function (products) {
                updateCartDisplay(products);
            },
            error: function (xhr, status, error) {
                console.error("Error loading cart:", status, error);
                updateCartDisplay([]);
            }
        });
    }

    // Add to cart functionality
    $(document).on('click', '.addtocart', function (e) {
        e.preventDefault();
        let $button = $(this);
        let productId = $button.data('id');
        let productName = $button.data('name');
        let price = $button.data('price');

        if (!productId || !productName || !price) {
            console.error("Missing product data attributes.");
            showNotification('Error: Missing product information', 'error');
            return;
        }

        $button.prop('disabled', true);
        $button.html('<i class="bi bi-hourglass-split me-1"></i>Adding...');

        $.ajax({
            url: '/Sales/AddToCart',
            type: 'POST',
            data: {
                productId: productId,
                productName: productName,
                price: price
            },
            dataType: 'json',
            success: function (products) {
                updateCartDisplay(products);
                showNotification('Product added to cart!', 'success');
            },
            error: function (xhr, status, error) {
                console.error("Error adding product to cart:", status, error);
                showNotification('Could not add product to cart. Please try again.', 'error');
            },
            complete: function () {
                $button.prop('disabled', false);
                $button.html('<i class="bi bi-plus-circle me-1"></i>Add');
            }
        });
    });

    // Update quantity functionality
    $(document).on('click', '.quantity-btn', function (e) {
        e.preventDefault();
        let $button = $(this);
        let productId = $button.data('id');
        let action = $button.data('action'); // 'increase' or 'decrease'

        $.ajax({
            url: '/Sales/UpdateQuantity',
            type: 'POST',
            data: {
                productId: productId,
                action: action
            },
            dataType: 'json',
            success: function (products) {
                updateCartDisplay(products);
                if (action === 'increase') {
                    showNotification('Quantity increased', 'success');
                } else {
                    showNotification('Quantity decreased', 'info');
                }
            },
            error: function (xhr, status, error) {
                console.error("Error updating quantity:", status, error);
                showNotification('Could not update quantity. Please try again.', 'error');
            }
        });
    });

    // Remove from cart functionality
    $(document).on('click', '.remove-item', function (e) {
        e.preventDefault();
        let $button = $(this);
        let productId = $button.data('id');

        $.ajax({
            url: '/Sales/RemoveFromCart',
            type: 'POST',
            data: {
                productId: productId
            },
            dataType: 'json',
            success: function (products) {
                updateCartDisplay(products);
                showNotification('Product removed from cart', 'info');
            },
            error: function (xhr, status, error) {
                console.error("Error removing product:", status, error);
                showNotification('Could not remove product. Please try again.', 'error');
            }
        });
    });

    // Clear cart functionality
    $('#clearCart').on('click', function () {
        if (!confirm('Are you sure you want to clear the cart?')) {
            return;
        }

        $.post('/Sales/ClearCart', function (isCleared) {
            if (isCleared) {
                updateCartDisplay([]);
                showNotification('Cart cleared successfully', 'info');
            }
        });
    });

    // Checkout button functionality
    $('#checkoutBtn').on('click', function () {
        $.ajax({
            url: '/Sales/GetCart',
            type: 'GET',
            dataType: 'json',
            success: function (products) {
                if (!products || products.length === 0) {
                    showNotification('Please add items to cart before checkout', 'error');
                    return;
                }
                // Navigate to checkout page
                window.location.href = '/Sales/Checkout';
            },
            error: function () {
                showNotification('Error checking cart', 'error');
            }
        });
    });

    // Update cart display
    function updateCartDisplay(products) {
        let total = 0;
        let itemsHtml = '';
        const cartItemsContainer = $('#cartItems');

        if (!products || products.length === 0) {
            cartItemsContainer.html(`
                <div class="cart-empty">
                    <div class="cart-empty-icon">🛒</div>
                    <p class="mb-0">Your cart is empty</p>
                    <small>Add products to get started</small>
                </div>
            `);
            $('#cartTotal').text('Rs. 0.00');
            return;
        }

        products.forEach(item => {
            const itemTotal = item.quantity * item.productPrice;
            total += itemTotal;
            itemsHtml += `
                <div class="cart-item">
                    <div class="d-flex justify-content-between align-items-start mb-2">
                        <div style="flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;" 
                             title="${item.productName}">
                            <strong>${item.productName}</strong>
                            <div class="text-muted small">Rs. ${item.productPrice.toFixed(2)} each</div>
                        </div>
                        <button class="remove-item btn btn-sm btn-link text-danger p-0 ms-2" 
                                data-id="${item.productId}"
                                title="Remove item">
                            <i class="bi bi-x-circle"></i>
                        </button>
                    </div>
                    <div class="d-flex justify-content-between align-items-center">
                        <div class="quantity-controls">
                            <button class="quantity-btn btn btn-sm btn-outline-secondary" 
                                    data-id="${item.productId}" 
                                    data-action="decrease"
                                    ${item.quantity <= 1 ? 'disabled' : ''}>
                                <i class="bi bi-dash"></i>
                            </button>
                            <span class="quantity-display mx-2">${item.quantity}</span>
                            <button class="quantity-btn btn btn-sm btn-outline-secondary" 
                                    data-id="${item.productId}" 
                                    data-action="increase">
                                <i class="bi bi-plus"></i>
                            </button>
                        </div>
                        <div class="item-total" style="font-weight: 600; color: #27ae60;">
                            Rs. ${itemTotal.toFixed(2)}
                        </div>
                    </div>
                </div>
            `;
        });

        cartItemsContainer.html(itemsHtml);
        $('#cartTotal').text(`Rs. ${total.toFixed(2)}`);

        cartItemsContainer.hide().fadeIn(300);
    }

    // Notification system
    function showNotification(message, type) {
        $('.custom-notification').remove();

        const bgColors = {
            'success': 'linear-gradient(135deg, #27ae60 0%, #229954 100%)',
            'error': 'linear-gradient(135deg, #e74c3c 0%, #c0392b 100%)',
            'info': 'linear-gradient(135deg, #3498db 0%, #2980b9 100%)'
        };

        const notification = $(`
            <div class="custom-notification" style="
                position: fixed;
                top: 20px;
                right: 20px;
                background: ${bgColors[type] || bgColors['info']};
                color: white;
                padding: 1rem 1.5rem;
                border-radius: 8px;
                box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                z-index: 9999;
                animation: slideIn 0.3s ease;
                font-weight: 500;
            ">
                <i class="bi bi-${type === 'success' ? 'check-circle' : type === 'error' ? 'x-circle' : 'info-circle'} me-2"></i>
                ${message}
            </div>
        `);

        $('body').append(notification);

        setTimeout(() => {
            notification.fadeOut(300, function () {
                $(this).remove();
            });
        }, 3000);
    }

    // Add animation keyframes and styles
    if (!$('#pos-animations').length) {
        $('head').append(`
            <style id="pos-animations">
                @keyframes slideIn {
                    from {
                        transform: translateX(400px);
                        opacity: 0;
                    }
                    to {
                        transform: translateX(0);
                        opacity: 1;
                    }
                }
                
                .quantity-controls {
                    display: flex;
                    align-items: center;
                }
                
                .quantity-display {
                    font-weight: 600;
                    font-size: 1rem;
                    min-width: 30px;
                    text-align: center;
                }
                
                .quantity-btn {
                    width: 28px;
                    height: 28px;
                    padding: 0;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    border-radius: 6px;
                    transition: all 0.2s ease;
                }
                
                .quantity-btn:hover:not(:disabled) {
                    background-color: #3498db;
                    color: white;
                    border-color: #3498db;
                    transform: scale(1.1);
                }
                
                .quantity-btn:disabled {
                    opacity: 0.4;
                    cursor: not-allowed;
                }
                
                .remove-item {
                    font-size: 1.2rem;
                    transition: all 0.2s ease;
                    text-decoration: none;
                }
                
                .remove-item:hover {
                    transform: scale(1.2);
                    color: #c0392b !important;
                }
                
                .cart-item {
                    padding: 0.75rem;
                    border-bottom: 1px solid #ecf0f1;
                    transition: background 0.2s ease;
                }
                
                .cart-item:hover {
                    background: #f8f9fa;
                }
                
                .cart-item:last-child {
                    border-bottom: none;
                }
            </style>
        `);
    }
});


$(document).ready(function () {
    loadInvoice();

    // Generate invoice number and date
    const invoiceId = 'POS' + Date.now().toString().slice(-6);
    $('.invoice-id').text(invoiceId);
    $('#invoiceDate').text(new Date().toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    }));

    function loadInvoice() {
        $.ajax({
            url: '/Sales/GetCart',
            type: 'GET',
            dataType: 'json',
            success: function (products) {
                if (!products || products.length === 0) {
                    $('#invoiceItems').html(`
                            <tr>
                                <td colspan="4">
                                    <div class="empty-cart-notice">
                                        <div class="empty-cart-icon">🛒</div>
                                        <h5>Cart is Empty</h5>
                                        <p>Please add items to cart before checkout</p>
                                    </div>
                                </td>
                            </tr>
                        `);
                    $('#confirmSaleBtn').prop('disabled', true);
                    return;
                }

                let itemsHtml = '';
                let subtotal = 0;

                products.forEach(item => {
                    const itemTotal = item.quantity * item.productPrice;
                    subtotal += itemTotal;

                    itemsHtml += `
                            <tr>
                                <td><strong>${item.productName}</strong></td>
                                <td class="text-center">${item.quantity}</td>
                                <td class="text-end">Rs. ${item.productPrice.toFixed(2)}</td>
                                <td class="text-end"><strong>Rs. ${itemTotal.toFixed(2)}</strong></td>
                            </tr>
                        `;
                });

                $('#invoiceItems').html(itemsHtml);
                $('#subtotal').text(`Rs. ${subtotal.toFixed(2)}`);
                $('#totalAmount').text(`Rs. ${subtotal.toFixed(2)}`);
            },
            error: function () {
                showNotification('Error loading cart items', 'error');
            }
        });
    }

    $('#confirmSaleBtn').on('click', function () {
        const form = $('#checkoutForm')[0];
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        const saleData = {
            customerName: $('#customerName').val(),
            customerPhone: $('#customerPhone').val(),
            customerEmail: $('#customerEmail').val(),
            paymentMethod: $('input[name="paymentMethod"]:checked').val(),
            invoiceNumber: $('.invoice-id').text()
        };

        // Show confirmation
        if (confirm('Are you sure you want to confirm this sale?')) {
            $.ajax({
                url: '/Sales/ConfirmSale',
                type: 'POST',
                data: saleData,
                success: function (response) {
                    if (response.success) {
                        showNotification('Sale completed successfully!', 'success');
                        setTimeout(() => {
                            window.location.href = '/Sales/POS';
                        }, 2000);
                    } else {
                        showNotification('Error completing sale', 'error');
                    }
                },
                error: function () {
                    showNotification('Error processing sale', 'error');
                }
            });
        }
    });

    function showNotification(message, type) {
        $('.custom-notification').remove();

        const bgColors = {
            'success': 'linear-gradient(135deg, #27ae60 0%, #229954 100%)',
            'error': 'linear-gradient(135deg, #e74c3c 0%, #c0392b 100%)',
            'info': 'linear-gradient(135deg, #3498db 0%, #2980b9 100%)'
        };

        const notification = $(`
                <div class="custom-notification" style="
                    position: fixed;
                    top: 20px;
                    right: 20px;
                    background: ${bgColors[type]};
                    color: white;
                    padding: 1rem 1.5rem;
                    border-radius: 8px;
                    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
                    z-index: 9999;
                    animation: slideIn 0.3s ease;
                    font-weight: 500;
                ">
                    <i class="bi bi-${type === 'success' ? 'check-circle' : 'x-circle'} me-2"></i>
                    ${message}
                </div>
            `);

        $('body').append(notification);

        setTimeout(() => {
            notification.fadeOut(300, function () {
                $(this).remove();
            });
        }, 3000);
    }
});

function selectPayment(method) {
    $('.payment-method-card').removeClass('selected');
    $(`#payment${method.charAt(0).toUpperCase() + method.slice(1)}`).closest('.payment-method-card').addClass('selected');
    $(`#payment${method.charAt(0).toUpperCase() + method.slice(1)}`).prop('checked', true);
}

// Initialize first payment method as selected
$(document).ready(function () {
    selectPayment('cash');
});