// Show step progress bar on ticket pages
document.addEventListener('DOMContentLoaded', () => {
    const stepProgress = document.getElementById('stepProgress');
    if (stepProgress && stepProgress.style.display === 'none') {
        // Check if we're on a ticket flow page
        const path = window.location.pathname;
        if (path.startsWith('/Ticket') || path.startsWith('/ticket')) {
            stepProgress.style.display = 'flex';
        }
    }
});
