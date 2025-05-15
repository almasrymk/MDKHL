using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SmartParking.Blazor.Components.Pages.LastInvoiceAtCamera;

public partial class CarImageDetails
{
    [Parameter] public string Title { get; set; }
    [Parameter] public string Image { get; set; }
    [Parameter] public DateTime Date { get; set; }

    private string TransformOrigin { get; set; } = "center center";
    private string Scale { get; set; } = "scale(1)";
    private string ComputedStyle => $"transform-origin: {TransformOrigin}; transform: {Scale};";



    private void ResetZoom(MouseEventArgs e)
    {
        TransformOrigin = "center center";
        Scale = "scale(1)"; // Reset zoom when mouse leaves
    }
    private void SetZoomOrigin(MouseEventArgs e)
    {
        // Calculate the position of the mouse relative to the container
        double xPercent = e.OffsetX / 300 * 100; // 400 is the container width
        double yPercent = e.OffsetY / 300 * 100; // 300 is the container height

        TransformOrigin = $"{xPercent}% {yPercent}%";
        Scale = "scale(1.2)"; // Zoom level when hovered
    }
}
