# Configuration

1. Generate `credential.json` (service account key) and put it in `src/src/ExportGooglePhraseBookFromSpreadSheet`:
   - Go to <https://console.cloud.google.com/iam-admin/serviceaccounts>, select the project.
   - Pick the service account (or create one) -> Actions (⋮) -> Manage keys -> Add key -> Create new key -> JSON.
   - Download the file, rename it to `credential.json`, and place it in the folder above.
2. Generate `client_secret.json` (OAuth client) and put it in `src/src/GoogleDriveFileControl`:
   - Go to <https://console.cloud.google.com/apis/credentials>, select the project.
   - Ensure the Google Drive API is enabled (APIs & Services -> Library -> Google Drive API -> Enable).
   - Configure the OAuth consent screen if not already done (add your account as a test user if in "Testing" mode).
   - Create Credentials -> OAuth client ID -> Application type: Desktop app -> Create.
   - Download the JSON, rename it to `client_secret.json`, and place it in the folder above.

Both files contain secrets and must never be committed to source control (already covered by `.gitignore`).
