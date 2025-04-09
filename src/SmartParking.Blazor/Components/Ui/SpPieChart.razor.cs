using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SmartParking.Blazor.Components.Ui;

public partial class SpPieChart
{
    [Parameter] public string msg { get; set; }
    [Parameter] public double[] data  { get; set; } = { 0, 0, 0, 0 };
    [Parameter] public string[] labels  { get; set; } = { "Customers", "Employees", "Visitors", "NoPackage" };
    ChartOptions options = new ChartOptions();
    string[] colors = { "#0D5480", "#16B7D3", "#CE3333", "#ca06e1" };
    protected override Task OnInitializedAsync()
    {
        options.ChartPalette = colors;
        options.ShowLegend = true;
        options.LineStrokeWidth = 20;
        return base.OnInitializedAsync();
    }
}
