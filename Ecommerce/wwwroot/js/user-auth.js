(function () {
    var statusEl = document.getElementById("auth-status");
    var loginForm = document.getElementById("user-login-form");
    var registerForm = document.getElementById("user-register-form");

    function showStatus(message) {
        if (!statusEl) {
            return;
        }
        statusEl.textContent = message || "";
        statusEl.hidden = !message;
    }

    async function post(url, body) {
        var response = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });
        return response.json();
    }

    if (loginForm) {
        loginForm.addEventListener("submit", async function (event) {
            event.preventDefault();
            showStatus("Signing in...");
            try {
                var result = await post("/Account/Login", {
                    email: document.getElementById("login-email").value,
                    password: document.getElementById("login-password").value
                });
                if (!(result.statusCode || result.StatusCode)) {
                    showStatus(result.responseMsg || result.ResponseMsg || "Could not log in.");
                    return;
                }
                window.location.href = loginForm.getAttribute("data-return-url") || "/Shop";
            } catch (error) {
                showStatus("Could not log in. Apply the user auth SQL scripts and try again.");
            }
        });
    }

    if (registerForm) {
        registerForm.addEventListener("submit", async function (event) {
            event.preventDefault();
            showStatus("Creating account...");
            try {
                var result = await post("/Account/Register", {
                    name: document.getElementById("register-name").value,
                    email: document.getElementById("register-email").value,
                    password: document.getElementById("register-password").value
                });
                if (!(result.statusCode || result.StatusCode)) {
                    showStatus(result.responseMsg || result.ResponseMsg || "Could not register.");
                    return;
                }
                window.location.href = "/Account/Login?returnUrl=" + encodeURIComponent(registerForm.getAttribute("data-return-url") || "/Shop");
            } catch (error) {
                showStatus("Could not register. Apply the user auth SQL scripts and try again.");
            }
        });
    }
})();
