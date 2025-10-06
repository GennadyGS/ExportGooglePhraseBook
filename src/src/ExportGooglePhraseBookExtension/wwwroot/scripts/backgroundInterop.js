window.waitForTabToLoad = async function (tabId) {
    function tabStatus(tabId) {
        return new Promise((resolve) => {
            chrome.tabs.get(tabId, function (tab) {
                resolve(tab.status);
            });
        });
    }

    function waitForComplete(tabId) {
        return new Promise((resolve) => {
            function listener(updatedTabId, changeInfo) {
                if (updatedTabId === tabId && changeInfo.status === "complete") {
                    console.log("Window is loaded");
                    chrome.tabs.onUpdated.removeListener(listener);
                    resolve();
                }
            }
            chrome.tabs.onUpdated.addListener(listener);
        });
    }

    const status = await tabStatus(tabId);
    if (status === "complete") {
        console.log("Window is already loaded");
        return;
    } else {
        console.log("Adding tab listener");
        await waitForComplete(tabId);
    }
};

const DEFAULT_TEMPLATE = 'exportGooglePhraseBook://open?spreadSheetId={sheetId}';
window.getExportToolUrlTemplate = async function () {
    return new Promise((resolve) => {
        chrome.storage.sync.get({ exportToolUrlTemplate: DEFAULT_TEMPLATE }, (items) => {
            resolve(items.exportToolUrlTemplate || DEFAULT_TEMPLATE);
        });
    });
};
