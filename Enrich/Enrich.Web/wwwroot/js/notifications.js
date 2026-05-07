document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.notification-item.unread').forEach(item => {
        item.addEventListener('click', async function () {
            const id = this.dataset.id;
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value
                       ?? document.cookie.match(/XSRF-TOKEN=([^;]+)/)?.[1] ?? '';
            try {
                await fetch(`/Notification/MarkRead/${id}`, {
                    method: 'POST',
                    headers: { 'X-CSRF-TOKEN': token }
                });
                this.classList.remove('unread');
                const badge = this.querySelector('.badge');
                if (badge) badge.remove();
                const text = this.querySelector('p');
                if (text) { 
                    text.classList.remove('fw-semibold'); 
                    text.classList.add('text-muted'); 
                }
            } catch { /* silent */ }
        });
    });
});
