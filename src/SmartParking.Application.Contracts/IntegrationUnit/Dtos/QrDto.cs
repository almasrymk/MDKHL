using System;

namespace SmartParking.IntegrationUnit.Dtos;

public class QrDto
{
    public bool Required { get; set; }
    public string QrCodePayload { get; set; }
    public string QrCodeUrl { get; set; }
    public DateTime ExpiresAt { get; set; }
}
