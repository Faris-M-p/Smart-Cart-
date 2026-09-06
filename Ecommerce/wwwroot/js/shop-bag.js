(function () {
    window.smartCartLoginUrl = function () {
        return "/Account/Login?returnUrl=" + encodeURIComponent(window.location.pathname + window.location.search);
    };

    window.smartCartHandleAuth = function (response) {
        if (response && response.status === 401) {
            window.location.href = window.smartCartLoginUrl();
            return true;
        }
        return false;
    };

    function readCounts(data) {
        return {
            cart: Number(data && (data.cartCount ?? data.CartCount) || 0),
            wishlist: Number(data && (data.wishlistCount ?? data.WishlistCount) || 0)
        };
    }

    function setCount(nodes, value) {
        nodes.forEach(function (node) {
            node.textContent = String(value);
            node.setAttribute("data-count", String(value));
        });
    }

    async function refreshAccount() {
        var loginLinks = document.querySelectorAll("[data-account-login]");
        var registerLinks = document.querySelectorAll("[data-account-register]");
        var userLabels = document.querySelectorAll("[data-account-name]");
        var logoutButtons = document.querySelectorAll("[data-account-logout]");

        try {
            var response = await fetch("/Account/Me");
            if (!response.ok) {
                throw new Error("guest");
            }
            var session = await response.json();
            var name = session.name || session.Name || "Account";
            loginLinks.forEach(function (node) { node.hidden = true; });
            registerLinks.forEach(function (node) { node.hidden = true; });
            userLabels.forEach(function (node) {
                node.hidden = false;
                node.textContent = name;
            });
            logoutButtons.forEach(function (node) { node.hidden = false; });
        } catch (error) {
            loginLinks.forEach(function (node) { node.hidden = false; });
            registerLinks.forEach(function (node) { node.hidden = false; });
            userLabels.forEach(function (node) { node.hidden = true; });
            logoutButtons.forEach(function (node) { node.hidden = true; });
        }
    }

    document.addEventListener("click", async function (event) {
        var button = event.target.closest("[data-account-logout]");
        if (!button) {
            return;
        }
        event.preventDefault();
        await fetch("/Account/Logout", { method: "POST" });
        window.location.href = "/Account/Login";
    });

    window.refreshSmartCartBag = async function () {
        try {
            var response = await fetch("/Cart/GetCounts");
            if (!response.ok) {
                return;
            }
            var counts = readCounts(await response.json());
            setCount(document.querySelectorAll("[data-cart-count]"), counts.cart);
            setCount(document.querySelectorAll("[data-wishlist-count]"), counts.wishlist);
        } catch (error) {
            /* keep last shown counts */
        }
    };

    refreshAccount();
    window.refreshSmartCartBag();
})();
