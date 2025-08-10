using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.FileProviders;
public interface IFileProviderAppService : IApplicationService
{
    Task<string> SaveFileAsync(byte[] file, string folderName, string fileName);
}
