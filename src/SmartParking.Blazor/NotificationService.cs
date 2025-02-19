
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

    public async Task StartConnectionAsync(NavigationManager navigation,ISnackbar snackbar)
    {
        
        if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
            return;

        _hubConnection = new HubConnectionBuilder()
                    .WithUrl(navigation.BaseUri + "notification")
                    .WithAutomaticReconnect()
                    .Build();

        _hubConnection.On<string, string>("ReceiveExportNotification", (status, message) =>
        {

           
            snackbar.Add(message, status == EventStatus.Complete.ToString() ? Severity.Success : Severity.Info);
            //Console.WriteLine($"Received SignalR Event: Status={status}, Message={message}");
        });

        try
        {
            await _hubConnection.StartAsync();
            Console.WriteLine("SignalR Connected!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR Connection Failed: {ex.Message}");
        }
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
