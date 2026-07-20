using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace SmartParking.Blazor.Components.PosTerminal;

public class GeideaWebEcrInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;
    private DotNetObjectReference<GeideaWebEcrCallbackHandler>? _dotNetRef;

    public GeideaWebEcrInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync(GeideaWebEcrCallbackHandler handler)
    {
        try
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/geidea-web-ecr.js");
            _dotNetRef = DotNetObjectReference.Create(handler);
            await _module.InvokeVoidAsync("initialize", _dotNetRef);
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
    }

    public async Task ConnectAsync(string serviceUrl)
    {
        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("connect", serviceUrl);
            }
            catch (JSDisconnectedException) { }
            catch (ObjectDisposedException) { }
        }
    }

    public async Task<bool> SendAsync(string jsonPayload)
    {
        if (_module is not null)
        {
            try
            {

                return await _module.InvokeAsync<bool>("send", jsonPayload);
            }
            catch (JSDisconnectedException) { }
            catch (ObjectDisposedException) { }
        }
        return false;
    }

    public async Task DisconnectAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("disconnect");
            }
            catch (JSDisconnectedException) { }
            catch (ObjectDisposedException) { }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_dotNetRef is not null)
        {
            _dotNetRef.Dispose();
            _dotNetRef = null;
        }

        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException) { }
            catch (ObjectDisposedException) { }
            _module = null;
        }
    }
}
