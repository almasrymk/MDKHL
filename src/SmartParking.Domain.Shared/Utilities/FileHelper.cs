using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Utilities;

public static class FileHelper
{
    public static async Task<string> SaveImageFromBase64Async(string base64 , string folderName)
    {
        
        var directoryPath = CreateImageDirectory(folderName);
        await Task.Yield();
        var base64Data = base64.Substring(base64.IndexOf(",") + 1);
        byte[] imageBytes = Convert.FromBase64String(base64Data);

        string fileName = $"{Guid.NewGuid()}{DateTime.Now:yyyyMMddHHmmssfff}.png";
        string filePath = Path.Combine(directoryPath, fileName); ;

        File.WriteAllBytes(filePath, imageBytes);
        return $"/{folderName}/{fileName}";
    }
    public static string CreateImageDirectory(string folderName)
    {
        string imageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
        if (!Directory.Exists(imageDirectoryPath))
        {
            Directory.CreateDirectory(imageDirectoryPath);
        }
        return imageDirectoryPath;
    }
}
