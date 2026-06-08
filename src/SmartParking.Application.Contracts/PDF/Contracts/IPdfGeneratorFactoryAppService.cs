using iText.Kernel.Geom;
using NuGet.Protocol.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.PDF.Contracts;

public interface IPdfGeneratorFactoryAppService : IApplicationService
{
    Task<Stream> ResolveGeneratorAsync(Guid paymentId,string PageSize);

}
