// UnityWebBrowser (UWB)
// Copyright (c) 2021-2022 Voltstro-Studios
// 
// This project is under the MIT license. See the LICENSE.md file for more details.

using System.Linq;
using System.Threading.Tasks;
using UnityWebBrowser.Engine.Cef.Core;
using VoltstroStudios.UnityWebBrowser.Shared;
using Xilium.CefGlue;

namespace UnityWebBrowser.Engine.Cef.Shared.Browser;

/// <summary>
///     <see cref="CefRenderHandler" /> implementation
/// </summary>
public class UwbCefRequestHandler : CefRequestHandler
{
    private readonly ProxySettings proxySettings;
    private readonly bool ignoreSslErrors;
    private readonly string[] ignoreSslErrorsDomains;

    public UwbCefRequestHandler(ProxySettings proxySettings, bool ignoreSslErrors, string[] ignoreSslErrorsDomains)
    {
        this.proxySettings = proxySettings;
        this.ignoreSslErrors = ignoreSslErrors;
        this.ignoreSslErrorsDomains = ignoreSslErrorsDomains;
    }

    protected override CefResourceRequestHandler GetResourceRequestHandler(CefBrowser browser, CefFrame frame,
        CefRequest request,
        bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
    {
        return null;
    }

    protected override bool GetAuthCredentials(CefBrowser browser, string originUrl, bool isProxy, string host,
        int port, string realm,
        string scheme, CefAuthCallback callback)
    {
        if (isProxy) callback.Continue(proxySettings.Username, proxySettings.Password);

        return base.GetAuthCredentials(browser, originUrl, isProxy, host, port, realm, scheme, callback);
    }

    protected override bool OnCertificateError(CefBrowser browser, CefErrorCode certError, string requestUrl, CefSslInfo sslInfo,
        CefCallback callback)
    {
        if (ignoreSslErrors && ignoreSslErrorsDomains != null)
        {
            requestUrl = requestUrl!.ToLower();
            bool contains = ignoreSslErrorsDomains.Any(x => requestUrl.Contains(x));
            if(contains)
                callback!.Continue();
            else
                callback!.Cancel();
            
            return true;
        }

        return false;
    }
}