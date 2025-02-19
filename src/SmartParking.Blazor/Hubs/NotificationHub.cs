using Microsoft.AspNetCore.SignalR;
using SmartParking.Enums;
using SmartParking.Localization;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;

namespace SmartParking.Blazor.Hubs;

public class NotificationHub : AbpHub
{
    public async Task NotifyClient(string status,string message)
    {
        await Clients.All.SendAsync("ReceiveExportNotification", status, message);
    }
}
