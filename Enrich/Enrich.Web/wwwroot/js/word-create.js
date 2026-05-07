(() => {
    const input = document.getElementById('category-input');
    const dropdown = document.getElementById('category-dropdown');
    const toggle = document.getElementById('category-toggle');
    const chevron = document.getElementById('category-chevron');
    let allCategories = [];

    async function fetchCategories() {
        try {
            const res = await fetch('/Word/GetCategories');
            if (res.ok) allCategories = await res.json();
        } catch (e) {
            console.error('Error loading categories:', e);
        }
    }

    function escapeHtml(str) {
        return str
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
    }

    function renderDropdown() {
        const val = input.value.trim().toLowerCase();

        const matches = allCategories.filter(c =>
            c.name.toLowerCase().includes(val)
        );

        dropdown.innerHTML = '';

        const exactMatch = allCategories.some(c =>
            c.name.toLowerCase() === val
        );
        if (val && !exactMatch) {
            const li = document.createElement('li');
            li.className = 'px-3 py-2 d-flex align-items-center gap-2 category-item new-item';
            li.setAttribute('tabindex', '0');
            li.innerHTML = `<i class="bi bi-plus-circle"></i>
                            <span>Create <strong>"${escapeHtml(input.value.trim())}"</strong></span>`;
            li.addEventListener('mousedown', e => {
                e.preventDefault();
                input.value = input.value.trim();
                closeDropdown();
            });
            li.addEventListener('keydown', handleItemKeydown);
            dropdown.appendChild(li);
        }

        matches.forEach(c => {
            const li = document.createElement('li');
            li.className = 'px-3 py-2 category-item';
            li.setAttribute('tabindex', '0');
            li.textContent = c.name;
            li.addEventListener('mousedown', e => {
                e.preventDefault();
                input.value = c.name;
                closeDropdown();
            });
            li.addEventListener('keydown', handleItemKeydown);
            dropdown.appendChild(li);
        });

        const hasItems = dropdown.children.length > 0;
        dropdown.style.display = hasItems ? 'block' : 'none';
        chevron.style.transform = hasItems ? 'rotate(180deg)' : '';
    }

    function handleItemKeydown(e) {
        const items = [...dropdown.querySelectorAll('.category-item')];
        const idx = items.indexOf(document.activeElement);
        if (e.key === 'ArrowDown' && idx < items.length - 1) {
            items[idx + 1].focus(); e.preventDefault();
        } else if (e.key === 'ArrowUp') {
            idx > 0 ? items[idx - 1].focus() : input.focus();
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
        chevron.style.transform = '';
    }

    // --- events ---
    input.addEventListener('focus', openDropdown);
    input.addEventListener('input', renderDropdown);

    input.addEventListener('keydown', e => {
        if (e.key === 'Escape') { closeDropdown(); return; }
        if (e.key === 'ArrowDown') {
            const first = dropdown.querySelector('.category-item');
            if (first) { first.focus(); e.preventDefault(); }
        }
    });

    toggle.addEventListener('mousedown', e => {
        e.preventDefault();
        dropdown.style.display === 'none' ? openDropdown() : closeDropdown();
    });

    document.addEventListener('mousedown', e => {
        if (!document.getElementById('category-wrapper').contains(e.target)) {
            closeDropdown();
        }
    });

    document.addEventListener('DOMContentLoaded', fetchCategories);

    // Fetch auto-fill
    const lookupBtn = document.getElementById('btn-lookup-word');
    const termInput = document.getElementById('term-input');
    const transInput = document.getElementById('transcription-input');
    const meanInput = document.getElementById('meaning-input');

    lookupBtn.addEventListener('click', async () => {
        const word = termInput.value.trim();
        if (!word) {
            alert('Please enter a word first.');
            return;
        }

        const originalText = lookupBtn.innerHTML;
        lookupBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
        lookupBtn.disabled = true;

        try {
            const res = await fetch(`/Word/LookupDefinition?term=${encodeURIComponent(word)}`);
            if (!res.ok) {
                alert('Definition not found or error occurred.');
                return;
            }
            const data = await res.json();

            if (data.transcription && !transInput.value) {
                transInput.value = data.transcription;
            }
            if (data.meaning && !meanInput.value) {
                meanInput.value = data.meaning;
            }
        } catch (e) {
            console.error('Lookup failed', e);
            alert('Failed to lookup word.');
        } finally {
            lookupBtn.innerHTML = originalText;
            lookupBtn.disabled = false;
        }
    });

})();