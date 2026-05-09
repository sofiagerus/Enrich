document.addEventListener("DOMContentLoaded", function () {
    const textareas = document.querySelectorAll("textarea");

    function resizeTextarea(el) {
        el.style.height = 'auto';
        el.style.height = (el.scrollHeight + 2) + 'px';
    }

    textareas.forEach(textarea => {
        resizeTextarea(textarea);

        textarea.addEventListener("input", function () {
            resizeTextarea(this);
        });
        
        window.addEventListener('resize', function() {
            resizeTextarea(textarea);
        });
    });
    const form = document.getElementById("edit-word-form");
    if (form) {
        form.addEventListener("submit", function (e) {
            const submitBtn = this.querySelector('button[type="submit"]');
            
            if (typeof $(form).valid === "function" && !$(form).valid()) {
                return;
            }

            if (submitBtn) {
                const originalContent = submitBtn.innerHTML;
                const originalWidth = submitBtn.offsetWidth;
                
                submitBtn.style.minWidth = originalWidth + 'px';
                
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> Saving...';
                
                setTimeout(function() {
                    if (document.contains(submitBtn)) {
                        submitBtn.disabled = false;
                        submitBtn.innerHTML = originalContent;
                    }
                }, 5000);
            }
        });
    }
});
