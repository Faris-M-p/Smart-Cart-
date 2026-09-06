(function () {
    var list = document.getElementById("cart-list");
    var empty = document.getElementById("cart-empty");
    var summary = document.getElementById("cart-summary");
    var subtotal = document.getElementById("cart-subtotal");
    var note = document.getElementById("cart-status");
    if (!list) {
        return;
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

    function showNote(message) {
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
            return '<article class="shop-cart-line">' +
                (image
                    ? '<img src="' + escapeHtml(image) + '" alt="">'
                    : '<div class="shop-cart-fallback">No image</div>') +
                '<div>' +
                    '<h3><a href="/Shop/Details/' + encodeURIComponent(item.slug || item.Slug || "") + '">' + escapeHtml(item.name || item.Name) + "</a></h3>" +
                    '<p class="shop-cart-meta">' + escapeHtml(item.label || item.Label || item.sku || item.SKU) + "</p>" +
                    '<p class="shop-cart-price">' + money(item.price || item.Price) +
                        ((item.mrp || item.MRP) > (item.price || item.Price)
                            ? '<span class="shop-cart-old">' + money(item.mrp || item.MRP) + "</span>"
                            : "") +
                    "</p>" +
                    (inStock ? "" : '<span class="shop-cart-oos">Out of stock</span>') +
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
        if (empty) empty.hidden = !isEmpty;
        if (summary) summary.hidden = isEmpty;
        if (subtotal) subtotal.textContent = money(totals.subtotal || totals.Subtotal);
        if (window.refreshSmartCartBag) {
            window.refreshSmartCartBag();
        }
    }

    async function load() {
        showNote("Loading cart...");
        try {
            var response = await fetch("/Cart/Get");
            if (window.smartCartHandleAuth && window.smartCartHandleAuth(response)) {
                return;
            }
            if (!response.ok) {
                throw new Error("not found");
            }
            render(await response.json());
            showNote("");
        } catch (error) {
            showNote("Could not load your cart. Apply the cart SQL scripts and try again.");
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
                showNote(result.responseMsg || result.ResponseMsg || "Could not update cart.");
                return;
            }
            await load();
        } catch (error) {
            showNote("Could not update cart.");
        }
    });

    load();
})();
