// UnityWebBrowser (UWB)
// Copyright (c) 2021-2022 Voltstro-Studios
// 
// This project is under the MIT license. See the LICENSE.md file for more details.

using System;
using UnityWebBrowser.Engine.Cef.Core;
using UnityWebBrowser.Engine.Cef.Shared.Browser.Popups;
using VoltstroStudios.UnityWebBrowser.Engine.Shared.Popups;
using VoltstroStudios.UnityWebBrowser.Shared;
using VoltstroStudios.UnityWebBrowser.Shared.Popups;
using Xilium.CefGlue;

namespace UnityWebBrowser.Engine.Cef.Shared.Browser;

/// <summary>
///     <see cref="CefLifeSpanHandler" /> implementation
/// </summary>
public class UwbCefLifespanHandler : CefLifeSpanHandler
{
    private readonly PopupAction popupAction;

    private readonly EnginePopupManager popupManager;
    private readonly ProxySettings proxySettings;
    private readonly bool ignoreSslErrors;
    private readonly string[] ignoreSslErrorsDomains;

    public UwbCefLifespanHandler(
        PopupAction popupAction,
        EnginePopupManager enginePopupManager,
        ProxySettings proxySettings,
        bool ignoreSslErrors,
        string[] ignoreSslErrorsDomains)
    {
        this.proxySettings = proxySettings;
        this.popupAction = popupAction;
        popupManager = enginePopupManager;
        this.ignoreSslErrors = ignoreSslErrors;
        this.ignoreSslErrorsDomains = ignoreSslErrorsDomains;
    }

    public event Action<CefBrowser> AfterCreated;

    protected override void OnAfterCreated(CefBrowser browser)
    {
        AfterCreated?.Invoke(browser);
    }

    protected override bool OnBeforePopup(CefBrowser browser, CefFrame frame, int popupId, string targetUrl,
        string targetFrameName,
        CefWindowOpenDisposition targetDisposition, bool userGesture, CefPopupFeatures popupFeatures,
        CefWindowInfo windowInfo, ref CefClient client, CefBrowserSettings settings,
        ref CefDictionaryValue extraInfo,
        ref bool noJavascriptAccess)
    {
        CefLoggerWrapper.Debug($"Popup: {targetFrameName}({targetUrl})");

        switch (popupAction)
        {
            case PopupAction.Ignore:
                break;
            case PopupAction.OpenExternalWindow:
                popupManager.OnPopup(new UwbCefEnginePopupInfo(popupManager, proxySettings, ref client, ignoreSslErrors, ignoreSslErrorsDomains));
                return false;
            case PopupAction.Redirect:
                frame.LoadUrl(targetUrl);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return true;
    }

    protected override bool DoClose(CefBrowser browser)
    {
        return false;
    }
}