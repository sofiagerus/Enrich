function initWordCreate() {
    var input = document.getElementById('category-input');
    var dropdown = document.getElementById('category-dropdown');
    var toggle = document.getElementById('category-toggle');
    var chevron = document.getElementById('category-chevron');
    var wrapper = document.getElementById('category-wrapper');

    if (!input || !dropdown || !toggle) return;

    var allCategories = [];

    fetch('/Word/GetCategories')
        .then(function (res) {
            if (!res.ok) throw new Error('Failed');
            return res.json();
        })
        .then(function (data) {
            allCategories = data.map(function (c) {
                return typeof c === 'string' ? c : c.name;
            });
            if (document.activeElement === input) renderDropdown();
        })
        .catch(function (e) {
            console.error('Error loading categories:', e);
        });

    function escapeHtml(str) {
        return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    }

    function renderDropdown() {
        var val = input.value.trim().toLowerCase();
        var matches = allCategories.filter(function (c) {
            return c.toLowerCase().includes(val);
        });
        dropdown.innerHTML = '';
        var exactMatch = allCategories.some(function (c) {
            return c.toLowerCase() === val;
        });

        if (val && !exactMatch) {
            var li = document.createElement('li');
            li.className = 'px-3 py-2 d-flex align-items-center gap-2 category-item new-item dropdown-item';
            li.setAttribute('tabindex', '0');
            li.style.cursor = 'pointer';
            li.innerHTML = '<i class="bi bi-plus-circle"></i><span>Create <strong>"' + escapeHtml(input.value.trim()) + '"</strong></span>';
            li.addEventListener('mousedown', function (e) {
                e.preventDefault();
                input.value = input.value.trim();
                closeDropdown();
            });
            li.addEventListener('keydown', handleItemKeydown);
            dropdown.appendChild(li);
        }

        matches.forEach(function (c) {
            var li = document.createElement('li');
            li.className = 'px-3 py-2 category-item dropdown-item';
            li.setAttribute('tabindex', '0');
            li.style.cursor = 'pointer';
            li.textContent = c;
            li.addEventListener('mousedown', function (e) {
                e.preventDefault();
                input.value = c;
                closeDropdown();
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
            if (idx > 0) { items[idx - 1].focus(); } else { input.focus(); }
            e.preventDefault();
        } else if (e.key === 'Enter') {
            e.preventDefault();
            document.activeElement.dispatchEvent(new MouseEvent('mousedown'));
        } else if (e.key === 'Escape') {
            input.focus(); closeDropdown();
        }
    }

    function openDropdown() { renderDropdown(); }
    function closeDropdown() {
        dropdown.style.display = 'none';
        if (chevron) chevron.style.transform = '';
    }

    input.addEventListener('focus', openDropdown);
    input.addEventListener('input', renderDropdown);
    input.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') { closeDropdown(); return; }
        if (e.key === 'ArrowDown') {
            var first = dropdown.querySelector('.category-item');
            if (first) { first.focus(); e.preventDefault(); }
        }
    });

    toggle.addEventListener('mousedown', function (e) {
        e.preventDefault();
        if (dropdown.style.display === 'none') { openDropdown(); } else { closeDropdown(); }
        input.focus();
    });

    document.addEventListener('mousedown', function (e) {
        if (wrapper && !wrapper.contains(e.target)) closeDropdown();
    });

    // Auto-fill
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