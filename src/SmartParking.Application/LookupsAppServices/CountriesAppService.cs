using SmartParking.Entities;
using SmartParking.Lookups;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices
{
    public class CountriesAppService : CrudAppService<Country,
                                        CountryDto, int,
                                        PagedAndSortedResultRequestDto,
                                        CreateUpdateCountryDto>, 
        ICountriesAppService
    {
        private readonly IRepository<Country, int> _countryRepository;
        public CountriesAppService(IRepository<Country, int> countryRepository) 
        : base(countryRepository)
        {


            _countryRepository = countryRepository;
          
        }

        /// <summary>
        /// method used to get list of countries
        /// </summary>
        /// <param name="input">specifies max number of country and if it sorted or not</param>
        /// <returns>CountryDto List</returns>
        public async Task<PagedResultDto<CountryDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
        {
            // Get the IQueryable<country> from the repository
            var queryable = await _countryRepository.GetQueryableAsync();
            // Apply filtering
            var filterExpression = BuildExpression<Country>.BuildFilterExpression(input.filters);
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
            var countries = await AsyncExecuter.ToListAsync(queryable);

            // Map the garages entities to garagesDto objects
            var countriesDto = ObjectMapper.Map<List<Country>, List<CountryDto>>(countries);
            Console.WriteLine(countriesDto.Count);
            // Return the paginated result
            return new PagedResultDto<CountryDto>(
                totalCount,
                countriesDto
            );
        }

        /// <summary>
        /// method used to get list of countries
        /// </summary>
        /// <param name="input">specifies max number of country and if it filtered or not</param>
        /// <returns>Country Dto List</returns>
        public async Task<PagedResultDto<CountryDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
        {
            // Get the IQueryable<country> from the repository
            var queryable = await _countryRepository.GetQueryableAsync();
            // Apply search filtering
            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {

                queryable = queryable.Where(c =>
                    c.ArabicName.StartsWith(input.SearchingTerm) ||
                    c.EnglishName.StartsWith(input.SearchingTerm)

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
            var countries = await AsyncExecuter.ToListAsync(queryable);

            // Map the hotel entities to hotelDto objects
            var countryDto = ObjectMapper.Map<List<Country>, List<CountryDto>>(countries);
            Console.WriteLine(countryDto.Count);
            // Return the paginated result
            return new PagedResultDto<CountryDto>(
                totalCount,
                countryDto
            );
        }
    }

        
    
}
