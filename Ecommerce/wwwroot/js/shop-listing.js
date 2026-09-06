(function () {
    const pageSize = 10;
    const grid = document.getElementById("shop-product-grid");
    const countEl = document.getElementById("shop-showing-count");
    const loaderEl = document.getElementById("shop-loader");
    const endEl = document.getElementById("shop-end");
    const emptyEl = document.getElementById("shop-empty");
    const sentinel = document.getElementById("shop-scroll-sentinel");
    const searchInput = document.getElementById("shop-search");
    const categorySelect = document.getElementById("shop-category");
    const subcategorySelect = document.getElementById("shop-subcategory");
    const brandSelect = document.getElementById("shop-brand");
    const priceFromInput = document.getElementById("shop-price-from");
    const priceToInput = document.getElementById("shop-price-to");
    const sortSelect = document.getElementById("shop-sort");
    const applyBtn = document.getElementById("shop-apply-filters");
    const resetBtn = document.getElementById("shop-reset-filters");
    const searchForm = document.getElementById("shop-search-form");

    if (!grid) {
        return;
    }

    const state = {
        pageIndex: 1,
        totalCount: 0,
        loadedCount: 0,
        loading: false,
        done: false,
        allSubCategories: []
    };

    function queryValue(name) {
        return new URLSearchParams(window.location.search).get(name) || "";
    }

    function currentFilters() {
        const sort = (sortSelect && sortSelect.value ? sortSelect.value : "4:DESC").split(":");
        return {
            searchName: (searchInput && searchInput.value || "").trim(),
            categoryIds: categorySelect && categorySelect.value ? categorySelect.value : "",
            subCategoryIds: subcategorySelect && subcategorySelect.value ? subcategorySelect.value : "",
            brandIds: brandSelect && brandSelect.value ? brandSelect.value : "",
            priceFrom: priceFromInput && priceFromInput.value ? Number(priceFromInput.value) : null,
            priceTo: priceToInput && priceToInput.value ? Number(priceToInput.value) : null,
            sortColumn: Number(sort[0] || 4),
            sortMode: sort[1] || "DESC"
        };
    }

    function escapeHtml(value) {
        return String(value || "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;");
    }

    function formatRupees(value) {
        const amount = Number(value || 0);
        return "₹" + amount.toLocaleString("en-IN", { maximumFractionDigits: 2 });
    }

    function detailsUrl(product) {
        const key = product.slug || product.productId;
        return "/Shop/Details/" + encodeURIComponent(key);
    }

    function fillSelect(select, options, placeholder, selected) {
        if (!select) {
            return;
        }

        const current = selected || "";
        select.innerHTML = "";
        const first = document.createElement("option");
        first.value = "";
        first.textContent = placeholder;
        select.appendChild(first);

        options.forEach(function (option) {
            const el = document.createElement("option");
            el.value = String(option.id);
            el.textContent = option.name;
            if (String(option.id) === String(current)) {
                el.selected = true;
            }
            select.appendChild(el);
        });
    }

    function refreshSubcategories() {
        const categoryId = categorySelect && categorySelect.value ? Number(categorySelect.value) : 0;
        const selected = subcategorySelect ? subcategorySelect.value : "";
        const rows = categoryId
            ? state.allSubCategories.filter(function (row) { return row.categoryId === categoryId; })
            : state.allSubCategories;
        fillSelect(subcategorySelect, rows, "All subcategories", selected);
    }

    function renderCard(product) {
        const name = escapeHtml(product.name);
        const href = detailsUrl(product);
        const subtitle = [product.categoryName, product.brandName].filter(Boolean).join(" · ");
        const inStock = !!product.inStock;
        const image = product.imageUrl
            ? '<img class="product__items--img product__primary--img" src="' + escapeHtml(product.imageUrl) + '" alt="' + name + '">'
            : '<div class="product__items--img-fallback">No image</div>';
        const badge = inStock ? "" : '<div class="product__badge"><span class="product__badge--items">Out of stock</span></div>';
        const mrp = Number(product.mrp || 0);
        const price = Number(product.price || 0);
        const oldPrice = mrp > price
            ? '<span class="old__price">' + formatRupees(mrp) + "</span>"
            : "";

        return (
            '<article class="product__items' + (inStock ? "" : " is-out-of-stock") + '">' +
                '<div class="product__items--thumbnail">' +
                    '<a class="product__items--link" href="' + href + '">' + image + "</a>" +
                    badge +
                "</div>" +
                '<div class="product__items--content">' +
                    '<span class="product__items--content__subtitle">' + escapeHtml(subtitle) + "</span>" +
                    '<h3 class="product__items--content__title h4"><a href="' + href + '">' + name + "</a></h3>" +
                    '<div class="product__items--price">' +
                        '<span class="current__price">' + formatRupees(price) + "</span>" +
                        oldPrice +
                    "</div>" +
                "</div>" +
            "</article>"
        );
    }

    function updateStatus(append) {
        if (countEl) {
            if (state.totalCount === 0) {
                countEl.textContent = "Showing 0 products";
            } else {
                countEl.textContent = "Showing " + state.loadedCount + " of " + state.totalCount + " products";
            }
        }

        if (emptyEl) {
            emptyEl.hidden = state.loadedCount > 0 || state.loading;
        }
        if (endEl) {
            endEl.hidden = !(state.done && state.loadedCount > 0);
        }
        if (loaderEl) {
            loaderEl.hidden = !state.loading;
        }
        if (!append && !state.loading && state.loadedCount === 0) {
            grid.innerHTML = "";
        }
    }

    async function loadPage(reset) {
        if (state.loading || (!reset && state.done)) {
            return;
        }

        if (reset) {
            state.pageIndex = 1;
            state.loadedCount = 0;
            state.totalCount = 0;
            state.done = false;
            grid.innerHTML = "";
        }

        state.loading = true;
        updateStatus(!reset);

        const filters = currentFilters();
        const body = {
            pageIndex: state.pageIndex,
            pageSize: pageSize,
            searchName: filters.searchName,
            sortColumn: filters.sortColumn,
            sortMode: filters.sortMode,
            categoryIds: filters.categoryIds,
            subCategoryIds: filters.subCategoryIds,
            brandIds: filters.brandIds,
            priceFrom: filters.priceFrom,
            priceTo: filters.priceTo
        };

        try {
            const response = await fetch("/Shop/GetProducts", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(body)
            });

            if (!response.ok) {
                throw new Error("Could not load products");
            }

            const data = await response.json();
            const rows = data.tableData || data.TableData || [];
            const settings = data.tableSettings || data.TableSettings || {};
            state.totalCount = Number(settings.totalCount || settings.TotalCount || 0);

            if (rows.length > 0) {
                grid.insertAdjacentHTML("beforeend", rows.map(renderCard).join(""));
                state.loadedCount += rows.length;
            }

            state.pageIndex += 1;
            state.done = state.loadedCount >= state.totalCount || rows.length === 0;
        } catch (error) {
            state.done = true;
            if (emptyEl && state.loadedCount === 0) {
                emptyEl.textContent = "Could not load products.";
                emptyEl.hidden = false;
            }
        } finally {
            state.loading = false;
            updateStatus(true);
            window.requestAnimationFrame(function () {
                if (!state.done && isSentinelVisible()) {
                    loadPage(false);
                }
            });
        }
    }

    function isSentinelVisible() {
        if (!sentinel) {
            return false;
        }

        var rect = sentinel.getBoundingClientRect();
        return rect.top < (window.innerHeight + 240);
    }

    function bindFilters() {
        if (searchForm) {
            searchForm.addEventListener("submit", function (event) {
                event.preventDefault();
                loadPage(true);
            });
        }
        if (applyBtn) {
            applyBtn.addEventListener("click", function () { loadPage(true); });
        }
        if (resetBtn) {
            resetBtn.addEventListener("click", function () {
                if (searchInput) searchInput.value = "";
                if (categorySelect) categorySelect.value = "";
                if (brandSelect) brandSelect.value = "";
                if (priceFromInput) priceFromInput.value = "";
                if (priceToInput) priceToInput.value = "";
                if (sortSelect) sortSelect.value = "4:DESC";
                refreshSubcategories();
                if (subcategorySelect) subcategorySelect.value = "";
                loadPage(true);
            });
        }
        if (categorySelect) {
            categorySelect.addEventListener("change", refreshSubcategories);
        }
        if (sortSelect) {
            sortSelect.addEventListener("change", function () { loadPage(true); });
        }
    }

    async function loadFilters() {
        const response = await fetch("/Shop/GetFilters");
        if (!response.ok) {
            return;
        }

        const data = await response.json();
        state.allSubCategories = data.subCategories || data.SubCategories || [];
        fillSelect(categorySelect, data.categories || data.Categories || [], "All categories", queryValue("category"));
        fillSelect(brandSelect, data.brands || data.Brands || [], "All brands", queryValue("brand"));
        refreshSubcategories();
        if (subcategorySelect && queryValue("subcategory")) {
            subcategorySelect.value = queryValue("subcategory");
        }
    }

    function bootstrapFromQuery() {
        if (searchInput) searchInput.value = queryValue("search");
        if (priceFromInput) priceFromInput.value = queryValue("priceFrom");
        if (priceToInput) priceToInput.value = queryValue("priceTo");
    }

    if (sentinel && "IntersectionObserver" in window) {
        const observer = new IntersectionObserver(function (entries) {
            if (entries.some(function (entry) { return entry.isIntersecting; })) {
                loadPage(false);
            }
        }, { rootMargin: "240px 0px" });
        observer.observe(sentinel);
    }

    bindFilters();
    bootstrapFromQuery();
    loadFilters().finally(function () { loadPage(true); });
})();
