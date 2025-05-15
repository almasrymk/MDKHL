using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Filters;
using SmartParking.Lookups;
using SmartParking.Lookups.City;
using SmartParking.Lookups.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.LookupsAppServices
{
    [Authorize]
    public class CitiesAppService : CrudAppService<City, CityDto,
                                                 int, PagedAndSortedResultRequestDto,
                                                 CreateUpdateCityDto>, ICitiesAppService
    {
        private readonly IRepository<City, int> _repository;

        public CitiesAppService(IRepository<City, int> repository)
        : base(repository)
        {
            _repository = repository;
        }

        #region Public Methods

        ///<inheritdoc/>
        public override async Task<CityDto> CreateAsync(CreateUpdateCityDto input)
        {
            await ValidateCityUniqunessAsync(input);
            return await base.CreateAsync(input);
        }

        ///<inheritdoc/>
        public override async Task<CityDto> UpdateAsync(int id, CreateUpdateCityDto input)
        {
            var existingCity = await _repository.GetAsync(id);

            if (existingCity is null)
            {
                throw new InvalidOperationException("City not found");
            }

            await ValidateCityUniqunessAsync(input);
            return await base.UpdateAsync(id, input);
        }

        ///<inheritdoc/>
        public override async Task DeleteAsync(int id)
        {
            var city = await _repository.FirstOrDefaultAsync(c => c.Id == id);
            if (city == null)
                throw new EntityNotFoundException(typeof(City), id);

            await _repository.HardDeleteAsync(city);
        }

        ///<inheritdoc/>
        public async Task<List<CityLookupDto>> GetCityLookupAsync()
        {
            using (DataFilter.Disable<IMultiTenant>())
            {
                var cities = await _repository.GetListAsync();
                return ObjectMapper.Map<List<City>, List<CityLookupDto>>(cities);
            }
        }

        ///<inheritdoc/>

        public async Task<PagedResultDto<CityDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
        {
            var queryable = await _repository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {
                queryable = queryable.Where(c =>
                    c.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    c.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    c.State.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    c.State.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower())
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

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

            var cities = await AsyncExecuter.ToListAsync(queryable);

            var cityDto = ObjectMapper.Map<List<City>, List<CityDto>>(cities);

            return new PagedResultDto<CityDto>(totalCount, cityDto);
        }

        ///<inheritdoc/>

        public async Task<PagedResultDto<CityDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
        {
            var queryable = await _repository.GetQueryableAsync();

            var filterExpression = BuildExpression<City>.BuildFilterExpression(input.filters);
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

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

            var cities = await AsyncExecuter.ToListAsync(queryable);

            var citiesDto = ObjectMapper.Map<List<City>, List<CityDto>>(cities);

            return new PagedResultDto<CityDto>(totalCount, citiesDto);
        }

        ///<inheritdoc/>
        public async Task<List<CityLookupDto>> GetCityByState(int stateId)
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(r => r.StateId == stateId);

            var cities = await AsyncExecuter.ToListAsync(query);
            return ObjectMapper.Map<List<City>, List<CityLookupDto>>(cities);
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Validates the uniqueness of a city's English and Arabic names before creation or update.
        /// Throws an exception if a city with the same English or Arabic name already exists.
        /// </summary>
        /// <param name="input">The input DTO containing the city names to validate.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a city with the same English or Arabic name already exists.
        /// </exception>
        private async Task ValidateCityUniqunessAsync(CreateUpdateCityDto input)
        {
            var city = await _repository.FirstOrDefaultAsync(c =>
                c.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                c.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));

            if (city != null)
            {
                if (city.EnglishName.ToLower().Trim(' ') == input.EnglishName.ToLower().Trim(' '))
                {
                    throw new InvalidOperationException("city english name already exists");
                }
                else if (city.ArabicName.ToLower().Trim(' ') == input.ArabicName.ToLower().Trim(' '))
                {
                    throw new InvalidOperationException("city arabic name already exists");
                }
            }
        }

        #endregion
    }
}
