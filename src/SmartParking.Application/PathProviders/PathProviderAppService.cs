using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PathProviders;
public class PathProviderAppService : SmartParkingAppService, IPathProviderAppService
{
    #region Public methods
    ///<inheritdoc/>
    public string CreatePath(string path)
    {
        Directory.CreateDirectory(path);
        return path;
    }
    ///<inheritdoc/>
    public string GetFullPath(string folder, string fileName)
    {
        string pathFolder;
        pathFolder = ValidatePath(folder) ?
            folder : pathFolder = CreatePath(folder);
        var fullPath = Path.Combine(pathFolder, fileName);
        return fullPath;
    }
    ///<inheritdoc/>
    public bool ValidatePath(string path)
    {
        return Directory.Exists(path);
    }
    #endregion
}
