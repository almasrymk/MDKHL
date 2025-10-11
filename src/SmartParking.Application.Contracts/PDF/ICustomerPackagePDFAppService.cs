using SmartParking.CustomerPackages.QRGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.PDF;

public interface ICustomerPackagePDFAppService : IApplicationService
{
    Task<string> GeneratePdfForCustomerPackage(CreateUpdateQrCodeDto input, byte[] qrCodePath);

}
