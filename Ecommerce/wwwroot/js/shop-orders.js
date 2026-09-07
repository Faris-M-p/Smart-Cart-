(function () {
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

    function statusClass(status) {
        var value = String(status || "").toLowerCase();
        if (value === "cancelled") {
            return "is-cancelled";
        }
        if (value === "delivered") {
            return "is-delivered";
        }
        if (value === "placed" || value === "pending") {
            return "is-placed";
        }
        return "is-progress";
    }

    function paymentLabel(method) {
        return method === "COD" ? "Cash on Delivery" : (method || "COD");
    }

    function flash(el, message, persist) {
        if (window.smartCartFlash) {
            window.smartCartFlash(el, { message: message, persist: !!persist });
            return;
        }
        if (!el) {
            return;
        }
        el.textContent = message || "";
        el.hidden = !message;
    }

    function renderItems(container, items) {
        container.innerHTML = (items || []).map(function (item) {
            var image = item.imageUrl || item.ImageUrl;
            var name = escapeHtml(item.name || item.Name);
            var href = detailsUrl(item);
            return '<article class="shop-checkout-line">' +
                (image
                    ? '<img src="' + escapeHtml(image) + '" alt="' + name + '">'
                    : '<div class="shop-checkout-fallback">No image</div>') +
                "<div>" +
                    "<h4><a href=\"" + escapeHtml(href) + "\">" + name + "</a></h4>" +
                    "<p>" + escapeHtml(item.label || item.Label || item.sku || item.SKU) +
                        " · Qty " + (item.quantity || item.Quantity) + "</p>" +
                "</div>" +
                "<strong>" + money(item.lineTotal ?? item.LineTotal ?? ((item.price || item.Price) * (item.quantity || item.Quantity))) + "</strong>" +
            "</article>";
        }).join("");
    }

    async function cancelOrder(orderId, statusEl) {
        if (!window.confirm("Cancel this order? The items will be returned to stock.")) {
            return false;
        }

        try {
            var response = await fetch("/Orders/Cancel", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    orderId: orderId,
                    reason: "Cancelled by customer"
                })
            });
            if (window.smartCartHandleAuth && window.smartCartHandleAuth(response)) {
                return false;
            }
            var result = await response.json();
            if (!(result.statusCode || result.StatusCode)) {
                flash(statusEl, result.responseMsg || result.ResponseMsg || "Could not cancel this order.", true);
                return false;
            }
            flash(statusEl, result.responseMsg || result.ResponseMsg || "Order cancelled.");
            return true;
        } catch (error) {
            flash(statusEl, "Could not cancel this order.", true);
            return false;
        }
    }

    function renderOrderList(orders) {
        var list = document.getElementById("orders-list");
        var empty = document.getElementById("orders-empty");
        if (!list) {
            return;
        }

        if (!orders.length) {
            list.hidden = true;
            list.innerHTML = "";
            if (empty) empty.hidden = false;
            return;
        }

        if (empty) empty.hidden = true;
        list.hidden = false;
        list.innerHTML = orders.map(function (order) {
            var orderId = order.orderId || order.OrderId;
            var status = order.orderStatus || order.OrderStatus || "Placed";
            var itemCount = Number(order.itemCount || order.ItemCount || 0);
            var name = escapeHtml(order.firstProductName || order.FirstProductName || "Order");
            var image = order.firstImageUrl || order.FirstImageUrl;
            var canCancel = !!(order.canCancel || order.CanCancel);
            var extra = itemCount > 1 ? " + " + (itemCount - 1) + " more" : "";
            return '<article class="shop-orders-card" data-order-card="' + orderId + '">' +
                '<a class="shop-orders-card__link" href="/Orders/Details/' + encodeURIComponent(orderId) + '">' +
                    (image
                        ? '<img src="' + escapeHtml(image) + '" alt="' + name + '">'
                        : '<div class="shop-orders-fallback">No image</div>') +
                    "<div>" +
                        "<h3>" + name + extra + "</h3>" +
                        "<p>Order " + escapeHtml(order.orderNumber || order.OrderNumber || orderId) +
                            " · " + new Date(order.orderDate || order.OrderDate || Date.now()).toLocaleString("en-IN") + "</p>" +
                        '<span class="shop-orders-badge ' + statusClass(status) + '">' + escapeHtml(status) + "</span>" +
                    "</div>" +
                    "<strong>" + money(order.totalAmount || order.TotalAmount) + "</strong>" +
                "</a>" +
                (canCancel
                    ? '<button type="button" class="shop-bag-remove" data-cancel-order="' + orderId + '">Cancel</button>'
                    : "") +
            "</article>";
        }).join("");
    }

    async function loadOrders() {
        var list = document.getElementById("orders-list");
        var status = document.getElementById("orders-status");
        if (!list) {
            return;
        }

        try {
            var response = await fetch("/Orders/List");
            if (window.smartCartHandleAuth && window.smartCartHandleAuth(response)) {
                return;
            }
            if (!response.ok) {
                throw new Error("list");
            }
            renderOrderList(await response.json());
        } catch (error) {
            flash(status, "Could not load your orders.", true);
        }
    }

    async function initList() {
        var list = document.getElementById("orders-list");
        if (!list) {
            return;
        }

        list.addEventListener("click", async function (event) {
            var button = event.target.closest("[data-cancel-order]");
            if (!button) {
                return;
            }
            event.preventDefault();
            event.stopPropagation();
            var cancelled = await cancelOrder(Number(button.getAttribute("data-cancel-order")), document.getElementById("orders-status"));
            if (cancelled) {
                await loadOrders();
            }
        });

        await loadOrders();
    }

    function fillOrder(data) {
        var order = data.order || data.Order || {};
        var items = data.items || data.Items || [];
        var orderId = order.orderId || order.OrderId;
        var status = order.orderStatus || order.OrderStatus || "Placed";
        var method = paymentLabel(order.paymentMethod || order.PaymentMethod || "COD");
        var payStatus = order.paymentStatus || order.PaymentStatus || "Pending";
        var cancelled = !!(order.cancelled || order.Cancelled);
        var canCancel = !!(order.canCancel || order.CanCancel);
        var cancelNote = document.getElementById("order-cancelled-note");
        var cancelButton = document.getElementById("order-cancel");

        document.getElementById("order-number").textContent =
            "Order " + (order.orderNumber || order.OrderNumber || orderId);
        document.getElementById("order-meta").innerHTML =
            '<span class="shop-orders-badge ' + statusClass(status) + '">' + escapeHtml(status) + "</span> " +
            new Date(order.orderDate || order.OrderDate || Date.now()).toLocaleString("en-IN");
        document.getElementById("order-pay").textContent = method + " · " + payStatus;
        document.getElementById("order-address").innerHTML =
            "<strong>Deliver to</strong><br>" +
            escapeHtml(order.receiverName || order.ReceiverName) + "<br>" +
            escapeHtml(order.phone || order.Phone) + "<br>" +
            escapeHtml(order.addressLine || order.AddressLine) + "<br>" +
            escapeHtml(order.city || order.City) + " - " + escapeHtml(order.pincode || order.Pincode);
        document.getElementById("order-total").textContent = money(order.totalAmount || order.TotalAmount);
        renderItems(document.getElementById("order-items"), items);

        if (cancelNote) {
            if (cancelled) {
                cancelNote.hidden = false;
                cancelNote.textContent = "This order was cancelled" +
                    (order.cancelledOn || order.CancelledOn
                        ? " on " + new Date(order.cancelledOn || order.CancelledOn).toLocaleString("en-IN")
                        : "") + ".";
            } else {
                cancelNote.hidden = true;
                cancelNote.textContent = "";
            }
        }

        if (cancelButton) {
            cancelButton.hidden = !canCancel;
            cancelButton.disabled = !canCancel;
        }
    }

    async function loadOrder(orderId) {
        var content = document.getElementById("order-content");
        var missing = document.getElementById("order-missing");
        var response = await fetch("/Orders/Get?orderId=" + encodeURIComponent(orderId));
        if (window.smartCartHandleAuth && window.smartCartHandleAuth(response)) {
            return;
        }
        if (!response.ok) {
            if (content) content.hidden = true;
            if (missing) missing.hidden = false;
            return;
        }
        fillOrder(await response.json());
        if (missing) missing.hidden = true;
        if (content) content.hidden = false;
    }

    async function initDetails() {
        var page = document.querySelector(".shop-orders-page[data-order-id]");
        if (!page || document.getElementById("orders-list")) {
            return;
        }

        var orderId = Number(page.getAttribute("data-order-id") || 0);
        var missing = document.getElementById("order-missing");
        var cancelButton = document.getElementById("order-cancel");
        if (!orderId) {
            if (missing) missing.hidden = false;
            return;
        }

        if (cancelButton) {
            cancelButton.addEventListener("click", async function () {
                cancelButton.disabled = true;
                var cancelled = await cancelOrder(orderId, document.getElementById("order-status"));
                if (cancelled) {
                    await loadOrder(orderId);
                    return;
                }
                cancelButton.disabled = false;
            });
        }

        try {
            await loadOrder(orderId);
        } catch (error) {
            if (missing) missing.hidden = false;
            flash(document.getElementById("order-status"), "Could not load this order.", true);
        }
    }

    initList();
    initDetails();
})();
