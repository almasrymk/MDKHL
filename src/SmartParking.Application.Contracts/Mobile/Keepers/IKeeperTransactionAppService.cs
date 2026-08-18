using SmartParking.GarageTransactions;
using SmartParking.Mobile.Keepers.Dtos;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Mobile.Keepers;

public interface IKeeperTransactionAppService : IApplicationService
{
    /// <summary>
    /// Retrieves all garage transactions with missing plate numbers for a keeper's organization.
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <param name="userId">Keeper user ID.</param>
    /// <returns>A list of <see cref="GarageTransactionDto"/> without plate numbers.</returns>
    Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsWithNoPlateNumberAsync(int organizationId, Guid userId);

    /// <summary>
    /// Retrieves a paged list of garage transactions for a keeper's organization.
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <param name="userId">Keeper user ID.</param>
    /// <param name="paged">Pagination and search parameters.</param>
    /// <returns>Paged result of <see cref="GarageTransactionDto"/>.</returns>
    Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllGarageTransactionsAsync(int organizationId, Guid userId, PagedAndSortedAndSearchResultRequestDto paged);

    /// <summary>
    /// Updates the plate number of a specific garage transaction.
    /// </summary>
    /// <param name="garageTransactionId">ID of the transaction to update.</param>
    /// <param name="plateNumber">Corrected plate number.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> CorrectTransactionByPlateNumberAsync(int garageTransactionId, string plateNumber);

    /// <summary>
    /// Retrieves a paged list of active in-garage transactions (IsIn == true).
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <param name="userId">Keeper user ID.</param>
    /// <param name="paged">Pagination and search parameters.</param>
    /// <returns>Paged result of active <see cref="GarageTransactionDto"/>.</returns>
    Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllExistingTransactionsAsync(int organizationId, Guid userId, PagedAndSortedAndSearchResultRequestDto paged);

    /// <summary>
    /// Retrieves the latest unlinked camera snapshot transaction for a selected watcher/camera device.
    /// </summary>
    /// <param name="organizationId">The organization ID.</param>
    /// <param name="watcherCode">The watcher/device code.</param>
    /// <returns>A Result containing the <see cref="UnlinkedTransactionViewDto"/>.</returns>
    Task<Result<UnlinkedTransactionViewDto>> GetLatestUnlinkedTransactionByWatcherCodeAsync(int organizationId, string watcherCode);

    /// <summary>
    /// Converts a pending unlinked transaction snapshot into an active ENTRY GarageTransaction and optionally triggers the barrier.
    /// </summary>
    /// <param name="input">The recovery input data.</param>
    /// <returns>A Result containing the created <see cref="GarageTransactionDto"/>.</returns>
    Task<Result<GarageTransactionDto>> SendUnlinkedTransactionAsEntryAsync(SendUnlinkedAsEntryDto input);
}
