(function () {
    const page = document.querySelector(".shop-details-page");
    if (!page) {
        return;
    }

    const slug = page.getAttribute("data-product-slug") || "";
    const statusEl = document.getElementById("details-status");
    const missingEl = document.getElementById("details-missing");
    const contentEl = document.getElementById("details-content");
    const mainImage = document.getElementById("details-main-image");
    const imageFallback = document.getElementById("details-image-fallback");
    const thumbs = document.getElementById("details-thumbs");
    const stockBadge = document.getElementById("details-stock-badge");
    const stockText = document.getElementById("details-stock-text");
    const skuEl = document.getElementById("details-sku");
    const variantsEl = document.getElementById("details-variants");
    const actionNote = document.getElementById("details-action-note");
    const buyNow = document.getElementById("details-buy-now");
    const addCart = document.getElementById("details-add-cart");
    const wishBtn = document.getElementById("details-wishlist");

    const REVIEW_PAGE_SIZE = 10;

    const reviewState = {
        pageIndex: 1,
        data: null
    };

    const state = {
        product: null,
        selectedSku: null,
        cartVariantIds: new Set()
    };

    function escapeHtml(value) {
        return String(value || "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;");
    }

    function formatRupees(value) {
        return "₹" + Number(value || 0).toLocaleString("en-IN", { maximumFractionDigits: 2 });
    }

    function stars(rating) {
        return "★★★★★".slice(0, rating) + "☆☆☆☆☆".slice(rating);
    }

    function requestedVariantId() {
        return Number(new URLSearchParams(window.location.search).get("variant") || 0);
    }

    function showActionNote(message, extraHtml, persist) {
        if (window.smartCartFlash) {
            window.smartCartFlash(actionNote, {
                message: message || "",
                html: extraHtml || "",
                persist: !!persist
            });
            return;
        }
        if (!actionNote) {
            return;
        }
        actionNote.textContent = "";
        if (message) {
            actionNote.appendChild(document.createTextNode(message));
        }
        if (extraHtml) {
            actionNote.insertAdjacentHTML("beforeend", extraHtml);
        }
        actionNote.hidden = !message && !extraHtml;
    }

    function productName() {
        return state.product && (state.product.name || state.product.Name) || "Item";
    }

    function variantInCart() {
        var id = state.selectedSku && skuId(state.selectedSku);
        return !!(id && state.cartVariantIds.has(Number(id)));
    }

    function syncCartButton() {
        if (!addCart) {
            return;
        }
        var inCart = variantInCart();
        var inStock = !!(state.selectedSku && (state.selectedSku.inStock || state.selectedSku.InStock));
        addCart.textContent = inCart ? "Go to Cart" : "Add to Cart";
        addCart.disabled = !state.selectedSku || (!inCart && !inStock);
        addCart.classList.toggle("is-in-cart", inCart);
    }

    async function loadCartVariants() {
        try {
            var response = await fetch("/Cart/Get");
            if (!response.ok) {
                return;
            }
            var page = await response.json();
            var items = (page && (page.items || page.Items)) || [];
            state.cartVariantIds = new Set(items.map(function (item) {
                return Number(item.productVariantId || item.ProductVariantId);
            }).filter(Boolean));
        } catch (error) {
            state.cartVariantIds = new Set();
        }
        syncCartButton();
    }

    function readList(product) {
        return {
            skus: product.skus || product.Skus || [],
            groups: product.attributeGroups || product.AttributeGroups || [],
            productImages: product.productImageUrls || product.ProductImageUrls || product.imageUrls || product.ImageUrls || []
        };
    }

    function skuId(sku) {
        return sku.productVariantId || sku.ProductVariantId;
    }

    function skuAttrs(sku) {
        return sku.attributes || sku.Attributes || [];
    }

    function skuImages(sku) {
        return sku.imageUrls || sku.ImageUrls || [];
    }

    function attrValue(attr) {
        return {
            variantId: attr.variantId || attr.VariantId,
            variantValueId: attr.variantValueId || attr.VariantValueId
        };
    }

    function hasValue(sku, variantId, valueId) {
        return skuAttrs(sku).some(function (attr) {
            var pair = attrValue(attr);
            return pair.variantId === variantId && pair.variantValueId === valueId;
        });
    }

    function selectionsFromSku(sku) {
        var selected = {};
        skuAttrs(sku).forEach(function (attr) {
            var pair = attrValue(attr);
            selected[pair.variantId] = pair.variantValueId;
        });
        return selected;
    }

    function matchesSelections(sku, selections) {
        return Object.keys(selections).every(function (variantId) {
            return hasValue(sku, Number(variantId), selections[variantId]);
        });
    }

    function findExactSku(skus, selections) {
        var matches = skus.filter(function (sku) { return matchesSelections(sku, selections); });
        return matches.find(function (sku) { return sku.inStock || sku.InStock; }) || matches[0] || null;
    }

    function findSkuForValue(skus, variantId, valueId) {
        return skus.find(function (sku) { return (sku.inStock || sku.InStock) && hasValue(sku, variantId, valueId); })
            || skus.find(function (sku) { return hasValue(sku, variantId, valueId); })
            || null;
    }

    function setMainImage(url, name) {
        if (!mainImage || !imageFallback) {
            return;
        }

        if (url) {
            mainImage.src = url;
            mainImage.alt = name || "";
            mainImage.hidden = false;
            imageFallback.hidden = true;
        } else {
            mainImage.removeAttribute("src");
            mainImage.hidden = true;
            imageFallback.hidden = false;
        }
    }

    function renderGallery(urls, name) {
        var images = (urls || []).filter(Boolean);
        setMainImage(images[0] || "", name);

        if (!thumbs) {
            return;
        }

        thumbs.innerHTML = images.map(function (url, index) {
            return '<button type="button" class="shop-details-thumb' + (index === 0 ? " is-active" : "") + '" data-image="' + escapeHtml(url) + '">' +
                '<img src="' + escapeHtml(url) + '" alt="">' +
                "</button>";
        }).join("");

        thumbs.querySelectorAll(".shop-details-thumb").forEach(function (button) {
            button.addEventListener("click", function () {
                thumbs.querySelectorAll(".shop-details-thumb").forEach(function (item) {
                    item.classList.remove("is-active");
                });
                button.classList.add("is-active");
                setMainImage(button.getAttribute("data-image"), name);
            });
        });
    }

    /* ------------------------------------------------------------------
       Ratings & reviews — READ ONLY on this page (data from /Reviews/Get).
       Reviews are written from My Orders > Order details, per purchased
       order item, so there is no review form here.
       ------------------------------------------------------------------ */

    function byId(id) {
        return document.getElementById(id);
    }

    function plural(count, word) {
        return count + " " + word + (count === 1 ? "" : "s");
    }

    function formatReviewDate(value) {
        if (!value) {
            return "";
        }
        var date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return "";
        }
        return date.toLocaleDateString("en-IN", { day: "2-digit", month: "short", year: "numeric" });
    }

    function renderTopRating(summary) {
        var link = byId("details-rating");
        var badge = byId("details-rating-badge");
        var text = byId("details-rating-text");
        if (!link || !badge || !text) {
            return;
        }

        var total = Number(summary.totalRatings || 0);
        var reviews = Number(summary.totalReviews || 0);
        if (total > 0) {
            badge.textContent = Number(summary.averageRating || 0).toFixed(1) + " ★";
            badge.hidden = false;
            text.textContent = plural(total, "Rating") + " | " + plural(reviews, "Review");
        } else {
            badge.hidden = true;
            text.textContent = "No ratings yet";
        }
        link.hidden = false;
    }

    function renderReviewSummary(data) {
        var summary = data.summary || {};
        var total = Number(summary.totalRatings || 0);
        var reviews = Number(summary.totalReviews || 0);
        var average = Number(summary.averageRating || 0);

        byId("reviews-average").textContent = total > 0 ? average.toFixed(1) : "0.0";
        byId("reviews-average-stars").textContent = stars(total > 0 ? Math.round(average) : 0);
        byId("reviews-count").textContent = total > 0
            ? plural(total, "rating") + " · " + plural(reviews, "review")
            : "No ratings yet";

        byId("reviews-breakdown").innerHTML = (data.distribution || []).map(function (row) {
            var percent = Math.max(0, Math.min(100, Number(row.percentage || 0)));
            var count = Number(row.ratingCount || 0);
            return '<div class="shop-reviews-bar-row" title="' + escapeHtml(plural(count, "rating")) + '">' +
                '<span class="shop-reviews-bar-label">' + Number(row.stars) + " ★</span>" +
                '<span class="shop-reviews-bar"><span class="shop-reviews-bar-fill" style="width:' + percent + '%"></span></span>' +
                '<span class="shop-reviews-bar-pct">' + percent + "%</span>" +
                "</div>";
        }).join("");

        renderTopRating(summary);
    }

    function renderReviewList(data) {
        var list = byId("reviews-list");
        var empty = byId("reviews-empty");
        var reviews = data.reviews || [];
        var total = Number((data.summary || {}).totalRatings || 0);

        empty.hidden = total > 0;

        list.innerHTML = reviews.map(function (review) {
            var rating = Number(review.rating || 0);
            var date = formatReviewDate(review.createdAt);
            var body = review.review ? "<p>" + escapeHtml(review.review) + "</p>" : "";
            var mine = !!review.isMine;
            var orderId = Number(review.myOrderId || 0);
            return '<article class="shop-review-card' + (mine ? " is-mine" : "") + '">' +
                '<div class="shop-review-head">' +
                    "<strong>" + escapeHtml(review.reviewerName || "Customer") + "</strong>" +
                    (mine ? '<span class="shop-review-you">You</span>' : "") +
                    '<span class="shop-reviews-stars" role="img" aria-label="' + rating + ' out of 5 stars">' + stars(rating) + "</span>" +
                    (review.isVerifiedPurchase ? '<span class="shop-review-verified">✔ Verified Purchase</span>' : "") +
                    (date ? '<span class="shop-review-date">' + escapeHtml(date) + "</span>" : "") +
                "</div>" +
                body +
                (mine && orderId > 0
                    ? '<a class="shop-review-link" href="/Orders/Details/' + orderId + '">View in your order</a>'
                    : "") +
                "</article>";
        }).join("");

        var pager = byId("reviews-pager");
        var totalPages = Number(data.totalPages || 0);
        var pageIndex = Number(data.pageIndex || 1);
        pager.hidden = totalPages <= 1;
        byId("reviews-page-info").textContent = "Page " + pageIndex + " of " + Math.max(totalPages, 1);
        byId("reviews-prev").disabled = pageIndex <= 1;
        byId("reviews-next").disabled = pageIndex >= totalPages;
    }

    function renderReviews(data) {
        reviewState.data = data;
        reviewState.pageIndex = Number(data.pageIndex || 1);

        renderReviewSummary(data);
        renderReviewList(data);
    }

    function showReviewsError(message) {
        byId("reviews-status").hidden = true;
        byId("reviews-error-text").textContent = message || "Could not load reviews.";
        byId("reviews-error").hidden = false;
        if (!reviewState.data) {
            byId("reviews-body").hidden = true;
        }
    }

    async function loadReviews(pageIndex) {
        var id = productId();
        if (!id || !byId("reviews-body")) {
            return;
        }

        var body = byId("reviews-body");
        var firstLoad = !reviewState.data;
        byId("reviews-error").hidden = true;
        byId("reviews-status").hidden = !firstLoad;
        body.classList.toggle("is-loading", !firstLoad);

        try {
            var url = "/Reviews/Get?productId=" + encodeURIComponent(id) +
                "&pageIndex=" + encodeURIComponent(pageIndex || 1) +
                "&pageSize=" + REVIEW_PAGE_SIZE;
            var response = await fetch(url);
            if (response.status === 404) {
                showReviewsError("Reviews are not available for this product.");
                return;
            }
            if (!response.ok) {
                throw new Error("reviews failed");
            }

            var data = await response.json();
            byId("reviews-status").hidden = true;
            body.hidden = false;
            renderReviews(data);
        } catch (error) {
            showReviewsError("Could not load reviews. Please check your connection and try again.");
        } finally {
            body.classList.remove("is-loading");
        }
    }

    function bindReviews() {
        var retry = byId("reviews-retry");
        if (!retry) {
            return;
        }

        retry.addEventListener("click", function () {
            loadReviews(reviewState.pageIndex);
        });
        byId("reviews-prev").addEventListener("click", function () {
            loadReviews(reviewState.pageIndex - 1).then(scrollToReviews);
        });
        byId("reviews-next").addEventListener("click", function () {
            loadReviews(reviewState.pageIndex + 1).then(scrollToReviews);
        });
    }

    function scrollToReviews() {
        var section = byId("reviews");
        if (section) {
            section.scrollIntoView({ behavior: "smooth", block: "start" });
        }
    }

    function currentImages() {
        var data = readList(state.product);
        var skuPics = state.selectedSku ? skuImages(state.selectedSku) : [];
        return skuPics.length > 0 ? skuPics : data.productImages;
    }

    function applySku(sku, renderOptions) {
        state.selectedSku = sku;
        var product = state.product;
        var name = product.name || product.Name || "";
        var inStock = !!(sku && (sku.inStock || sku.InStock));
        var price = sku ? (sku.price || sku.Price || 0) : 0;
        var mrp = sku ? (sku.mrp || sku.MRP || 0) : 0;

        document.getElementById("details-price").textContent = formatRupees(price);
        var mrpEl = document.getElementById("details-mrp");
        if (mrp > price) {
            mrpEl.textContent = formatRupees(mrp);
            mrpEl.hidden = false;
        } else {
            mrpEl.hidden = true;
        }

        if (skuEl) {
            skuEl.textContent = sku && (sku.sku || sku.SKU)
                ? "SKU: " + (sku.sku || sku.SKU)
                : "";
        }

        if (stockText) {
            stockText.textContent = inStock ? "In Stock" : "Out of Stock";
            stockText.classList.toggle("is-out", !inStock);
        }
        if (stockBadge) {
            stockBadge.hidden = inStock;
        }

        if (buyNow) buyNow.disabled = !sku || !inStock;
        syncCartButton();

        renderGallery(currentImages(), name);
        if (renderOptions) {
            renderVariantOptions();
        }
    }

    function valueStatus(variantId, valueId, selections, skus) {
        var trial = Object.assign({}, selections, {});
        trial[variantId] = valueId;
        var exact = findExactSku(skus, trial);
        if (exact) {
            return (exact.inStock || exact.InStock) ? "available" : "oos";
        }
        return "invalid";
    }

    function renderVariantOptions() {
        if (!variantsEl || !state.product) {
            return;
        }

        var data = readList(state.product);
        var selections = state.selectedSku ? selectionsFromSku(state.selectedSku) : {};
        var html = "";

        if (data.groups.length > 0) {
            html = data.groups.map(function (group) {
                var variantId = group.variantId || group.VariantId;
                var name = group.name || group.Name;
                var values = group.values || group.Values || [];
                var buttons = values.map(function (value) {
                    var valueId = value.variantValueId || value.VariantValueId;
                    var label = value.name || value.Name;
                    var selected = selections[variantId] === valueId;
                    var status = valueStatus(variantId, valueId, selections, data.skus);
                    var classes = "shop-variant-option";
                    if (selected) classes += " is-selected";
                    if (status === "oos") classes += " is-out";
                    if (status === "invalid") classes += " is-invalid";
                    return '<button type="button" class="' + classes + '" data-variant="' + variantId + '" data-value="' + valueId + '">' +
                        escapeHtml(label) +
                        (status === "oos" ? '<span class="shop-variant-oos">Out of stock</span>' : "") +
                        "</button>";
                }).join("");
                return '<div class="shop-variant-group"><p class="shop-variant-label">' + escapeHtml(name) + '</p><div class="shop-variant-options">' + buttons + "</div></div>";
            }).join("");
        } else if (data.skus.length > 1) {
            html = '<div class="shop-variant-group"><p class="shop-variant-label">Options</p><div class="shop-variant-options">' +
                data.skus.map(function (sku) {
                    var selected = state.selectedSku && skuId(sku) === skuId(state.selectedSku);
                    var inStock = !!(sku.inStock || sku.InStock);
                    var classes = "shop-variant-option";
                    if (selected) classes += " is-selected";
                    if (!inStock) classes += " is-out";
                    return '<button type="button" class="' + classes + '" data-sku="' + skuId(sku) + '">' +
                        escapeHtml(sku.label || sku.Label || sku.sku || sku.SKU) +
                        (inStock ? "" : '<span class="shop-variant-oos">Out of stock</span>') +
                        "</button>";
                }).join("") +
                "</div></div>";
        }

        variantsEl.innerHTML = html;

        variantsEl.querySelectorAll("[data-variant]").forEach(function (button) {
            button.addEventListener("click", function () {
                var variantId = Number(button.getAttribute("data-variant"));
                var valueId = Number(button.getAttribute("data-value"));
                var next = Object.assign({}, selectionsFromSku(state.selectedSku || {}), {});
                next[variantId] = valueId;
                var sku = findExactSku(data.skus, next) || findSkuForValue(data.skus, variantId, valueId);
                if (sku) {
                    applySku(sku, true);
                }
            });
        });

        variantsEl.querySelectorAll("[data-sku]").forEach(function (button) {
            button.addEventListener("click", function () {
                var id = Number(button.getAttribute("data-sku"));
                var sku = data.skus.find(function (item) { return skuId(item) === id; });
                if (sku) {
                    applySku(sku, true);
                }
            });
        });
    }

    function renderProduct(product) {
        state.product = product;
        var data = readList(product);
        var name = product.name || product.Name || "";
        var category = product.categoryName || product.CategoryName || "";
        var subcategory = product.subCategoryName || product.SubCategoryName || "";
        var brand = product.brandName || product.BrandName || "";
        var description = product.description || product.Description || "";
        var selectedId = requestedVariantId() || product.selectedVariantId || product.SelectedVariantId || 0;

        document.getElementById("details-crumb-name").textContent = name;
        document.getElementById("details-name").textContent = name;
        document.getElementById("details-meta").textContent = [category, subcategory, brand].filter(Boolean).join(" · ");
        document.getElementById("details-description").textContent = description || "No description is available for this product.";

        var selected = data.skus.find(function (sku) { return skuId(sku) === selectedId; }) || data.skus[0] || null;
        applySku(selected, true);
        loadReviews(1);

        if (statusEl) statusEl.hidden = true;
        if (contentEl) contentEl.hidden = false;
        loadWishState();
        loadCartVariants();
    }

    function selectedLabel() {
        if (!state.selectedSku) {
            return "";
        }
        var attrs = skuAttrs(state.selectedSku).map(function (attr) {
            return (attr.variantValueName || attr.VariantValueName);
        }).filter(Boolean);
        return attrs.length > 0
            ? attrs.join(" / ")
            : (state.selectedSku.label || state.selectedSku.Label || state.selectedSku.sku || state.selectedSku.SKU);
    }

    function productId() {
        return state.product && (state.product.productId || state.product.ProductId) || 0;
    }

    function setWishState(active) {
        if (!wishBtn) {
            return;
        }
        wishBtn.classList.toggle("is-active", !!active);
        wishBtn.setAttribute("aria-pressed", active ? "true" : "false");
        wishBtn.setAttribute("aria-label", active ? "Remove from wishlist" : "Add to wishlist");
    }

    async function postJson(url, body) {
        var response = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });
        if (window.smartCartHandleAuth && window.smartCartHandleAuth(response)) {
            return null;
        }
        if (!response.ok) {
            throw new Error("Request failed");
        }
        return response.json();
    }

    async function addSelectedSku(redirectToCart) {
        if (!state.selectedSku || !(state.selectedSku.inStock || state.selectedSku.InStock)) {
            showActionNote("This variant is out of stock.", "", true);
            return false;
        }

        var result = await postJson("/Cart/Add", {
            productVariantId: skuId(state.selectedSku),
            quantity: 1
        });
        if (!result) {
            return false;
        }

        if (!(result.statusCode || result.StatusCode)) {
            showActionNote(result.responseMsg || result.ResponseMsg || "Could not add to cart.", "", true);
            return false;
        }

        state.cartVariantIds.add(Number(skuId(state.selectedSku)));
        syncCartButton();

        if (window.refreshSmartCartBag) {
            window.refreshSmartCartBag();
        }

        if (redirectToCart) {
            window.location.href = "/Cart";
            return true;
        }

        showActionNote("Added " + selectedLabel() + " to cart.");
        return true;
    }

    async function loadWishState() {
        var id = productId();
        if (!id || !wishBtn) {
            return;
        }

        try {
            var response = await fetch("/Wishlist/Status?productId=" + encodeURIComponent(id));
            if (!response.ok) {
                return;
            }
            var status = await response.json();
            setWishState(!!(status.inWishlist || status.InWishlist));
        } catch (error) {
            /* keep default heart */
        }
    }

    function bindActions() {
        if (buyNow) {
            buyNow.addEventListener("click", function () {
                if (!state.selectedSku || !(state.selectedSku.inStock || state.selectedSku.InStock)) {
                    showActionNote("This variant is out of stock.", "", true);
                    return;
                }
                window.location.href = "/Checkout?buyNow=1&variantId=" + encodeURIComponent(skuId(state.selectedSku)) + "&qty=1";
            });
        }
        if (addCart) {
            addCart.addEventListener("click", async function () {
                if (variantInCart()) {
                    window.location.href = "/Cart";
                    return;
                }
                try {
                    await addSelectedSku(false);
                } catch (error) {
                    showActionNote("Could not add to cart.", "", true);
                }
            });
        }
        if (wishBtn) {
            wishBtn.addEventListener("click", async function () {
                var id = productId();
                if (!id) {
                    return;
                }
                try {
                    var result = await postJson("/Wishlist/Toggle", { productId: id });
                    if (!result) {
                        return;
                    }
                    if (!(result.statusCode || result.StatusCode)) {
                        showActionNote(result.responseMsg || result.ResponseMsg || "Could not update wishlist.", "", true);
                        return;
                    }
                    var added = Number(result.responseCode || result.ResponseCode) > 0;
                    setWishState(added);
                    showActionNote(productName() + (added ? " added to wishlist." : " removed from wishlist."));
                    if (window.refreshSmartCartBag) {
                        window.refreshSmartCartBag();
                    }
                } catch (error) {
                    showActionNote("Could not update wishlist.", "", true);
                }
            });
        }
    }

    async function loadProduct() {
        if (!slug) {
            if (statusEl) statusEl.hidden = true;
            if (missingEl) missingEl.hidden = false;
            return;
        }

        try {
            var response = await fetch("/Shop/GetProduct/" + encodeURIComponent(slug));
            if (!response.ok) {
                throw new Error("not found");
            }
            renderProduct(await response.json());
        } catch (error) {
            if (statusEl) statusEl.hidden = true;
            if (missingEl) missingEl.hidden = false;
        }
    }

    bindActions();
    bindReviews();
    loadProduct();
})();
