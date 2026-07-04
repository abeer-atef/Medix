/* ============================================================
   Medix - Doctor Search & Filters
   Handles: free-text search on the doctors grid, and specialty
   filtering/navigation from the home page "Find a Doctor" chips.
   ============================================================ */

"use strict";

function filterDoctors(query) {
    const q = (query || "").toLowerCase();
    document.querySelectorAll("#doctorGrid .doctor-card").forEach((card) => {
        card.style.display = (card.dataset.search || "").includes(q) ? "" : "none";
    });
}

// ==================== DOCTOR FILTER (home page specialization filter) ====================

function filterAndGo(specialty) {
    showPage("doctors");
    setTimeout(() => {
        filterDoctorCards(specialty, null);
        const chips = document.querySelectorAll(".filter-chip");
        chips.forEach((chip) => {
            chip.classList.remove("active");
            if (
                chip.getAttribute("onclick") &&
                chip.getAttribute("onclick").includes(specialty)
            ) {
                chip.classList.add("active");
            }
        });
        // Set "All" if no match
        const hasActive = Array.from(chips).some((c) =>
            c.classList.contains("active"),
        );
        if (!hasActive && chips[0]) chips[0].classList.add("active");
    }, 100);
}

// Used internally by filterAndGo() above to apply the specialty filter
// to the full doctors grid once the "doctors" page is shown.
function filterDoctorCards(specialty, chipEl) {
    const cards = document.querySelectorAll(".doctor-full-card");
    cards.forEach((card) => {
        const cardSpecialty = card.getAttribute("data-specialty");
        if (specialty === "all" || cardSpecialty === specialty) {
            card.style.display = "flex";
            card.style.animation = "popIn 0.3s ease forwards";
        } else {
            card.style.display = "none";
        }
    });

    if (chipEl) {
        document
            .querySelectorAll(".filter-chip")
            .forEach((c) => c.classList.remove("active"));
        chipEl.classList.add("active");
    }
}
