document.addEventListener('DOMContentLoaded', function () {
    const dropdownContainer = document.getElementById('category-dropdown-container');
    const dropdown = document.getElementById('category-dropdown');
    const button = document.getElementById('categoryBtn');
    const selectedText = document.getElementById('category-selected-text');
    const hiddenCategory = document.getElementById('NewCategory');
    const manualCategoryWrapper = document.getElementById('manual-category-wrapper');
    const manualCategoryInput = document.getElementById('manual-category-input');
    const list = document.getElementById('category-list');
    const difficultyContainer = document.getElementById('difficulty-dropdown-container');
    const posContainer = document.getElementById('pos-dropdown-container');
    const difficultyHidden = document.getElementById('DifficultyLevel');
    const posHidden = document.getElementById('PartOfSpeech');
    let allCategories = [];

    async function fetchCategories() {
        try {
            const res = await fetch('/Word/GetCategories');
            if (res.ok) {
                allCategories = await res.json();
                renderCategoryDropdown();
            }
        } catch (error) {
            console.error('Error loading categories:', error);
        }
    }

    function escapeHtml(value) {
        return value
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
    }

    function openCategoryDropdown() {
        if (dropdownContainer && window.bootstrap && bootstrap.Dropdown) {
            const dropdownInstance = bootstrap.Dropdown.getInstance(button) || new bootstrap.Dropdown(button);
            dropdownInstance.show();
        }
    }

    function closeCategoryDropdown() {
        if (dropdownContainer && window.bootstrap && bootstrap.Dropdown) {
            const dropdownInstance = bootstrap.Dropdown.getInstance(button);
            if (dropdownInstance) {
                dropdownInstance.hide();
            }
        }
    }

    function setCategorySelection(value, text, manualMode) {
        hiddenCategory.value = value || '';
        selectedText.innerText = text || 'Any';
        selectedText.classList.toggle('text-muted', !value && text !== 'Any');

        if (manualCategoryWrapper) {
            manualCategoryWrapper.classList.toggle('d-none', !manualMode);
        }

        if (manualMode && manualCategoryInput) {
            manualCategoryInput.value = value || '';
        }
        
        // Update active state in dropdown
        const list = document.getElementById('category-list');
        if (list) {
            list.querySelectorAll('.category-item').forEach(item => {
                item.classList.remove('active', 'bg-success', 'text-white');
            });
            const activeItem = list.querySelector(`[data-category-name="${value}"]`);
            if (activeItem) {
                activeItem.classList.add('active', 'bg-success', 'text-white');
            }
        }
    }

    function renderCategoryDropdown() {
        const list = document.getElementById('category-list');
        const anyItem = list.querySelector('[data-category-name=""]');
        
        // Clear all items except the "Any" option
        list.innerHTML = '';
        
        // Re-add the "Any" option
        const anyOption = document.createElement('a');
        anyOption.href = '#';
        anyOption.className = 'dropdown-item dropdown-item-custom category-item rounded-4 mx-1 px-3 py-2';
        anyOption.setAttribute('data-category-name', '');
        anyOption.setAttribute('data-category-label', 'Any');
        anyOption.style.width = 'auto';
        anyOption.innerHTML = '<span>Any</span><i class="bi bi-check2 opacity-0"></i>';
        anyOption.addEventListener('click', function (e) {
            e.preventDefault();
            setCategorySelection('', 'Any', false);
            closeCategoryDropdown();
        });
        list.appendChild(anyOption);
        
        // Add existing categories
        allCategories.forEach(category => {
            const item = document.createElement('a');
            item.href = '#';
            item.className = 'dropdown-item dropdown-item-custom category-item rounded-4 mx-1 px-3 py-2';
            item.setAttribute('data-category-name', category.name);
            item.setAttribute('data-category-label', category.name);
            item.style.width = 'auto';
            item.innerHTML = `<span>${category.name}</span><i class="bi bi-check2 opacity-0"></i>`;
            item.addEventListener('click', function (e) {
                e.preventDefault();
                setCategorySelection(category.name, category.name, false);
                closeCategoryDropdown();
            });
            list.appendChild(item);
        });
        
        // Set initial active state
        const currentValue = hiddenCategory.value.trim();
        const activeItem = list.querySelector(`[data-category-name="${currentValue}"]`);
        if (activeItem) {
            list.querySelectorAll('.category-item').forEach(item => item.classList.remove('active', 'bg-success', 'text-white'));
            activeItem.classList.add('active', 'bg-success', 'text-white');
        } else {
            // Default to "Any"
            anyOption.classList.add('active', 'bg-success', 'text-white');
        }
    }

    function handleCategoryItemKeydown(e) {
        const items = [...list.querySelectorAll('.category-item')];
        const idx = items.indexOf(document.activeElement);

        if (e.key === 'ArrowDown' && idx < items.length - 1) {
            items[idx + 1].focus();
            e.preventDefault();
        } else if (e.key === 'ArrowUp') {
            idx > 0 ? items[idx - 1].focus() : button.focus();
            e.preventDefault();
        } else if (e.key === 'Enter') {
            e.preventDefault();
            document.activeElement.dispatchEvent(new MouseEvent('click'));
        } else if (e.key === 'Escape') {
            closeCategoryDropdown();
            button.focus();
        }
    }

    function setupSingleDropdown(container, hiddenInput) {
        if (!container) return;

        const btn = container.querySelector('.btn-dropdown');
        const display = container.querySelector('.selected-items-display');
        const items = container.querySelectorAll('.dropdown-item-custom');

        items.forEach(item => {
            item.addEventListener('click', function (e) {
                e.preventDefault();
                const value = this.getAttribute('data-value') || '';
                const text = this.getAttribute('data-label') || this.innerText.trim();

                hiddenInput.value = value;
                display.innerText = text;
                display.classList.toggle('text-muted', !value);

                items.forEach(option => option.classList.remove('active', 'bg-success', 'text-white'));
                this.classList.add('active');
                this.classList.add('bg-success', 'text-white');

                if (window.bootstrap && bootstrap.Dropdown) {
                    const dropdownInstance = bootstrap.Dropdown.getInstance(btn) || new bootstrap.Dropdown(btn);
                    dropdownInstance.hide();
                }
            });
        });

        const currentValue = hiddenInput.value;
        if (currentValue) {
            const activeItem = container.querySelector(`.dropdown-item-custom[data-value="${currentValue}"]`);
            if (activeItem) {
                display.innerText = activeItem.getAttribute('data-label') || activeItem.innerText.trim();
                display.classList.remove('text-muted');
                activeItem.classList.add('active', 'bg-success', 'text-white');
            }
        }
    }

    if (button) {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            renderCategoryDropdown();
        });
    }

    if (manualCategoryInput) {
        manualCategoryInput.addEventListener('input', function () {
            hiddenCategory.value = this.value.trim();
            selectedText.innerText = this.value.trim() || 'Any';
            selectedText.classList.toggle('text-muted', !this.value.trim());
        });
    }

    if (difficultyContainer && difficultyHidden) {
        setupSingleDropdown(difficultyContainer, difficultyHidden);
    }

    if (posContainer && posHidden) {
        setupSingleDropdown(posContainer, posHidden);
    }

    // Initialize with "Any" as default and render all categories
    setCategorySelection('', 'Any', false);
    fetchCategories();
});
