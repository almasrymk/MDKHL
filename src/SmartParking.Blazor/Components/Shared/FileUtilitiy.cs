using Microsoft.JSInterop;

namespace SmartParking.Blazor.Components.Shared;

public static class FileUtilitiy
{
    private static IServiceProvider _serviceProvider;

    public static void Configure(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public static async Task OpenNewBlank(string url)
    {
        var jSRuntime = _serviceProvider.GetRequiredService<IJSRuntime>();
        await jSRuntime.InvokeAsync<object>("open", url, "_blank");
    }
}
