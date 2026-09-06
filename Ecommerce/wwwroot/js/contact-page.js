(function () {
    var form = document.getElementById("smartcart-contact-form");
    var success = document.getElementById("smartcart-contact-success");
    if (!form) {
        return;
    }

    form.addEventListener("submit", function (event) {
        event.preventDefault();
        form.reset();
        if (success) {
            success.classList.add("is-visible");
        }
    });
})();
