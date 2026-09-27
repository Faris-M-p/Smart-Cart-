document.addEventListener("DOMContentLoaded", function () {
    loadCategoryOptions();
    loadProductOptions();
    loadBanners();
    loadFeaturedCategories();
    loadFeaturedProducts();

    document.getElementById("bannerForm").addEventListener("submit", handleSaveBanner);
    document.getElementById("featuredCategoryForm").addEventListener("submit", handleSaveFeaturedCategory);
    document.getElementById("featuredProductForm").addEventListener("submit", handleSaveFeaturedProduct);
});

let categoryOptionsMap = [];
let productOptionsMap = [];

async function loadCategoryOptions() {
    try {
        const res = await fetch("/Admin/Homepage/GetAvailableCategoryOptions");
        const json = await res.json();
        if (json.success && json.data) {
            categoryOptionsMap = json.data;

            const bannerCatSelect = document.getElementById("bannerCategories");
            const fcSelect = document.getElementById("featuredCategorySelect");

            if (bannerCatSelect) {
                bannerCatSelect.innerHTML = json.data
                    .map(c => `<option value="${c.categoryId}">${escapeHtml(c.categoryName)}</option>`)
                    .join("");
            }

            if (fcSelect) {
                fcSelect.innerHTML = `<option value="">-- Select Category --</option>` + json.data
                    .map(c => `<option value="${c.categoryId}">${escapeHtml(c.categoryName)}</option>`)
                    .join("");
            }
        }
    } catch (err) {
        console.error("Error loading category options:", err);
    }
}

async function loadProductOptions() {
    try {
        const res = await fetch("/Admin/Homepage/GetAvailableProductOptions");
        const json = await res.json();
        if (json.success && json.data) {
            productOptionsMap = json.data;

            const fpSelect = document.getElementById("featuredProductSelect");
            if (fpSelect) {
                fpSelect.innerHTML = `<option value="">-- Select Product --</option>` + json.data
                    .map(p => `<option value="${p.productId}">${escapeHtml(p.productName)} (${escapeHtml(p.categoryName)})</option>`)
                    .join("");
            }
        }
    } catch (err) {
        console.error("Error loading product options:", err);
    }
}

// BANNERS
async function loadBanners() {
    const tbody = document.getElementById("bannersTableBody");
    if (!tbody) return;
    try {
        const res = await fetch("/Admin/Homepage/GetBanners");
        const json = await res.json();
        if (json.success && json.data) {
            if (json.data.length === 0) {
                tbody.innerHTML = `<tr><td colspan="7" class="text-center py-4 text-muted">No banners found. Click "Add New Banner" to create one.</td></tr>`;
                return;
            }

            tbody.innerHTML = json.data.map(b => {
                const imgTag = b.imageUrl
                    ? `<img src="${b.imageUrl}" class="rounded border" style="width: 80px; height: 45px; object-fit: cover;" />`
                    : `<span class="badge bg-light text-dark border">No Image</span>`;

                const catsBadges = (b.categoryNames && b.categoryNames.length > 0)
                    ? b.categoryNames.map(c => `<span class="badge bg-secondary me-1">${escapeHtml(c)}</span>`).join("")
                    : `<span class="text-muted small">None</span>`;

                const statusBadge = b.isActive
                    ? `<span class="badge bg-success">Active</span>`
                    : `<span class="badge bg-danger">Inactive</span>`;

                const targetUrlDisplay = b.targetUrl
                    ? `<code class="small text-truncate d-inline-block" style="max-width: 180px;">${escapeHtml(b.targetUrl)}</code>`
                    : `<span class="text-muted small">Auto-generated</span>`;

                return `
                    <tr>
                        <td>${imgTag}</td>
                        <td>
                            <div class="fw-bold">${escapeHtml(b.title)}</div>
                            <div class="small text-muted">${escapeHtml(b.subtitle || '')}</div>
                        </td>
                        <td>${catsBadges}</td>
                        <td>${targetUrlDisplay}</td>
                        <td>
                            <input type="number" class="form-control form-control-sm text-center banner-order-input" data-id="${b.bannerId}" value="${b.displayOrder}" style="width: 70px;" onchange="updateBannerOrder(${b.bannerId}, this.value)" />
                        </td>
                        <td>${statusBadge}</td>
                        <td class="text-end">
                            <button type="button" class="btn btn-outline-primary btn-sm me-1" onclick="openEditBannerModal(${b.bannerId})" data-permission="Homepage.Edit">
                                <i class="ti ti-edit"></i>
                            </button>
                            <button type="button" class="btn btn-outline-danger btn-sm" onclick="deleteBanner(${b.bannerId})" data-permission="Homepage.Edit">
                                <i class="ti ti-trash"></i>
                            </button>
                        </td>
                    </tr>
                `;
            }).join("");
        }
    } catch (err) {
        console.error("Error loading banners:", err);
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-danger py-4">Failed to load banners.</td></tr>`;
    }
}

function openCreateBannerModal() {
    document.getElementById("bannerForm").reset();
    document.getElementById("bannerId").value = "0";
    document.getElementById("bannerModalTitle").innerText = "Add New Banner";
    document.getElementById("bannerImagePreviewContainer").style.display = "none";
    document.getElementById("bannerImagePreview").src = "";
    document.getElementById("bannerIsActive").checked = true;

    const bannerCatSelect = document.getElementById("bannerCategories");
    for (let opt of bannerCatSelect.options) {
        opt.selected = false;
    }

    const modal = new bootstrap.Modal(document.getElementById("bannerModal"));
    modal.show();
}

async function openEditBannerModal(bannerId) {
    try {
        const res = await fetch(`/Admin/Homepage/GetBanner/${bannerId}`);
        const json = await res.json();
        if (json.success && json.data) {
            const b = json.data;
            document.getElementById("bannerId").value = b.bannerId;
            document.getElementById("bannerTitle").value = b.title || '';
            document.getElementById("bannerSubtitle").value = b.subtitle || '';
            document.getElementById("bannerTargetUrl").value = b.targetUrl || '';
            document.getElementById("bannerDisplayOrder").value = b.displayOrder;
            document.getElementById("bannerIsActive").checked = b.isActive;

            if (b.imageUrl) {
                document.getElementById("bannerImagePreview").src = b.imageUrl;
                document.getElementById("bannerImagePreviewContainer").style.display = "block";
            } else {
                document.getElementById("bannerImagePreviewContainer").style.display = "none";
            }

            const bannerCatSelect = document.getElementById("bannerCategories");
            const selectedCatIds = new Set(b.categoryIds || []);
            for (let opt of bannerCatSelect.options) {
                opt.selected = selectedCatIds.has(parseInt(opt.value));
            }

            document.getElementById("bannerModalTitle").innerText = "Edit Banner";
            const modal = new bootstrap.Modal(document.getElementById("bannerModal"));
            modal.show();
        }
    } catch (err) {
        console.error("Error opening edit banner modal:", err);
        alert("Failed to load banner details.");
    }
}

function previewBannerImage(input) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            document.getElementById("bannerImagePreview").src = e.target.result;
            document.getElementById("bannerImagePreviewContainer").style.display = "block";
        };
        reader.readAsDataURL(input.files[0]);
    }
}

async function handleSaveBanner(e) {
    e.preventDefault();
    const btn = document.getElementById("btnSaveBanner");
    btn.disabled = true;
    btn.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Saving...`;

    try {
        const formData = new FormData(this);

        formData.delete("CategoryIds");
        const bannerCatSelect = document.getElementById("bannerCategories");
        for (let opt of bannerCatSelect.options) {
            if (opt.selected) {
                formData.append("CategoryIds", opt.value);
            }
        }

        const res = await fetch("/Admin/Homepage/SaveBanner", {
            method: "POST",
            body: formData
        });
        const json = await res.json();

        if (json.statusCode || json.success) {
            bootstrap.Modal.getInstance(document.getElementById("bannerModal")).hide();
            await loadBanners();
        } else {
            alert(json.responseMsg || json.message || "Failed to save banner.");
        }
    } catch (err) {
        console.error("Error saving banner:", err);
        alert("An error occurred while saving banner.");
    } finally {
        btn.disabled = false;
        btn.innerHTML = `Save Banner`;
    }
}

async function deleteBanner(id) {
    if (!confirm("Are you sure you want to delete this banner?")) return;
    try {
        const res = await fetch(`/Admin/Homepage/DeleteBanner/${id}`, { method: "POST" });
        const json = await res.json();
        if (json.statusCode || json.success) {
            await loadBanners();
        } else {
            alert(json.responseMsg || "Failed to delete banner.");
        }
    } catch (err) {
        console.error("Error deleting banner:", err);
    }
}

// FEATURED CATEGORIES
async function loadFeaturedCategories() {
    const tbody = document.getElementById("categoriesTableBody");
    if (!tbody) return;
    try {
        const res = await fetch("/Admin/Homepage/GetCategories");
        const json = await res.json();
        if (json.success && json.data) {
            if (json.data.length === 0) {
                tbody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-muted">No featured categories added yet.</td></tr>`;
                return;
            }

            tbody.innerHTML = json.data.map(c => {
                const imgTag = c.imageUrl
                    ? `<img src="${c.imageUrl}" class="rounded border" style="width: 45px; height: 45px; object-fit: cover;" />`
                    : `<span class="badge bg-light text-dark border">No Image</span>`;

                const statusBadge = c.isActive
                    ? `<span class="badge bg-success">Active</span>`
                    : `<span class="badge bg-secondary">Inactive</span>`;

                return `
                    <tr>
                        <td>${imgTag}</td>
                        <td class="fw-bold">${escapeHtml(c.categoryName)}</td>
                        <td>
                            <input type="number" class="form-control form-control-sm text-center" value="${c.displayOrder}" style="width: 70px;" onchange="updateCategoryOrder(${c.homepageCategoryId}, this.value)" />
                        </td>
                        <td>${statusBadge}</td>
                        <td class="text-end">
                            <button type="button" class="btn btn-outline-${c.isActive ? 'warning' : 'success'} btn-sm me-1" onclick="toggleCategoryStatus(${c.homepageCategoryId})" data-permission="Homepage.Edit">
                                ${c.isActive ? 'Disable' : 'Enable'}
                            </button>
                            <button type="button" class="btn btn-outline-danger btn-sm" onclick="deleteCategory(${c.homepageCategoryId})" data-permission="Homepage.Edit">
                                <i class="ti ti-trash"></i>
                            </button>
                        </td>
                    </tr>
                `;
            }).join("");
        }
    } catch (err) {
        console.error("Error loading featured categories:", err);
        tbody.innerHTML = `<tr><td colspan="5" class="text-center text-danger py-4">Failed to load categories.</td></tr>`;
    }
}

function openAddCategoryModal() {
    document.getElementById("featuredCategoryForm").reset();
    document.getElementById("homepageCategoryId").value = "0";
    document.getElementById("featuredCategoryIsActive").checked = true;
    const modal = new bootstrap.Modal(document.getElementById("featuredCategoryModal"));
    modal.show();
}

async function handleSaveFeaturedCategory(e) {
    e.preventDefault();
    const btn = document.getElementById("btnSaveFeaturedCategory");
    btn.disabled = true;

    try {
        const input = {
            HomepageCategoryId: parseInt(document.getElementById("homepageCategoryId").value) || 0,
            CategoryId: parseInt(document.getElementById("featuredCategorySelect").value) || 0,
            DisplayOrder: parseInt(document.getElementById("featuredCategoryDisplayOrder").value) || 0,
            IsActive: document.getElementById("featuredCategoryIsActive").checked
        };

        const res = await fetch("/Admin/Homepage/SaveCategory", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(input)
        });
        const json = await res.json();

        if (json.statusCode || json.success) {
            bootstrap.Modal.getInstance(document.getElementById("featuredCategoryModal")).hide();
            await loadFeaturedCategories();
        } else {
            alert(json.responseMsg || "Failed to save category.");
        }
    } catch (err) {
        console.error("Error saving category:", err);
    } finally {
        btn.disabled = false;
    }
}

async function toggleCategoryStatus(id) {
    try {
        const res = await fetch(`/Admin/Homepage/ToggleCategoryStatus/${id}`, { method: "POST" });
        const json = await res.json();
        if (json.statusCode || json.success) {
            await loadFeaturedCategories();
        }
    } catch (err) {
        console.error("Error toggling category status:", err);
    }
}

async function updateCategoryOrder(id, order) {
    try {
        await fetch("/Admin/Homepage/UpdateCategoryOrder", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ id: parseInt(id), displayOrder: parseInt(order) || 0 })
        });
    } catch (err) {
        console.error("Error updating category order:", err);
    }
}

async function deleteCategory(id) {
    if (!confirm("Remove this category from homepage?")) return;
    try {
        const res = await fetch(`/Admin/Homepage/DeleteCategory/${id}`, { method: "POST" });
        const json = await res.json();
        if (json.statusCode || json.success) {
            await loadFeaturedCategories();
        }
    } catch (err) {
        console.error("Error deleting category:", err);
    }
}

// FEATURED PRODUCTS
async function loadFeaturedProducts() {
    const tbody = document.getElementById("productsTableBody");
    if (!tbody) return;
    try {
        const res = await fetch("/Admin/Homepage/GetProducts");
        const json = await res.json();
        if (json.success && json.data) {
            if (json.data.length === 0) {
                tbody.innerHTML = `<tr><td colspan="7" class="text-center py-4 text-muted">No featured products added yet.</td></tr>`;
                return;
            }

            tbody.innerHTML = json.data.map(p => {
                const imgTag = p.imageUrl
                    ? `<img src="${p.imageUrl}" class="rounded border" style="width: 45px; height: 45px; object-fit: cover;" />`
                    : `<span class="badge bg-light text-dark border">No Image</span>`;

                const statusBadge = p.isActive
                    ? `<span class="badge bg-success">Active</span>`
                    : `<span class="badge bg-secondary">Inactive</span>`;

                return `
                    <tr>
                        <td>${imgTag}</td>
                        <td class="fw-bold">${escapeHtml(p.productName)}</td>
                        <td><span class="badge bg-light text-dark border">${escapeHtml(p.categoryName)}</span></td>
                        <td class="fw-bold text-success">$${p.price.toFixed(2)}</td>
                        <td>
                            <input type="number" class="form-control form-control-sm text-center" value="${p.displayOrder}" style="width: 70px;" onchange="updateProductOrder(${p.homepageProductId}, this.value)" />
                        </td>
                        <td>${statusBadge}</td>
                        <td class="text-end">
                            <button type="button" class="btn btn-outline-${p.isActive ? 'warning' : 'success'} btn-sm me-1" onclick="toggleProductStatus(${p.homepageProductId})" data-permission="Homepage.Edit">
                                ${p.isActive ? 'Disable' : 'Enable'}
                            </button>
                            <button type="button" class="btn btn-outline-danger btn-sm" onclick="deleteProduct(${p.homepageProductId})" data-permission="Homepage.Edit">
                                <i class="ti ti-trash"></i>
                            </button>
                        </td>
                    </tr>
                `;
            }).join("");
        }
    } catch (err) {
        console.error("Error loading featured products:", err);
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-danger py-4">Failed to load products.</td></tr>`;
    }
}

function openAddProductModal() {
    document.getElementById("featuredProductForm").reset();
    document.getElementById("homepageProductId").value = "0";
    document.getElementById("featuredProductIsActive").checked = true;
    const modal = new bootstrap.Modal(document.getElementById("featuredProductModal"));
    modal.show();
}

async function handleSaveFeaturedProduct(e) {
    e.preventDefault();
    const btn = document.getElementById("btnSaveFeaturedProduct");
    btn.disabled = true;

    try {
        const input = {
            HomepageProductId: parseInt(document.getElementById("homepageProductId").value) || 0,
            ProductId: parseInt(document.getElementById("featuredProductSelect").value) || 0,
            DisplayOrder: parseInt(document.getElementById("featuredProductDisplayOrder").value) || 0,
            IsActive: document.getElementById("featuredProductIsActive").checked
        };

        const res = await fetch("/Admin/Homepage/SaveProduct", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(input)
        });
        const json = await res.json();

        if (json.statusCode || json.success) {
            bootstrap.Modal.getInstance(document.getElementById("featuredProductModal")).hide();
            await loadFeaturedProducts();
        } else {
            alert(json.responseMsg || "Failed to save product.");
        }
    } catch (err) {
        console.error("Error saving product:", err);
    } finally {
        btn.disabled = false;
    }
}

async function toggleProductStatus(id) {
    try {
        const res = await fetch(`/Admin/Homepage/ToggleProductStatus/${id}`, { method: "POST" });
        const json = await res.json();
        if (json.statusCode || json.success) {
            await loadFeaturedProducts();
        }
    } catch (err) {
        console.error("Error toggling product status:", err);
    }
}

async function updateProductOrder(id, order) {
    try {
        await fetch("/Admin/Homepage/UpdateProductOrder", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ id: parseInt(id), displayOrder: parseInt(order) || 0 })
        });
    } catch (err) {
        console.error("Error updating product order:", err);
    }
}

async function deleteProduct(id) {
    if (!confirm("Remove this product from homepage?")) return;
    try {
        const res = await fetch(`/Admin/Homepage/DeleteProduct/${id}`, { method: "POST" });
        const json = await res.json();
        if (json.statusCode || json.success) {
            await loadFeaturedProducts();
        }
    } catch (err) {
        console.error("Error deleting product:", err);
    }
}

function escapeHtml(str) {
    if (!str) return '';
    return str
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}
