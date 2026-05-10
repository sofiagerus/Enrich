/* Admin Bundle Edit page */
const AdminBundleEdit = (() => {
    const state = {
        selectedCategoryIds: [],
        selectedWordIds: [],
        selectedLevels: [],
        selectedStatus: null
    };

    const elements = {
        descriptionInput: null,
        descriptionCount: null,
        fileInput: null,
        hiddenImageInput: null,
        previewContainer: null,
        uploadPlaceholder: null,
        changeImageBtn: null,
        previewImage: null,
        imageUploadBox: null,

        categoriesDropdown: null,
        wordsDropdown: null,
        levelsDropdown: null,
        statusDropdown: null
    };

    function init(options) {
        elements.descriptionInput = document.querySelector('textarea[name="Description"]');
        elements.descriptionCount = document.getElementById('description-count');
        elements.fileInput = document.getElementById('file-input');
        elements.hiddenImageInput = document.getElementById('ImageUrl');
        elements.previewContainer = document.getElementById('image-preview-container');
        elements.uploadPlaceholder = document.getElementById('upload-placeholder');
        elements.changeImageBtn = document.getElementById('change-image-btn-container');
        elements.previewImage = document.getElementById('image-preview');
        elements.imageUploadBox = document.getElementById('image-upload-box');

        elements.categoriesDropdown = document.getElementById('categories-dropdown-container');
        elements.wordsDropdown = document.getElementById('words-dropdown-container');
        elements.levelsDropdown = document.getElementById('levels-dropdown-container');
        elements.statusDropdown = document.getElementById('statusDropdownContainer');

        state.selectedCategoryIds = options.selectedCategoryIds || [];
        state.selectedWordIds = options.selectedWordIds || [];
        state.selectedLevels = options.selectedLevels || [];
        state.selectedStatus = options.selectedStatus ?? null;

        bindDropdown('categories-dropdown-container', 'CategoryIds', state.selectedCategoryIds, false);
        bindDropdown('words-dropdown-container', 'WordIds', state.selectedWordIds, false);
        bindDropdown('levels-dropdown-container', 'DifficultyLevels', state.selectedLevels, false);
        bindDropdown('statusDropdownContainer', 'Status', state.selectedStatus !== null && state.selectedStatus !== undefined ? [state.selectedStatus] : [], true);

        initListeners();
        updateDescriptionCount();
        syncHiddenInputs('category-ids-hidden', 'CategoryIds', state.selectedCategoryIds);
        syncHiddenInputs('word-ids-hidden', 'WordIds', state.selectedWordIds);
        syncHiddenInputs('level-ids-hidden', 'DifficultyLevels', state.selectedLevels);
        syncStatusInput();
        updateButtonText(elements.categoriesDropdown, state.selectedCategoryIds, 'categories');
        updateButtonText(elements.wordsDropdown, state.selectedWordIds, 'words');
        updateButtonText(elements.levelsDropdown, state.selectedLevels, 'levels');
        updateStatusButtonText();
    }

    function initListeners() {
        if (elements.descriptionInput) {
            elements.descriptionInput.addEventListener('input', updateDescriptionCount);
        }
        if (elements.fileInput) {
            elements.fileInput.addEventListener('change', handleImageUpload);
        }

        if (elements.imageUploadBox) {
            elements.imageUploadBox.addEventListener('click', function () {
                if (elements.fileInput) elements.fileInput.click();
            });
        }

        if (document.getElementById('upload-image-btn')) {
            document.getElementById('upload-image-btn').addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                if (elements.fileInput) elements.fileInput.click();
            });
        }

        if (document.getElementById('change-image-btn')) {
            document.getElementById('change-image-btn').addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                if (elements.fileInput) elements.fileInput.click();
            });
        }
    }

    function bindDropdown(containerId, hiddenName, selectedValues, singleSelect) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const button = container.querySelector('.btn-dropdown');
        const displaySpan = button.querySelector('.selected-items-display');
        const items = container.querySelectorAll('.dropdown-item-custom');
        const hiddenContainer = document.getElementById(hiddenName === 'CategoryIds' ? 'category-ids-hidden' : hiddenName === 'WordIds' ? 'word-ids-hidden' : hiddenName === 'DifficultyLevels' ? 'level-ids-hidden' : null);

        function sync() {
            const selected = Array.from(items).filter(item => item.classList.contains('selected'));

            if (singleSelect) {
                const selectedItem = selected[0];
                if (!selectedItem) {
                    displaySpan.textContent = 'Select status...';
                    displaySpan.classList.add('text-muted');
                } else {
                    displaySpan.textContent = selectedItem.getAttribute('data-label') || selectedItem.textContent.trim();
                    displaySpan.classList.remove('text-muted');
                }
                return;
            }

            const count = selected.length;
            if (count === 0) {
                displaySpan.textContent = 'Select ' + hiddenName.replace('Ids', '').toLowerCase() + '...';
                displaySpan.classList.add('text-muted');
            } else if (count === 1) {
                displaySpan.textContent = selected[0].getAttribute('data-label') || selected[0].textContent.trim();
                displaySpan.classList.remove('text-muted');
            } else if (count === 2) {
                displaySpan.textContent = selected.map(item => item.getAttribute('data-label') || item.textContent.trim()).join(', ');
                displaySpan.classList.remove('text-muted');
            } else {
                displaySpan.textContent = count + ' selected';
                displaySpan.classList.remove('text-muted');
            }
        }

        items.forEach(function (item) {
            item.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                if (singleSelect) {
                    items.forEach(function (other) {
                        other.classList.remove('selected');
                    });
                    item.classList.add('selected');
                    state.selectedStatus = item.getAttribute('data-value');
                    syncStatusInput();
                    sync();

                    if (window.bootstrap && bootstrap.Dropdown) {
                        const dropdown = bootstrap.Dropdown.getInstance(button) || new bootstrap.Dropdown(button);
                        dropdown.hide();
                    }
                    return;
                }

                item.classList.toggle('selected');

                const values = Array.from(items)
                    .filter(x => x.classList.contains('selected'))
                    .map(x => x.getAttribute('data-value'))
                    .filter(Boolean);

                if (hiddenName === 'CategoryIds') state.selectedCategoryIds = values;
                if (hiddenName === 'WordIds') state.selectedWordIds = values;
                if (hiddenName === 'DifficultyLevels') state.selectedLevels = values;

                if (hiddenContainer) {
                    hiddenContainer.innerHTML = '';
                    values.forEach(function (value) {
                        const input = document.createElement('input');
                        input.type = 'hidden';
                        input.name = hiddenName;
                        input.value = value;
                        hiddenContainer.appendChild(input);
                    });
                }

                sync();
            });
        });

        sync();
    }

    function syncHiddenInputs(containerId, hiddenName, values) {
        const hiddenContainer = document.getElementById(containerId);
        if (!hiddenContainer) return;
        hiddenContainer.innerHTML = '';
        values.forEach(function (value) {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = hiddenName;
            input.value = value;
            hiddenContainer.appendChild(input);
        });
    }

    function syncStatusInput() {
        const statusInput = document.querySelector('input[name="Status"]');
        if (statusInput) {
            statusInput.value = state.selectedStatus || statusInput.value || '';
        }
        updateStatusButtonText();
    }

    function updateStatusButtonText() {
        const container = document.getElementById('statusDropdownContainer');
        if (!container) return;
        const button = container.querySelector('.btn-dropdown');
        const displaySpan = button.querySelector('.selected-items-display');
        const selected = container.querySelector('.dropdown-item-custom.selected');
        if (selected) {
            displaySpan.textContent = selected.getAttribute('data-label') || selected.textContent.trim();
            displaySpan.classList.remove('text-muted');
        } else {
            displaySpan.textContent = 'Select status...';
            displaySpan.classList.add('text-muted');
        }
    }

    function updateButtonText(container, selectedValues, type) {
        if (!container) return;
        const display = container.querySelector('.selected-items-display');
        if (!display) return;

        const emptyLabels = {
            categories: 'Select categories...',
            words: 'Select words...',
            levels: 'Select levels...'
        };

        if (!selectedValues || selectedValues.length === 0) {
            display.textContent = emptyLabels[type] || 'Select items...';
            display.classList.add('text-muted');
            return;
        }

        const items = Array.from(container.querySelectorAll('.dropdown-item-custom.selected'));
        if (items.length === 1) {
            display.textContent = items[0].getAttribute('data-label') || items[0].textContent.trim();
        } else if (items.length === 2) {
            display.textContent = items.map(item => item.getAttribute('data-label') || item.textContent.trim()).join(', ');
        } else {
            display.textContent = items.length + ' selected';
        }
        display.classList.remove('text-muted');
    }

    function updateDescriptionCount() {
        if (elements.descriptionInput && elements.descriptionCount) {
            elements.descriptionCount.textContent = elements.descriptionInput.value.length;
        }
    }

    function handleImageUpload(e) {
        const file = e.target.files && e.target.files[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = function (event) {
            const img = new Image();
            img.onload = function () {
                const canvas = document.createElement('canvas');
                let width = img.width;
                let height = img.height;
                const maxSize = 800;

                if (width > height) {
                    if (width > maxSize) {
                        height *= maxSize / width;
                        width = maxSize;
                    }
                } else if (height > maxSize) {
                    width *= maxSize / height;
                    height = maxSize;
                }

                canvas.width = width;
                canvas.height = height;
                const ctx = canvas.getContext('2d');
                ctx.drawImage(img, 0, 0, width, height);

                const dataUrl = canvas.toDataURL('image/jpeg', 0.7);
                if (elements.previewImage) elements.previewImage.src = dataUrl;
                if (elements.previewContainer) elements.previewContainer.style.display = 'block';
                if (elements.uploadPlaceholder) elements.uploadPlaceholder.style.display = 'none';
                if (elements.changeImageBtn) elements.changeImageBtn.style.display = 'block';
                if (elements.hiddenImageInput) elements.hiddenImageInput.value = dataUrl;
            };
            img.src = event.target.result;
        };
        reader.readAsDataURL(file);
    }

    return { init };
})();

document.addEventListener('DOMContentLoaded', function () {
    const selectedCategoryIds = Array.from(document.querySelectorAll('#categories-dropdown-container .dropdown-item-custom.selected')).map(item => item.getAttribute('data-value'));
    const selectedWordIds = Array.from(document.querySelectorAll('#words-dropdown-container .dropdown-item-custom.selected')).map(item => item.getAttribute('data-value'));
    const selectedLevels = Array.from(document.querySelectorAll('#levels-dropdown-container .dropdown-item-custom.selected')).map(item => item.getAttribute('data-value'));
    const selectedStatus = document.querySelector('#statusDropdownContainer .dropdown-item-custom.selected')?.getAttribute('data-value') || null;

    AdminBundleEdit.init({
        selectedCategoryIds,
        selectedWordIds,
        selectedLevels,
        selectedStatus
    });
});
