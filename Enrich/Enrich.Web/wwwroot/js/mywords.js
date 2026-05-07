let currentFilters = { searchTerm: '', category: '', level: '', pos: '' };
let currentPage = 1;
const pageSize = parseInt(document.getElementById('words-container').dataset.pageSize, 10) || 20;

async function loadWords(page = 1) {
    currentPage = page;
    const { searchTerm, category, level, pos } = currentFilters;
    const url = `/Word/GetMyWords?searchTerm=${encodeURIComponent(searchTerm)}&category=${encodeURIComponent(category)}&difficultyLevel=${encodeURIComponent(level)}&partOfSpeech=${encodeURIComponent(pos)}&page=${currentPage}&pageSize=${pageSize}`;

    try {
        const response = await fetch(url);
        const result = await response.json();
        const words = result.items || result.Items || [];
        const totalCount = result.totalCount || result.TotalCount || 0;
        const container = document.getElementById('words-container');

        if (words.length === 0) {
            container.innerHTML = `
                <div class="col-12">
                    <div class="empty-state">
                        <i class="bi bi-journal-x"></i>
                        <p class="fw-semibold mb-1">No words found</p>
                        <p class="small">Try adjusting your filters or add a new word.</p>
                    </div>
                </div>`;
            document.getElementById('pagination-container').innerHTML = '';
            return;
        }

        container.innerHTML = words.map(word => {
            const progress = word.progress ?? 0;
            const isLearned = progress >= 100;
            
            let progressColor = '#dc3545'; 
            if (progress >= 30 && progress < 70) progressColor = '#ffc107'; 
            if (progress >= 70) progressColor = 'var(--active-green)'; 
            
            const progressLabel = isLearned
                ? `<span class="progress-label learned"><i class="bi bi-star-fill text-warning me-1"></i>Learned</span>`
                : `<span class="progress-label" style="color: ${progressColor}">${progress}/100</span>`;
                
            const glowEffect = isLearned ? 'box-shadow: 0 0 8px var(--active-green);' : '';

            return `
            <div class="col-md-6" id="word-${word.id}">
                <div class="word-card h-100 ${isLearned ? 'border-success' : ''}">
                    <div class="word-header">
                        <div>
                            <h5 class="term-title">
                                ${escapeHtml(word.term)}
                                <span class="badge-level">${escapeHtml(word.difficultyLevel || 'B2')}</span>
                            </h5>
                            <span class="pos-label">${escapeHtml(word.partOfSpeech || 'noun')}</span>
                        </div>
                        <div class="translation-title">${escapeHtml(word.translation)}</div>
                    </div>

                    <div class="transcription">[${escapeHtml(word.transcription || '...')}]</div>

                    <div class="mt-2">
                        <div class="section-label">Category</div>
                        <span style="
                            display: inline-flex;
                            align-items: center;
                            gap: 5px;
                            background: #edf7f1;
                            color: #198754;
                            border: 1px solid #c3e6cb;
                            border-radius: 20px;
                            font-size: 0.75rem;
                            font-weight: 600;
                            padding: 3px 10px;
                        ">

                            ${escapeHtml(word.categoryName || word.category || 'General')}
                        </span>
                    </div>

                    <div class="mt-2">
                        <div class="section-label">Meaning</div>
                        <div class="section-text">${escapeHtml(word.meaning || '...')}</div>
                    </div>

                    <div class="mt-2">
                        <div class="section-label">Example</div>
                        <div class="section-text fst-italic">${escapeHtml(word.example || '...')}</div>
                    </div>

                    <div class="progress-container mt-3">
                        <div class="progress-custom" style="height: 8px; background-color: #e9ecef;">
                            <div class="progress-fill" style="width: ${progress}%; background-color: ${progressColor}; transition: width 0.5s ease-in-out; ${glowEffect}"></div>
                        </div>
                        ${progressLabel}
                    </div>

                    <div class="card-footer-row">
                        <div class="dropdown">
                            <button class="btn-dots" data-bs-toggle="dropdown" aria-expanded="false">
                                <i class="bi bi-three-dots"></i>
                            </button>
                            <ul class="dropdown-menu dropdown-menu-end shadow border-0 rounded-3">
                                <li><a class="dropdown-item" href="/Word/Edit/${word.id}">Edit</a></li>
                                <li><hr class="dropdown-divider"></li>
                                <li><a class="dropdown-item text-danger" href="#" onclick="deleteWord(${word.id}); return false;">Delete</a></li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div>`;
        }).join('');

        renderPagination(totalCount);

    } catch (e) {
        console.error(e);
    }
}

function renderPagination(totalCount) {
    const totalPages = Math.ceil(totalCount / pageSize);
    const container = document.getElementById('pagination-container');
    
    if (totalPages <= 1) {
        container.innerHTML = '';
        return;
    }

    let html = '<ul class="pagination justify-content-center">';
    for (let i = 1; i <= totalPages; i++) {
        html += `
            <li class="page-item ${i === currentPage ? 'active' : ''}">
                <button class="page-link" onclick="changePage(${i})">${i}</button>
            </li>`;
    }
    html += '</ul>';
    container.innerHTML = html;
}

function changePage(page) {
    loadWords(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

async function deleteWord(id) {
    if (!confirm('Are you sure you want to delete this word?')) return;
    try {
        const response = await fetch(`/Word/Delete/${id}`, { method: 'POST' });
        if (response.ok) {
            loadWords();
        } else {
            alert("Error while deleting the word.");
        }
    } catch (err) {
        console.error("Delete request failed", err);
    }
}

function escapeHtml(text) {
    const map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' };
    return text ? String(text).replace(/[&<>"']/g, m => map[m]) : '';
}

document.getElementById('searchTerm').addEventListener('input', (e) => {
    currentFilters.searchTerm = e.target.value;
    loadWords();
});

async function loadCategories() {
    try {
        const resp = await fetch('/Word/GetCategories');
        const cats = await resp.json();
        const ul = document.getElementById('category-filters');
        ul.innerHTML = '';
        cats.forEach(c => {
            const li = document.createElement('li');
            li.className = 'checkbox-item';
            li.innerHTML = `<input type="checkbox" class="form-check-input me-2 category-checkbox" id="cat-${c.id}" value="${c.name}" onchange="onCheckboxChange('category', this)">
                           <label class="form-check-label" for="cat-${c.id}">${c.name}</label>`;
            ul.appendChild(li);
        });

        const catSearch = document.getElementById('categorySearch');
        catSearch.addEventListener('input', (e) => {
            const q = e.target.value.trim().toLowerCase();
            ul.querySelectorAll('li').forEach(li => {
                const text = li.innerText.trim().toLowerCase();
                li.style.display = q === '' || text.includes(q) ? '' : 'none';
            });
        });
    } catch (err) {
        console.error('Failed to load categories', err);
    }
}

function onCheckboxChange(type, checkbox) {
    const selector = type === 'category' ? '.category-checkbox' : (type === 'level' ? '.level-checkbox' : '.pos-checkbox');
    const checked = Array.from(document.querySelectorAll(selector))
        .filter(c => c.checked)
        .map(c => c.value);

    if (type === 'category') currentFilters.category = checked.join(',');
    if (type === 'level') currentFilters.level = checked.join(',');
    if (type === 'pos') currentFilters.pos = checked.join(',');

    loadWords();
}

document.addEventListener('DOMContentLoaded', () => {
    loadCategories();
    loadWords();
});