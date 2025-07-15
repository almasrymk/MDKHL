using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PathProviders;
public interface IPathProviderAppService 
{
    /// <summary>
    /// Validates whether the specified path is valid.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <returns><c>true</c> if the path is valid; otherwise, <c>false</c>.</returns>
    bool ValidatePath(string path);

    /// <summary>
    /// Creates a new path by combining the specified path components.
    /// </summary>
    /// <param name="path">The path to create.</param>
    /// <returns>The created path.</returns>
    string CreatePath(string path);

    /// <summary>
    /// Gets the full path for the specified folder and file name.
    /// </summary>
    /// <param name="folder">The folder path.</param>
    /// <param name="fileName">The file name.</param>
    /// <returns>The full path.</returns>
    string GetFullPath(string folder, string fileName);

}
