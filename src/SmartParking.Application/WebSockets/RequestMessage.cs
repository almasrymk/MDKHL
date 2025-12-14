using System;

namespace SmartParking.WebSockets;

public class RequestMessage<TData>
{
    public Guid RequestId { get; set; }
    public string action { get; set; }
    public TData Data { get; set; }
}
