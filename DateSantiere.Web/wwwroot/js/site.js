// DateSantiere site JavaScript

// Account dropdown toggle
document.addEventListener('DOMContentLoaded', function() {
    const accountToggle = document.getElementById('accountToggle');
    const accountDropdownMenu = document.getElementById('accountDropdownMenu');
    
    if (accountToggle && accountDropdownMenu) {
        accountToggle.addEventListener('click', function(e) {
            e.preventDefault();
            accountToggle.classList.toggle('active');
            accountDropdownMenu.classList.toggle('active');
        });
        
        // Close dropdown when clicking outside
        document.addEventListener('click', function(e) {
            if (!accountToggle.contains(e.target) && !accountDropdownMenu.contains(e.target)) {
                accountToggle.classList.remove('active');
                accountDropdownMenu.classList.remove('active');
            }
        });
    }
});

// Smooth scrolling for anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth'
            });
        }
    });
});

// Auto-hide alerts
document.addEventListener('DOMContentLoaded', function() {
    const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.transition = 'opacity 0.5s';
            alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 500);
        }, 5000);
    });
});
