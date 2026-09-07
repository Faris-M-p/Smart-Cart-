(function () {
    var grid = document.getElementById("wishlist-grid");
    var empty = document.getElementById("wishlist-empty");
    var note = document.getElementById("wishlist-status");
    var guide = document.getElementById("wishlist-guide");
    if (!grid) {
        return;
    }

    if (empty && !empty.querySelector(".shop-bag-empty-icon")) {
        empty.className = "shop-bag-empty";
        empty.innerHTML =
            '<div class="shop-bag-empty-icon" aria-hidden="true">' +
                '<svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 512 512"><path d="M352.92 80C288 80 256 144 256 144s-32-64-96.92-64c-52.76 0-94.54 44.14-95.08 96.81-1.1 109.33 86.73 187.08 183 252.42a16 16 0 0018 0c96.26-65.34 184.09-143.09 183-252.42-.54-52.67-42.32-96.81-95.08-96.81z" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="32"></path></svg>' +
            "</div>" +
            "<h2>Your wishlist is empty.</h2>" +
            "<p>Save products you like, then open them to add a SKU to cart.</p>" +
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
        return key ? "/Shop/Details/" + encodeURIComponent(key) : "/Shop";
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

    function render(items) {
        var rows = items || [];
        grid.innerHTML = rows.map(function (item) {
            var image = item.imageUrl || item.ImageUrl;
            var inStock = !!(item.inStock || item.InStock);
            var href = detailsUrl(item);
            var name = escapeHtml(item.name || item.Name);
            var media = '<span class="shop-wishlist-media">' +
                '<span class="shop-wishlist-fallback">No image</span>' +
                (image ? '<img src="' + escapeHtml(image) + '" alt="' + name + '">' : "") +
                "</span>";
            return '<article class="shop-wishlist-card">' +
                '<a class="shop-wishlist-link" href="' + escapeHtml(href) + '">' +
                    media +
                    '<div class="shop-wishlist-body">' +
                        '<p class="shop-cart-meta">' + escapeHtml([item.categoryName || item.CategoryName, item.brandName || item.BrandName].filter(Boolean).join(" · ")) + "</p>" +
                        "<h3>" + name + "</h3>" +
                        '<p class="shop-cart-price">' + money(item.price || item.Price) +
                            ((item.mrp || item.MRP) > (item.price || item.Price)
                                ? '<span class="shop-cart-old">' + money(item.mrp || item.MRP) + "</span>"
                                : "") +
                        "</p>" +
                        (inStock ? "" : '<p class="shop-cart-oos">Out of stock</p>') +
                    "</div>" +
                "</a>" +
                '<div class="shop-wishlist-actions">' +
                    '<button class="shop-bag-remove" type="button" data-remove="' + (item.wishlistItemId || item.WishlistItemId) + '" data-name="' + name + '">Remove</button>' +
                "</div>" +
            "</article>";
        }).join("");

        grid.querySelectorAll(".shop-wishlist-media img").forEach(function (img) {
            img.addEventListener("error", function () {
                img.remove();
            });
        });

        var isEmpty = rows.length === 0;
        grid.hidden = isEmpty;
        if (empty) empty.hidden = !isEmpty;
        if (guide) guide.hidden = isEmpty;
        if (window.refreshSmartCartBag) {
            window.refreshSmartCartBag();
        }
    }

    async function load() {
        try {
            var response = await fetch("/Wishlist/Get");
            if (window.smartCartHandleAuth && window.smartCartHandleAuth(response)) {
                return;
            }
            if (!response.ok) {
                throw new Error("not found");
            }
            render(await response.json());
        } catch (error) {
            showNote("Could not load your wishlist.", true);
        }
    }

    grid.addEventListener("click", async function (event) {
        var button = event.target.closest("[data-remove]");
        if (!button) {
            return;
        }

        try {
            var response = await fetch("/Wishlist/Remove", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ wishlistItemId: Number(button.getAttribute("data-remove")) })
            });
            if (window.smartCartHandleAuth && window.smartCartHandleAuth(response)) {
                return;
            }
            var result = await response.json();

            if (!(result.statusCode || result.StatusCode)) {
                showNote(result.responseMsg || result.ResponseMsg || "Could not update wishlist.", true);
                return;
            }
            var name = button.getAttribute("data-name") || "Item";
            await load();
            showNote(name + " removed from wishlist.");
        } catch (error) {
            showNote("Could not update wishlist.", true);
        }
    });

    load();
})();
