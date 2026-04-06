document.addEventListener("DOMContentLoaded", function () {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.forEach(function (el) {
        new bootstrap.Tooltip(el);
    });

    var siteHeader = document.getElementById("siteHeader");
    if (siteHeader) {
        var scrollThreshold = 12;
        function syncHeaderScroll() {
            if (window.scrollY > scrollThreshold) {
                siteHeader.classList.add("site-header--scrolled");
            } else {
                siteHeader.classList.remove("site-header--scrolled");
            }
        }
        syncHeaderScroll();
        window.addEventListener("scroll", syncHeaderScroll, { passive: true });
    }

    var reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    var reveals = document.querySelectorAll(".reveal, .reveal-up, .reveal-fade");
    if (reveals.length) {
        if (reduceMotion) {
            reveals.forEach(function (el) {
                el.classList.add("is-visible");
            });
        } else if (!("IntersectionObserver" in window)) {
            reveals.forEach(function (el) {
                el.classList.add("is-visible");
            });
        } else {
            var io = new IntersectionObserver(
                function (entries) {
                    entries.forEach(function (entry) {
                        if (entry.isIntersecting) {
                            entry.target.classList.add("is-visible");
                            io.unobserve(entry.target);
                        }
                    });
                },
                { root: null, rootMargin: "0px 0px -48px 0px", threshold: 0.06 }
            );

            reveals.forEach(function (el) {
                io.observe(el);
            });
        }
    }

    var homeSections = document.querySelectorAll(".home-section");
    if (homeSections.length) {
        if (reduceMotion) {
            homeSections.forEach(function (el) {
                el.classList.add("is-inview");
            });
        } else if (!("IntersectionObserver" in window)) {
            homeSections.forEach(function (el) {
                el.classList.add("is-inview");
            });
        } else {
            var secIo = new IntersectionObserver(
                function (entries) {
                    entries.forEach(function (entry) {
                        if (entry.isIntersecting) {
                            entry.target.classList.add("is-inview");
                            secIo.unobserve(entry.target);
                        }
                    });
                },
                { root: null, rootMargin: "0px 0px -12% 0px", threshold: 0.08 }
            );
            homeSections.forEach(function (el) {
                secIo.observe(el);
            });
        }
    }
});

/* Back to top — hiển thị khi cuộn, scroll mượt (tôn trọng prefers-reduced-motion) */
document.addEventListener("DOMContentLoaded", function () {
    var btn = document.getElementById("backToTop");
    if (!btn) return;

    var threshold = 240;
    var reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    function toggleVisibility() {
        if (window.scrollY > threshold) {
            btn.classList.add("is-visible");
        } else {
            btn.classList.remove("is-visible");
        }
    }

    toggleVisibility();
    window.addEventListener("scroll", toggleVisibility, { passive: true });

    btn.addEventListener("click", function () {
        if (reduceMotion) {
            window.scrollTo(0, 0);
        } else {
            window.scrollTo({ top: 0, left: 0, behavior: "smooth" });
        }
    });
});
