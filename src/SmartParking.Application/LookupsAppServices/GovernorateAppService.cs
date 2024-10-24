using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities;
using SmartParking.Entities.Cameras;
using SmartParking.Lookups;
using SmartParking.Lookups.Governorate;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices;
[Authorize]

public class GovernorateAppService : CrudAppService<Governorate,
                                                   GovernorateDto,
                                                   int,
                                                   PagedAndSortedResultRequestDto,
                                                   CreateUpdateGovernorateDto>,
                                      IGovernorateAppService
{
    private readonly IRepository<Country, int> _countryRepository;
    private readonly IRepository<Governorate, int> _governorateRepository;

    public GovernorateAppService(IRepository<Governorate, int> governorateRepository, IRepository<Country, int> countryRepository)
    : base(governorateRepository)
    {
        _governorateRepository = governorateRepository;
        _countryRepository = countryRepository;
    }

        ///<inheritdoc/>
        public override async Task<GovernorateDto> CreateAsync(CreateUpdateGovernorateDto input)
        {
            var governorate = await _governorateRepository.FirstOrDefaultAsync
            (governorate =>
            governorate.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
            governorate.ArabicName.ToLower().Contains(input.ArabicName.ToLower()) ||
            governorate.EnglishCapital.ToLower().Contains(input.EnglishCapital.ToLower()) ||
            governorate.ArabicCapital.ToLower().Contains(input.ArabicCapital.ToLower()));

            if (governorate != null)
            {
                if (governorate.EnglishName == input.EnglishName.ToLower().Trim(' ') && governorate.Id != input.Id)
                {
                    throw new InvalidOperationException("governrate english name already exists");

                }
                else if (governorate.ArabicName == input.ArabicName.ToLower().Trim(' ') && governorate.Id != input.Id)
                {
                    throw new InvalidOperationException("governrate arabic name already exists");

                }
                else if (governorate.EnglishCapital == input.EnglishCapital.ToLower().Trim(' ') && governorate.Id != input.Id)
                {
                    throw new InvalidOperationException("governrate english capital already exists");

                }
                else if (governorate.ArabicCapital == input.ArabicCapital.ToLower().Trim(' ') && governorate.Id != input.Id)
                {
                    throw new InvalidOperationException("governrate arabic capital already exists");

                }

            }

            return await base.CreateAsync(input);
            
        }

    /// <inheritdoc/>

    public override async Task<GovernorateDto> UpdateAsync(int id, CreateUpdateGovernorateDto input)
    {
        var governorate = await _governorateRepository.FirstOrDefaultAsync
        (governorate =>
        governorate.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
        governorate.ArabicName.ToLower().Contains(input.ArabicName.ToLower()) ||
        governorate.EnglishCapital.ToLower().Contains(input.EnglishCapital.ToLower()) ||
        governorate.ArabicCapital.ToLower().Contains(input.ArabicCapital.ToLower()));

        if (governorate != null)
        {
            if (governorate.EnglishName == input.EnglishName.Trim(' ') && governorate.Id != input.Id)
            {
                throw new InvalidOperationException("governrate english name already exists");

            }
            else if (governorate.ArabicName == input.ArabicName.Trim(' ') && governorate.Id != input.Id)
            {
                throw new InvalidOperationException("governrate arabic name already exists");

            }
            else if (governorate.EnglishCapital == input.EnglishCapital.Trim(' ') && governorate.Id != input.Id)
            {
                throw new InvalidOperationException("governrate english capital already exists");

            }
            else if (governorate.ArabicCapital == input.ArabicCapital.Trim(' ') && governorate.Id != input.Id)
            {
                throw new InvalidOperationException("governrate arabic capital already exists");

            }
           
        }
            return await base.UpdateAsync(id,input);
        
    }

    /// <summary>
    /// method used to get list of governorates
    /// </summary>
    /// <param name="input">specifies max number of governorate and if it filtered or not</param>
    /// <returns>GovernorateDto List</returns>

    public async Task<PagedResultDto<GovernorateDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        // Get the IQueryable<governorate> from the repository
        var queryable = await _governorateRepository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<Governorate>.BuildFilterExpression(input.filters);
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
        // Execute the query and get the list of garages entities
        var governorates = await AsyncExecuter.ToListAsync(queryable);

        // Map the garages entities to garagesDto objects
        var governorateDto = ObjectMapper.Map<List<Governorate>, List<GovernorateDto>>(governorates);
        Console.WriteLine(governorateDto.Count);
        // Return the paginated result
        return new PagedResultDto<GovernorateDto>(
            totalCount,
            governorateDto
        );
    }

    public async Task<List<CountryLookupDto>> GetCountryLookupAsync()
    {
        var countries = await _countryRepository.GetListAsync();
        var countryList = ObjectMapper.Map<List<Country>, List<CountryLookupDto>>(countries)
;
        return countryList;
       
    }

    /// <summary>
    /// method used to get list of governorates
    /// </summary>
    /// <param name="input">specifies max number of governorate and if it filtered or not</param>
    /// <returns>GovernorateDto List</returns>
    public async Task<PagedResultDto<GovernorateDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<governorate> from the repository
        var queryable = await _governorateRepository.GetQueryableAsync();
        // Apply search filtering
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(c =>
                c.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                c.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower())

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
        var governorates = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to hotelDto objects
        var governorateDto = ObjectMapper.Map<List<Governorate>, List<GovernorateDto>>(governorates);
        Console.WriteLine(governorateDto.Count);
        // Return the paginated result
        return new PagedResultDto<GovernorateDto>(
            totalCount,
            governorateDto
        );
    }
    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var camera = await GetAsync(id);
        if (camera == null)
            throw new EntityNotFoundException(typeof(Governorate), id);

        await base.DeleteAsync(id);
    }
}
