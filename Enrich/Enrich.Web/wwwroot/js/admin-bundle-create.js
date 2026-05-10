/* Admin Bundle Create page  */
const AdminBundleCreate = (() => {
    const state = {
        selectedCategoryIds: [],
        selectedWordIds: [],
        selectedLevels: [],
        allCategories: [],
        allWords: [],
        availableLevels: []
    };

    const elements = {
        descriptionInput: null,
        descriptionCount: null,
        fileInput: null,
        hiddenImageInput: null,
        previewContainer: null,
        uploadPlaceholder: null,
        previewImage: null,
        imageUploadBox: null,
        categoriesDropdown: null,
        wordsDropdown: null,
        levelsDropdown: null
    };

    function init(options) {
        elements.descriptionInput = document.querySelector('textarea[name="Description"]');
        elements.descriptionCount = document.getElementById('description-count');
        elements.fileInput = document.getElementById('file-input');
        elements.hiddenImageInput = document.getElementById('ImageUrl');
        elements.previewContainer = document.getElementById('image-preview-container');
        elements.uploadPlaceholder = document.getElementById('upload-placeholder');
        elements.previewImage = document.getElementById('image-preview');
        elements.imageUploadBox = document.getElementById('image-upload-box');

        elements.categoriesDropdown = document.getElementById('categories-dropdown-container');
        elements.wordsDropdown = document.getElementById('words-dropdown-container');
        elements.levelsDropdown = document.getElementById('levels-dropdown-container');

        state.allWords = options.allWords || [];
        state.availableLevels = options.availableLevels || [];
        state.selectedCategoryIds = options.selectedCategoryIds || [];
        state.selectedWordIds = options.selectedWordIds || [];
        state.selectedLevels = options.selectedLevels || [];

        initListeners();

        const form = document.querySelector('.bundle-create-form');
        if (form) {
            form.addEventListener('submit', () => {
                console.log('Submitting form. Image data length:', elements.hiddenImageInput.value ? elements.hiddenImageInput.value.length : 0);
            });
        }

        loadCategories();
        renderWordsDropdown(state.allWords);
        renderLevelsDropdown(state.availableLevels);
        updateDescriptionCount();
    }

    function initListeners() {
        if (elements.descriptionInput) elements.descriptionInput.addEventListener('input', updateDescriptionCount);
        if (elements.fileInput) elements.fileInput.addEventListener('change', handleImageUpload);
        if (elements.imageUploadBox) {
            elements.imageUploadBox.addEventListener('click', function () {
                if (elements.fileInput) elements.fileInput.click();
            });
        }

        const uploadButton = document.getElementById('upload-image-btn');
        if (uploadButton) {
            uploadButton.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                if (elements.fileInput) elements.fileInput.click();
            });
        }
    }

    async function loadCategories() {
        try {
            const res = await fetch('/Word/GetCategories');
            if (res.ok) {
                state.allCategories = await res.json();
                renderCategoriesDropdown(state.allCategories);
            }
        } catch (e) {
            console.error('Error loading categories:', e);
        }
    }

    function renderCategoriesDropdown(categories) {
        const listContainer = elements.categoriesDropdown.querySelector('.dropdown-list-items');
        const render = (cats) => {
            listContainer.innerHTML = '';
            cats.forEach(cat => {
                const isSelected = state.selectedCategoryIds.includes(parseInt(cat.id));
                const item = createDropdownItem(cat.id, cat.name.split('(')[0].trim(), isSelected, (id, selected) => {
                    if (selected) {
                        if (!state.selectedCategoryIds.includes(id)) state.selectedCategoryIds.push(id);
                    } else {
                        state.selectedCategoryIds = state.selectedCategoryIds.filter(x => x !== id);
                    }
                    updateToggleText(elements.categoriesDropdown, state.selectedCategoryIds.length, 'categories');
                    updateHiddenInputs('category-ids-hidden', 'CategoryIds', state.selectedCategoryIds);
                });
                listContainer.appendChild(item);
            });
        };

        render(categories);
        updateToggleText(elements.categoriesDropdown, state.selectedCategoryIds.length, 'categories');
        updateHiddenInputs('category-ids-hidden', 'CategoryIds', state.selectedCategoryIds);
    }

    function renderWordsDropdown(words) {
        const listContainer = elements.wordsDropdown.querySelector('.dropdown-list-items');
        const render = (wds) => {
            listContainer.innerHTML = '';
            wds.forEach(word => {
                const isSelected = state.selectedWordIds.includes(parseInt(word.id));
                const item = createDropdownItem(word.id, word.term, isSelected, (id, selected) => {
                    if (selected) {
                        if (!state.selectedWordIds.includes(id)) state.selectedWordIds.push(id);
                    } else {
                        state.selectedWordIds = state.selectedWordIds.filter(x => x !== id);
                    }
                    updateToggleText(elements.wordsDropdown, state.selectedWordIds.length, 'words');
                    updateHiddenInputs('word-ids-hidden', 'WordIds', state.selectedWordIds);
                });
                listContainer.appendChild(item);
            });
        };

        render(words);
        updateToggleText(elements.wordsDropdown, state.selectedWordIds.length, 'words');
        updateHiddenInputs('word-ids-hidden', 'WordIds', state.selectedWordIds);
    }

    function renderLevelsDropdown(levels) {
        const listContainer = elements.levelsDropdown.querySelector('.dropdown-list-items');
        listContainer.innerHTML = '';
        levels.forEach(level => {
            const isSelected = state.selectedLevels.includes(level);
            const item = createDropdownItem(level, level, isSelected, (val, selected) => {
                if (selected) {
                    if (!state.selectedLevels.includes(val)) state.selectedLevels.push(val);
                } else {
                    state.selectedLevels = state.selectedLevels.filter(x => x !== val);
                }
                updateToggleText(elements.levelsDropdown, state.selectedLevels.length, 'levels');
                updateHiddenInputs('level-ids-hidden', 'DifficultyLevels', state.selectedLevels);
            });
            listContainer.appendChild(item);
        });
        updateToggleText(elements.levelsDropdown, state.selectedLevels.length, 'levels');
        updateHiddenInputs('level-ids-hidden', 'DifficultyLevels', state.selectedLevels);
    }

    function createDropdownItem(value, label, isSelected, onToggle) {
        const div = document.createElement('div');
        div.className = `dropdown-item-custom ${isSelected ? 'selected' : ''}`;
        div.innerHTML = `<span>${label}</span><i class="bi bi-check2 opacity-0"></i>`;
        div.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            const nowSelected = !div.classList.contains('selected');
            div.classList.toggle('selected', nowSelected);
            onToggle(value, nowSelected);
        });
        return div;
    }

    function updateToggleText(container, count, type) {
        if (!container) return;
        const display = container.querySelector('.selected-items-display');
        const emptyLabels = {
            categories: 'Select categories...',
            words: 'Select words...',
            levels: 'Select levels...'
        };

        display.textContent = count === 0 ? (emptyLabels[type] || 'Select items...') : `${count} ${type} selected`;
        display.classList.toggle('text-muted', count === 0);
    }

    function updateHiddenInputs(containerId, name, values) {
        const container = document.getElementById(containerId);
        if (!container) return;
        container.innerHTML = '';
        values.forEach(val => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = name;
            input.value = val;
            container.appendChild(input);
        });
    }

    function updateDescriptionCount() {
        if (elements.descriptionInput && elements.descriptionCount) {
            elements.descriptionCount.textContent = elements.descriptionInput.value.length;
        }
    }

    function handleImageUpload(e) {
        const file = e.target.files?.[0];
        if (file) {
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
                    elements.previewImage.src = dataUrl;
                    elements.previewContainer.style.display = 'block';
                    elements.uploadPlaceholder.style.display = 'none';
                    elements.hiddenImageInput.value = dataUrl;
                };
                img.src = event.target.result;
            };
            reader.readAsDataURL(file);
        }
    }

    return { init };
})();
