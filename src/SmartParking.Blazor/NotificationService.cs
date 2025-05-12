
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Enums;

namespace SmartParking.Blazor;

public class NotificationService : IAsyncDisposable
{
    private HubConnection? _hubConnection;

    public  HubConnection? StartConnectionAsync(NavigationManager navigation,ISnackbar snackbar)
    {
        if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
            return _hubConnection;

        _hubConnection = new HubConnectionBuilder()
                    .WithUrl(navigation.BaseUri + "notification")
                    .WithAutomaticReconnect()
                    .Build();

       return _hubConnection;
    }

    public async Task StopConnectionAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopConnectionAsync();
    }
}
