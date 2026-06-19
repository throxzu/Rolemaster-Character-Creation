window.initTooltips = function () {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (el) {
        bootstrap.Tooltip.getOrCreateInstance(el, { trigger: 'hover focus', html: false });
    });
};

window.scrollToBottom = function (el) {
    if (el) el.scrollTop = el.scrollHeight;
};
