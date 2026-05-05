/**
 * Generator Page Logic
 */

const Generator = (() => {
    const rules = [];
    let selectors = {};

    function init() {
        selectors = {
            bundleTitle: document.getElementById('bundleTitle'),
            bundleDescription: document.getElementById('bundleDescription'),
            categorySelect: document.getElementById('categorySelect'),
            posSelect: document.getElementById('posSelect'),
            minDifficulty: document.getElementById('minDifficulty'),
            maxDifficulty: document.getElementById('maxDifficulty'),
            wordCount: document.getElementById('wordCount'),
            btnAddRule: document.getElementById('btnAddRule'),
            btnGenerate: document.getElementById('btnGenerate'),
            ruleList: document.getElementById('ruleList'),
            emptyMsg: document.getElementById('emptyRulesMessage'),
            badge: document.getElementById('ruleCountBadge'),
            resultContainer: document.getElementById('resultContainer'),
            wordsResultList: document.getElementById('wordsResultList'),
            resultTitle: document.getElementById('resultTitle'),
            resultDescription: document.getElementById('resultDescription'),
            previewTitle: document.getElementById('previewTitle'),
            previewDescription: document.getElementById('previewDescription'),
            previewWordsJson: document.getElementById('previewWordsJson')
        };

        if (selectors.btnAddRule) {
            selectors.btnAddRule.addEventListener('click', addRule);
        }

        if (selectors.btnGenerate) {
            selectors.btnGenerate.addEventListener('click', generateBundle);
        }

        initCustomDropdowns();
        renderRules();
    }

    function initCustomDropdowns() {
        const dropdownContainers = document.querySelectorAll('.custom-dropdown');
        
        dropdownContainers.forEach(container => {
            const btn = container.querySelector('.btn-dropdown');
            const input = container.querySelector('input[type="hidden"]');
            const items = container.querySelectorAll('.dropdown-item');
            const textSpan = btn.querySelector('.selected-text');

            items.forEach(item => {
                item.addEventListener('click', (e) => {
                    e.preventDefault();
                    
                    // Update active state
                    items.forEach(i => i.classList.remove('active'));
                    item.classList.add('active');

                    // Update value and text
                    const value = item.getAttribute('data-value');
                    const text = item.innerText;
                    
                    input.value = value;
                    textSpan.innerText = text;

                    // Trigger a custom event if needed
                    input.dispatchEvent(new Event('change'));
                });
            });
        });
    }

    function addRule() {
        // Find the active category name from the dropdown
        const categoryContainer = document.getElementById('categoryDropdownContainer');
        const activeCategory = categoryContainer.querySelector('.dropdown-item.active');
        const categoryName = activeCategory ? activeCategory.innerText : 'Any';

        const rule = {
            categoryId: selectors.categorySelect.value ? parseInt(selectors.categorySelect.value) : null,
            categoryName: categoryName,
            partOfSpeech: selectors.posSelect.value,
            minDifficulty: selectors.minDifficulty.value,
            maxDifficulty: selectors.maxDifficulty.value,
            wordCount: parseInt(selectors.wordCount.value) || 5
        };

        // Basic validation
        if (rule.wordCount < 1) {
            showToast('Word count must be at least 1', 'warning');
            return;
        }

        rules.push(rule);
        renderRules();
        showToast('Rule added to queue');
        
        // Reset word count to default for convenience
        selectors.wordCount.value = 5;
    }

    function removeRule(index) {
        rules.splice(index, 1);
        renderRules();
    }

    function renderRules() {
        if (!selectors.ruleList) return;

        // Clear current items
        const currentItems = selectors.ruleList.querySelectorAll('.rule-wrapper');
        currentItems.forEach(c => c.remove());

        selectors.badge.innerText = rules.length;
        selectors.badge.style.display = rules.length > 0 ? 'inline-block' : 'none';

        if (rules.length === 0) {
            selectors.emptyMsg.style.display = 'block';
            return;
        }

        selectors.emptyMsg.style.display = 'none';

        rules.forEach((rule, index) => {
            const wrapper = document.createElement('div');
            wrapper.className = 'col-md-6 col-lg-12 col-xl-6 rule-wrapper';
            wrapper.innerHTML = `
                <div class="rule-item-card">
                    <button type="button" class="btn-remove-rule" onclick="Generator.removeRule(${index})">
                        <i class="bi bi-x-lg" style="font-size: 0.8rem;"></i>
                    </button>
                    <div class="rule-detail">
                        <strong>Category</strong> 
                        <span>${rule.categoryId ? rule.categoryName : 'Any'}</span>
                    </div>
                    <div class="rule-detail">
                        <strong>P.O.S</strong> 
                        <span>${rule.partOfSpeech}</span>
                    </div>
                    <div class="rule-detail">
                        <strong>Difficulty</strong> 
                        <span>${rule.minDifficulty || 'Min'} &rarr; ${rule.maxDifficulty || 'Max'}</span>
                    </div>
                    <div class="rule-divider"></div>
                    <div class="rule-detail mb-0">
                        <strong>Words Count</strong> 
                        <span>${rule.wordCount}</span>
                    </div>
                </div>
            `;
            selectors.ruleList.appendChild(wrapper);
        });
    }

    async function generateBundle() {
        if (rules.length === 0) {
            showToast('Please add at least one rule.', 'warning');
            return;
        }

        const title = selectors.bundleTitle.value.trim() || 'Auto-generated collection';
        const description = selectors.bundleDescription.value.trim();

        const payload = {
            title: title,
            description: description,
            rules: rules.map(r => ({
                categoryId: r.categoryId,
                partOfSpeech: r.partOfSpeech,
                minDifficulty: r.minDifficulty,
                maxDifficulty: r.maxDifficulty,
                wordCount: r.wordCount
            }))
        };

        const btn = selectors.btnGenerate;
        const originalContent = btn.innerHTML;
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Generating...';

        try {
            const response = await fetch('/Bundle/Generate', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify(payload)
            });

            if (response.ok) {
                const data = await response.json();
                renderWords(data);
                showToast('Generation completed successfully!', 'success');
            } else {
                const error = await response.json();
                showToast(error.message || 'Generation failed.', 'danger');
            }
        } catch (err) {
            console.error(err);
            showToast('An unexpected error occurred.', 'danger');
        } finally {
            btn.disabled = false;
            btn.innerHTML = originalContent;
        }
    }

    function renderWords(data) {
        selectors.resultTitle.innerText = data.title;
        selectors.resultDescription.innerText = data.description || 'Temporarily generated word list';
        selectors.wordsResultList.innerHTML = '';

        // Set hidden form fields for the full preview
        selectors.previewTitle.value = data.title || '';
        selectors.previewDescription.value = data.description || '';
        selectors.previewWordsJson.value = JSON.stringify(data.words || []);

        data.words.forEach(word => {
            const col = document.createElement('div');
            col.className = 'col-md-6 col-lg-4';
            col.innerHTML = `
                <div class="word-card-preview">
                    <div class="word-header">
                        <div>
                            <h5 class="term-title">
                                ${word.term}
                                ${word.difficultyLevel ? `<span class="badge-level-word">${word.difficultyLevel}</span>` : ''}
                            </h5>
                            <span class="pos-label">${word.partOfSpeech || 'unknown'}</span>
                        </div>
                        <div class="translation-title">${word.translation}</div>
                    </div>

                    <div class="transcription mt-1">[${word.transcription || '...'}]</div>

                    <div class="mt-2">
                        <div class="section-label">Category</div>
                        <span class="badge-category-word">${word.categoryName}</span>
                    </div>

                    <div class="mt-2">
                        <div class="section-label">Meaning</div>
                        <div class="section-text">${word.meaning || '...'}</div>
                    </div>

                    <div class="mt-2">
                        <div class="section-label">Example</div>
                        <div class="section-text fst-italic">${word.example || '...'}</div>
                    </div>
                </div>
            `;
            selectors.wordsResultList.appendChild(col);
        });

        selectors.resultContainer.style.display = 'block';
        selectors.resultContainer.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    return {
        init,
        removeRule
    };
})();

// Initialize on DOMContentLoaded
document.addEventListener('DOMContentLoaded', Generator.init);
