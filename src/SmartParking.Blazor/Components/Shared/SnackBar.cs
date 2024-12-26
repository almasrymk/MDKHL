using MudBlazor;

namespace SmartParking.Blazor.Components.Shared;

public static class SnackBar
{
    /// <summary>
    /// Displays a snackbar notification with the specified message and severity.
    /// </summary>
    /// <param name="message">The message to display in the snackbar.</param>
    /// <param name="severity">The severity level of the snackbar (e.g., Success, Error).</param>
    public static void ShowSnackbar(ISnackbar snackbar ,string message, Severity severity)
    {
        snackbar.Clear();
        snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);
    }
}
