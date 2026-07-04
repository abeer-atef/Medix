/* ============================================================
   Medix - Page Init
   Runs once on every page load: shows the home "page" section
   (only relevant on the single-page landing layout), reveals the
   chat notification bubble after a short delay, and animates the
   admin dashboard's revenue bars on first render.

   NOTE: feature-specific logic lives in its own file —
   see navigation.js, doctor-filters.js, and chatbot.js.
   ============================================================ */

"use strict";

document.addEventListener("DOMContentLoaded", () => {
    // Show home page only on the main landing page (page-home must exist)
    if (document.getElementById("page-home")) {
        showPage("home");
    }

    // Auto-show chat notification after 3s
    setTimeout(() => {
        const notif = document.querySelector(".chat-notification");
        if (notif) notif.style.display = "flex";
    }, 3000);

    // Animate bars when admin dashboard is opened
    document.querySelectorAll(".bar").forEach((bar, i) => {
        bar.style.opacity = "0";
        bar.style.transform = "scaleY(0)";
        bar.style.transformOrigin = "bottom";
        setTimeout(
            () => {
                bar.style.transition = "all 0.6s ease " + i * 0.1 + "s";
                bar.style.opacity = "1";
                bar.style.transform = "scaleY(1)";
            },
            300 + i * 80,
        );
    });

    // Keyboard shortcuts
    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape") {
            const chatWindow = document.getElementById("chatbot-window");
            if (chatWindow && chatWindow.classList.contains("open")) toggleChatbot();
        }
    });

    console.log(
        "%c🏥 Medix Loaded Successfully!",
        "color:#1a73e8;font-size:16px;font-weight:bold;",
    );
    console.log(
        "%cVersion 1.0.0 | Designed for healthcare excellence",
        "color:#64748b;font-size:12px;",
    );
});
