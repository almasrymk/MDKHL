using Microsoft.AspNetCore.Components;

namespace SmartParking.Blazor.Customs;

public partial class DeviceDisplayComponent
{
    [Parameter] public string Role { get; set; } = string.Empty;
    [Parameter] public string Device { get; set; } = string.Empty;
    [Parameter] public List<string> Devices { get; set; } = new();

    private bool IsManager =>
        string.Equals(Role, "Manager", StringComparison.OrdinalIgnoreCase);

    private string SingleDeviceLabel =>
        string.IsNullOrWhiteSpace(Device) ? "—" : Device;
}
