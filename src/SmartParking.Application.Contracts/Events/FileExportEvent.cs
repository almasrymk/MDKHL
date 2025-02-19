using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EventBus;

namespace SmartParking.Events;

[EventName("FileExportEvent")]
public class FileExportEvent
{
    public string Status { get; set; } 
    public string Message { get; set; }

    public FileExportEvent(string status, string message)
    {
        Status = status;
        Message = message;
    }
}
