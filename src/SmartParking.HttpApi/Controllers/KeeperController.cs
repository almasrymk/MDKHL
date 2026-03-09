using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Authentication;
using SmartParking.Discounts;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Mobile.Invoices;
using SmartParking.Mobile.Keepers;
using SmartParking.OpeningGate;
using SmartParking.Organizations;
using SmartParking.Results;
using SmartParking.Shifts;
using SmartParking.Utilities;
using SmartParking.Watchers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace SmartParking.Controllers;

[Route("api/Keeper")]
[ApiController]

public class KeeperController : AbpController
{
    private readonly IKeeperAppService _keeperAppService;
    private readonly IAuthenticationAppService _authenticationAppService;
    private readonly IUsersAppService _usersAppService;
    private readonly ICurrentUser _currentUser;
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly IKeeperTransactionsAppService _keeperTransactionsAppService;
    private readonly IGarageTransactionsAppService _garageTransactionsAppService;
    private readonly IDiscountAppService _discountAppService;
    private readonly IShiftZoneActionAppService _shiftZoneActionAppService;

    private readonly IWatcherAppService _watcherAppService;
    public KeeperController(IKeeperAppService keeperAppService,
        IAuthenticationAppService authenticationAppService,
        ICurrentUser currentUser,
        IInvoiceAppService invoiceAppService,
        IDiscountAppService discountAppService,
        IUsersAppService usersAppService,
        IGarageTransactionsAppService garageTransactionsAppService = null,
        IKeeperTransactionsAppService keeperTransactionsAppService = null,
        IWatcherAppService watcherAppService = null,
        IShiftZoneActionAppService shiftZoneActionAppService = null)
    {
        _keeperAppService = keeperAppService;
        _authenticationAppService = authenticationAppService;
        _currentUser = currentUser;
        _invoiceAppService = invoiceAppService;
        _usersAppService = usersAppService;
        _garageTransactionsAppService = garageTransactionsAppService;
        _discountAppService = discountAppService;
        _keeperTransactionsAppService = keeperTransactionsAppService;
        _watcherAppService = watcherAppService;
        _shiftZoneActionAppService = shiftZoneActionAppService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]

    public async Task<Result<LoginResponseDto>> Login(LoginRequestDto input)
    {
        try
        {
            
            if (_currentUser.IsAuthenticated)
            {
                return Result<LoginResponseDto>.Invalid(new ValidationError
                {
                    
                    ErrorCode = "400",
                    ErrorMessage = "You are already logged in."
                });
            }
            return await _keeperAppService.LoginAsync(input);
        }
        catch
        {
            return Result<LoginResponseDto>.Invalid(new Results.ValidationError
            {
                ErrorCode = "403",
                ErrorMessage = "This user not found",
            });
        }
    }

    [HttpGet("getgatesforcurrentkeepers")]
    public async Task<Result<ICollection<ShiftWatcherActionDto>>> GetShiftWatcherActionAsync()
    {
        return await _keeperAppService.GetShiftWatcherActionAsync();
    }
    [HttpGet("getkeepersassignedgates")]
    public async Task<Result<ICollection<ShiftWatcherActionDto>>> GetKeepersAssignedGatesAsync()
    {
        return await _shiftZoneActionAppService.GetShiftZoneDevicesDataAsync();
    }

    [HttpPost("Mdkhl/login")]

    public async Task<Result<LoginAppResultDto>> LoginMdkhlKeeper(LoginMobileInputDto input)
    {
        try
        {
            
            if (_currentUser.IsAuthenticated)
            {
                return Result<LoginAppResultDto>.Invalid(new ValidationError
                {
                    
                    ErrorCode = "400",
                    ErrorMessage = "You are already logged in."
                });
            }
            return await _authenticationAppService.LoginAppAsync(input);
        }
        catch
        {
            return Result<LoginAppResultDto>.Invalid(new Results.ValidationError
            {
                ErrorCode = "403",
                ErrorMessage = "This user not found",
            });
        }
    }

    [HttpPost("invoice")]
    [Authorize]
    public async Task<Result<PagedResultDto<InvoiceDto>>> GetAllInvoicesByOrganizationId([FromQuery] int organizationId , PagedAndSortedAndSearchResultRequestDto paged)
    {
        return await _keeperAppService.GetAllInvoicesByOrganizationIdAsync(organizationId, paged);
    }    
    
    [HttpGet("paid/invoice")]
    [Authorize]
    public async Task<Result<List<InvoiceDto>>> GetAllInvoicesByOrganizationPaidId([FromQuery] int organizationId)
    {
        return await _keeperAppService.GetAllInvoicesByOrganizationIdPaidAsync(organizationId);
    }
    [HttpGet("lastinvoice")]
    [Authorize]
    public async Task<Result<InvoiceDto>> GetLastInvoiceByOrganizationId([FromQuery] int organizationId)
    {
        return await _keeperAppService.GetLastInvoiceByOrganizationIdAsync(organizationId);
    }

    [HttpGet("garageTransactionWithoutPlateNumber")]
    [Authorize]
    public async Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsWithNoPlateNumber([FromQuery] int organizationId, Guid userId)
    {
        return await _keeperAppService.GetAllGarageTransactionsWithNoPlateNumberAsync(organizationId, userId);
    }

    [HttpPost("garageTransaction")]
    [Authorize]
    public async Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllGarageTransactions([FromQuery] int organizationId, Guid userId , PagedAndSortedAndSearchResultRequestDto paged)
    {
        return await _keeperAppService.GetAllGarageTransactionsAsync(organizationId, userId , paged);
    }

    [HttpPut("correctTransaction")]
    [Authorize]
    public async Task<Result<LoginResponseDto>> CorrectTransactionByPlateNumber([FromQuery] int garageTransactionId, string plateNumber)
    {
        return await _keeperAppService.CorrectTransactionByPlateNumberAsync(garageTransactionId, plateNumber);
    }

    [HttpPost("existingTransaction")]
    [Authorize]
    public async Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllExistingTransactions([FromQuery] int organizationId, Guid userId , PagedAndSortedAndSearchResultRequestDto paged)
    {
        return await _keeperAppService.GetAllExistingTransactionsAsync(organizationId, userId , paged);
    }

    [HttpGet("PayAtExitCamera")]
    [Authorize]
    public async Task<Result<InvoiceMobileDto>> PayInvoice(int InvoiceId, decimal DiscountValue, int paymentMethodType)
    {
        var invoice = await _keeperAppService.CheckoutAsync(InvoiceId, paymentMethodType, DiscountValue);
        var gt = await _garageTransactionsAppService.GetAsync(invoice.Value.GarageTransactionExitId);
        string code = gt.LaneNO;
        if (!string.IsNullOrEmpty(gt.LaneNO))
        {
            code =await _watcherAppService.GetWatcherCodeByWatcherIdAsync(gt.CameraId.Value);
        }
        await _keeperTransactionsAppService.OpenGate(new OpenGateDto
        {
            WatcherCode = code ,
            OrganizationId = invoice.Value.OrganizationId,
        });
        return invoice;
    }
    [HttpPut("cancelinvoiceandmapping")]
    [Authorize]
    public async Task<Result<bool>> RemoveInvoiceAndUnMappEntryTransaction(int InvoiceId,int ExitTransactionId)
    {
        try
        {
            await _invoiceAppService.DeleteAsync(InvoiceId);
            await _garageTransactionsAppService.DeleteAsync(ExitTransactionId);
            return Result.Success();
        }
        catch (Exception ex) {
            return Result<bool>.Invalid(new ValidationError
            {
                ErrorCode = "404",
                ErrorMessage = ex.Message
            });

        }
    }

    [HttpGet("GetUnpaidInvoicesForPlateNumber")]
    [Authorize]
    public async Task<Result<List<InvoiceDto>>> GetUnpaidInvoicesForPlateNumber(int OrganizationId, string PlateNumber)
    {
        return await _invoiceAppService.GetUnpaidInvoiceForKeeperInHisOrganization(OrganizationId, PlateNumber);
    }

    [HttpGet("PayInvoiceByCustomerRequest")]
    [Authorize]
    public async Task<Result<InvoiceMobileDto>> PayInvoiceByCustomerRequest(int InvoiceId, string PaymentType, decimal DiscountValue)
    {
        return await _keeperAppService.PayCashForCustomerRequestAsync(InvoiceId, PaymentType, DiscountValue);

    }
    [HttpGet("KeeperCheckout")]
    [Authorize]
    public async Task<Result<InvoiceMobileDto>> KeeperCheckoutForTransaction(int transactionId, string PaymentType,decimal DiscountValue)
    {
        var invoice = await _keeperTransactionsAppService.Checkout(transactionId);
        return await _keeperAppService.PayCashForCustomerRequestAsync(invoice.Value.Id, PaymentType, DiscountValue);
    }


    [HttpGet("KeeperPreCheckout")]
    [Authorize]
    public async Task<Result<CreateUpdateInvoiceDto>> KeeperPreCheckoutForTransaction(int transactionId)
    {
        var invoice = await _keeperTransactionsAppService.PreCheckout(transactionId);
        return invoice;
    }

    [HttpPost("changeImage")]
    [Authorize]
    [IgnoreAntiforgeryToken]
    public async Task<Result<string>> ChangeImage([FromBody] string image)
    {
        var user = await _usersAppService.UpdateImageAsync(_currentUser.Id, image);
        return Result.Success(user.Image);
    }

    [HttpGet("getAllDiscount")]
    [Authorize]
    public async Task<Result<List<DiscountDto>>> GetAllDiscount()
    {
        return await _discountAppService.GetListMobileAsync();
    }

    [HttpGet("getpaymentbyinvoiceid")]
    [Authorize]
    public async Task<Result<string>> GetPaymentQrCodeByInvoiceId([FromQuery]int InvoiceId)
    {
        return await _keeperAppService.GetPaymentQrCodeByInvoiceId(InvoiceId);
    }

}
