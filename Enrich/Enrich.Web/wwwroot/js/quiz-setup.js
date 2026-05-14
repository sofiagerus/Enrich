function initQuizSetup() {
    const dropdownContainers = document.querySelectorAll('.custom-dropdown');
    if (dropdownContainers.length === 0) return;

    dropdownContainers.forEach(container => {
        const btn = container.querySelector('.btn-dropdown');
        const input = container.querySelector('.dropdown-hidden-input');
        const items = container.querySelectorAll('.dropdown-item');
        const textSpan = btn.querySelector('.selected-text');
        if (!btn || !input || !textSpan) return;

   
        const defaultValue = input.value;
        items.forEach(item => {
            if (item.getAttribute('data-value') === defaultValue) {
                item.classList.add('active');
            }
        });


        items.forEach(item => {
            item.addEventListener('click', (e) => {
                e.preventDefault();
                items.forEach(i => i.classList.remove('active'));
                item.classList.add('active');
                input.value = item.getAttribute('data-value');
                textSpan.innerText = item.innerText;
            });
        });
    });
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initQuizSetup);
} else {
    initQuizSetup();
}