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
using SmartParking.Enums.PosTerminal;
using SmartParking.Invoices;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.Payments.PaymentIntegrations;
using SmartParking.Results;
using SmartParking.Utilities;
using SmartParking.ZATCATaxInvoice;
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

using InvoiceDto = SmartParking.Invoices.InvoiceDto;
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
            payment.ErrorMessagesZatca = res.generalError?.ToString();
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
        var res = await _zatcaTaxInvoiceAppService.SendToZACTA(paymentCheckoutDto.PaymentId);
        if (res != null)
        {
            _logger.LogInformation("ZATCA Integration Result for PaymentId {PaymentId}: Status: {Status}, InvoiceQRCode: {InvoiceQRCode}", paymentCheckoutDto.PaymentId, res.status, res.invoiceQRCode);
            await UpdatePaymentForZatca(paymentCheckoutDto.PaymentId, res);
            await UpdateInvoiceStatusAsync(paymentCheckoutRequest.InvoiceId,paymentCheckoutDto.PaymentNumber);
        }
        return paymentCheckoutDto;
    }
    [RemoteService(IsEnabled = false)]
    public async Task<PaymentCheckoutDto?> CreateCheckoutPackagePaymentWebAsync(PaymentCheckoutRequestDto paymentCheckoutRequest)
    {
        var paymentCheckoutDto = await SavePaymentRequest(paymentCheckoutRequest, PaymentStatus.Success, paymentCheckoutRequest.CustomerId);
        var res = await _zatcaTaxInvoiceAppService.SendToZACTA(paymentCheckoutDto.PaymentId);
        if (res != null)
        {
            _logger.LogInformation("ZATCA Integration Result for PaymentId {PaymentId}: Status: {Status}, InvoiceQRCode: {InvoiceQRCode}", paymentCheckoutDto.PaymentId, res.status, res.invoiceQRCode);
            await UpdatePaymentForZatca(paymentCheckoutDto.PaymentId, res);
        }
        return paymentCheckoutDto;
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
            request.BaseURI= GetBaseUri();
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
            PaymentNumber = paymentCheckoutRequest.PaymentNumber,
            ArPlateNumber = paymentCheckoutRequest.ArPlateNumber,
            EnPlateNumber = paymentCheckoutRequest.EnPlateNumber,
            TenantId = CurrentTenant.Id.HasValue ? CurrentTenant.Id.Value : paymentCheckoutRequest.TenantId,
            IsSentZatca=false

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

    [RemoteService(IsEnabled = false)]
    public async Task<PaymentCheckoutDto> CreatePendingPosPaymentAsync(PaymentCheckoutRequestDto request, string? terminalId = null, System.Threading.CancellationToken cancellationToken = default)
    {
        request.PaymentMethod = PaymentMethod.POS;
        return await SavePaymentRequest(request, PaymentStatus.Pending, request.CustomerId, checkoutid: null, entityId: terminalId);
    }

    [RemoteService(IsEnabled = false)]
    public async Task<PaymentDto> CompletePosTerminalPaymentAsync(CompletePosTerminalPaymentDto input, System.Threading.CancellationToken cancellationToken = default)
    {
        var payment = await _repository.GetAsync(input.PaymentId, cancellationToken: cancellationToken);

        if (payment.PaymentStatus == PaymentStatus.Success || payment.PaymentStatus == PaymentStatus.Cancelled)
        {
            _logger.LogInformation("Payment {PaymentId} already processed as {Status}. Short-circuiting.", payment.Id, payment.PaymentStatus);
            return ObjectMapper.Map<Payment, PaymentDto>(payment);
        }

        var outcome = SamaResponseCodeMapping.Evaluate(input.SamaCode);
        
        var detail = PosTerminalTransactionDetail.Create(
            payment.Id,
            input.SamaCode,
            outcome,
            input.RawJsonResult,
            input.TerminalId,
            input.TransactionStan,
            input.Rrn,
            input.AuthCode,
            input.MaskedPan,
            input.CardScheme,
            payment.TenantId
        );

        var detailRepo = LazyServiceProvider.LazyGetRequiredService<Volo.Abp.Domain.Repositories.IRepository<PosTerminalTransactionDetail, Guid>>();
        await detailRepo.InsertAsync(detail, autoSave: false, cancellationToken: cancellationToken);

        if (outcome == SamaResponseOutcome.Approved)
        {
            payment.PaymentStatus = PaymentStatus.Success;
            await CallingZatcaIntegration(payment);
            await UpdateInvoiceStatusAsync(payment.InvoiceId, payment.PaymentNumber);
        }
        else
        {
            payment.PaymentStatus = PaymentStatus.Cancelled;
        }

        await _repository.UpdateAsync(payment, autoSave: false, cancellationToken: cancellationToken);
        await CurrentUnitOfWork!.SaveChangesAsync(cancellationToken);

        return ObjectMapper.Map<Payment, PaymentDto>(payment);
    }
    #endregion
    #region private methods


 
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

    private async Task UpdatePaymentForZatca(Guid paymentId , ResponseInvoice responseInvoice)
    {
        using(dataFilter.Disable<IMultiTenant>())
        {
            var payment = await _repository.FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null)
            {
                throw new Exception("Payment not found.");
            }
            payment.QrCodeInvoice = responseInvoice.invoiceQRCode;
            payment.IsSentZatca = responseInvoice.status;
            payment.ErrorMessagesZatca = null;
            if (responseInvoice.validationResults != null && responseInvoice.validationResults.errorMessages != null && responseInvoice.validationResults.errorMessages.Count > 0)
            {
                var listErrors = responseInvoice.validationResults.errorMessages.Select(e => e.Message).ToList();
                payment.ErrorMessagesZatca = string.Join(",", listErrors);
            }
            else if (responseInvoice.validationResults != null && responseInvoice.validationResults.warningMessages != null && responseInvoice.validationResults.warningMessages.Count > 0)
            {
                var listErrors = responseInvoice.validationResults.warningMessages.Select(e => e.Message).ToList();
                payment.ErrorMessagesZatca = string.Join(",", listErrors);
            }
            await _repository.UpdateAsync(payment);
        }
    }

    #endregion
}