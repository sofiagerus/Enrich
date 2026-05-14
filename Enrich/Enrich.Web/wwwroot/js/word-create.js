function initWordCreate() {
    var display = document.getElementById('category-display');
    var displayTxt = document.getElementById('category-display-text');
    var hiddenInput = document.getElementById('category-input');
    var dropdown = document.getElementById('category-dropdown');
    var toggle = document.getElementById('category-toggle');
    var chevron = document.getElementById('category-chevron');
    var wrapper = document.getElementById('category-wrapper');

    if (!display || !dropdown || !hiddenInput) return;

    var allCategories = [];
    var selectedCategory = null;

    fetch('/Word/GetCategories')
        .then(function (res) {
            if (!res.ok) throw new Error('Failed');
            return res.json();
        })
        .then(function (data) {
            allCategories = data.map(function (c) {
                return typeof c === 'string' ? c : c.name;
            });
        })
        .catch(function (e) {
            console.error('Error loading categories:', e);
        });

    function renderDropdown() {
        dropdown.innerHTML = '';

        allCategories.forEach(function (c) {
            var li = document.createElement('li');
            var isSelected = (c === selectedCategory);
            li.className = 'px-3 py-2 category-item dropdown-item' + (isSelected ? ' selected-item' : '');
            li.setAttribute('tabindex', '0');
            li.style.cursor = 'pointer';
            li.textContent = c;


            li.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                var allItems = Array.prototype.slice.call(dropdown.querySelectorAll('.category-item'));
                allItems.forEach(function (i) { i.classList.remove('selected-item'); });

                li.classList.add('selected-item');

                selectedCategory = c;
                hiddenInput.value = c;
                displayTxt.textContent = c;
                displayTxt.classList.remove('text-muted');

                setTimeout(function () {
                    closeDropdown();
                    display.focus();
                }, 120);
            });

            li.addEventListener('keydown', handleItemKeydown);
            dropdown.appendChild(li);
        });

        var hasItems = dropdown.children.length > 0;
        dropdown.style.display = hasItems ? 'block' : 'none';
        if (chevron) chevron.style.transform = hasItems ? 'rotate(180deg)' : '';
    }

    function handleItemKeydown(e) {
        var items = Array.prototype.slice.call(dropdown.querySelectorAll('.category-item'));
        var idx = items.indexOf(document.activeElement);
        if (e.key === 'ArrowDown' && idx < items.length - 1) {
            items[idx + 1].focus(); e.preventDefault();
        } else if (e.key === 'ArrowUp') {
            if (idx > 0) { items[idx - 1].focus(); } else { display.focus(); }
            e.preventDefault();
        } else if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            document.activeElement.click();
        } else if (e.key === 'Escape') {
            display.focus(); closeDropdown();
        }
    }

    function openDropdown() { renderDropdown(); }
    function closeDropdown() {
        dropdown.style.display = 'none';
        if (chevron) chevron.style.transform = '';
    }

    display.addEventListener('mousedown', function (e) {
        e.preventDefault();
        if (dropdown.style.display === 'none') { openDropdown(); } else { closeDropdown(); }
        display.focus();
    });

    if (toggle) {
        toggle.addEventListener('mousedown', function (e) {
            e.preventDefault();
            if (dropdown.style.display === 'none') { openDropdown(); } else { closeDropdown(); }
            display.focus();
        });
    }

    display.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') { closeDropdown(); return; }
        if (e.key === 'Enter' || e.key === ' ' || e.key === 'ArrowDown') {
            if (dropdown.style.display === 'none') { openDropdown(); }
            var first = dropdown.querySelector('.category-item');
            if (first) { first.focus(); }
            e.preventDefault();
        }
    });

    document.addEventListener('mousedown', function (e) {
        if (wrapper && !wrapper.contains(e.target)) closeDropdown();
    });

    var lookupBtn = document.getElementById('btn-lookup-word');
    var termInput = document.getElementById('term-input');
    var transInput = document.getElementById('transcription-input');
    var meanInput = document.getElementById('meaning-input');

    if (lookupBtn && termInput) {
        lookupBtn.addEventListener('click', function () {
            var word = termInput.value.trim();
            if (!word) { alert('Please enter a word first.'); return; }

            var originalHTML = lookupBtn.innerHTML;
            lookupBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
            lookupBtn.disabled = true;

            fetch('/Word/LookupDefinition?term=' + encodeURIComponent(word))
                .then(function (res) {
                    if (!res.ok) throw new Error('Not found');
                    return res.json();
                })
                .then(function (data) {
                    if (transInput && data.transcription && !transInput.value) transInput.value = data.transcription;
                    if (meanInput && data.meaning && !meanInput.value) meanInput.value = data.meaning;
                })
                .catch(function (e) {
                    console.error('Lookup failed', e);
                    alert('Definition not found or error occurred.');
                })
                .finally(function () {
                    lookupBtn.innerHTML = originalHTML;
                    lookupBtn.disabled = false;
                });
        });
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initWordCreate);
} else {
    initWordCreate();
}