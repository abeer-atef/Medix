

"use strict";

// ==================== PAGE NAVIGATION ====================

function showPage(pageId) {
    const allPages = document.querySelectorAll(".page");
    allPages.forEach((page) => {
        page.classList.remove("active");
    });

    const targetPage = document.getElementById("page-" + pageId);
    if (targetPage) {
        targetPage.classList.add("active");
    } else {
        console.error("Page with id 'page-" + pageId + "' not found!");
    }

    const allLinks = document.querySelectorAll(".nav-link");
    allLinks.forEach((link) => {
        link.classList.remove("active");

        const onClickAttr = link.getAttribute("onclick");
        if (onClickAttr && onClickAttr.includes(`'${pageId}'`)) {
            link.classList.add("active");
        }
    });

    const navLinks = document.getElementById("navLinks");
    if (navLinks && navLinks.classList.contains("active")) {
        navLinks.classList.remove("active");
    }

    window.scrollTo({
        top: 0,
        behavior: "smooth",
    });
}

function showDoctorLogin() {
    showPage("doctor-login");
}

function showAdminLogin() {
    showPage("admin-login");
}

// ==================== NAVBAR ====================

window.addEventListener("scroll", () => {
    const navbar = document.getElementById("navbar");
    if (navbar) {
        if (window.scrollY > 20) {
            navbar.classList.add("scrolled");
        } else {
            navbar.classList.remove("scrolled");
        }
    }
});

function toggleMenu() {
    const navLinks = document.getElementById("navLinks");
    const hamburger = document.getElementById("hamburger");
    if (navLinks) {
        navLinks.classList.toggle("open");
        hamburger.classList.toggle("active");
    }
}

function toggleSidebar() {
    const sidebar = document.querySelector(".sidebar");
    if (sidebar) {
        sidebar.classList.toggle("open");
    }
}

// Close menu on outside click
document.addEventListener("click", (e) => {
    const navLinks = document.getElementById("navLinks");
    const hamburger = document.getElementById("hamburger");
    if (navLinks && hamburger) {
        if (!navLinks.contains(e.target) && !hamburger.contains(e.target)) {
            navLinks.classList.remove("open");
        }
    }
});

// ==================== AUTH FORMS ====================

function toggleAuthForm(formId) {
    const forms = document.querySelectorAll(".auth-form");
    forms.forEach((form) => form.classList.remove("active"));

    const targetForm = document.getElementById(formId);
    if (targetForm) {
        targetForm.classList.add("active");
    }
}

function togglePass(inputId) {
    const input = document.getElementById(inputId);
    if (!input) return;
    const icon = input.nextElementSibling;
    if (input.type === "password") {
        input.type = "text";
        if (icon) icon.classList.replace("fa-eye", "fa-eye-slash");
    } else {
        input.type = "password";
        if (icon) icon.classList.replace("fa-eye-slash", "fa-eye");
    }
}