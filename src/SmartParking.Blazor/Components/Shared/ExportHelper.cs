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
                                                ISnackbar snackbar , string fileName = default)
    {

        var folderName = FileHelper.CreateImageDirectory("Folders/Excel");
        string filePath = Path.Combine(folderName, string.IsNullOrEmpty(fileName) ? $"{items.Name}.xlsx" : $"{fileName}.xlsx");

        await generateFilesAppService.ExportToExcel(items, filePath);
        await Task.Delay(1000);
        await jsRuntime.InvokeVoidAsync("window.open", $"Folders/Excel/{items.Name}.xlsx", "_blank");
    }  
    public static async Task ExportToCsvAsync(this IJSRuntime jsRuntime, TableData items,
                                                IGenerateFilesAppService generateFilesAppService,
                                                ISnackbar snackbar , string fileName = default)
    {

        var folderName = FileHelper.CreateImageDirectory("Folders/Csv");
        string filePath = Path.Combine(folderName, string.IsNullOrEmpty(fileName) ? $"{items.Name}.csv" : $"{fileName}.csv");

        await generateFilesAppService.ExportToExcel(items, filePath);
        await Task.Delay(1000);
        await jsRuntime.InvokeVoidAsync("window.open", $"Folders/Csv/{items.Name}.csv", "_blank");
    }

    public static async Task ExportToPdfAsync(this IJSRuntime jsRuntime, TableData items,
                                                IGenerateFilesAppService generateFilesAppService,
                                                ISnackbar snackbar, string fileName = default)
    {

        var folderName = FileHelper.CreateImageDirectory("Folders/Pdf");

        string filePath = Path.Combine(folderName, string.IsNullOrEmpty(fileName) ? $"{items.Name}.pdf" : $"{fileName}.pdf");
        await generateFilesAppService.ExportToPdf(items, filePath);
        await Task.Delay(1000);
        await jsRuntime.InvokeVoidAsync("window.open", $"Folders/Pdf/{items.Name}.pdf", "_blank");
    }
}
