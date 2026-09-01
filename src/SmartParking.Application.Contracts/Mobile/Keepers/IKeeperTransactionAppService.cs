using SmartParking.GarageTransactions;
using SmartParking.Mobile.Keepers.Dtos;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Mobile.Keepers;

[Obsolete("IKeeperTransactionAppService is deprecated. Use IKeeperTransactionQueryAppService, IKeeperTransactionCorrectionAppService, or IUnlinkedTransactionRecoveryAppService instead.")]
public interface IKeeperTransactionAppService : IApplicationService
{
    Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsWithNoPlateNumberAsync(int organizationId, Guid userId);

    Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllGarageTransactionsAsync(int organizationId, Guid userId, PagedAndSortedAndSearchResultRequestDto paged);

    Task<Result> CorrectTransactionByPlateNumberAsync(int garageTransactionId, string plateNumber);

    Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllExistingTransactionsAsync(int organizationId, Guid userId, PagedAndSortedAndSearchResultRequestDto paged);

    Task<Result<UnlinkedTransactionViewDto>> GetLatestUnlinkedTransactionByWatcherCodeAsync(int organizationId, string watcherCode);

    Task<Result<GarageTransactionDto>> SendUnlinkedTransactionAsEntryAsync(SendUnlinkedAsEntryDto input);
}
