using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SmartParking.ZATCATaxInvoice;
using Volo.Abp.Application.Services;

namespace SmartParking.Utilities
{
    public interface IZatcaTaxInvoiceAppService : IApplicationService
    {
         Task<string> SendRequestForZACTA(Guid paymentId);       
    }
}