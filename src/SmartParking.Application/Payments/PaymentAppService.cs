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
    private readonly IOrganizationsAppService organizationsAppService;
    private readonly IRepository<Invoice, int> _invoiceRepo;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly IInvoiceUtilityAppService _invoiceUtility;
    private readonly IZatcaTaxInvoiceAppService _zatcaTaxInvoiceAppService;
    private readonly ILogger<PaymentAppService> _logger;
    private readonly IRepository<CustomerPackage, int> _customerPackageRepository;
    private readonly IRepository<Vehicle, int> _vehicleRepository;
    private const string Currency = "SAR";
    private const string token = "OGFjN2E0Y2E5MjQ0ZjUzYzAxOTI0NzNkMjBlNDAzMTV8NXJDQjdzYUpQN2U3U1lrWiU6QjQ=";

    public PaymentAppService(IRepository<Payment, Guid> repository,
        IRepository<Invoice, int> invoiceRepo, 
        IRepository<Customer, Guid> customerRepo, 
        IDataFilter dataFilter,
        IOrganizationsAppService _organizationsAppService,
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
        organizationsAppService = _organizationsAppService;
        _invoiceRepo = invoiceRepo;
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
            var paymentInitializationService = LazyServiceProvider.LazyGetRequiredService<IPaymentInitializationService>();
            var result = await paymentInitializationService.InitializeAsync(PaymentCategory.PaymentParking, invoiceId, paymentMethod);
            if (!result.IsSuccess)
            {
                throw new Exception(result.Errors.FirstOrDefault());
            }
            return result;
        }
    }

    [AbpAllowAnonymous]
    [RemoteService(IsEnabled = false)]
    public async Task<Result<MobilePaymentSessionDto>> InitializePackagePaymentSessionAsync(int packageId, PaymentMethod paymentMethod)
    {
        using (dataFilter.Disable<IMultiTenant>())
        {
            var paymentInitializationService = LazyServiceProvider.LazyGetRequiredService<IPaymentInitializationService>();
            var result = await paymentInitializationService.InitializeAsync(PaymentCategory.Package, packageId, paymentMethod);
            if (!result.IsSuccess)
            {
                throw new Exception(result.Errors.FirstOrDefault());
            }
            return result;
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


    [RemoteService(IsEnabled = false)]
    public async Task<Guid> LinkCheckoutIdToPaymentAsync(string paymentNumber, string checkoutId, PaymentMethod paymentMethod)
    {
        using (dataFilter.Disable<IMultiTenant>())
        {
            var payment = await _repository.FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber);
            if (payment == null)
            {
                throw new Exception("Payment not found.");
            }
            payment.CheckoutId = checkoutId;
            payment.PaymentMethod = paymentMethod;
            await _repository.UpdateAsync(payment);
            return payment.Id;
        }
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