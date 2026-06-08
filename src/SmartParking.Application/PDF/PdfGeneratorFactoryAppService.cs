using Microsoft.Extensions.DependencyInjection;
using NuGet.Protocol.Plugins;
using SmartParking.Payments;
using SmartParking.PDF.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PDF;

public class PdfGeneratorFactoryAppService : SmartParkingAppService, IPdfGeneratorFactoryAppService
{
    private readonly IPaymentWebAppService _paymentAppService;
    private readonly IServiceProvider _serviceProvider;

    public PdfGeneratorFactoryAppService(IPaymentWebAppService paymentAppService, IServiceProvider serviceProvider)
    {
        _paymentAppService = paymentAppService;
        _serviceProvider = serviceProvider;
    }

    public async Task<Stream> ResolveGeneratorAsync(Guid paymentId,string PageSize)
    {
        var payment = await _paymentAppService.GetMdkhlAsync(paymentId);

        if (payment.Invoice is not null)
        {
             var service = _serviceProvider.GetRequiredService<InvoicePDFAppService>();
            return await service.CreatePdf(payment,PageSize);
        }
        else if (payment.customerPackageDto is not null && payment.customerPackageDto.CustomerId != Guid.Empty)
        {
            var service= _serviceProvider.GetRequiredService<PackagePDFAppService>();
            return await service.CreatePdf(payment, PageSize);
        }
        else
        {
            var service = _serviceProvider.GetRequiredService<UnitPackagePDFAppService>();
            return await service.CreatePdf(payment, PageSize);
        }

    }
}
