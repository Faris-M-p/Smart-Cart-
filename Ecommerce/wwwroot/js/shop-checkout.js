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

    async function initCheckout() {
        var form = document.getElementById("checkout-form");
        var content = document.getElementById("checkout-content");
        var empty = document.getElementById("checkout-empty");
        var status = document.getElementById("checkout-status");
        var itemsEl = document.getElementById("checkout-items");
        if (!form || !content) {
            return;
        }

        var params = new URLSearchParams(window.location.search);
        var buyNow = params.get("buyNow") === "1";
        var variantId = Number(params.get("variantId") || 0);
        var quantity = Number(params.get("qty") || 1);
        if (quantity < 1) {
            quantity = 1;
        }

        var query = buyNow && variantId
            ? "?productVariantId=" + encodeURIComponent(variantId) + "&quantity=" + encodeURIComponent(quantity)
            : "";

        try {
            var response = await fetch("/Checkout/Preview" + query);
            if (window.smartCartHandleAuth && window.smartCartHandleAuth(response)) {
                return;
            }
            if (!response.ok) {
                throw new Error("preview");
            }
            var page = await response.json();
            var items = page.items || page.Items || [];
            var summary = page.summary || page.Summary || {};
            var customer = page.customer || page.Customer || {};
            var canPlace = !!(summary.canPlace || summary.CanPlace);

            if (!items.length) {
                content.hidden = true;
                if (empty) empty.hidden = false;
                return;
            }

            if (empty) empty.hidden = true;
            content.hidden = false;
            document.getElementById("checkout-source").textContent = buyNow
                ? "Buying this item now. Your cart stays as it is."
                : "Review your cart items, add a delivery address, and place the order.";
            document.getElementById("checkout-name").value = customer.fullName || customer.FullName || "";
            document.getElementById("checkout-phone").value = customer.phone || customer.Phone || "";
            document.getElementById("checkout-total").textContent = money(summary.subtotal || summary.Subtotal);
            renderItems(itemsEl, items);

            if (!canPlace) {
                flash(status, "One or more items are out of stock.", true);
                document.getElementById("checkout-place").disabled = true;
            }
        } catch (error) {
            flash(status, "Could not load checkout.", true);
            return;
        }

        form.addEventListener("submit", async function (event) {
            event.preventDefault();
            var button = document.getElementById("checkout-place");
            button.disabled = true;
            try {
                var resultResponse = await fetch("/Checkout/Place", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        productVariantId: buyNow ? variantId : 0,
                        quantity: buyNow ? quantity : 1,
                        receiverName: document.getElementById("checkout-name").value,
                        phone: document.getElementById("checkout-phone").value,
                        addressLine: document.getElementById("checkout-address").value,
                        city: document.getElementById("checkout-city").value,
                        pincode: document.getElementById("checkout-pincode").value,
                        paymentMethod: "COD"
                    })
                });
                if (window.smartCartHandleAuth && window.smartCartHandleAuth(resultResponse)) {
                    return;
                }
                var result = await resultResponse.json();
                if (!(result.statusCode || result.StatusCode)) {
                    flash(status, result.responseMsg || result.ResponseMsg || "Could not place the order.", true);
                    button.disabled = false;
                    return;
                }
                if (window.refreshSmartCartBag) {
                    window.refreshSmartCartBag();
                }
                window.location.href = "/Checkout/Confirmation/" + (result.responseCode || result.ResponseCode);
            } catch (error) {
                flash(status, "Could not place the order.", true);
                button.disabled = false;
            }
        });
    }

    async function initConfirmation() {
        var page = document.querySelector("[data-order-id]");
        if (!page || document.getElementById("checkout-form")) {
            return;
        }

        var orderId = Number(page.getAttribute("data-order-id") || 0);
        var content = document.getElementById("confirm-content");
        var missing = document.getElementById("confirm-missing");
        var status = document.getElementById("confirm-status");
        if (!orderId) {
            if (missing) missing.hidden = false;
            return;
        }

        try {
            var response = await fetch("/Checkout/GetOrder?orderId=" + encodeURIComponent(orderId));
            if (window.smartCartHandleAuth && window.smartCartHandleAuth(response)) {
                return;
            }
            if (!response.ok) {
                throw new Error("missing");
            }
            var data = await response.json();
            var order = data.order || data.Order || {};
            var items = data.items || data.Items || [];
            var method = (order.paymentMethod || order.PaymentMethod || "COD") === "COD"
                ? "Cash on Delivery"
                : (order.paymentMethod || order.PaymentMethod);
            var payStatus = order.paymentStatus || order.PaymentStatus || "Pending";

            document.getElementById("confirm-number").textContent =
                "Order " + (order.orderNumber || order.OrderNumber || orderId);
            document.getElementById("confirm-meta").textContent =
                (order.orderStatus || order.OrderStatus || "Placed") +
                " · " + new Date(order.orderDate || order.OrderDate || Date.now()).toLocaleString("en-IN");
            document.getElementById("confirm-pay").textContent = method + " · " + payStatus;
            document.getElementById("confirm-address").innerHTML =
                "<strong>Deliver to</strong><br>" +
                escapeHtml(order.receiverName || order.ReceiverName) + "<br>" +
                escapeHtml(order.phone || order.Phone) + "<br>" +
                escapeHtml(order.addressLine || order.AddressLine) + "<br>" +
                escapeHtml(order.city || order.City) + " - " + escapeHtml(order.pincode || order.Pincode);
            document.getElementById("confirm-total").textContent = money(order.totalAmount || order.TotalAmount);
            renderItems(document.getElementById("confirm-items"), items);
            var orderLink = document.getElementById("confirm-order-link");
            if (orderLink) {
                orderLink.href = "/Orders/Details/" + encodeURIComponent(order.orderId || order.OrderId || orderId);
            }
            if (missing) missing.hidden = true;
            if (content) content.hidden = false;
            if (window.refreshSmartCartBag) {
                window.refreshSmartCartBag();
            }
        } catch (error) {
            if (content) content.hidden = true;
            if (missing) missing.hidden = false;
            flash(status, "Could not load this order.", true);
        }
    }

    initCheckout();
    initConfirmation();
})();
