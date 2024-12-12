using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;

namespace SmartParking.Blazor.Hubs;

public class MessagingHub : AbpHub
{
    public async Task SendMessage(string targetUserName, string message)
    {
        var currentUserName = "hager"; //Access to the current user info

        await Clients.All.SendAsync("ReceiveMessage", currentUserName, message);
    }
}
