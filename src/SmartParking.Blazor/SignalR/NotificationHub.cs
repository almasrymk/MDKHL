using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SmartParking.Blazor.SignalR;

public sealed class NotificationHub : Hub
{
    public async Task SendNotification()
    {
        await Clients.All.SendAsync("ReceiveNotification");
    }
}