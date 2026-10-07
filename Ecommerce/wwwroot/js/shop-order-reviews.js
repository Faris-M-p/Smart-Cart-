/*
 * My Orders > Order details: ratings & reviews for the purchased order items.
 *
 * The server (/Reviews/Order) decides, per order item, whether the customer can add / edit /
 * delete a review and how long the 7-day window (from delivery) lasts. This script only displays
 * that and sends the actions. The signed-in customer is never sent: only the order, order item,
 * product and review ids travel to the server, which validates them against the logged-in user.
 */
(function () {
    var page = document.querySelector(".shop-orders-page[data-order-id]");
    if (!page || document.getElementById("orders-list")) {
        return;
    }

    var orderId = Number(page.getAttribute("data-order-id") || 0);
    if (!orderId) {
        return;
    }

    var MAX_LENGTH = 1000;
    var RATING_LABELS = ["", "Poor", "Fair", "Good", "Very good", "Excellent"];

    var state = {
        items: {},          // orderItemId -> status from the server
        mode: "create",     // create | update
        item: null,         // item being reviewed in the modal
        rating: 0,
        busy: false,
        opener: null
    };

    function byId(id) {
        return document.getElementById(id);
    }

    function escapeHtml(value) {
        return String(value || "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;");
    }

    function stars(rating) {
        return "★★★★★".slice(0, rating) + "☆☆☆☆☆".slice(rating);
    }

    function plural(count, word) {
        return count + " " + word + (count === 1 ? "" : "s");
    }

    function formatDate(value) {
        if (!value) {
            return "";
        }
        var date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return "";
        }
        return date.toLocaleDateString("en-IN", { day: "2-digit", month: "short", year: "numeric" });
    }

    function flash(message, persist) {
        var el = byId("order-status");
        if (!el) {
            return;
        }
        if (window.smartCartFlash) {
            window.smartCartFlash(el, { message: message || "", persist: !!persist });
            return;
        }
        el.textContent = message || "";
        el.hidden = !message;
    }

    /* ----------------------------------------------------------------
       Per-item review block
       ---------------------------------------------------------------- */

    function windowText(item, verb) {
        var until = formatDate(item.reviewExpiresOn);
        var days = Number(item.daysLeft || 0);
        if (!until) {
            return "";
        }
        return verb + " " + until + (days > 0 ? " · " + plural(days, "day") + " left" : "") + ".";
    }

    function quickStars() {
        var html = "";
        for (var value = 1; value <= 5; value += 1) {
            html += '<button type="button" class="shop-review-star" data-review-rate="' + value + '" ' +
                'aria-label="Rate ' + plural(value, "star") + '">★</button>';
        }
        return '<div class="shop-order-review__stars" role="group" aria-label="Rate this product">' + html + "</div>";
    }

    function reviewBody(item) {
        var rating = Number(item.rating || 0);
        return '<div class="shop-order-review__rating" role="img" aria-label="' + rating + ' out of 5 stars">' + stars(rating) + "</div>" +
            (item.review ? '<p class="shop-order-review__text">' + escapeHtml(item.review) + "</p>" : "");
    }

    function findSlot(item) {
        var orderItemId = Number(item.orderItemId || item.OrderItemId || 0);
        var productId = Number(item.productId || item.ProductId || 0);
        var holder = orderItemId
            ? document.querySelector('.shop-order-item[data-order-item-id="' + orderItemId + '"]')
            : null;
        if (!holder && productId) {
            holder = document.querySelector('.shop-order-item[data-product-id="' + productId + '"]');
        }
        return holder ? holder.querySelector("[data-review-slot]") : null;
    }

    function renderSlot(item) {
        var slot = findSlot(item);
        if (!slot) {
            return false;
        }

        slot.classList.remove("is-reviewed", "is-locked");
        var status = item.reviewStatus || item.ReviewStatus;

        if (status === "CanReview" && (item.canAddReview || item.CanAddReview)) {
            slot.innerHTML = quickStars() +
                '<p class="shop-order-review__note">' + escapeHtml(windowText(item, "You can review this product until")) + "</p>" +
                '<div class="shop-order-review__actions">' +
                    '<button type="button" class="shop-review-btn shop-review-btn-primary shop-review-btn-sm" data-review-add>Add Review</button>' +
                "</div>";
        } else if (status === "Reviewed") {
            slot.classList.add("is-reviewed");
            slot.innerHTML = '<span class="shop-order-review__status">✓ Reviewed</span>' +
                reviewBody(item) +
                '<p class="shop-order-review__note">' + escapeHtml(windowText(item, "You can edit or delete your review until")) + "</p>" +
                '<div class="shop-order-review__actions">' +
                    ((item.canEditReview || item.CanEditReview)
                        ? '<button type="button" class="shop-review-btn shop-review-btn-ghost shop-review-btn-sm" data-review-edit>Edit Review</button>'
                        : "") +
                    ((item.canDeleteReview || item.CanDeleteReview)
                        ? '<button type="button" class="shop-review-btn shop-review-btn-danger-ghost shop-review-btn-sm" data-review-delete>Delete Review</button>'
                        : "") +
                "</div>";
        } else if (status === "Locked") {
            slot.classList.add("is-locked");
            slot.innerHTML = '<span class="shop-order-review__status">✓ Reviewed</span>' +
                reviewBody(item) +
                '<span class="shop-order-review__locked">Review period ended</span>';
        } else if (status === "Expired") {
            slot.classList.add("is-locked");
            slot.innerHTML = '<span class="shop-order-review__locked">Review period ended</span>';
        } else {
            // Not eligible (e.g. not delivered yet or cancelled): no review action at all.
            slot.innerHTML = "";
            return true;
        }

        return true;
    }

    function renderAll(items) {
        state.items = {};
        var missing = 0;
        (items || []).forEach(function (item) {
            var id = Number(item.orderItemId || item.OrderItemId || 0);
            if (id) {
                state.items[id] = item;
            }
            if (!renderSlot(item)) {
                missing += 1;
            }
        });
        return missing;
    }

    var loadSeq = 0;
    var loadPromise = null;

    async function loadStatus(forceOrderId) {
        var targetOrderId = Number(forceOrderId || orderId || 0);
        if (!targetOrderId) {
            return;
        }

        var seq = ++loadSeq;
        var attempt = 0;

        async function run() {
            try {
                var response = await fetch("/Reviews/Order?orderId=" + encodeURIComponent(targetOrderId), {
                    credentials: "same-origin",
                    cache: "no-store"
                });
                if (window.smartCartHandleAuth && window.smartCartHandleAuth(response)) {
                    return;
                }
                if (!response.ok) {
                    throw new Error("status");
                }
                if (seq !== loadSeq) {
                    return;
                }

                var items = await response.json();
                var missing = renderAll(items);

                // Items markup may still be painting; retry a couple of times.
                while (missing > 0 && attempt < 5 && seq === loadSeq) {
                    attempt += 1;
                    await new Promise(function (resolve) { setTimeout(resolve, 120 * attempt); });
                    if (seq !== loadSeq) {
                        return;
                    }
                    missing = renderAll(items);
                }
            } catch (error) {
                if (seq === loadSeq) {
                    flash("Could not load review options for this order. Please refresh the page.", true);
                }
            }
        }

        loadPromise = run();
        return loadPromise;
    }

    window.smartCartLoadOrderReviews = function (id) {
        return loadStatus(id);
    };

    /* ----------------------------------------------------------------
       Modals (same standalone open/close pattern as the checkout modals)
       ---------------------------------------------------------------- */

    function openModal(id) {
        var modal = byId(id);
        if (!modal) {
            return;
        }
        modal.classList.add("shop-modal-open", "show");
        modal.removeAttribute("aria-hidden");
        document.body.classList.add("modal-open");
    }

    function closeModal(id) {
        var modal = byId(id);
        if (!modal) {
            return;
        }
        modal.classList.remove("shop-modal-open", "show");
        modal.setAttribute("aria-hidden", "true");
        if (!document.querySelector(".shop-review-modal.show")) {
            document.body.classList.remove("modal-open");
        }
    }

    function closeAllModals() {
        closeModal("reviewModal");
        closeModal("reviewDeleteModal");
        if (state.opener && document.contains(state.opener)) {
            state.opener.focus({ preventScroll: true });
        }
        state.opener = null;
    }

    function setFormNote(message) {
        var note = byId("review-form-note");
        note.textContent = message || "";
        note.hidden = !message;
    }

    function setFormRating(value) {
        state.rating = value;
        document.querySelectorAll("#review-rating-input .shop-review-star").forEach(function (star) {
            var starValue = Number(star.getAttribute("data-value"));
            star.classList.toggle("is-active", starValue <= value);
            star.setAttribute("aria-checked", starValue === value ? "true" : "false");
            star.tabIndex = (value === 0 ? starValue === 1 : starValue === value) ? 0 : -1;
        });
        byId("review-rating-hint").textContent = value > 0 ? value + " / 5 · " + RATING_LABELS[value] : "";
    }

    function previewFormRating(value) {
        document.querySelectorAll("#review-rating-input .shop-review-star").forEach(function (star) {
            star.classList.toggle("is-preview", Number(star.getAttribute("data-value")) <= value);
        });
    }

    function updateCounter() {
        byId("review-text-count").textContent = byId("review-text").value.length + " / " + MAX_LENGTH;
    }

    function setBusy(busy) {
        state.busy = busy;
        var submit = byId("review-submit");
        submit.disabled = busy;
        byId("review-text").disabled = busy;
        submit.textContent = busy
            ? "Saving..."
            : (state.mode === "update" ? "Update review" : "Submit review");
        byId("review-delete-confirm").disabled = busy;
    }

    function openReviewForm(item, mode, rating, text, opener) {
        state.item = item;
        state.mode = mode;
        state.opener = opener || null;

        byId("review-modal-title").textContent = mode === "update" ? "Edit your review" : "Rate this product";
        byId("review-modal-product").textContent = item.productName || "Product";
        byId("review-modal-variant").textContent = item.variantLabel || "";
        byId("review-modal-variant").hidden = !item.variantLabel;
        byId("review-submit").textContent = mode === "update" ? "Update review" : "Submit review";
        byId("review-text").value = text || "";
        setFormRating(rating || 0);
        updateCounter();
        setFormNote("");
        setBusy(false);
        openModal("reviewModal");

        var target = document.querySelector("#review-rating-input .shop-review-star[tabindex='0']");
        if (target) {
            target.focus({ preventScroll: true });
        }
    }

    /* ----------------------------------------------------------------
       Requests
       ---------------------------------------------------------------- */

    async function send(url, body) {
        var response = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        if (response.status === 401) {
            if (window.smartCartHandleAuth) {
                window.smartCartHandleAuth(response);
            }
            return { redirected: true };
        }

        var payload = null;
        try {
            payload = await response.json();
        } catch (error) {
            payload = null;
        }

        var ok = response.ok && !!(payload && payload.statusCode);
        return {
            ok: ok,
            status: response.status,
            message: (payload && payload.responseMsg) || (ok ? "" : "Something went wrong. Please try again.")
        };
    }

    // The server state changed under us (already reviewed, window over, ...): show it and refresh.
    function isStateConflict(status) {
        return status === 403 || status === 404 || status === 409;
    }

    async function submitReview(event) {
        event.preventDefault();
        if (state.busy || !state.item) {
            return;
        }

        var text = byId("review-text").value.trim();
        if (state.rating < 1 || state.rating > 5) {
            setFormNote("Please select a rating from 1 to 5 stars.");
            return;
        }
        if (text.length > MAX_LENGTH) {
            setFormNote("Review cannot exceed " + MAX_LENGTH + " characters.");
            return;
        }

        var item = state.item;
        var isUpdate = state.mode === "update";
        setFormNote("");
        setBusy(true);

        try {
            var result = await send(
                isUpdate ? "/Reviews/Update" : "/Reviews/Submit",
                isUpdate
                    ? { reviewId: item.reviewId, rating: state.rating, review: text }
                    : {
                        orderId: item.orderId,
                        orderItemId: item.orderItemId,
                        productId: item.productId,
                        rating: state.rating,
                        review: text
                    });

            if (result.redirected) {
                return;
            }

            if (result.ok) {
                closeAllModals();
                flash(result.message);
                await loadStatus();
                return;
            }

            if (isStateConflict(result.status)) {
                closeAllModals();
                flash(result.message, true);
                await loadStatus();
                return;
            }

            setFormNote(result.message);
        } catch (error) {
            setFormNote("Could not save your review. Please check your connection and try again.");
        } finally {
            setBusy(false);
        }
    }

    function askDelete(item, opener) {
        state.item = item;
        state.opener = opener || null;
        var note = byId("review-delete-note");
        note.textContent = "";
        note.hidden = true;
        setBusy(false);
        openModal("reviewDeleteModal");
        byId("review-delete-confirm").focus({ preventScroll: true });
    }

    async function confirmDelete() {
        if (state.busy || !state.item) {
            return;
        }

        var item = state.item;
        var note = byId("review-delete-note");
        note.hidden = true;
        setBusy(true);

        try {
            var result = await send("/Reviews/Delete", { reviewId: item.reviewId });
            if (result.redirected) {
                return;
            }

            if (result.ok || isStateConflict(result.status)) {
                closeAllModals();
                flash(result.message, !result.ok);
                await loadStatus();
                return;
            }

            note.textContent = result.message;
            note.hidden = false;
        } catch (error) {
            note.textContent = "Could not delete your review. Please try again.";
            note.hidden = false;
        } finally {
            setBusy(false);
        }
    }

    /* ----------------------------------------------------------------
       Events
       ---------------------------------------------------------------- */

    function itemFromElement(element) {
        var holder = element.closest(".shop-order-item");
        var id = holder ? Number(holder.getAttribute("data-order-item-id")) : 0;
        return id ? state.items[id] : null;
    }

    function bindItems() {
        var container = byId("order-items");
        if (!container) {
            return;
        }

        container.addEventListener("click", function (event) {
            var rate = event.target.closest("[data-review-rate]");
            var add = event.target.closest("[data-review-add]");
            var edit = event.target.closest("[data-review-edit]");
            var del = event.target.closest("[data-review-delete]");
            var trigger = rate || add || edit || del;
            if (!trigger) {
                return;
            }

            var item = itemFromElement(trigger);
            if (!item) {
                return;
            }

            if (rate && item.canAddReview) {
                openReviewForm(item, "create", Number(rate.getAttribute("data-review-rate")), "", trigger);
            } else if (add && item.canAddReview) {
                openReviewForm(item, "create", 0, "", trigger);
            } else if (edit && item.canEditReview) {
                openReviewForm(item, "update", Number(item.rating || 0), item.review || "", trigger);
            } else if (del && item.canDeleteReview) {
                askDelete(item, trigger);
            }
        });

        // Hover preview on the quick-rate stars
        container.addEventListener("mouseover", function (event) {
            var star = event.target.closest("[data-review-rate]");
            if (!star) {
                return;
            }
            var value = Number(star.getAttribute("data-review-rate"));
            star.parentElement.querySelectorAll("[data-review-rate]").forEach(function (other) {
                other.classList.toggle("is-preview", Number(other.getAttribute("data-review-rate")) <= value);
            });
        });
        container.addEventListener("mouseout", function (event) {
            var group = event.target.closest(".shop-order-review__stars");
            if (group) {
                group.querySelectorAll(".is-preview").forEach(function (other) {
                    other.classList.remove("is-preview");
                });
            }
        });
    }

    function bindModals() {
        var form = byId("review-form");
        if (!form) {
            return;
        }

        form.addEventListener("submit", submitReview);
        byId("review-text").addEventListener("input", updateCounter);
        byId("review-delete-confirm").addEventListener("click", confirmDelete);

        document.querySelectorAll(".shop-review-modal").forEach(function (modal) {
            modal.addEventListener("click", function (event) {
                if (state.busy) {
                    return;
                }
                if (event.target === modal || event.target.closest("[data-review-close]")) {
                    closeAllModals();
                }
            });
        });

        document.addEventListener("keydown", function (event) {
            if (event.key === "Escape" && !state.busy && document.querySelector(".shop-review-modal.show")) {
                closeAllModals();
            }
        });

        var starInput = byId("review-rating-input");
        starInput.addEventListener("click", function (event) {
            var star = event.target.closest(".shop-review-star");
            if (star) {
                setFormRating(Number(star.getAttribute("data-value")));
                setFormNote("");
            }
        });
        starInput.addEventListener("mouseover", function (event) {
            var star = event.target.closest(".shop-review-star");
            if (star) {
                previewFormRating(Number(star.getAttribute("data-value")));
            }
        });
        starInput.addEventListener("mouseleave", function () {
            previewFormRating(0);
        });
        starInput.addEventListener("keydown", function (event) {
            var step = event.key === "ArrowRight" || event.key === "ArrowUp" ? 1
                : (event.key === "ArrowLeft" || event.key === "ArrowDown" ? -1 : 0);
            if (!step) {
                return;
            }
            event.preventDefault();
            var next = Math.max(1, Math.min(5, (state.rating || 0) + step));
            setFormRating(next);
            var target = starInput.querySelector(".shop-review-star[data-value='" + next + "']");
            if (target) {
                target.focus();
            }
        });
    }

    try {
        bindItems();
        bindModals();
    } catch (error) {
        flash("Could not prepare the review form. Please refresh the page.", true);
    }

    // shop-orders.js renders the items and announces it; if that already happened, load right away.
    document.addEventListener("smartcart:order-loaded", function (event) {
        var id = event && event.detail ? event.detail.orderId : orderId;
        loadStatus(id);
    });
    if (document.querySelector("#order-items .shop-order-item")) {
        loadStatus();
    }
})();
