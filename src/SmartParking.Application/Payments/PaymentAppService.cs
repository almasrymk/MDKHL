using Castle.Core.Resource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Payments;
[Authorize]
public class PaymentAppService : CrudAppService<Payment, PaymentDto, Guid, PagedAndSortedResultRequestDto, CreateUpdatePaymentDto>, IPaymentAppService
{
    private readonly IRepository<Payment, Guid> _repository;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly IDataFilter dataFilter;
    private readonly ICustomerPackagesAppService customerPackagesAppService;
    private readonly IOrganizationsAppService organizationsAppService;
    private readonly IRepository<Invoice, int> _invoiceRepo;
    private readonly IRepository<Package, int> _packageRepo;
    private readonly HttpClient _httpClient;
    private readonly IInvoiceUtilityAppService _invoiceUtility;
    private const string EntityId = "8ac7a4ca9244f53c01924741733e0327";
    private const string Currency = "SAR";
    private const string PaymentType = "DB";
    private const string PaymentUrl = "https://eu-test.oppwa.com/v1/checkouts";
    private const string token = "OGFjN2E0Y2E5MjQ0ZjUzYzAxOTI0NzNkMjBlNDAzMTV8NXJDQjdzYUpQN2U3U1lrWiU6QjQ=";
    private const string tokenType = "Bearer";


    private readonly Dictionary<string, string> PaymentEntityID = new Dictionary<string, string>
    {
        { "VISA", "8ac7a4ca9244f53c01924741733e0327" },
        { "MADA", "8ac7a4ca9244f53c019247402c950320" }
    };

    public PaymentAppService(IRepository<Payment, Guid> repository,
        IRepository<Invoice, int> invoiceRepo, IRepository<Package, int> packageRepo,
        IRepository<Customer, Guid> customerRepo, IDataFilter dataFilter,
        ICustomerPackagesAppService customerPackagesAppService,
        IOrganizationsAppService _organizationsAppService,
        HttpClient httpClient,
        IInvoiceUtilityAppService invoiceUtility)
    : base(repository)
    {

        _repository = repository;
        _customerRepo = customerRepo;
        this.dataFilter = dataFilter;
        this.customerPackagesAppService = customerPackagesAppService;
        organizationsAppService = _organizationsAppService;
        _invoiceRepo = invoiceRepo;
        _packageRepo = packageRepo;
        _httpClient = httpClient;
        _invoiceUtility = invoiceUtility;
    }



    public async Task<PaymentCheckoutDto> CreateCheckoutPaymentAsync(PaymentCheckoutRequestDto paymentCheckoutRequest)
    {

        using (dataFilter.Disable<IMultiTenant>())
        {
            var customer = await _customerRepo.GetQueryableAsync();
            var customerId = customer.Where(s => s.IdentityUserId == CurrentUser.Id).Select(s => s.Id).FirstOrDefault();
            // ENABLE TO VALIDATE var organizationId = organizationsAppService.GetAsync(paymentCheckoutRequest.OrganizationId);

            if (customerId != null )
            {

                // handle cash transaction
                if (paymentCheckoutRequest.PaymentMethod == PaymentMethod.Cash)
                {


                    return await SavePaymentRequest(paymentCheckoutRequest, PaymentStatus.Success, customerId);

                }
                else   //handle online transaction
                {

                    var entityId = GetEntityIdByPaymentMethod(paymentCheckoutRequest.PaymentMethod);
                    var checkoutId = await HyperPayPayment(paymentCheckoutRequest, entityId);

                    return await SavePaymentRequest(paymentCheckoutRequest, PaymentStatus.Pending,customerId,
                        checkoutid: checkoutId, entityId: entityId);

                }





            }
            else
            {
                throw new Exception("Customer Not Found");
            }


        }
    }

    private async Task<PaymentCheckoutDto> SavePaymentRequest(PaymentCheckoutRequestDto paymentCheckoutRequest,
        PaymentStatus paymentStatus,
        Guid? customerId,
        string checkoutid = null,
        string entityId = null)
    {
        // add QrCode
        // var token = await _invoiceUtility.GetTokenAsync();
        paymentCheckoutRequest.PaymentNumber = await GeneratePaymentNumberAsync(paymentCheckoutRequest.OrganizationId);
        var qrCode = await _invoiceUtility.GetQrCodeAsync(paymentCheckoutRequest.Amount, paymentCheckoutRequest.OrganizationId);
        Payment payment = new Payment
        {
            Amount = paymentCheckoutRequest.Amount,
            Currency = Currency,
            PaymentMethod = paymentCheckoutRequest.PaymentMethod,
            InvoiceId = paymentCheckoutRequest.InvoiceId,
            PackageId = paymentCheckoutRequest.PackageId,
            OrganizationId = paymentCheckoutRequest.OrganizationId,
            PaymentCategory = paymentCheckoutRequest.PaymentCategory,
            CheckoutId = checkoutid,
            EntityId = entityId,
            CustomerId = customerId,
            PaymentStatus = paymentStatus,
            QrCodeInvoice = qrCode.QRCode,
            PaymentNumber = paymentCheckoutRequest.PaymentNumber,
            ArPlateNumber = paymentCheckoutRequest.ArPlateNumber,
            EnPlateNumber = paymentCheckoutRequest.EnPlateNumber,
        };

        payment = await _repository.InsertAsync(payment);


        return new PaymentCheckoutDto
        {
            CheckoutId = payment.CheckoutId,
            PaymentId = payment.Id,
            PaymentStatus = payment.PaymentStatus,
            PaymentMethod = payment.PaymentMethod,
            EntityId = payment.EntityId,
            TokenId = token,
            QrCodeInvoice = qrCode.QRCode,
            PaymentNumber = payment.PaymentNumber,
            

        };



    }
    private async Task<string> HyperPayPayment(PaymentCheckoutRequestDto paymentCheckoutRequest, string entityId)
    {
        var content = new StringContent(
          $"entityId={entityId}&amount={paymentCheckoutRequest.Amount}&currency={Currency}&paymentType={PaymentType}",
          Encoding.ASCII,
          "application/x-www-form-urlencoded"
      );

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, token);


        var response = await _httpClient.PostAsync(PaymentUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Payment request failed with status: {response.StatusCode}");
        }


        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObject = JObject.Parse(responseContent);
        var checkoutId = responseObject["id"]?.ToString();
        if (string.IsNullOrEmpty(checkoutId))
        {
            throw new Exception("Unable to Get CheckOut ID");
        }
        return checkoutId;


    }

    private string GetEntityIdByPaymentMethod(PaymentMethod paymentMethod)
    {
        return paymentMethod switch
        {
            PaymentMethod.VISA => PaymentEntityID["VISA"],
            PaymentMethod.MASTERCARD => PaymentEntityID["VISA"],
            PaymentMethod.MADA => PaymentEntityID["MADA"],
            _ => throw new Exception("Payment method is not supported.")
        };
    }

    public async Task<PaymentDto> GetPaymentStatus(string input, string entityId,string ArabicPlateNumber , string EnglishPlateNumber)
    {
        var url = $"{PaymentUrl}/{input}/payment?entityId={entityId}";

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, token);

        var response = await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JObject.Parse(responseContent);

            var resultDescription = responseObject["result"]?["description"]?.ToString();
            var status = PaymentStatus.Cancelled;

            if (!string.IsNullOrEmpty(resultDescription))
            {
                if (resultDescription.Contains("pending", StringComparison.OrdinalIgnoreCase))
                {
                    status = PaymentStatus.Pending;
                    return new PaymentDto
                    {
                        CheckoutId = input,
                        PaymentStatus = status,
                    };
                }
                else if (resultDescription.Contains("successfuly", StringComparison.OrdinalIgnoreCase))
                {
                    status = PaymentStatus.Success;
                }
            }
            using (dataFilter.Disable<IMultiTenant>())
            {

                var payment = await _repository.FirstOrDefaultAsync(p => p.CheckoutId == input);
                if (payment != null)
                {
                    if (payment.PaymentCategory == PaymentCategory.PaymentParking)
                    {

                        var invoice = await _invoiceRepo.FirstOrDefaultAsync(i => i.Id == payment.InvoiceId);
                        if (invoice != null)
                        {
                            invoice.InvoiceType = status == PaymentStatus.Success ? InvoiceType.Paid : InvoiceType.UnPaid;
                            await _invoiceRepo.UpdateAsync(invoice);
                        }
                    }
                    else if (payment.PaymentCategory == PaymentCategory.Package)
                    {

                        if (status == PaymentStatus.Success)
                        {
                            //package.Status = status == PaymentStatus.Success ? Status.Active : Status.Inactive;
                            //await _packageRepo.UpdateAsync(package);
                            var assignStatus = await customerPackagesAppService.AssignPackageToCustomerWebAsync(payment.PackageId.Value, payment.CustomerId,ArabicPlateNumber,EnglishPlateNumber);
                            if (!assignStatus)
                            {
                                throw new Exception("Can't assign package to customer");
                            }
                        }
                    }

                    // add QrCode
                    payment.PaymentStatus = status;
                    //var token = await _invoiceUtility.GetTokenAsync();
                    var qrCode = await _invoiceUtility.GetQrCodeAsync(payment.Amount, payment.OrganizationId);
                    payment.QrCodeInvoice = qrCode.QRCode;
                    await _repository.UpdateAsync(payment);
                }

                return new PaymentDto
                {
                    CheckoutId = payment.CheckoutId,
                    CustomerId = (Guid)payment.CustomerId,
                    Id = payment.Id,
                    PaymentStatus = status,
                    PaymentCategory = payment.PaymentCategory,
                    InvoiceId = payment.InvoiceId,
                    PackageId = payment.PackageId,
                    OrganizationId = payment.OrganizationId,
                    QrCodeInvoice = payment.QrCodeInvoice,
                };
            }
        }
        else
        {
            throw new Exception($"Payment status request failed with status: {response.StatusCode}");
        }
    }
    public async Task<Result<PayementPackageDto>> GetPackagePaymentDetailsAsync(int OrganizationId, Guid CustomerId, Guid PayemntId)
    {

        using (dataFilter.Disable<IMultiTenant>())
        {
            var payement = await _repository.GetAsync(x => x.Id == PayemntId && x.OrganizationId == OrganizationId && x.CustomerId == CustomerId);
            var org = await organizationsAppService.GetAsync(OrganizationId);
            var customerquery = await _customerRepo.GetQueryableAsync();
            customerquery = customerquery.Include(cust => cust.IdentityUser);
            var customer = await AsyncExecuter.FirstAsync(customerquery);

            var package = await _packageRepo.GetAsync(x => x.Id == payement.PackageId);
            var packagedto = new PayementPackageDto()
            {
                OrganizationAddress = org.Address,
                OrganizationArabicName = org.ArabicName,
                OrganizationEnglishName = org.EnglishName,
                CustomerArabicName = customer.ArabicName,
                CustomerEnglishName = customer.IdentityUser.Name,
                PayementTime = payement.CreationTime,
                Price = package.Price,
                Description = package.Description,
                


            };
            return Result<PayementPackageDto>.Success(packagedto, "Package Details Retrieved Succesuffuly");


        }
    }
    public async Task<Result<InvoiceDto>> GetInvoicePaymentDetailsAsync(int OrganizationId, Guid CustomerId, Guid PayemntId)
    {

        using (dataFilter.Disable<IMultiTenant>())
        {
            var payement = await _repository.GetAsync(x => x.Id == PayemntId && x.OrganizationId == OrganizationId && x.CustomerId == CustomerId);
            var invoice = await _invoiceRepo.GetAsync(x => x.Id == payement.InvoiceId);
            var invoicedto = ObjectMapper.Map<Invoice, InvoiceDto>(invoice);
            return Result<InvoiceDto>.Success(invoicedto, "Invoice Details Retrieved Succesuffuly");

        }
    }

    public async Task<PaymentCheckoutDto> CreateCheckoutInvoicePaymentWebAsync(PaymentCheckoutRequestDto paymentCheckoutRequest)
    {
        var paymentCheckoutDto= await SavePaymentRequest(paymentCheckoutRequest, PaymentStatus.Success, paymentCheckoutRequest.CustomerId);
       await UpdateInvoiceStatusAsync(paymentCheckoutRequest.InvoiceId);
        return paymentCheckoutDto;
    }

    private async Task<Result<bool>> UpdateInvoiceStatusAsync(int? InvoiceId)
    {
        using (dataFilter.Disable<IMultiTenant>())
        {
            var invoice =await _invoiceRepo.FirstOrDefaultAsync(x => x.Id == InvoiceId);
            if (invoice == null) {  return Result<bool>.Success(false); }
            invoice.InvoiceType = InvoiceType.Paid;
            await _invoiceRepo.UpdateAsync(invoice);
            return Result.Success(true);

        }
    }

    [RemoteService(IsEnabled = false)]

    private async Task<string> GeneratePaymentNumberAsync(int organizationId)
    {
        using (dataFilter.Disable<IMultiTenant>())
        {
           var organization = await organizationsAppService.GetAsync(organizationId);

            var currentYearSuffix = DateTime.Now.Year % 100;  // Last 2 digits of the year
            var prefix = organization.EnglishName.Substring(0, 3).ToUpper();

            // Fetch the last invoice for the current year and organization
            var invoices = await _repository.GetListAsync(i => i.OrganizationId == organizationId);

            var lastInvoice = invoices
            .Where(i => i.CreationTime.Year == DateTime.Now.Year && i.OrganizationId == organization.Id)
            .OrderByDescending(i => i.PaymentNumber)
            .FirstOrDefault();


            int nextInvoiceSequence = 1;

            if (lastInvoice != null)
            {
                if (lastInvoice.PaymentNumber != null)
                {
                    var lastSequenceString = lastInvoice.PaymentNumber[^5..];
                    if (int.TryParse(lastSequenceString, out int lastSequence))
                    {
                        nextInvoiceSequence = lastSequence + 1;
                    }
                }
               
            }

            // Format the sequence as a three-digit number with leading zeros
            var formattedSequence = nextInvoiceSequence.ToString().PadLeft(7, '0');

            // Combine all parts to form the final invoice number
            return $"{prefix}{currentYearSuffix}{formattedSequence}";
        }
    }
}


