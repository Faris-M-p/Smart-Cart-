(function () {
    var list = document.getElementById("cart-list");
    var empty = document.getElementById("cart-empty");
    var summary = document.getElementById("cart-summary");
    var subtotal = document.getElementById("cart-subtotal");
    var note = document.getElementById("cart-status");
    var guide = document.getElementById("cart-guide");
    if (!list) {
        return;
    }

    var summaryActions = document.querySelector("#cart-summary > div");
    if (summaryActions) {
        summaryActions.classList.add("shop-bag-summary-actions");
    }

    if (empty && !empty.querySelector(".shop-bag-empty-icon")) {
        empty.className = "shop-bag-empty";
        empty.innerHTML =
            '<div class="shop-bag-empty-icon" aria-hidden="true">' +
                '<svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 14.706 13.534"><path d="M4.738,472.271h7.814a.434.434,0,0,0,.414-.328l1.723-6.316a.466.466,0,0,0-.071-.4.424.424,0,0,0-.344-.179H3.745L3.437,463.6a.435.435,0,0,0-.421-.353H.431a.451.451,0,0,0,0,.9h2.24c.054.257,1.474,6.946,1.555,7.33a1.36,1.36,0,0,0-.779,1.242,1.326,1.326,0,0,0,1.293,1.354h7.812a.452.452,0,0,0,0-.9H4.74a.451.451,0,0,1,0-.9Zm8.966-6.317-1.477,5.414H5.085l-1.149-5.414Z" transform="translate(0 -463.248)" fill="currentColor"/><path d="M5.5,478.8a1.294,1.294,0,1,0,1.293-1.353A1.325,1.325,0,0,0,5.5,478.8Zm1.293-.451a.452.452,0,1,1-.431.451A.442.442,0,0,1,6.793,478.352Z" transform="translate(-1.191 -466.622)" fill="currentColor"/><path d="M13.273,478.8a1.294,1.294,0,1,0,1.293-1.353A1.325,1.325,0,0,0,13.273,478.8Zm1.293-.451a.452.452,0,1,1-.431.451A.442.442,0,0,1,14.566,478.352Z" transform="translate(-2.875 -466.622)" fill="currentColor"/></svg>' +
            "</div>" +
            "<h2>Your cart is empty.</h2>" +
            "<p>Add a product from the shop to see it here.</p>" +
            '<a class="shop-bag-cta" href="/Shop">Browse the Shop</a>';
    }

    function escapeHtml(value) {
        return String(value || "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;");
    }

    function money(value) {
        return "₹" + Number(value || 0).toLocaleString("en-IN", { maximumFractionDigits: 2 });
    }

    function detailsUrl(item) {
        var key = item.slug || item.Slug || item.productId || item.ProductId;
        if (!key) {
            return "/Shop";
        }
        var href = "/Shop/Details/" + encodeURIComponent(key);
        var variantId = item.productVariantId || item.ProductVariantId;
        if (variantId) {
            href += "?variant=" + encodeURIComponent(variantId);
        }
        return href;
    }

    function showNote(message, persist) {
        if (window.smartCartFlash) {
            window.smartCartFlash(note, { message: message, persist: !!persist });
            return;
        }
        if (!note) {
            return;
        }
        note.textContent = message;
        note.hidden = !message;
    }

    async function post(url, body) {
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

    function render(page) {
        var items = (page && (page.items || page.Items)) || [];
        var totals = (page && (page.summary || page.Summary)) || {};
        list.innerHTML = items.map(function (item) {
            var image = item.imageUrl || item.ImageUrl;
            var inStock = !!(item.inStock || item.InStock);
            var href = detailsUrl(item);
            var name = escapeHtml(item.name || item.Name);
            var media = image
                ? '<img src="' + escapeHtml(image) + '" alt="' + name + '">'
                : '<div class="shop-cart-fallback">No image</div>';
            return '<article class="shop-cart-line">' +
                '<a class="shop-bag-media" href="' + escapeHtml(href) + '">' + media + "</a>" +
                "<div>" +
                    "<h3><a href=\"" + escapeHtml(href) + "\">" + name + "</a></h3>" +
                    '<p class="shop-cart-meta">' + escapeHtml(item.label || item.Label || item.sku || item.SKU) + "</p>" +
                    '<p class="shop-cart-price">' + money(item.price || item.Price) +
                        ((item.mrp || item.MRP) > (item.price || item.Price)
                            ? '<span class="shop-cart-old">' + money(item.mrp || item.MRP) + "</span>"
                            : "") +
                    "</p>" +
                    (inStock ? "" : '<span class="shop-cart-oos">Out of stock</span>') +
                    '<a class="shop-bag-view" href="' + escapeHtml(href) + '">View product</a>' +
                "</div>" +
                '<div class="shop-cart-actions">' +
                    '<div class="shop-qty">' +
                        '<button type="button" data-update="' + (item.cartItemId || item.CartItemId) + '" data-qty="' + ((item.quantity || item.Quantity) - 1) + '">−</button>' +
                        "<span>" + (item.quantity || item.Quantity) + "</span>" +
                        '<button type="button" data-update="' + (item.cartItemId || item.CartItemId) + '" data-qty="' + ((item.quantity || item.Quantity) + 1) + '"' + (inStock ? "" : " disabled") + ">+</button>" +
                    "</div>" +
                    '<button class="shop-bag-remove" type="button" data-remove="' + (item.cartItemId || item.CartItemId) + '">Remove</button>' +
                "</div>" +
            "</article>";
        }).join("");

        var isEmpty = items.length === 0;
        list.hidden = isEmpty;
        if (empty) empty.hidden = !isEmpty;
        if (summary) summary.hidden = isEmpty;
        if (guide) guide.hidden = isEmpty;
        if (subtotal) subtotal.textContent = money(totals.subtotal || totals.Subtotal);
        if (window.refreshSmartCartBag) {
            window.refreshSmartCartBag();
        }
    }

    async function load() {
        try {
            var response = await fetch("/Cart/Get");
            if (window.smartCartHandleAuth && window.smartCartHandleAuth(response)) {
                return;
            }
            if (!response.ok) {
                throw new Error("not found");
            }
            render(await response.json());
        } catch (error) {
            showNote("Could not load your cart.", true);
        }
    }

    document.addEventListener("click", async function (event) {
        if (!event.target.closest("#cart-list, #cart-summary")) {
            return;
        }
        var button = event.target.closest("button");
        if (!button) {
            return;
        }

        try {
            var result;
            if (button.hasAttribute("data-remove")) {
                result = await post("/Cart/Remove", { cartItemId: Number(button.getAttribute("data-remove")) });
            } else if (button.hasAttribute("data-clear")) {
                result = await post("/Cart/Clear", {});
            } else if (button.hasAttribute("data-update")) {
                result = await post("/Cart/Update", {
                    cartItemId: Number(button.getAttribute("data-update")),
                    quantity: Number(button.getAttribute("data-qty"))
                });
            } else {
                return;
            }

            if (!result) {
                return;
            }
            if (!(result.statusCode || result.StatusCode)) {
                showNote(result.responseMsg || result.ResponseMsg || "Could not update cart.", true);
                return;
            }
            await load();
            if (button.hasAttribute("data-remove")) {
                showNote("Item removed from cart.");
            } else if (button.hasAttribute("data-clear")) {
                showNote("Cart cleared.");
            }
        } catch (error) {
            showNote("Could not update cart.", true);
        }
    });

    load();
})();
