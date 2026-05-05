let searchTimeout;

function getFilters() {
    return {
        searchTerm: document.getElementById('searchTerm').value,
        categories: Array.from(document.querySelectorAll('.category-checkbox:checked')).map(c => c.value).join(','),
        levels: Array.from(document.querySelectorAll('.level-checkbox:checked')).map(c => c.value).join(','),
        minWords: document.getElementById('minWordCount').value,
        maxWords: document.getElementById('maxWordCount').value
    };
}

async function loadBundles(page = 1) {
    const f = getFilters();
    const params = new URLSearchParams({
        page: page,
        pageSize: parseInt(document.getElementById('bundles-list-container').dataset.pageSize, 10) || 6,
        search: f.searchTerm,
        categoryFilter: f.categories,
        levelFilter: f.levels,
        minWordCount: f.minWords,
        maxWordCount: f.maxWords
    });

    try {
        const response = await fetch(`/Bundle/Index?${params.toString()}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        if (response.ok) {
            const html = await response.text();
            document.getElementById('bundles-list-container').innerHTML = html;
        }
    } catch (err) { console.error(err); }
}

function onFilterChange() { loadBundles(1); }
function debounceSearch() {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => onFilterChange(), 500);
}
function changePage(page) {
    loadBundles(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// Category search functionality
document.getElementById('categorySearch').addEventListener('input', function() {
    const searchTerm = this.value.toLowerCase().trim();
    const categoryItems = document.querySelectorAll('#category-filters .checkbox-item');

    categoryItems.forEach(item => {
        const label = item.querySelector('label');
        const categoryName = label.textContent.toLowerCase();

        if (searchTerm === '' || categoryName.includes(searchTerm)) {
            item.style.display = '';
        } else {
            item.style.display = 'none';
        }
    });
});

function submitForReview(bundleId, bundleTitle = 'this collection') {
    if (confirm(`Are you sure? After sending for review you will not be able to edit the collection "${bundleTitle}" until the admin approves it.`)) {
        submitBundle(bundleId);
    }
}

async function submitBundle(bundleId) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    if (!token) {
         console.error("Verification token not found");
         return;
    }

    try {
        const response = await fetch(`/Bundle/SubmitForReview/${bundleId}`, {
            method: 'POST',
            headers: {
                'X-CSRF-TOKEN': token,
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        if (response.ok) {
            const result = await response.json();
            showToast(result.message);
            loadBundles(); // Refresh the list
        } else {
            let errorMessage = 'Error submitting collection.';
            try {
                const error = await response.json();
                errorMessage = error.message || errorMessage;
            } catch(e) {}
            showToast(errorMessage, 'danger');
        }
    } catch (err) {
        console.error(err);
        showToast('An unexpected error occurred.', 'danger');
    }
}
