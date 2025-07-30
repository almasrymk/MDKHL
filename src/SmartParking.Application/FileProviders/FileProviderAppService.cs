using SmartParking.PathProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.FileProviders;
public class FileProviderAppService : SmartParkingAppService, IFileProviderAppService
{
    private readonly IPathProviderAppService _pathProvider;
    public FileProviderAppService(IPathProviderAppService pathProvider)
    {
        _pathProvider = pathProvider;
    }
    #region Public Methods
    /// <inheritdoc />
    public async Task<string> SaveFileAsync(byte[] file, string folderName, string fileName)
    {
        string imagePath = _pathProvider.GetFullPath(folderName, fileName);

        await File.WriteAllBytesAsync(imagePath, file);

        imagePath = imagePath.Substring(imagePath.IndexOf("wwwroot"));

        return imagePath.Replace("wwwroot", "").Replace("\\", "/");
    }
    #endregion
}
