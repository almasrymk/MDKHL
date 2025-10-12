using Microsoft.Extensions.DependencyInjection;
using NuGet.Protocol.Plugins;
using SmartParking.Payments;
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

    public async Task<Stream> ResolveGeneratorAsync(Guid paymentId)
    {
        var payment = await _paymentAppService.GetMdkhlAsync(paymentId);

        if (payment.Invoice is not null)
        {
             var service = _serviceProvider.GetRequiredService<InvoicePDFAppService>();
            return await service.CreatePdf(payment.Invoice.Id);
        }
        else if (payment.customerPackageDto is not null )
        {
            var service= _serviceProvider.GetRequiredService<PackagePDFAppService>();
            return await service.CreatePdf(payment.customerPackageDto.Id);
        }

        throw new InvalidOperationException("Payment is not associated with either Invoice or CustomerPackage.");
    }
}
