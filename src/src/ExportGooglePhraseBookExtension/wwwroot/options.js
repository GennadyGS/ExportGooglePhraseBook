const DEFAULT_TEMPLATE = 'exportGooglePhraseBook://open?spreadSheetId={sheetId}';

function load() {
  chrome.storage.sync.get({ exportToolUrlTemplate: DEFAULT_TEMPLATE }, (items) => {
    document.getElementById('template').value = items.exportToolUrlTemplate || DEFAULT_TEMPLATE;
  });
}

function save() {
  const value = document.getElementById('template').value.trim();
  if (!value.includes('{sheetId}')) {
    setStatus('Template must contain {sheetId} placeholder', true);
    return;
  }
  chrome.storage.sync.set({ exportToolUrlTemplate: value }, () => {
    setStatus('Saved successfully');
  });
}

function setStatus(message, isError) {
  const el = document.getElementById('status');
  el.textContent = message;
  el.style.color = isError ? 'red' : 'green';
  setTimeout(() => { el.textContent = ''; }, 4000);
}

document.getElementById('save').addEventListener('click', save);

document.addEventListener('DOMContentLoaded', load);
