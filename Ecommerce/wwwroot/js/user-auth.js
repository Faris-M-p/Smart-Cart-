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

    // Password visibility eye toggle handler
    document.addEventListener("click", function (event) {
        var toggleBtn = event.target.closest(".password-toggle-btn");
        if (!toggleBtn) return;

        event.preventDefault();
        var group = toggleBtn.closest(".password-input-group");
        if (!group) return;

        var input = group.querySelector("input");
        if (!input) return;

        var isPassword = input.type === "password";
        input.type = isPassword ? "text" : "password";

        var eyeOpen = toggleBtn.querySelector(".eye-icon-open");
        var eyeClosed = toggleBtn.querySelector(".eye-icon-closed");

        if (eyeOpen && eyeClosed) {
            if (isPassword) {
                eyeOpen.classList.add("d-none");
                eyeClosed.classList.remove("d-none");
                toggleBtn.setAttribute("title", "Hide password");
                toggleBtn.setAttribute("aria-label", "Hide password");
            } else {
                eyeOpen.classList.remove("d-none");
                eyeClosed.classList.add("d-none");
                toggleBtn.setAttribute("title", "Show password");
                toggleBtn.setAttribute("aria-label", "Show password");
            }
        }
    });

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
                showStatus("Could not log in. Please check your credentials and try again.");
            }
        });
    }

    if (registerForm) {
        registerForm.addEventListener("submit", async function (event) {
            event.preventDefault();
            
            var name = document.getElementById("register-name").value.trim();
            var email = document.getElementById("register-email").value.trim();
            var passwordInput = document.getElementById("register-password");
            var confirmPasswordInput = document.getElementById("register-confirm-password");
            var password = passwordInput ? passwordInput.value : "";
            var confirmPassword = confirmPasswordInput ? confirmPasswordInput.value : password;

            if (confirmPasswordInput && password !== confirmPassword) {
                showStatus("Passwords do not match.");
                confirmPasswordInput.focus();
                return;
            }

            showStatus("Creating account...");
            try {
                var result = await post("/Account/Register", {
                    name: name,
                    email: email,
                    password: password
                });
                if (!(result.statusCode || result.StatusCode)) {
                    showStatus(result.responseMsg || result.ResponseMsg || "Could not register.");
                    return;
                }
                
                // Auto-login and redirect upon successful registration
                var returnUrl = registerForm.getAttribute("data-return-url") || "/Shop";
                window.location.href = returnUrl;
            } catch (error) {
                showStatus("Could not register. Please try again.");
            }
        });
    }
})();
