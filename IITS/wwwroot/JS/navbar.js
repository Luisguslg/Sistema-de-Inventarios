window.IITSToggleNavbar = function (targetId, forceClose) {
    var burger = document.querySelector('.navbar-burger');
    var id = targetId || (burger && burger.dataset.target) || 'navbarBasicExample';
    var menu = document.getElementById(id);
    if (burger && menu) {
        if (forceClose) {
            burger.classList.remove('is-active');
            menu.classList.remove('is-active');
        } else {
            burger.classList.toggle('is-active');
            menu.classList.toggle('is-active');
        }
    }
};
window.IITSCloseNavbarOnClickOutside = function () {
    document.addEventListener('click', function closeNavbar(e) {
        var menu = document.getElementById('navbarBasicExample');
        var burger = document.querySelector('.navbar-burger');
        if (menu && menu.classList.contains('is-active') && burger && !menu.contains(e.target) && !burger.contains(e.target)) {
            window.IITSToggleNavbar('navbarBasicExample', true);
        }
    }, true);
};
(function () {
    function initNavbarBurger() {
        document.querySelectorAll('.navbar-burger').forEach(function (el) {
            if (el.dataset.navbarInit) return;
            el.dataset.navbarInit = 'true';
            el.addEventListener('click', function () {
                window.IITSToggleNavbar(el.dataset.target);
            });
        });
    }
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initNavbarBurger);
    } else {
        initNavbarBurger();
    }
    document.addEventListener('DOMContentLoaded', function () {
        setTimeout(initNavbarBurger, 800);
        if (typeof window.IITSCloseNavbarOnClickOutside === 'function') {
            window.IITSCloseNavbarOnClickOutside();
        }
    });
})();
