using Microsoft.AspNetCore.Components.Forms;
using System.IO;
using System;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Shared;

public static class ImageHelper
{
    public static async Task<string> GetImageBase64Async(IBrowserFile file)
    {
        using MemoryStream ms = new();
        var resized = await file.RequestImageFileAsync(file.ContentType, maxWidth: 500, maxHeight: 800);
        using Stream fileStream = resized.OpenReadStream();
        await fileStream.CopyToAsync(ms);

        return $"data:{file.ContentType};base64,{Convert.ToBase64String(ms.ToArray())}";
    }
}
