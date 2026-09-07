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

    const dummyReviews = [
        {
            name: "Anita Sharma",
            rating: 5,
            date: "12 Aug 2026",
            title: "Fresh and as described",
            text: "Quality matched the listing. Packing was neat and delivery was on time."
        },
        {
            name: "Rahul Menon",
            rating: 4,
            date: "3 Aug 2026",
            title: "Good value",
            text: "Price was fair and the product was fine. Would buy again from this brand."
        },
        {
            name: "Sneha Iyer",
            rating: 4,
            date: "21 Jul 2026",
            title: "Satisfied overall",
            text: "Description was accurate. Dummy review for UI testing only."
        }
    ];

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

    function renderReviews() {
        var list = document.getElementById("reviews-list");
        if (!list) {
            return;
        }

        list.innerHTML = dummyReviews.map(function (review) {
            return '<article class="shop-review-card">' +
                '<div class="shop-review-head">' +
                    "<strong>" + escapeHtml(review.name) + "</strong>" +
                    '<span class="shop-reviews-stars">' + stars(review.rating) + "</span>" +
                    '<span class="shop-review-date">' + escapeHtml(review.date) + "</span>" +
                "</div>" +
                '<h3 class="shop-review-title">' + escapeHtml(review.title) + "</h3>" +
                "<p>" + escapeHtml(review.text) + "</p>" +
                "</article>";
        }).join("");
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
        renderReviews();

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
            buyNow.addEventListener("click", async function () {
                try {
                    await addSelectedSku(true);
                } catch (error) {
                    showActionNote("Could not add to cart.", "", true);
                }
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
    loadProduct();
})();
