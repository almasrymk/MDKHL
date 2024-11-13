

using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Garages;
using SmartParking.GarageTransactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;

namespace SmartParking.GarageTransactionEndPoints;

public class UnLinkedTransactionAppService : CrudAppService<UnLinkedTransaction, UnLinkedTransactionDto, int,
    PagedAndSortedResultRequestDto, CreateUpdateGarageTransactionEndPointDto>,
    IUnLinkedTransactionAppService
{
    private readonly IRepository<UnLinkedTransaction, int> _repository;
    private readonly IDataFilter _dataFilter;

    public UnLinkedTransactionAppService(IRepository<UnLinkedTransaction, int> repository,IDataFilter dataFilter) :base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
    }

    ///<inheritdoc/>
    public async override  Task<PagedResultDto<UnLinkedTransactionDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await base.GetListAsync(input);
        }
    }

    ///<inheritdoc/>
    public async override  Task<UnLinkedTransactionDto> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await base.GetAsync(id);
        }
    }

    ///<inheritdoc/>
    public async Task<PagedResultDto<UnLinkedTransactionDto>> SearchForMDKHL(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<unLinkedTransaction> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply search filtering
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.PlateNumber.ToLower().Contains(input.SearchingTerm.ToLower()) 
            );
        }
        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                queryable = queryable.OrderBy(sorting[0] + " descending");
            }
            else
            {
                queryable = queryable.OrderBy(input.Sorting);
            }

        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of hotel entities
        var unLinkedTransactions = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to hotelDto objects
        var unLinkedTransactionDtos = ObjectMapper.Map<List<UnLinkedTransaction>, List<UnLinkedTransactionDto>>(unLinkedTransactions);
        // Return the paginated result
        return new PagedResultDto<UnLinkedTransactionDto>(
            totalCount,
            unLinkedTransactionDtos
        );
    }
    ///<inheritdoc/>
    public async Task<PagedResultDto<UnLinkedTransactionDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            // Get the IQueryable<unLinkedTransaction> from the repository
            var queryable = await _repository.GetQueryableAsync();
            // Apply filtering
            var filterExpression = BuildExpression<Garage>.BuildFilterExpression(input.filters);
            if (filterExpression is not null)
            {
                queryable = queryable.Where(filterExpression);
            }
            if (!input.Sorting.IsNullOrEmpty())
            {
                if (input.Sorting.Contains("DESC"))
                {
                    var sorting = input.Sorting.Split(' ').ToList();
                    queryable = queryable.OrderBy(sorting[0] + " descending");
                }
                else
                {
                    queryable = queryable.OrderBy(input.Sorting);
                }

            }
            // Get the total count of filtered records
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // Apply pagination
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
            // Execute the query and get the list of unLinkedTransactions entities
            var unLinkedTransactions = await AsyncExecuter.ToListAsync(queryable);

            // Map the unLinkedTransaction entities to unLinkedTransactionDto objects
            var unLinkedTransactionDto = ObjectMapper.Map<List<UnLinkedTransaction>, List<UnLinkedTransactionDto>>(unLinkedTransactions);
            // Return the paginated result
            return new PagedResultDto<UnLinkedTransactionDto>(
                totalCount,
                unLinkedTransactionDto
            );
        }
    }

    
}
