using Microsoft.AspNetCore.Components;

namespace SmartParking.Blazor.Components.Pages.Payments;

public partial class DynamicPrintDialog
{
    [Parameter] public string JsonString { get; set; }
    private Dictionary<string, object> _data;

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(JsonString))
        {
            try
            {
                // Deserialize the JSON string into a dictionary
                _data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(JsonString);
            }
            catch (Exception ex)
            {
                _data = new Dictionary<string, object> { { "Error", "Invalid JSON format" } };
            }
        }
    }
}
