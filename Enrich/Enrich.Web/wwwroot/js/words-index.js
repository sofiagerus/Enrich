let currentPage = 1;

function getFilters() {
    const searchTerm = document.getElementById('searchTerm').value;
    const category = Array.from(document.querySelectorAll('.category-checkbox:checked')).map(c => c.value).join(',');
    const level = Array.from(document.querySelectorAll('.level-checkbox:checked')).map(c => c.value).join(',');
    const pos = Array.from(document.querySelectorAll('.pos-checkbox:checked')).map(c => c.value).join(',');

    return { searchTerm, categoryFilter: category, levelFilter: level, posFilter: pos };
}

async function loadWords(page = 1) {
    currentPage = page;
    const filters = getFilters();
    const params = new URLSearchParams({
        ...filters,
        page: page,
        pageSize: parseInt(document.getElementById('words-list-container').dataset.pageSize, 10) || 20
    });

    try {
        const response = await fetch(`/Word/Index?${params.toString()}`, {
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        if (response.ok) {
            const html = await response.text();
            document.getElementById('words-list-container').innerHTML = html;
        }
    } catch (err) {
        console.error("Failed to load words", err);
    }
}

function onFilterChange() {
    loadWords(1);
}

function changePage(page) {
    loadWords(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

document.getElementById('searchTerm').addEventListener('input', debounce(() => {
    onFilterChange();
}, 500));

function debounce(func, timeout = 300) {
    let timer;
    return (...args) => {
        clearTimeout(timer);
        timer = setTimeout(() => { func.apply(this, args); }, timeout);
    };
}

// Category search
document.getElementById('categorySearch').addEventListener('input', (e) => {
    const q = e.target.value.trim().toLowerCase();
    document.querySelectorAll('#category-filters li').forEach(li => {
        const text = li.innerText.trim().toLowerCase();
        li.style.display = q === '' || text.includes(q) ? '' : 'none';
    });
});

async function toggleSaveWord(wordId, btnElement) {
    try {
        const response = await fetch(`/Word/SaveSystemWord?id=${wordId}`, {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            if (btnElement) {
                const icon = btnElement.querySelector('i');
                icon.classList.remove('bi-star');
                icon.classList.add('bi-star-fill');
                
                btnElement.classList.remove('text-secondary');
                btnElement.classList.add('text-warning'); 
                
                btnElement.title = "Saved";
                btnElement.disabled = true;
                btnElement.style.opacity = '1'; 
            }
        } else {
            const error = await response.json();
            alert(error.message || 'Error saving the word.');
        }
    } catch (err) {
        console.error("Failed to save word", err);
        alert('A network error occurred.');
    }
}