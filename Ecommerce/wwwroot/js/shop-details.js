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
    const actionNote = document.getElementById("details-action-note");

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

    function showActionNote(message) {
        if (!actionNote) {
            return;
        }
        actionNote.textContent = message;
        actionNote.hidden = false;
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

    function renderGallery(product) {
        const images = (product.imageUrls || product.ImageUrls || []).filter(Boolean);
        const fallback = product.imageUrl || product.ImageUrl || "";
        const urls = images.length > 0 ? images : (fallback ? [fallback] : []);
        setMainImage(urls[0] || "", product.name || product.Name);

        if (!thumbs) {
            return;
        }

        thumbs.innerHTML = urls.map(function (url, index) {
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
                setMainImage(button.getAttribute("data-image"), product.name || product.Name);
            });
        });
    }

    function renderReviews() {
        const list = document.getElementById("reviews-list");
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

    function renderProduct(product) {
        const name = product.name || product.Name || "";
        const category = product.categoryName || product.CategoryName || "";
        const subcategory = product.subCategoryName || product.SubCategoryName || "";
        const brand = product.brandName || product.BrandName || "";
        const price = product.price || product.Price || 0;
        const mrp = product.mrp || product.MRP || 0;
        const inStock = !!(product.inStock || product.InStock);
        const description = product.description || product.Description || "";

        document.getElementById("details-crumb-name").textContent = name;
        document.getElementById("details-name").textContent = name;
        document.getElementById("details-meta").textContent = [category, subcategory, brand].filter(Boolean).join(" · ");
        document.getElementById("details-price").textContent = formatRupees(price);

        const mrpEl = document.getElementById("details-mrp");
        if (mrp > price) {
            mrpEl.textContent = formatRupees(mrp);
            mrpEl.hidden = false;
        }

        const descriptionEl = document.getElementById("details-description");
        descriptionEl.textContent = description || "No description is available for this product.";

        if (stockBadge) {
            stockBadge.hidden = inStock;
        }

        renderGallery(product);
        renderReviews();

        if (statusEl) statusEl.hidden = true;
        if (contentEl) contentEl.hidden = false;
    }

    function bindActions() {
        const buyNow = document.getElementById("details-buy-now");
        const addCart = document.getElementById("details-add-cart");
        if (buyNow) {
            buyNow.addEventListener("click", function () {
                showActionNote("Buy Now will continue to checkout in a later phase.");
            });
        }
        if (addCart) {
            addCart.addEventListener("click", function () {
                showActionNote("Add to Cart will save items to the cart in a later phase.");
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
            const response = await fetch("/Shop/GetProduct/" + encodeURIComponent(slug));
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
