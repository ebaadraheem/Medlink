document.addEventListener("DOMContentLoaded", function () {
    // Mobile Menu Toggle
    const menuBtn = document.querySelector('.mobile-menu-btn');
    const mobileMenu = document.querySelector('.mobile-menu');

    if (menuBtn && mobileMenu) {
        menuBtn.addEventListener('click', () => {
            mobileMenu.classList.toggle('open');
        });
    }

    // Appointment Booking AJAX - Time slot fetching (Used in Appointment/Book view)
    const dateInput = document.getElementById('AppointmentDate');
    const doctorIdInput = document.getElementById('DoctorId');
    const slotContainer = document.getElementById('slotContainer');
    const timeSlotIdInput = document.getElementById('TimeSlotId');

    if (dateInput && doctorIdInput && slotContainer) {
        dateInput.addEventListener('change', function () {
            const dateStr = this.value;
            const docId = doctorIdInput.value;
            
            slotContainer.innerHTML = '<div class="text-muted text-sm">Loading slots...</div>';
            timeSlotIdInput.value = '';

            fetch(`/Appointment/GetSlots?doctorId=${docId}&date=${dateStr}`)
                .then(res => res.json())
                .then(data => {
                    slotContainer.innerHTML = '';
                    if (data.length === 0) {
                        slotContainer.innerHTML = '<div class="alert alert-warning text-sm">No available slots for this date.</div>';
                        return;
                    }

                    data.forEach(slot => {
                        const btn = document.createElement('button');
                        btn.type = 'button';
                        btn.className = 'slot-btn';
                        btn.textContent = slot.display;
                        btn.dataset.id = slot.id;

                        btn.addEventListener('click', function () {
                            document.querySelectorAll('.slot-btn').forEach(b => b.classList.remove('selected'));
                            this.classList.add('selected');
                            timeSlotIdInput.value = this.dataset.id;
                        });

                        slotContainer.appendChild(btn);
                    });
                })
                .catch(err => {
                    slotContainer.innerHTML = '<div class="text-danger text-sm">Error loading slots.</div>';
                });
        });
    }
});