using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace SmartParking.Blazor.Customs;

public partial class DeviceDisplayComponent : ComponentBase
{
    [Parameter] public string Role { get; set; } = string.Empty;
    [Parameter] public string Device { get; set; } = string.Empty;
    [Parameter] public IEnumerable<string>? Devices { get; set; }

    // Backing list to avoid multiple enumeration and to provide O(1) Count checks
    private List<string> _devices = new();

    protected override void OnParametersSet()
    {
        // Normalize Devices into a list once; trim null/whitespace entries if desired
        _devices = Devices?.Where(d => d != null).Select(d => d ?? string.Empty).ToList()
                   ?? new List<string>();
    }

    private bool IsManager =>
        string.Equals(Role, "Manager", StringComparison.OrdinalIgnoreCase);

    private string SingleDeviceLabel =>
        string.IsNullOrWhiteSpace(Device) ? "—" : Device;
}