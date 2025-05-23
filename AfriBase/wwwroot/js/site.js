// AfriBase site JavaScript

// Initialize tooltips
document.addEventListener('DOMContentLoaded', function () {
    // Initialize Bootstrap tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize Bootstrap popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Auto-dismiss alerts after 5 seconds
    setTimeout(function () {
        var alerts = document.querySelectorAll('.alert.alert-dismissible');
        alerts.forEach(function (alert) {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);

    // Artifact image preview
    var imageInput = document.getElementById('ImageFile');
    var imagePreview = document.getElementById('imagePreview');

    if (imageInput && imagePreview) {
        imageInput.addEventListener('change', function (event) {
            if (event.target.files.length > 0) {
                var src = URL.createObjectURL(event.target.files[0]);
                imagePreview.src = src;
                imagePreview.style.display = 'block';
            }
        });
    }

    // Bid form validation
    var bidForm = document.getElementById('bidForm');
    var bidAmount = document.getElementById('BidAmount');
    var currentBid = document.getElementById('currentHighestBid');
    var startingBid = document.getElementById('startingBid');
    var bidError = document.getElementById('bidError');

    if (bidForm && bidAmount && (currentBid || startingBid)) {
        bidForm.addEventListener('submit', function (event) {
            var isValid = true;
            var errorMessage = '';

            // Clear previous error
            if (bidError) {
                bidError.textContent = '';
                bidError.style.display = 'none';
            }

            // Validate bid amount
            if (currentBid && parseFloat(bidAmount.value) <= parseFloat(currentBid.value)) {
                isValid = false;
                errorMessage = 'Your bid must be higher than the current highest bid.';
            } else if (startingBid && parseFloat(bidAmount.value) < parseFloat(startingBid.value)) {
                isValid = false;
                errorMessage = 'Your bid must be at least the starting bid amount.';
            }

            if (!isValid && bidError) {
                event.preventDefault();
                bidError.textContent = errorMessage;
                bidError.style.display = 'block';

                // Shake the bid form to indicate error
                bidForm.classList.add('shake');
                setTimeout(function () {
                    bidForm.classList.remove('shake');
                }, 500);
            }
        });
    }

    // Countdown timer for bid end date
    var countdownElements = document.querySelectorAll('.bid-countdown');

    countdownElements.forEach(function (element) {
        var endTime = new Date(element.getAttribute('data-end-time')).getTime();

        // Update the countdown every second
        var countdownInterval = setInterval(function () {
            var now = new Date().getTime();
            var timeLeft = endTime - now;

            if (timeLeft < 0) {
                // Auction ended
                clearInterval(countdownInterval);
                element.innerHTML = 'Auction ended';
                return;
            }

            // Calculate days, hours, minutes, seconds
            var days = Math.floor(timeLeft / (1000 * 60 * 60 * 24));
            var hours = Math.floor((timeLeft % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            var minutes = Math.floor((timeLeft % (1000 * 60 * 60)) / (1000 * 60));
            var seconds = Math.floor((timeLeft % (1000 * 60)) / 1000);

            // Display countdown
            element.innerHTML = days + 'd ' + hours + 'h ' + minutes + 'm ' + seconds + 's';

            // Add urgency class when less than 1 hour remains
            if (timeLeft < 3600000) {
                element.classList.add('text-danger', 'fw-bold');
            }
        }, 1000);
    });

    // Animation for featured artifacts
    var featuredArtifacts = document.querySelectorAll('.featured-artifact');

    featuredArtifacts.forEach(function (artifact, index) {
        setTimeout(function () {
            artifact.classList.add('animate__animated', 'animate__fadeInUp');
            artifact.style.opacity = 1;
        }, index * 200);
    });

    // Region cards hover effect
    var regionCards = document.querySelectorAll('.region-card');

    regionCards.forEach(function (card) {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'scale(1.05)';
            this.style.boxShadow = '0 15px 25px rgba(0, 0, 0, 0.2)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'scale(1)';
            this.style.boxShadow = '0 5px 15px rgba(0, 0, 0, 0.1)';
        });
    });

    // Smooth scroll to sections
    var scrollLinks = document.querySelectorAll('.scroll-link');

    scrollLinks.forEach(function (link) {
        link.addEventListener('click', function (event) {
            event.preventDefault();
            var targetId = this.getAttribute('href');
            var targetElement = document.querySelector(targetId);

            if (targetElement) {
                window.scrollTo({
                    top: targetElement.offsetTop - 80,
                    behavior: 'smooth'
                });
            }
        });
    });
});

// Function to confirm delete
function confirmDelete(formId) {
    if (confirm('Are you sure you want to delete this item? This action cannot be undone.')) {
        document.getElementById(formId).submit();
    }
    return false;
}

// Function to format currency
function formatCurrency(amount, currency = 'USD') {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: currency
    }).format(amount);
}

// Function to show confirmation modals
function showConfirmModal(title, message, confirmText, cancelText, confirmCallback) {
    var confirmModal = new bootstrap.Modal(document.getElementById('confirmModal'));

    document.getElementById('confirmModalTitle').textContent = title;
    document.getElementById('confirmModalBody').textContent = message;
    document.getElementById('confirmModalConfirm').textContent = confirmText;
    document.getElementById('confirmModalCancel').textContent = cancelText;

    document.getElementById('confirmModalConfirm').onclick = function () {
        confirmModal.hide();
        confirmCallback();
    };

    confirmModal.show();
}