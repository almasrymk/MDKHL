using Abp.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.Payments.PaymentIntegrations;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
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

//[Authorize]
public class PaymentAppService : CrudAppService<Payment, PaymentDto, Guid, PagedAndSortedResultRequestDto, CreateUpdatePaymentDto>, IPaymentAppService
{
    private readonly IRepository<Payment, Guid> _repository;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly IDataFilter dataFilter;
    private readonly ICustomerPackagesAppService customerPackagesAppService;
    private readonly IOrganizationsAppService organizationsAppService;
    private readonly IRepository<Invoice, int> _invoiceRepo;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly HttpClient _httpClient;
    private readonly IInvoiceUtilityAppService _invoiceUtility;
    private readonly IZatcaTaxInvoiceAppService _zatcaTaxInvoiceAppService;
    private readonly ILogger<PaymentAppService> _logger;
    private readonly IRepository<CustomerPackage, int> _customerPackageRepository;
    private readonly IRepository<Vehicle, int> _vehicleRepository;
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
        IRepository<Invoice, int> invoiceRepo, 
        IRepository<Customer, Guid> customerRepo, 
        IDataFilter dataFilter,
        ICustomerPackagesAppService customerPackagesAppService,
        IOrganizationsAppService _organizationsAppService,
        HttpClient httpClient,
        IInvoiceUtilityAppService invoiceUtility,
         IZatcaTaxInvoiceAppService zatcaTaxInvoiceAppService,
        ILogger<PaymentAppService> logger,
        IRepository<CustomerPackage, int> customerPackageRepository,
        IRepository<Organization, int> organizationRepo,
        IRepository<Vehicle,int> vehicleRepository)
    : base(repository)
    {

        _repository = repository;
        _customerRepo = customerRepo;
        this.dataFilter = dataFilter;
        this.customerPackagesAppService = customerPackagesAppService;
        organizationsAppService = _organizationsAppService;
        _invoiceRepo = invoiceRepo;
        _httpClient = httpClient;
        _invoiceUtility = invoiceUtility;
        _zatcaTaxInvoiceAppService = zatcaTaxInvoiceAppService;
        _logger = logger;
        _customerPackageRepository = customerPackageRepository;
        _organizationRepository = organizationRepo;
        _vehicleRepository = vehicleRepository;
    }


    #region public methods 
    [AbpAllowAnonymous]
    [RemoteService(IsEnabled = false)]
    public async Task<PaymentCheckoutDto> CreateCheckoutPaymentAsync(PaymentCheckoutRequestDto paymentCheckoutRequest)
    {

        using (dataFilter.Disable<IMultiTenant>())
        {
          
                // handle cash transaction
            if (paymentCheckoutRequest.PaymentMethod == PaymentMethod.Cash)
            {

                var result = await SavePaymentRequest(paymentCheckoutRequest, PaymentStatus.Success, paymentCheckoutRequest.CustomerId);
                await UpdatePaymentStausByCheckOutIdAsync(result.CheckoutId);
                var entity = await _repository.FirstOrDefaultAsync(p => p.CheckoutId == result.CheckoutId);
                result.QrCodeInvoice = entity!.QrCodeInvoice;
                return result;
            }
            else
            {

                var entityId = GetEntityIdByPaymentMethod(paymentCheckoutRequest.PaymentMethod);
                var checkoutId = await HyperPayPayment(paymentCheckoutRequest, entityId);
                return await SavePaymentRequest(paymentCheckoutRequest, PaymentStatus.Pending, paymentCheckoutRequest.CustomerId,
                    checkoutid: checkoutId, entityId: entityId);
            }
           

        }
    }
  
    [RemoteService(IsEnabled = false)]
    public async Task<bool> ResendZatca(Guid Id)
    {
        try
        {             
            var payment = await _repository.FirstOrDefaultAsync(p => p.Id == Id);
            if (payment == null)
                return false;
            var res = await _zatcaTaxInvoiceAppService.SendToZACTA(payment.Id);
            payment.QrCodeInvoice = res.invoiceQRCode;
            payment.IsSentZatca = res.status;
            payment.ErrorMessagesZatca = null;
            if (res.validationResults != null && res.validationResults.errorMessages != null && res.validationResults.errorMessages.Count > 0)
            {
                var listErrors = res.validationResults.errorMessages.Select(e => e.Message).ToList();
                payment.ErrorMessagesZatca = string.Join(",", listErrors);
            }
            else if (res.validationResults != null && res.validationResults.warningMessages != null && res.validationResults.warningMessages.Count > 0)
            {
                var listErrors = res.validationResults.warningMessages.Select(e => e.Message).ToList();
                payment.ErrorMessagesZatca = string.Join(",", listErrors);
            }
            await _repository.UpdateAsync(payment);

            return res.status;
        }
        catch (Exception ex)
        {
            var x = ex.Message;
            throw;
        }
    }
    [RemoteService(IsEnabled = false)]
    public async Task<Result<PaymentPackageDto>> GetPackagePaymentDetailsAsync(Guid PayemntId)
    {

        using (dataFilter.Disable<IMultiTenant>())
        {
            var payement = await _repository.GetAsync(x => x.Id == PayemntId);
            if (payement is null || payement.CustomerPackage is null)
                return Result.Success(new PaymentPackageDto());
            var packagedto = new PaymentPackageDto()
            {
                PaymentId = PayemntId,
                ExpiryDateOfPackage = DateOnly.FromDateTime(payement.CustomerPackage.ExpiryDate),
                ExpiryTimeOfPackage = TimeOnly.FromDateTime(payement.CustomerPackage.ExpiryDate),
                PaymentNumber = payement.PaymentNumber,
                OrganizationAddress = payement.CustomerPackage!.Organization.Address,
                OrganizationArabicName = payement.CustomerPackage!.Organization.ArabicName,
                OrganizationEnglishName = payement.CustomerPackage!.Organization.EnglishName,
                CustomerArabicName = payement.Customers.ArabicName,
                CustomerEnglishName = payement.Customers.IdentityUser.Name,
                PaymentTime = payement.CreationTime,
                TotalAmount = payement.Amount,
                Price = payement.CustomerPackage!.PriceOfPackage.Value,
                Discount = payement.CustomerPackage!.Discount,
                Vat = payement.CustomerPackage!.Vat,
                Description = payement.CustomerPackage.DescriptionOfPackage,
                ActivationDate = DateOnly.FromDateTime(payement.CustomerPackage.ActivationDateOfPackage.Value),
                ActivationTime = TimeOnly.FromDateTime(payement.CustomerPackage.ActivationDateOfPackage.Value) ,
                PackageCode = payement.CustomerPackage.PackageCode,
                QRCodePath=payement.CustomerPackage.QRCodePath,
                ArabicName=payement.CustomerPackage.PackageArabicName,
                EnglishName=payement.CustomerPackage.PackageEnglishName,
                ArPlateNumber=payement.EnPlateNumber,
                EnPlateNumber=payement.ArPlateNumber,
                
            };
            return Result<PaymentPackageDto>.Success(packagedto, "Package Details Retrieved Succesuffuly");
        }
    }
    [RemoteService(IsEnabled = false)]
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
    [RemoteService(IsEnabled = false)]
    public async Task<PaymentCheckoutDto?> CreateCheckoutInvoicePaymentWebAsync(PaymentCheckoutRequestDto paymentCheckoutRequest)
    {
        var paymentCheckoutDto = await SavePaymentRequest(paymentCheckoutRequest, PaymentStatus.Success, paymentCheckoutRequest.CustomerId);
        await UpdateInvoiceStatusAsync(paymentCheckoutRequest.InvoiceId,paymentCheckoutDto.PaymentNumber);
        return paymentCheckoutDto;
    }
    [RemoteService(IsEnabled = false)]
    public async Task<PaymentCheckoutDto?> CreateCheckoutPackagePaymentWebAsync(PaymentCheckoutRequestDto paymentCheckoutRequest)
    {
        var paymentCheckoutDto = await SavePaymentRequest(paymentCheckoutRequest, PaymentStatus.Success, paymentCheckoutRequest.CustomerId);
        return paymentCheckoutDto;
    }
    [AbpAllowAnonymous]
    [RemoteService(IsEnabled = false)]
    public async Task<Result<PaymentCheckoutDto>> PayCustomerInvoiceAsync(PayCustomerInvoiceDto payCustomerInvoiceDto)
    {
        using (dataFilter.Disable<IMultiTenant>())
        {
            var result= await GetInvoiceByIdAsync(payCustomerInvoiceDto.InvoiceId);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("PayCustomerInvoiceAsync: {Error}", result.Errors.FirstOrDefault());
                throw new Exception (result.Errors.FirstOrDefault());
            }
            var paymentResult = await CreateCheckoutPaymentAsync(new PaymentCheckoutRequestDto
            {
                InvoiceId = payCustomerInvoiceDto.InvoiceId,
                OrganizationId = result.Value.OrganizationId,
                Amount= PaymentUtilityCalculationts.GetPriceForPayment(result.Value.InvoicePrice, result.Value.Discount),
                PaymentCategory=PaymentCategory.PaymentParking,
                PaymentMethod= payCustomerInvoiceDto.Method,
                TenantId=result.Value.TenantId.Value,
                
            });
            await UpdateInvoiceStatusAsync(payCustomerInvoiceDto.InvoiceId, paymentResult.PaymentNumber);
            _logger.LogInformation("PayCustomerInvoiceAsync: Payment processed successfully for InvoiceId {InvoiceId}", payCustomerInvoiceDto.InvoiceId);
            return paymentResult;

        }
    }
    
    [AbpAllowAnonymous]
    [RemoteService(IsEnabled = false)]
    public async Task<Result<MobilePaymentSessionDto>> InitializeMobilePaymentSessionAsync(int invoiceId, PaymentMethod paymentMethod)
    {
        using (dataFilter.Disable<IMultiTenant>())
        {
            var invoiceResult = await GetInvoiceByIdAsync(invoiceId);
            if (!invoiceResult.IsSuccess)
            {
                _logger.LogWarning("InitializeMobilePaymentSessionAsync: {Error}", invoiceResult.Errors.FirstOrDefault());
                throw new Exception(invoiceResult.Errors.FirstOrDefault());
            }

            var invoice = invoiceResult.Value;
            var amount = PaymentUtilityCalculationts.GetPriceForPayment(invoice.InvoicePrice, invoice.Discount);

            var gatewayService = LazyServiceProvider.LazyGetRequiredService<IPaymentGatewayIntegrationService>();
            
            var request = invoice.ToPaymentCheckoutRequestDto(
                amount, 
                paymentMethod, 
                PaymentCallbackHelper.ResolveCallbackUrl(ServiceProvider, GetBaseUri(), gatewayService.Gateway));

            var initiateResult = await gatewayService.InitiateAsync(request);

            var sessionDto = initiateResult.ToMobilePaymentSessionDto();

            var payment = await _repository.FirstOrDefaultAsync(p => p.PaymentNumber == sessionDto.MerchantReferenceId);
            if (payment != null)
            {
                sessionDto.PaymentId = payment.Id;
            }

            _logger.LogInformation("InitializeMobilePaymentSessionAsync: Session initialized for InvoiceId {InvoiceId}, PaymentNumber {PaymentNumber}", invoiceId, sessionDto.MerchantReferenceId);
            return Result<MobilePaymentSessionDto>.Success(sessionDto, "Payment session initialized successfully");
        }
    }

    [RemoteService(IsEnabled = false)]
    public async Task<Result<List<PaymentPackageDto>>> GetAllCustomerPackgePaymentsAsync(int organizationId)
    {
        using (dataFilter.Disable<IMultiTenant>())
        {

            var data = await GetPaymentPackageDtosAsync(organizationId);

            if (data == null || !data.Any())
            {
                return Result<List<PaymentPackageDto>>.Success(new List<PaymentPackageDto>(), "No Payments Found");
            }
            


            //var paymentPackageDtos = ObjectMapper.Map<List<Payment>, List<PaymentPackageDto>>(result);
            return Result<List<PaymentPackageDto>>.Success(data, "Payments Retrieved Successfully");
        }
    }
    [AbpAllowAnonymous]
    [RemoteService(IsEnabled = false)]
    private async Task<Result<Invoice>> GetInvoiceByIdAsync(int invoiceId)
    {
        var invoice = await _invoiceRepo.FirstOrDefaultAsync(inv => inv.Id == invoiceId);
        if (invoice == null)
        {
            return Result.Error("Invoice not found");
        }
        if (invoice.InvoiceType == InvoiceType.Paid || invoice.InvoicePrice ==0)
        {
            return Result.Error("Invoice already paid");

        }
        return invoice;
    }

    [RemoteService(IsEnabled = false)]
    public async Task<PaymentCheckoutDto> SavePaymentRequest(PaymentCheckoutRequestDto paymentCheckoutRequest,
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
            CustomerPackageId = paymentCheckoutRequest.CustomerPackageId,
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
            TenantId = CurrentTenant.Id.HasValue ? CurrentTenant.Id.Value : paymentCheckoutRequest.TenantId,

        };

        

        var PaymentNumberExist = await _repository.FirstOrDefaultAsync(p=> p.PaymentNumber == payment.PaymentNumber);
        if(PaymentNumberExist != null)
        {
            _logger.LogError("This Payment Number Is already Exist with payment id : {PaymentId}", payment.Id);
            throw new Exception("This Payment Number Is already Exist");
        }

        payment = await _repository.InsertAsync(payment);
        _logger.LogInformation("Payment created with ID: {PaymentId}", payment.Id);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return new PaymentCheckoutDto
        {
            CheckoutId = payment.CheckoutId,
            PaymentId = payment.Id,
            PaymentStatus = payment.PaymentStatus,
            PaymentMethod = payment.PaymentMethod,
            EntityId = payment.EntityId,
            TokenId = token,
            QrCodeInvoice = null,
            PaymentNumber = payment.PaymentNumber,
        };
    }

    [RemoteService(IsEnabled = false)]
    public async Task<Guid> UpdatePaymentStausByCheckOutIdAsync(string checkoutid)
    {
        var payment = await _repository.FirstOrDefaultAsync(p => p.CheckoutId == checkoutid);
        if (payment == null)
        {
            throw new Exception("Payment not found.");
        }
        var invoice = await _invoiceRepo.FirstOrDefaultAsync(i => i.Id == payment.InvoiceId);
        if (invoice == null)
        {
            throw new Exception("Invoice not found.");
        }
        invoice.InvoiceType = InvoiceType.Paid;
        invoice.InvoiceNumber = payment.PaymentNumber;
        payment.PaymentStatus = PaymentStatus.Success;
        await CallingZatcaIntegration(payment);
        return payment.Id;
    }

    public async Task<Guid> UpdatePaymentStausByPaymentNumberAsync(string paymentNumber, string checkoutid, PaymentMethod paymentMethod)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var payment = await _repository.FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber);
            if (payment == null)
            {
                throw new Exception("Payment not found.");
            }
            var invoice = await _invoiceRepo.FirstOrDefaultAsync(i => i.Id == payment.InvoiceId);
            if (invoice == null)
            {
                throw new Exception("Invoice not found.");
            }
            invoice.InvoiceNumber = payment.PaymentNumber;
            payment.CheckoutId = checkoutid;
            payment.PaymentMethod = paymentMethod;
            return payment.Id;
        }
    }

    private async Task CallingZatcaIntegration(Payment payment)
    {
        QRCodeDto qrCode = new QRCodeDto();
        var organization = await organizationsAppService.GetAsync(payment.OrganizationId);
        if (organization != null && organization.StatusZatca == Status.Active && payment.Amount > 0)
        {
            var res = await _zatcaTaxInvoiceAppService.SendToZACTA(payment.Id);
            payment.QrCodeInvoice = qrCode.QRCode = res.invoiceQRCode;
            payment.IsSentZatca = res.status;
            payment.ErrorMessagesZatca = null;
            if (res.validationResults != null && res.validationResults.errorMessages != null && res.validationResults.errorMessages.Count > 0)
            {
                var listErrors = res.validationResults.errorMessages.Select(e => e.Message).ToList();
                payment.ErrorMessagesZatca = string.Join(",", listErrors);
            }
            else if (res.validationResults != null && res.validationResults.warningMessages != null && res.validationResults.warningMessages.Count > 0)
            {
                var listErrors = res.validationResults.warningMessages.Select(e => e.Message).ToList();
                payment.ErrorMessagesZatca = string.Join(",", listErrors);
            }
        }
        else
        {
            qrCode = await _invoiceUtility.GetQrCodeAsync(payment.Amount, payment.OrganizationId);
            payment.QrCodeInvoice = qrCode.QRCode;
        }
        await CurrentUnitOfWork!.SaveChangesAsync();
    }
    #endregion
    #region private methods


    [RemoteService(IsEnabled = false)]
    private async Task<string> HyperPayPayment(PaymentCheckoutRequestDto paymentCheckoutRequest, string entityId)
    {
        var content = new StringContent(
          $"entityId={entityId}&amount={paymentCheckoutRequest.Amount}&currency={Currency}&testMode=EXTERNAL&paymentType={PaymentType}",
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
    [RemoteService(IsEnabled = false)]
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
    [AbpAllowAnonymous]
    [RemoteService(IsEnabled = false)]
    private async Task<Result<bool>> UpdateInvoiceStatusAsync(int? InvoiceId,string PaymnetNumber)
    {
        using (dataFilter.Disable<IMultiTenant>())
        {
            var invoice =await _invoiceRepo.FirstOrDefaultAsync(x => x.Id == InvoiceId);
            if (invoice == null) {  return Result<bool>.Success(false); }
            invoice.InvoiceType = InvoiceType.Paid;
            invoice.InvoiceNumber=PaymnetNumber;
            await _invoiceRepo.UpdateAsync(invoice);
            return Result.Success(true);

        }
    }

    [RemoteService(IsEnabled = false)]
    public async Task<string> GeneratePaymentNumberAsync(int organizationId)
    {
        using (dataFilter.Disable<IMultiTenant>())
        {
           var organization = await organizationsAppService.GetAsync(organizationId);

            var currentYearSuffix = DateTime.Now.Year % 100;  // Last 2 digits of the year
            var prefix = organization.PaymentCode ?? organization.EnglishName.Substring(0, 3).ToUpper();

            // Fetch the last invoice for the current year and organization

            var query = await _repository.GetQueryableAsync();

            query = query.Where(i => i.CreationTime.Year == DateTime.Now.Year && i.OrganizationId == organization.Id)
            .OrderByDescending(i => i.CreationTime);
            var lastInvoice=query.FirstOrDefault();


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
    [RemoteService(IsEnabled = false)]
    private async Task<List<PaymentPackageDto>> GetPaymentPackageDtosAsync(int organizationId)
    {
        using (dataFilter.Disable<IMultiTenant>())
        {
            var customers = await _customerRepo.GetQueryableAsync();
            var payments = await _repository.GetQueryableAsync();
            var customerPackages = await _customerPackageRepository.GetQueryableAsync();
            var organizations = await _organizationRepository.GetQueryableAsync();
            var vehicles = await _vehicleRepository.GetQueryableAsync();
            var customer = await _customerRepo.FirstOrDefaultAsync(_ => _.IdentityUserId == CurrentUser.Id);
            
            var result = 
                await (
                    from p in payments
                    join c in customers
                    on p.CustomerId equals c.Id
                    where c.Id == customer.Id

                    join o in organizations
                    on p.OrganizationId equals o.Id
                    where o.Id == organizationId

                    join cp in customerPackages
                    on p.CustomerPackageId equals cp.Id

                    join v in vehicles
                    on cp.ArPlateNumber equals v.ArPlateNumber

                    select new PaymentPackageDto
                    {
                        PaymentId = p.Id,
                        PaymentNumber = p.PaymentNumber,

                        ArabicName = cp.PackageArabicName,
                        EnglishName = cp.PackageEnglishName,
                        Description = cp.DescriptionOfPackage,

                        PaymentTime = p.CreationTime,

                        OrganizationArabicName = o.ArabicName,
                        OrganizationEnglishName = o.EnglishName,
                        OrganizationAddress = o.Address,

                        CustomerArabicName = c.ArabicName,
                        CustomerEnglishName = c.IdentityUser.Name,

                        EnPlateNumber = cp.EnPlateNumber,
                        ArPlateNumber = cp.ArPlateNumber,

                        ExpiryDateOfPackage = DateOnly.FromDateTime(cp.ExpiryDate),
                        ExpiryTimeOfPackage = TimeOnly.FromDateTime(cp.ExpiryDate),

                        ActivationDate = cp.ActivationDateOfPackage.HasValue
                                ? DateOnly.FromDateTime(cp.ActivationDateOfPackage.Value)
                                : default,

                        ActivationTime = cp.ActivationDateOfPackage.HasValue
                                ? TimeOnly.FromDateTime(cp.ActivationDateOfPackage.Value)
                                : default,

                        PackageCode = cp.PackageCode,
                        QRCodePath = cp.QRCodePath,

                        TotalAmount = p.Amount,
                        Price = cp.PriceOfPackage ?? 0,
                        Discount = cp.Discount,
                        Vat = cp.Vat,

                        LicenseTypeImage = v != null ? v.LicenceType.Image : null,
                        VehicleModelImage = v != null ? v.VehicleModel.Image : null
                    }

                ).ToListAsync();


            return result;
        }
    }

    private string GetBaseUri()
    {
        var httpContextAccessor = LazyServiceProvider.LazyGetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var request = httpContextAccessor?.HttpContext?.Request;

        if (request != null)
        {
            return $"{request.Scheme}://{request.Host}{request.PathBase}".TrimEnd('/');
        }
        
        var configuration = LazyServiceProvider.LazyGetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        return configuration["App:SelfUrl"]?.TrimEnd('/') ?? string.Empty;
    }

    #endregion
}