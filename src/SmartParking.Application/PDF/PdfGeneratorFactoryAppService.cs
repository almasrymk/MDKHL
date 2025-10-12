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
    private readonly IPaymentAppService _paymentAppService;
    private readonly IServiceProvider _serviceProvider;

    public PdfGeneratorFactoryAppService(IPaymentAppService paymentRepository, IServiceProvider serviceProvider)
    {
        _paymentAppService = paymentRepository;
        _serviceProvider = serviceProvider;
    }

    public async Task<Stream> ResolveGeneratorAsync(Guid paymentId)
    {
        var payment = await _paymentAppService.GetAsync(paymentId);

        if (payment.InvoiceId.HasValue && payment.InvoiceId != 0)
        {
             var service = _serviceProvider.GetRequiredService<InvoicePDFAppService>();
            return await service.CreatePdf(payment.InvoiceId.Value);
        }
        else if (payment.CustomerPackage is not null && payment.CustomerPackageId != 0)
        {
            var service= _serviceProvider.GetRequiredService<PackagePDFAppService>();
            return await service.CreatePdf(payment.CustomerPackageId);
        }

        throw new InvalidOperationException("Payment is not associated with either Invoice or CustomerPackage.");
    }
}
