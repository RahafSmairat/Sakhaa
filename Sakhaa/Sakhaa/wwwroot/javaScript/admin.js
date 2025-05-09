// Admin Dashboard JavaScript

// Initialize any date pickers
document.addEventListener('DOMContentLoaded', function() {
    // Add event listeners for any delete confirmations
    const deleteButtons = document.querySelectorAll('.delete-confirm');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function(event) {
            if (!confirm('هل أنت متأكد من حذف هذا العنصر؟')) {
                event.preventDefault();
            }
        });
    });

    // Initialize any tooltips
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    if (tooltipTriggerList.length > 0) {
        const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
    }
    
    // Mobile navigation for sidebar
    const sidebarToggle = document.querySelector('#sidebarToggle');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function() {
            document.querySelector('.sidebar').classList.toggle('show');
        });
    }
    
    // Handle table row clicks to navigate to detail pages
    const tableRows = document.querySelectorAll('.clickable-row');
    tableRows.forEach(row => {
        row.addEventListener('click', function() {
            window.location = this.dataset.href;
        });
    });
    
    // Form validation
    const forms = document.querySelectorAll('.needs-validation');
    Array.from(forms).forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });
}); 