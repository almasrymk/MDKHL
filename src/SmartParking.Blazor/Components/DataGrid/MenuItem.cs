using Microsoft.AspNetCore.Components;

namespace SmartParking.Blazor.Components.DataGrid;

public class MenuItem
{
    public string MenueName { get; set; }=string.Empty;
    public string Label { get; set; } = string.Empty;
    public EventCallback<string> OnClick { get; set; }
    public bool IsShow { get; set; } = true;
    public bool ISColumnVisibility { get; set; } = true;
}