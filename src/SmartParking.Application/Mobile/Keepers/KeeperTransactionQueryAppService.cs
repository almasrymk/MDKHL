using SmartParking.Entities.GarageTransactions;
using SmartParking.GarageTransactions;
using SmartParking.Repositories;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Mobile.Keepers;

[RemoteService(isEnabled: false)]
public class KeeperTransactionQueryAppService : SmartParkingAppService, IKeeperTransactionQueryAppService
{
    private readonly IMyUserRepository _myUserRepository;
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepository;

    public KeeperTransactionQueryAppService(
        IMyUserRepository myUserRepository,
        IRepository<GarageTransaction, int> garageTransactionRepository)
    {
        _myUserRepository = myUserRepository;
        _garageTransactionRepository = garageTransactionRepository;
    }

    /// <inheritdoc/>
    public async Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsWithNoPlateNumberAsync(int organizationId, Guid userId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var user = await _myUserRepository.FindUserByOrganizationAsync(organizationId, userId);
            if (user is null)
            {
                return Result.Invalid(new ValidationError
                {
                    ErrorMessage = "User not found"
                });
            }

            var garageTransaction = await _garageTransactionRepository.GetListAsync(t => t.PlateNumber == null);
            var sortedTransaction = garageTransaction.OrderByDescending(t => t.AccurateTime).ToList();
            var mappedTransaction = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(sortedTransaction);

            if (mappedTransaction.Count < 0)
            {
                return Result<List<GarageTransactionDto>>.NotFound("Transaction not found");
            }

            return Result<List<GarageTransactionDto>>.Success(mappedTransaction);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllGarageTransactionsAsync(int organizationId, Guid userId, PagedAndSortedAndSearchResultRequestDto paged)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var user = await _myUserRepository.FindUserByOrganizationAsync(organizationId, userId);
            if (user is null)
            {
                return Result.Invalid(new ValidationError
                {
                    ErrorMessage = "User not found"
                });
            }

            var query = await _garageTransactionRepository.GetQueryableAsync();
            query = query.OrderByDescending(t => t.AccurateTime);

            var totalCount = await AsyncExecuter.CountAsync(query);
            query = query.Skip(paged.SkipCount).Take(paged.MaxResultCount);

            var garageTransactions = await AsyncExecuter.ToListAsync(query);
            var mappedTransaction = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(garageTransactions);

            if (totalCount == 0)
            {
                return Result<PagedResultDto<GarageTransactionDto>>.NotFound("Transaction not found");
            }

            var result = new PagedResultDto<GarageTransactionDto>(totalCount, mappedTransaction);
            return Result<PagedResultDto<GarageTransactionDto>>.Success(result);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllExistingTransactionsAsync(int organizationId, Guid userId, PagedAndSortedAndSearchResultRequestDto paged)
    {
        var validationResult = await CheckUserIsFound(organizationId, userId);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var existingTransactionsQuery = await _garageTransactionRepository.GetQueryableAsync();
        existingTransactionsQuery = existingTransactionsQuery.Where(t => t.IsIn).OrderByDescending(t => t.AccurateTime);

        var totalCount = await AsyncExecuter.CountAsync(existingTransactionsQuery);
        existingTransactionsQuery = existingTransactionsQuery.Skip(paged.SkipCount).Take(paged.MaxResultCount);

        var existingTransactions = await AsyncExecuter.ToListAsync(existingTransactionsQuery);
        var items = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(existingTransactions);
        var finalPaged = new PagedResultDto<GarageTransactionDto>(totalCount, items);

        return Result<PagedResultDto<GarageTransactionDto>>.Success(finalPaged);
    }

    #region Private Helpers

    private async Task<Result<PagedResultDto<GarageTransactionDto>>?> CheckUserIsFound(int organizationId, Guid userId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var user = await _myUserRepository.FindUserByOrganizationAsync(organizationId, userId);
            if (user is null)
            {
                return Result<PagedResultDto<GarageTransactionDto>>.Invalid(new ValidationError
                {
                    ErrorMessage = "User not found"
                });
            }
        }
        return null;
    }

    #endregion
}
