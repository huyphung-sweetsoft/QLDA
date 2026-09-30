(function () {
    "use strict";

    var modalSelector = ".dashboard-overview .modal, .dashboard-progress .modal, .dashboard-cost .modal, .dashboard-resource .modal";

    function ensureCloseButton(modal) {
        var content = modal.querySelector(".modal-content");
        if (!content) { return; }

        var footer = content.querySelector(".modal-footer");
        if (footer && footer.querySelector('[data-bs-dismiss="modal"]')) { return; }

        if (!footer) {
            footer = document.createElement("div");
            footer.className = "modal-footer dashboard-modal-footer";
            content.appendChild(footer);
        }

        var close = document.createElement("button");
        close.type = "button";
        close.className = "btn btn-outline-dark dashboard-modal-close-button";
        close.setAttribute("data-bs-dismiss", "modal");

        var icon = document.createElement("i");
        icon.className = "bx bx-x";
        icon.setAttribute("aria-hidden", "true");
        close.appendChild(icon);

        var isEnglish = /^en\b/i.test(document.documentElement.lang || "");
        close.appendChild(document.createTextNode(isEnglish ? "Close" : "Đóng"));
        footer.appendChild(close);
    }

    function init() {
        document.querySelectorAll(modalSelector).forEach(ensureCloseButton);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
}());
