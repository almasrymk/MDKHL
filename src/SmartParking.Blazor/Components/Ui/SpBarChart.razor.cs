using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SmartParking.Blazor.Components.Ui;

public partial class SpBarChart
{
    [Parameter] public List<ChartSeries> Series { get; set; } = new List<ChartSeries>();
    private int index = -1;
    [Parameter] public string[] XAxisLabels { get; set; } = { " " };

}
