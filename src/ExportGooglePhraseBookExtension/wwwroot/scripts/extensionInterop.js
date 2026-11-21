chrome.runtime.onMessage.addListener(async (message, sender, sendResponse) => {
    if (message.command === "clickButton") {
        try {
            console.log("clickButton command received with selector:", message.args.selector);
            var btn = document.querySelector(message.args.selector);
            if (btn) btn.click();
            sendResponse({ success: true });
        } catch (e) {
            sendResponse({ success: false, error: e.toString() });
        }
    }

    if (message.command === "getInputValue") {
        try {
            var input = document.querySelector(message.args.selector);
            sendResponse(input ? input.value : null);
        } catch (e) {
            sendResponse(null);
        }
    }

    return true;
});

console.log("content script loaded");