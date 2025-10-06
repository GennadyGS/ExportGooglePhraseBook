using System.Text.RegularExpressions;
using Blazor.BrowserExtension;
using Microsoft.JSInterop;
using WebExtensions.Net.ActionNs;
using WebExtensions.Net.Tabs;

namespace ExportGooglePhraseBookExtension;

public partial class BackgroundWorker(IJSRuntime jsRuntime) : BackgroundWorkerBase
{
    private const string GoogleTranslateUrl = "https://translate.google.com/saved";

    private const string ExportConfirmationUrl =
        "https://docs.google.com/spreadsheets/import/inline?authuser=0";

    private const string ExportButtonSelector =
        "button[aria-label=\"Export to Google Sheets (new tab)\"]";

    private const string ConfirmationButtonSelector = "#confirmActionButton";

    private const string DefaultExportToolUrlTemplate =
        "exportGooglePhraseBook://open?spreadSheetId={sheetId}";

    private readonly IJSRuntime _jsRuntime = jsRuntime;

    private string? _exportToolUrlTemplate;

    private enum State
    {
        Start,
        OriginalPage,
        PhraseBook,
        ExportConfirmation,
        StyleSheet,
    }

    [GeneratedRegex(@"^https://docs\.google\.com/spreadsheets/d/([a-zA-Z0-9-_]+)")]
    private static partial Regex GoogleSheetUrlRegex { get; }

    [BackgroundWorkerMain]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2012:Use ValueTasks correctly",
        Justification = "Code is coming from official template")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage",
        "VSTHRD110:Observe result of async calls",
        Justification = "Code is coming from official template")]
    public override void Main()
    {
        WebExtensions.Action.OnClicked.AddListener(OnClickAsync);
    }

    public async Task OnClickAsync(Tab tab, OnClickData data)
    {
        if (_exportToolUrlTemplate is null)
        {
            _exportToolUrlTemplate =
                await GetExportToolUrlTemplateAsync() ?? DefaultExportToolUrlTemplate;
            await LogInfoAsync($"Loaded export tool url template: {_exportToolUrlTemplate}");
        }

        var currentState = new TabState(State.Start, tab);
        while (true)
        {
            currentState = await WaitForTabLoadAsync(currentState);
            var updatedState = await PerformContinueFlowActionAsync(currentState);
            if (updatedState.State > currentState.State)
            {
                currentState = updatedState;
            }
            else
            {
                break;
            }
        }
    }

    private Uri GetExportToolUrl(string sheetId)
    {
        var template = _exportToolUrlTemplate ?? DefaultExportToolUrlTemplate;
        var urlString = template.Replace("{sheetId}", sheetId, StringComparison.OrdinalIgnoreCase);
        return new Uri(urlString);
    }

    private async Task<string?> GetExportToolUrlTemplateAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("getExportToolUrlTemplate");
        }
        catch (Exception ex)
        {
            await LogErrorAsync($"Failed to read template from storage: {ex.Message}");
            return null;
        }
    }

    private async Task<TabState> PerformContinueFlowActionAsync(TabState tabState)
    {
        if (tabState.Tab is not { Id: { } tabId, Url: { } url })
        {
            await LogInfoAsync("Tab ID or URL is not defined");
            return tabState;
        }

        await LogInfoAsync($"Performing continue flow action; url: {url}");
        if (tabState.State < State.PhraseBook && url.StartsWith(GoogleTranslateUrl))
        {
            var newTab = await ClickButtonAndGetNewTabAsync(tabState.Tab, ExportButtonSelector);
            return new TabState(State.PhraseBook, newTab);
        }

        if (tabState.State < State.ExportConfirmation && url.Equals(ExportConfirmationUrl))
        {
            await ClickButtonBySelectorAsync(tabState.Tab, ConfirmationButtonSelector);
            return new TabState(State.ExportConfirmation, tabState.Tab);
        }

        var match = GoogleSheetUrlRegex.Match(url);
        if (tabState.State < State.StyleSheet && match.Success)
        {
            var sheetId = match.Groups[1].Value;
            var redirectUrl = GetExportToolUrl(sheetId);
            await NavigateToUrlAsync(tabId, redirectUrl);
            return new TabState(State.StyleSheet, tabState.Tab);
        }

        if (tabState.State < State.OriginalPage)
        {
            await NavigateToUrlAsync(tabId, new Uri(GoogleTranslateUrl));
            return new TabState(State.OriginalPage, tabState.Tab);
        }

        await LogInfoAsync("Cannot continue flow from current url");
        return tabState;
    }

    private async Task NavigateToUrlAsync(int tabId, Uri url)
    {
        await LogInfoAsync($"Navigate to URL {url}");
        var updateProperties = new UpdateProperties { Url = url.ToString() };
        await WebExtensions.Tabs.Update(tabId, updateProperties);
    }

    private async Task<Tab> ClickButtonAndGetNewTabAsync(Tab currentTab, string selector)
    {
        var newTabTaskCompletionSource = new TaskCompletionSource<Tab>();
        WebExtensions.Tabs.OnCreated.AddListener(OnTabCreatedAsync);
        await ClickButtonBySelectorAsync(currentTab, selector);
        await LogInfoAsync("Waiting for new tab");
        var result = await newTabTaskCompletionSource.Task;
        await LogInfoAsync("Waiting for new tab complete");
        return result;

        async Task OnTabCreatedAsync(Tab tab)
        {
            await LogInfoAsync("New tab is created");
            newTabTaskCompletionSource.TrySetResult(tab);
            WebExtensions.Tabs.OnCreated.RemoveListener(OnTabCreatedAsync);
        }
    }

    private async Task ClickButtonBySelectorAsync(Tab tab, string selector)
    {
        var tabId = tab.Id ?? throw new InvalidOperationException("Tab ID is not available");
        await LogInfoAsync($"Click button with selector {selector}");
        await SendMessageAsync(tabId, "clickButton", new { selector });
    }

    private async Task<TabState> WaitForTabLoadAsync(TabState state)
    {
        if (state.Tab is not { Id: { } tabId })
        {
            return state;
        }

        try
        {
            await LogInfoAsync("Waiting for page load.");
            await _jsRuntime.InvokeVoidAsync("waitForTabToLoad", tabId);
            await LogInfoAsync("Page finished loading.");
            var updatedTab = await WebExtensions.Tabs.Get(tabId);
            return new TabState(state.State, updatedTab);
        }
        catch (JSException e)
        {
            if (e.Message.Contains("Timeout waiting for tab"))
            {
                await LogErrorAsync($"Tab {tabId} timed out after waiting: {e.Message}");
            }

            throw;
        }
    }

    private async Task SendMessageAsync(int tabId, string command, object args)
    {
        await _jsRuntime.InvokeVoidAsync("chrome.tabs.sendMessage", tabId, new { command, args });
    }

    private async Task LogInfoAsync(string message)
    {
        await _jsRuntime.InvokeVoidAsync("console.log", message);
    }

    private async Task LogErrorAsync(string message)
    {
        await _jsRuntime.InvokeVoidAsync("console.error", message);
    }

    private sealed record TabState(State State, Tab Tab);
}
