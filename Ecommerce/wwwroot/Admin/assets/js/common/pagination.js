class Pagination {
    constructor(options) {
        this.containerId = options.containerId;
        this.infoId = options.infoId;
        this.onPageChange = options.onPageChange; // callback
    }

    render(settings) {
        if (!settings) return;

        const totalPages = Math.ceil(settings.totalCount / settings.pageSize);
        const pagination = document.getElementById(this.containerId);
        const paginationInfo = document.getElementById(this.infoId);

        const start = ((settings.pageIndex - 1) * settings.pageSize) + 1;
        const end = Math.min(settings.pageIndex * settings.pageSize, settings.totalCount);

        paginationInfo.textContent =
            `${start} to ${end} of ${settings.totalCount} entries`;

        let html = '';

        // ✅ Previous
        html += this.createButton(
            settings.pageIndex - 1,
            settings.pageIndex <= 1,
            "Previous"
        );

        // ✅ Smart Page Numbers (Google style)
        for (let i = 1; i <= totalPages; i++) {

            if (
                i === 1 ||
                i === totalPages ||
                (i >= settings.pageIndex - 1 && i <= settings.pageIndex + 1)
            ) {

                html += (i === settings.pageIndex)
                    ? `<li class="page-item active">
                        <span class="page-link">${i}</span>
                       </li>`
                    : this.createButton(i, false, i);

            } else if (
                i === settings.pageIndex - 2 ||
                i === settings.pageIndex + 2
            ) {
                html += `<li class="page-item disabled">
                            <span class="page-link">...</span>
                         </li>`;
            }
        }

        // ✅ Next
        html += this.createButton(
            settings.pageIndex + 1,
            settings.pageIndex >= totalPages,
            "Next"
        );

        pagination.innerHTML = html;
    }

    createButton(page, disabled, text) {
        if (disabled) {
            return `<li class="page-item disabled">
                        <span class="page-link">${text}</span>
                    </li>`;
        }

        return `<li class="page-item">
                    <a class="page-link" href="#!"
                       onclick="paginationInstance.goto(${page}); return false;">
                       ${text}
                    </a>
                </li>`;
    }

    goto(page) {
        if (this.onPageChange) {
            this.onPageChange(page);
        }
    }
}
