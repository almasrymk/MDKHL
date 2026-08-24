using SmartParking.GarageTransactions;
using SmartParking.Mobile.Keepers.Dtos;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Mobile.Keepers;

[RemoteService(isEnabled: false)]
[Obsolete("KeeperTransactionAppService is deprecated. Use KeeperTransactionQueryAppService, KeeperTransactionCorrectionAppService, or UnlinkedTransactionRecoveryAppService instead.")]
public class KeeperTransactionAppService : SmartParkingAppService, IKeeperTransactionAppService
{
    private readonly IKeeperTransactionQueryAppService _queryAppService;
    private readonly IKeeperTransactionCorrectionAppService _correctionAppService;
    private readonly IUnlinkedTransactionRecoveryAppService _recoveryAppService;

    public KeeperTransactionAppService(
        IKeeperTransactionQueryAppService queryAppService,
        IKeeperTransactionCorrectionAppService correctionAppService,
        IUnlinkedTransactionRecoveryAppService recoveryAppService)
    {
        _queryAppService = queryAppService;
        _correctionAppService = correctionAppService;
        _recoveryAppService = recoveryAppService;
    }

    /// <inheritdoc/>
    public Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsWithNoPlateNumberAsync(int organizationId, Guid userId)
    {
        return _queryAppService.GetAllGarageTransactionsWithNoPlateNumberAsync(organizationId, userId);
    }

    /// <inheritdoc/>
    public Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllGarageTransactionsAsync(int organizationId, Guid userId, PagedAndSortedAndSearchResultRequestDto paged)
    {
        return _queryAppService.GetAllGarageTransactionsAsync(organizationId, userId, paged);
    }

    /// <inheritdoc/>
    public Task<Result> CorrectTransactionByPlateNumberAsync(int garageTransactionId, string plateNumber)
    {
        return _correctionAppService.CorrectTransactionByPlateNumberAsync(garageTransactionId, plateNumber);
    }

    /// <inheritdoc/>
    public Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllExistingTransactionsAsync(int organizationId, Guid userId, PagedAndSortedAndSearchResultRequestDto paged)
    {
        return _queryAppService.GetAllExistingTransactionsAsync(organizationId, userId, paged);
    }

    /// <inheritdoc/>
    public Task<Result<UnlinkedTransactionViewDto>> GetLatestUnlinkedTransactionByWatcherCodeAsync(int organizationId, string watcherCode)
    {
        return _recoveryAppService.GetLatestUnlinkedTransactionByWatcherCodeAsync(organizationId, watcherCode);
    }

    /// <inheritdoc/>
    public Task<Result<GarageTransactionDto>> SendUnlinkedTransactionAsEntryAsync(SendUnlinkedAsEntryDto input)
    {
        return _recoveryAppService.SendUnlinkedTransactionAsEntryAsync(input);
    }
}
