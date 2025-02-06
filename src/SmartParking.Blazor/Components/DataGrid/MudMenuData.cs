namespace SmartParking.Blazor.Components.DataGrid;

public class MudMenuData
{
    public string Name { get; set; }
    public string Label { get; set; } = string.Empty;
    public List<MenuItem> Items { get; set; } = new();
    public bool IsExport { get; set; } 
    public bool IsMenueColumnVisible { get; set; }
}
