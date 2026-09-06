(function () {
    var grid = document.getElementById("wishlist-grid");
    var empty = document.getElementById("wishlist-empty");
    var note = document.getElementById("wishlist-status");
    if (!grid) {
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

    function render(items) {
        var rows = items || [];
        grid.innerHTML = rows.map(function (item) {
            var image = item.imageUrl || item.ImageUrl;
            var inStock = !!(item.inStock || item.InStock);
            return '<article class="shop-wishlist-card">' +
                (image
                    ? '<img src="' + escapeHtml(image) + '" alt="">'
                    : '<div class="shop-wishlist-fallback">No image</div>') +
                '<div class="shop-wishlist-body">' +
                    '<p class="shop-cart-meta">' + escapeHtml([item.categoryName || item.CategoryName, item.brandName || item.BrandName].filter(Boolean).join(" · ")) + "</p>" +
                    '<h3><a href="/Shop/Details/' + encodeURIComponent(item.slug || item.Slug || "") + '">' + escapeHtml(item.name || item.Name) + "</a></h3>" +
                    '<p class="shop-cart-price">' + money(item.price || item.Price) +
                        ((item.mrp || item.MRP) > (item.price || item.Price)
                            ? '<span class="shop-cart-old">' + money(item.mrp || item.MRP) + "</span>"
                            : "") +
                    "</p>" +
                    (inStock ? "" : '<p class="shop-cart-oos">Out of stock</p>') +
                    '<button class="shop-bag-remove" type="button" data-remove="' + (item.wishlistItemId || item.WishlistItemId) + '">Remove</button>' +
                "</div>" +
            "</article>";
        }).join("");

        if (empty) empty.hidden = rows.length > 0;
        if (window.refreshSmartCartBag) {
            window.refreshSmartCartBag();
        }
    }

    async function load() {
        showNote("Loading wishlist...");
        try {
            var response = await fetch("/Wishlist/Get");
            if (window.smartCartHandleAuth && window.smartCartHandleAuth(response)) {
                return;
            }
            if (!response.ok) {
                throw new Error("not found");
            }
            render(await response.json());
            showNote("");
        } catch (error) {
            showNote("Could not load your wishlist. Apply the wishlist SQL scripts and try again.");
        }
    }

    grid.addEventListener("click", async function (event) {
        var button = event.target.closest("[data-remove]");
        if (!button) {
            return;
        }

        try {
            var result = await fetch("/Wishlist/Remove", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ wishlistItemId: Number(button.getAttribute("data-remove")) })
            }).then(function (response) { return response.json(); });

            if (!(result.statusCode || result.StatusCode)) {
                showNote(result.responseMsg || result.ResponseMsg || "Could not update wishlist.");
                return;
            }
            await load();
        } catch (error) {
            showNote("Could not update wishlist.");
        }
    });

    load();
})();
