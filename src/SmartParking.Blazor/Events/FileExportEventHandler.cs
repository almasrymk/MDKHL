using Microsoft.AspNetCore.SignalR;
using SmartParking.Blazor.Hubs;
using SmartParking.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace SmartParking.Blazor.Events;

public class FileExportEventHandler : IDistributedEventHandler<FileExportEvent>, ITransientDependency
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public FileExportEventHandler(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task HandleEventAsync(FileExportEvent eventData)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveExportNotification", eventData.Status, eventData.Message);
    }
}
