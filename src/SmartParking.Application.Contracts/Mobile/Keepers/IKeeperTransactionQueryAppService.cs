using SmartParking.GarageTransactions;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Mobile.Keepers;

public interface IKeeperTransactionQueryAppService : IApplicationService
{
    /// <summary>
    /// Retrieves all garage transactions with missing plate numbers for a keeper's organization.
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <param name="userId">Keeper user ID.</param>
    /// <returns>A list of <see cref="GarageTransactionDto"/> without plate numbers.</returns>
    Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsWithNoPlateNumberAsync(
        int organizationId, 
        Guid userId);

    /// <summary>
    /// Retrieves a paged list of garage transactions for a keeper's organization.
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <param name="userId">Keeper user ID.</param>
    /// <param name="paged">Pagination and search parameters.</param>
    /// <returns>Paged result of <see cref="GarageTransactionDto"/>.</returns>
    Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllGarageTransactionsAsync(
        int organizationId, 
        Guid userId, 
        PagedAndSortedAndSearchResultRequestDto paged);

    /// <summary>
    /// Retrieves a paged list of active in-garage transactions (IsIn == true).
    /// </summary>
    /// <param name="organizationId">Organization ID.</param>
    /// <param name="userId">Keeper user ID.</param>
    /// <param name="paged">Pagination and search parameters.</param>
    /// <returns>Paged result of active <see cref="GarageTransactionDto"/>.</returns>
    Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllExistingTransactionsAsync(
        int organizationId, 
        Guid userId, 
        PagedAndSortedAndSearchResultRequestDto paged);
}
