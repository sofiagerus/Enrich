document.addEventListener('DOMContentLoaded', () => {
    const deleteForms = document.querySelectorAll('.js-delete-category');

    deleteForms.forEach((form) => {
        form.addEventListener('submit', (event) => {
            const shouldDelete = window.confirm('Delete this category?');
            if (!shouldDelete) {
                event.preventDefault();
            }
        });
    });
});
