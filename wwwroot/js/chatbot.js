/* ============================================================
   Medix - AI Chatbot Widget
   Handles: opening/closing the chat window, sending messages to
   the backend (/Chat/GetBotResponse), rendering user/bot bubbles,
   typing indicator, and the small toast helper used by the chat
   (and a couple of other widgets) for lightweight feedback.
   ============================================================ */

"use strict";

// ==================== TOAST ====================
// Shared by the chatbot and a few other widgets (e.g. copy-to-clipboard).

let toastTimer = null;

function showToast(message) {
    const toast = document.getElementById("toast");
    const msgEl = document.getElementById("toast-message");
    if (!toast || !msgEl) return;

    msgEl.textContent = message;
    toast.classList.add("show");

    if (toastTimer) clearTimeout(toastTimer);
    toastTimer = setTimeout(() => toast.classList.remove("show"), 3000);
}

// ==================== AI CHATBOT ====================

// Conversation history for AI context memory
const conversationHistory = [];

function toggleChatbot() {
    const chatWindow = document.getElementById("chatbot-window");
    const icon = document.getElementById("chat-icon");
    const notif = document.querySelector(".chat-notification");

    if (!chatWindow || !icon) return;

    const isOpen = chatWindow.classList.contains("open");

    if (isOpen) {
        chatWindow.classList.remove("open");
        icon.className = "fas fa-robot";
    } else {
        chatWindow.classList.add("open");
        icon.className = "fas fa-times";
        if (notif) notif.style.display = "none";
        scrollChatToBottom();
    }
}

async function sendMessage() {
    const input = document.getElementById("chat-input");
    if (!input || !input.value.trim()) return;

    const message = input.value.trim();
    input.value = "";
    addUserMessage(message);
    await getBotResponseFromApi(message);
}

async function getBotResponseFromApi(message) {
    // Push user message into history
    conversationHistory.push({ role: "user", content: message });

    // Show a typing indicator while waiting
    const typingId = addTypingIndicator();

    try {
        const response = await fetch("/Chat/GetBotResponse", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ messages: conversationHistory }),
        });

        removeTypingIndicator(typingId);

        // --- Custom Check for 401 or Redirect to Login ---
        if (response.status === 401 || response.redirected || response.url.toLowerCase().includes("login")) {
            addBotMessage("⚠️ Please log in first to use the MedixBot.");
            // Remove the unsent message from history to prevent context corruption
            conversationHistory.pop();
            return;
        }

        if (!response.ok) {
            throw new Error(`Server error: ${response.status}`);
        }

        const data = await response.json();
        const reply = data.reply;

        // Display the AI reply and store it in history
        addBotMessage(reply);
        conversationHistory.push({ role: "assistant", content: reply });
    } catch (err) {
        removeTypingIndicator(typingId);
        console.error("Chatbot API error:", err);
        addBotMessage("⚠️ Sorry, I'm having trouble connecting right now. Please try again in a moment.");
    }
}

function addTypingIndicator() {
    const messages = document.getElementById("chat-messages");
    if (!messages) return null;

    const id = "typing-" + Date.now();
    const el = document.createElement("div");
    el.className = "chat-message bot";
    el.id = id;
    el.innerHTML = `
        <div class="msg-avatar"><i class="fas fa-robot"></i></div>
        <div class="msg-bubble typing-indicator">
            <span></span><span></span><span></span>
        </div>
    `;
    messages.appendChild(el);
    scrollChatToBottom();
    return id;
}

function removeTypingIndicator(id) {
    if (!id) return;
    const el = document.getElementById(id);
    if (el) el.remove();
}

function addUserMessage(text) {
    const messages = document.getElementById("chat-messages");
    if (!messages) return;

    const msgEl = document.createElement("div");
    msgEl.className = "chat-message user";
    msgEl.innerHTML = `
        <div class="msg-avatar">U</div>
        <div class="msg-bubble">${escapeHtml(text)}</div>
    `;
    messages.appendChild(msgEl);
    scrollChatToBottom();
}

function addBotMessage(text) {
    const messages = document.getElementById("chat-messages");
    if (!messages) return;

    const msgEl = document.createElement("div");
    msgEl.className = "chat-message bot";
    msgEl.innerHTML = `
        <div class="msg-avatar"><i class="fas fa-robot"></i></div>
        <div class="msg-bubble"><p>${escapeHtml(text)}</p></div>
    `;
    messages.appendChild(msgEl);
    scrollChatToBottom();
}

function scrollChatToBottom() {
    const messages = document.getElementById("chat-messages");
    if (messages) {
        setTimeout(() => {
            messages.scrollTop = messages.scrollHeight;
        }, 100);
    }
}

function handleChatKey(event) {
    if (event.key === "Enter") sendMessage();
}

function escapeHtml(text) {
    const div = document.createElement("div");
    div.appendChild(document.createTextNode(text));
    return div.innerHTML;
}