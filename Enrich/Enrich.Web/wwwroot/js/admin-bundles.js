/* JS for Manage System Collections page */
(function () {
    var debounceTimer;

    window.debounceSearch = function (delay) {
        delay = typeof delay === 'number' ? delay : 500;
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(function () {
            var form = document.querySelector('form[method="get"]');
            if (form) form.submit();
        }, delay);
    };

    document.addEventListener('DOMContentLoaded', function () {
        // Attach delete form handlers to provide consistent confirm + disable behavior
        var deleteForms = document.querySelectorAll('.delete-form');
        deleteForms.forEach(function (form) {
            form.addEventListener('submit', function (e) {
                e.preventDefault();
                var ok = confirm('Are you sure you want to delete this collection?');
                if (!ok) return;
                var btn = form.querySelector('button[type="submit"]');
                if (btn) {
                    btn.disabled = true;
                }
                // submit the form after disabling button to prevent double submits
                form.submit();
            });
        });
    });
})();
