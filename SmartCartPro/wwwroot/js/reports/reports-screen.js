(function () {
    document.querySelectorAll("form.sc-report-filters").forEach(function (form) {
        form.addEventListener("submit", function () {
            var loading = document.getElementById("sc-report-loading");
            if (loading) loading.classList.remove("d-none");
        });
    });
})();
