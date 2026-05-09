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

    function setupDropdown(dropdownId, hiddenInputId) {
        const container = document.getElementById(dropdownId);
        if (!container) return;

        const button = container.querySelector('.btn-dropdown');
        const displaySpan = button.querySelector('.selected-items-display');
        const items = container.querySelectorAll('.dropdown-item-custom');
        const hiddenInput = document.getElementById(hiddenInputId);

        items.forEach(item => {
            item.addEventListener('click', function (e) {
                e.preventDefault();
                const value = this.getAttribute('data-value');
                const text = this.innerText.trim();

                hiddenInput.value = value;
                
                displaySpan.innerText = text;

                items.forEach(i => i.classList.remove('active'));
                this.classList.add('active');

                if (typeof bootstrap !== 'undefined' && bootstrap.Dropdown) {
                    const bsDropdown = bootstrap.Dropdown.getInstance(button) || new bootstrap.Dropdown(button);
                    if (bsDropdown) {
                        bsDropdown.hide();
                    }
                }
            });
        });
        
        const currentValue = hiddenInput.value;
        if (currentValue) {
            const activeItem = Array.from(items).find(i => 
                (i.getAttribute('data-value') || '').toLowerCase() === currentValue.toLowerCase()
            );
            if (activeItem) {
                displaySpan.innerText = activeItem.innerText.trim();
                activeItem.classList.add('active');
            }
        } else {
            const defaultItem = container.querySelector(`.dropdown-item-custom[data-value=""]`);
            if (defaultItem) {
                defaultItem.classList.add('active');
            }
        }
    }

    setupDropdown('pos-dropdown', 'PartOfSpeech');
    setupDropdown('level-dropdown', 'DifficultyLevel');

    function setupCombobox(containerId, inputId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const input = document.getElementById(inputId);
        const items = container.querySelectorAll('.dropdown-item-custom');

        items.forEach(item => {
            item.addEventListener('click', function (e) {
                e.preventDefault();
                const text = this.innerText.trim();
                input.value = text;
                items.forEach(i => i.classList.remove('active'));
                this.classList.add('active');

                if (typeof bootstrap !== 'undefined' && bootstrap.Dropdown) {
                    const bsDropdown = bootstrap.Dropdown.getInstance(input) || new bootstrap.Dropdown(input);
                    if (bsDropdown) {
                        bsDropdown.hide();
                    }
                }
            });
        });

        const currentValue = input.value;
        if (currentValue) {
            const activeItem = Array.from(items).find(i => i.innerText.trim() === currentValue);
            if (activeItem) {
                activeItem.classList.add('active');
            }
        }

        input.addEventListener('input', function() {
            const val = this.value.trim().toLowerCase();
            items.forEach(i => {
                i.classList.remove('active');
                if (val && i.innerText.trim().toLowerCase() === val) {
                    i.classList.add('active');
                }
            });
        });
    }

    setupCombobox('category-combobox', 'NewCategory');

    const form = document.querySelector("form");
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
