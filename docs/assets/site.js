(function () {
    var header = document.querySelector(".site-header");
    var year = document.querySelector("[data-year]");

    if (year) {
        year.textContent = new Date().getFullYear();
    }

    if (!header) {
        return;
    }

    function updateHeader() {
        if (window.scrollY > 8) {
            header.classList.add("is-scrolled");
        } else {
            header.classList.remove("is-scrolled");
        }
    }

    updateHeader();
    window.addEventListener("scroll", updateHeader, { passive: true });
})();
