using Abp.Application.Services;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages.QRGenerator;
public interface ICustomerPackageQRGenerator  : IApplicationService
{
    Task<(string base64Image, string relativePath)> CreateAsync(CreateUpdateQrCodeDto input);
}
