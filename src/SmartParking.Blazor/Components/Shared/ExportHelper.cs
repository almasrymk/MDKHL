using Microsoft.JSInterop;
using MudBlazor;
using SmartParking.Localization;
using SmartParking.PDF;
using SmartParking.Utilities;

namespace SmartParking.Blazor.Components.Shared;

public static class ExportHelper
{
    public static async Task ExportToExcelAsync(this IJSRuntime jsRuntime, TableData items,
                                                IGenerateFilesAppService generateFilesAppService,
                                                ISnackbar snackbar)
    {
        var folderName = FileHelper.CreateImageDirectory("Folders/Excel");
        string filePath = Path.Combine(folderName, $"{items.Name}.xlsx");

        await generateFilesAppService.ExportToExcel(items, filePath);

        SnackBar.ShowSnackbar(snackbar, LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.SUCCESS) + " " +
                   LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.EXPORT), Severity.Success);

        await Task.Delay(1000);
        await jsRuntime.InvokeVoidAsync("window.open", $"Folders/Excel/{items.Name}.xlsx", "_blank");
    }
}
