(function () {
  const MOBILE_PX = 992;
  const STORAGE_KEY = "sidebar-collapsed";

  function ready(fn) {
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", fn);
    } else {
      fn();
    }
  }

  ready(function initAdminSidebar() {
    const wrapper = document.getElementById("admin-wrapper");
    const sidebar = document.getElementById("admin-sidebar");
    const toggle = document.querySelector("[data-sc-sidebar-toggle]");
    if (!wrapper || !sidebar || !toggle) return;

    let backdrop = document.querySelector(".sidebar-backdrop");
    if (!backdrop) {
      backdrop = document.createElement("div");
      backdrop.className = "sidebar-backdrop";
      backdrop.setAttribute("aria-hidden", "true");
      wrapper.appendChild(backdrop);
    }

    toggle.setAttribute("aria-controls", sidebar.id);

    sidebar.querySelectorAll(".sidebar-nav .nav-link").forEach(function (link) {
      if (!link.getAttribute("title")) {
        const label = link.querySelector("span");
        if (label && label.textContent) {
          link.setAttribute("title", label.textContent.trim());
        }
      }
    });

    function isMobile() {
      return window.innerWidth < MOBILE_PX;
    }

    function syncAria() {
      const expanded = isMobile()
        ? sidebar.classList.contains("show")
        : !wrapper.classList.contains("sidebar-collapsed");
      toggle.setAttribute("aria-expanded", String(expanded));
      toggle.classList.toggle("is-active", !expanded && !isMobile());
    }

    function closeMobile() {
      sidebar.classList.remove("show");
      backdrop.classList.remove("show");
      document.body.style.overflow = "";
      syncAria();
    }

    function openMobile() {
      sidebar.classList.add("show");
      backdrop.classList.add("show");
      document.body.style.overflow = "hidden";
      syncAria();
    }

    function applyDesktopState() {
      const collapsed = localStorage.getItem(STORAGE_KEY) === "true";
      wrapper.classList.toggle("sidebar-collapsed", collapsed);
      closeMobile();
    }

    function toggleDesktop() {
      const collapsed = !wrapper.classList.contains("sidebar-collapsed");
      wrapper.classList.toggle("sidebar-collapsed", collapsed);
      localStorage.setItem(STORAGE_KEY, String(collapsed));
      syncAria();
    }

    toggle.addEventListener("click", function (e) {
      e.preventDefault();
      if (isMobile()) {
        if (sidebar.classList.contains("show")) closeMobile();
        else openMobile();
      } else {
        toggleDesktop();
      }
    });

    backdrop.addEventListener("click", closeMobile);

    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape" && isMobile() && sidebar.classList.contains("show")) {
        closeMobile();
      }
    });

    sidebar.addEventListener("click", function (e) {
      const link = e.target.closest("a.nav-link");
      if (link && isMobile()) closeMobile();
    });

    let resizeTimer;
    window.addEventListener("resize", function () {
      clearTimeout(resizeTimer);
      resizeTimer = setTimeout(function () {
        if (isMobile()) {
          wrapper.classList.remove("sidebar-collapsed");
          closeMobile();
        } else {
          applyDesktopState();
        }
        syncAria();
      }, 120);
    });

    if (isMobile()) {
      closeMobile();
    } else {
      applyDesktopState();
    }
    syncAria();
  });
})();
