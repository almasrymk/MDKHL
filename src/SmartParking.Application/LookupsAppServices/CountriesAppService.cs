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
using Volo.Abp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities.Lookups;
using SmartParking.Lookups.Country;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using Volo.Abp.MultiTenancy;
using SmartParking.Filters;

namespace SmartParking.LookupsAppServices
{
    [Authorize]
    public class CountriesAppService : CrudAppService<Country,
                                                CountryDto, int,
                                                PagedAndSortedResultRequestDto,
                                                CreateUpdateCountryDto>,
        ICountriesAppService
    {

        private readonly IRepository<Country, int> _countryRepository;
        private readonly IRepository<Region, int> _regionRepository;



        public CountriesAppService(IRepository<Country, int> countryRepository, IRepository<Region, int> regionRepository)
            : base(countryRepository)
        {
            _regionRepository = regionRepository;
            _countryRepository = countryRepository;
        }


        #region Public Methods

        ///<inheritdoc/>
        public override async Task<CountryDto> CreateAsync(CreateUpdateCountryDto input)
        {
            await ValidateCountryUniqunessAsync(input);
            return await base.CreateAsync(input);
        }

        ///<inheritdoc/>
        public override async Task<CountryDto> UpdateAsync(int id, CreateUpdateCountryDto input)
        {
            var existingCountry = await _countryRepository.GetAsync(id);
            if (existingCountry is null)
            {
                throw new InvalidOperationException("Country not found");
            }
            await ValidateCountryUniqunessAsync(input);
            return await base.UpdateAsync(id, input);
        }

        ///<inheritdoc/>
        public override async Task DeleteAsync(int id)
        {
            var country = await _countryRepository.FindAsync(id);
            if (country is null)
            {
                throw new EntityNotFoundException(typeof(Country), id);
            }
            await _countryRepository.HardDeleteAsync(country);
        }

        /// <summary>
        /// Method used to get a list of countries with optional filtering and sorting.
        /// </summary>
        /// <param name="input">The input containing filter, sorting, and pagination data.</param>
        /// <returns>A paged result of CountryDto objects.</returns>
        public async Task<PagedResultDto<CountryDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
        {
            var queryable = await _countryRepository.GetQueryableAsync();
            var filterExpression = BuildExpression<Country>.BuildFilterExpression(input.filters);
            if (filterExpression != null)
            {
                queryable = queryable.Where(filterExpression);
            }

            ApplySorting(ref queryable, input.Sorting);

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
            var countries = await AsyncExecuter.ToListAsync(queryable);

            var countriesDto = ObjectMapper.Map<List<Country>, List<CountryDto>>(countries);
            return new PagedResultDto<CountryDto>(totalCount, countriesDto);
        }

        ///<inheritdoc/>
        public async Task<PagedResultDto<CountryDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
        {
            var queryable = await _countryRepository.GetQueryableAsync();
            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {
                queryable = queryable.Where(c =>
                    c.ArabicName.StartsWith(input.SearchingTerm) ||
                    c.EnglishName.StartsWith(input.SearchingTerm)
                );
            }

            ApplySorting(ref queryable, input.Sorting);

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
            var countries = await AsyncExecuter.ToListAsync(queryable);

            var countryDto = ObjectMapper.Map<List<Country>, List<CountryDto>>(countries);
            return new PagedResultDto<CountryDto>(totalCount, countryDto);
        }

        ///<inheritdoc/>
        public async Task<List<CountryLookupDto>> GetCountryLookupAsync()
        {
            using (DataFilter.Disable<IMultiTenant>())
            {
                var countries = await _countryRepository.GetListAsync();
                var countryList = ObjectMapper.Map<List<Country>, List<CountryLookupDto>>(countries);
                return countryList;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates the uniqueness of a country based on various attributes.
        /// </summary>
        /// <param name="input">The country DTO to validate.</param>
        private async Task ValidateCountryUniqunessAsync(CreateUpdateCountryDto input)
        {
            var country = await _countryRepository.FirstOrDefaultAsync(c =>
                c.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                c.ArabicName.ToLower().Contains(input.ArabicName.ToLower()) ||
                c.EnglishCapital.ToLower().Contains(input.EnglishCapital.ToLower()) ||
                c.ArabicCapital.ToLower().Contains(input.ArabicCapital.ToLower()));

            if (country != null)
            {
                if (country.EnglishName == input.EnglishName.Trim())
                {
                    throw new InvalidOperationException("Country English name already exists");
                }
                else if (country.ArabicName == input.ArabicName.Trim())
                {
                    throw new InvalidOperationException("Country Arabic name already exists");
                }
                else if (country.ArabicCapital == input.ArabicCapital.Trim())
                {
                    throw new InvalidOperationException("Country Arabic capital already exists");
                }
                else if (country.EnglishCapital == input.EnglishCapital.Trim())
                {
                    throw new InvalidOperationException("Country English capital already exists");
                }
            }
        }

        /// <summary>
        /// Applies sorting based on the input sorting value.
        /// </summary>
        /// <param name="queryable">The queryable collection to apply sorting on.</param>
        /// <param name="sorting">The sorting string from the input.</param>
        private void ApplySorting(ref IQueryable<Country> queryable, string sorting)
        {
            if (!sorting.IsNullOrEmpty())
            {
                if (sorting.Contains("DESC"))
                {
                    var sortingFields = sorting.Split(' ').ToList();
                    queryable = queryable.OrderBy(sortingFields[0] + " descending");
                }
                else
                {
                    queryable = queryable.OrderBy(sorting);
                }
            }
        }

        #endregion
    }
}
