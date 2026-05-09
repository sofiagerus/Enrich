document.addEventListener("DOMContentLoaded", function () {
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
            const activeItem = container.querySelector(`.dropdown-item-custom[data-value="${currentValue}"]`);
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

    setupDropdown('category-dropdown', 'CategoryFilter');
    setupDropdown('pos-dropdown', 'PosFilter');
    setupDropdown('level-dropdown', 'LevelFilter');
});
