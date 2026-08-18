using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Authentication;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Mobile.Invoices;
using SmartParking.Mobile.Keepers;
using SmartParking.Mobile.Keepers.Dtos;
using SmartParking.OpeningGate;
using SmartParking.Permissions.UnlinkedEntryPermissions;
using SmartParking.Results;
using SmartParking.Shifts;
using SmartParking.Watchers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Users;

namespace SmartParking.Controllers;

[Route("api/Keeper")]
[ApiController]
public class KeeperController : AbpController
{
    private readonly IKeeperAuthenticationAppService _keeperAuthenticationAppService;
    private readonly IKeeperInvoiceAppService _keeperInvoiceAppService;
    private readonly IKeeperTransactionAppService _keeperTransactionAppService;
    private readonly IShiftZoneActionAppService _shiftZoneActionAppService;
    private readonly IAuthenticationAppService _authenticationAppService;
    private readonly IUsersAppService _usersAppService;
    private readonly ICurrentUser _currentUser;
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly IKeeperTransactionsAppService _keeperTransactionsAppService;
    private readonly IGarageTransactionsAppService _garageTransactionsAppService;
    private readonly IWatcherAppService _watcherAppService;

    public KeeperController(
        IKeeperAuthenticationAppService keeperAuthenticationAppService,
        IKeeperInvoiceAppService keeperInvoiceAppService,
        IKeeperTransactionAppService keeperTransactionAppService,
        IShiftZoneActionAppService shiftZoneActionAppService,
        IAuthenticationAppService authenticationAppService,
        ICurrentUser currentUser,
        IInvoiceAppService invoiceAppService,
        IUsersAppService usersAppService,
        IGarageTransactionsAppService garageTransactionsAppService = null,
        IKeeperTransactionsAppService keeperTransactionsAppService = null,
        IWatcherAppService watcherAppService = null)
    {
        _keeperAuthenticationAppService = keeperAuthenticationAppService;
        _keeperInvoiceAppService = keeperInvoiceAppService;
        _keeperTransactionAppService = keeperTransactionAppService;
        _shiftZoneActionAppService = shiftZoneActionAppService;
        _authenticationAppService = authenticationAppService;
        _currentUser = currentUser;
        _invoiceAppService = invoiceAppService;
        _usersAppService = usersAppService;
        _garageTransactionsAppService = garageTransactionsAppService;
        _keeperTransactionsAppService = keeperTransactionsAppService;
        _watcherAppService = watcherAppService;
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
            return await _keeperAuthenticationAppService.LoginAsync(input);
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
        var result = await _shiftZoneActionAppService.GetShiftZoneDevicesDataAsync();
        if (result == null)
        {
            return Result<ICollection<ShiftWatcherActionDto>>.Success(new List<ShiftWatcherActionDto>());
        }
        return Result<ICollection<ShiftWatcherActionDto>>.Success(result.Value);
    }

    [HttpGet("hasactiveshiftzoneaction")]
    public async Task<Result<bool>> HasActiveShiftZoneActionAsync()
    {
        return await _shiftZoneActionAppService.GetCurrentUserActiveShiftZoneStatusAsync();
    }

    [HttpPost("Mdkhl/login")]
    public async Task<Result<LoginKeeperAppResponse>> LoginMdkhlKeeper(LoginMobileInputDto input)
    {
        try
        {
            if (_currentUser.IsAuthenticated)
            {
                return Result<LoginKeeperAppResponse>.Invalid(new ValidationError
                {
                    ErrorCode = "400",
                    ErrorMessage = "You are already logged in."
                });
            }
            return await _authenticationAppService.LoginAppAsync(input);
        }
        catch
        {
            return Result<LoginKeeperAppResponse>.Invalid(new Results.ValidationError
            {
                ErrorCode = "403",
                ErrorMessage = "This user not found",
            });
        }
    }

    [HttpPost("invoice")]
    [Authorize]
    public async Task<Result<PagedResultDto<InvoiceDto>>> GetAllInvoicesByOrganizationId([FromQuery] int organizationId, PagedAndSortedAndSearchResultRequestDto paged)
    {
        return await _keeperInvoiceAppService.GetAllInvoicesByOrganizationIdAsync(organizationId, paged);
    }

    [HttpGet("paid/invoice")]
    [Authorize]
    public async Task<Result<List<InvoiceDto>>> GetAllInvoicesByOrganizationPaidId([FromQuery] int organizationId)
    {
        return await _keeperInvoiceAppService.GetAllInvoicesByOrganizationIdPaidAsync(organizationId);
    }

    [HttpGet("lastinvoice")]
    [Authorize]
    public async Task<Result<InvoiceDto>> GetLastInvoiceByOrganizationId([FromQuery] int organizationId, [FromQuery] string watcherCode)
    {
        return await _keeperInvoiceAppService.GetLastInvoiceByOrganizationIdAndWatcherCodeAsync(organizationId, watcherCode);
    }

    [HttpGet("garageTransactionWithoutPlateNumber")]
    [Authorize]
    public async Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsWithNoPlateNumber([FromQuery] int organizationId, Guid userId)
    {
        return await _keeperTransactionAppService.GetAllGarageTransactionsWithNoPlateNumberAsync(organizationId, userId);
    }

    [HttpPost("garageTransaction")]
    [Authorize]
    public async Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllGarageTransactions([FromQuery] int organizationId, Guid userId, PagedAndSortedAndSearchResultRequestDto paged)
    {
        return await _keeperTransactionAppService.GetAllGarageTransactionsAsync(organizationId, userId, paged);
    }

    [HttpPut("correctTransaction")]
    [Authorize]
    public async Task<Result> CorrectTransactionByPlateNumber([FromQuery] int garageTransactionId, string plateNumber)
    {
        return await _keeperTransactionAppService.CorrectTransactionByPlateNumberAsync(garageTransactionId, plateNumber);
    }

    [HttpPost("existingTransaction")]
    [Authorize]
    public async Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllExistingTransactions([FromQuery] int organizationId, Guid userId, PagedAndSortedAndSearchResultRequestDto paged)
    {
        return await _keeperTransactionAppService.GetAllExistingTransactionsAsync(organizationId, userId, paged);
    }

    [HttpGet("PayAtExitCamera")]
    [Authorize]
    public async Task<Result<InvoiceMobileDto>> PayInvoice(int InvoiceId, decimal DiscountValue, int paymentMethodType)
    {
        var invoice = await _keeperInvoiceAppService.CheckoutAsync(InvoiceId, paymentMethodType, DiscountValue);
        var gt = await _garageTransactionsAppService.GetAsync(invoice.Value.GarageTransactionExitId);
        string code = gt.LaneNO;
        if (string.IsNullOrEmpty(gt.LaneNO))
        {
            code = await _watcherAppService.GetWatcherCodeByWatcherIdAsync(gt.CameraId.Value);
        }
        await _keeperTransactionsAppService.OpenGate(new OpenGateDto
        {
            WatcherCode = code,
            OrganizationId = invoice.Value.OrganizationId,
        });
        return invoice;
    }

    [HttpPut("cancelinvoiceandmapping")]
    [Authorize]
    public async Task<Result<bool>> RemoveInvoiceAndUnMappEntryTransaction(int InvoiceId, int ExitTransactionId)
    {
        try
        {
            await _invoiceAppService.DeleteAsync(InvoiceId);
            await _garageTransactionsAppService.DeleteAsync(ExitTransactionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
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
        return await _keeperInvoiceAppService.PayCashForCustomerRequestAsync(InvoiceId, PaymentType, DiscountValue);
    }

    [HttpGet("KeeperCheckout")]
    [Authorize]
    public async Task<Result<InvoiceMobileDto>> KeeperCheckoutForTransaction(int transactionId, string PaymentType, decimal DiscountValue)
    {
        var invoice = await _keeperTransactionsAppService.Checkout(transactionId);
        return await _keeperInvoiceAppService.PayCashForCustomerRequestAsync(invoice.Value.Id, PaymentType, DiscountValue);
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

    [HttpGet("getpaymentbyinvoiceid")]
    [Authorize]
    public async Task<Result<string>> GetPaymentQrCodeByInvoiceId([FromQuery] int InvoiceId)
    {
        return await _keeperInvoiceAppService.GetPaymentQrCodeByInvoiceId(InvoiceId);
    }

    [HttpGet("latest-unlinked")]
    [Authorize(UnlinkedEntryPermissions.View)]
    public async Task<Result<UnlinkedTransactionViewDto>> GetLatestUnlinkedTransaction([FromQuery] int organizationId, [FromQuery] string watcherCode)
    {
        return await _keeperTransactionAppService.GetLatestUnlinkedTransactionByWatcherCodeAsync(organizationId, watcherCode);
    }

    [HttpPost("send-unlinked-as-entry")]
    [Authorize(UnlinkedEntryPermissions.SendAsEntry)]
    public async Task<Result<GarageTransactionDto>> SendUnlinkedTransactionAsEntry([FromBody] SendUnlinkedAsEntryDto input)
    {
        return await _keeperTransactionAppService.SendUnlinkedTransactionAsEntryAsync(input);
    }
}
