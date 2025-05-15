using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.Filters;
using SmartParking.Lookups.PlateColor;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.Results;
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
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
namespace SmartParking.Vehicles.LookUps
{
    [Authorize]
    public class VehicleBrandsAppService : CrudAppService<VehicleBrand, VehicleBrandDto, int, PagedAndSortedResultRequestDto, CreateUpdateVehicleBrandDto>, IVehicleBrandsAppService
    {
        private readonly IRepository<VehicleBrand, int> _repository;
        private readonly IDataFilter _dataFilter;

        public VehicleBrandsAppService(IRepository<VehicleBrand, int> repository, IDataFilter dataFilter) : base(repository)
        {
            _repository = repository;
            _dataFilter = dataFilter;
        }

        #region Public Methods
        /// <inheritdoc/>
        public override async Task<VehicleBrandDto> CreateAsync(CreateUpdateVehicleBrandDto input)
        {
            await ValidateVehicleBrandNameUniquenessAsync(input);
            return await base.CreateAsync(input);
        }
        /// <inheritdoc/>

        public override async Task<VehicleBrandDto> UpdateAsync(int id, CreateUpdateVehicleBrandDto input)
        {
            var existingVehicleBrand = await _repository.GetAsync(id);
            if (existingVehicleBrand is null)
            {
                throw new InvalidOperationException("Vehicle brand not found");
            }

            await ValidateVehicleBrandNameUniquenessAsync(input, id);
            return await base.UpdateAsync(id, input);
        }
        /// <inheritdoc/>

        public override async Task DeleteAsync(int id)
        {
            var vehicleBrand = await _repository.FindAsync(id);
            if (vehicleBrand is null)
            {
                throw new EntityNotFoundException(typeof(VehicleBrand), id);
            }

            await _repository.HardDeleteAsync(vehicleBrand);
        }
        /// <inheritdoc/>

        public async Task<PagedResultDto<VehicleBrandDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
        {
            var queryable = await _repository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {
                queryable = queryable.Where(or =>
                    or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()));
            }

            var totalCount = await AsyncExecuter.CountAsync(queryable);
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

            var vehicleBrands = await AsyncExecuter.ToListAsync(queryable);
            var vehicleBrandsDto = ObjectMapper.Map<List<VehicleBrand>, List<VehicleBrandDto>>(vehicleBrands);

            return new PagedResultDto<VehicleBrandDto>(totalCount, vehicleBrandsDto);
        }
        /// <inheritdoc/>

        public async Task<PagedResultDto<VehicleBrandDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
        {
            var queryable = await _repository.GetQueryableAsync();
            var filterExpression = BuildExpression<VehicleBrand>.BuildFilterExpression(input.filters);

            if (filterExpression is not null)
            {
                queryable = queryable.Where(filterExpression);
            }

            var totalCount = await AsyncExecuter.CountAsync(queryable);
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

            var vehicleBrands = await AsyncExecuter.ToListAsync(queryable);
            var vehicleBrandsDto = ObjectMapper.Map<List<VehicleBrand>, List<VehicleBrandDto>>(vehicleBrands);

            return new PagedResultDto<VehicleBrandDto>(totalCount, vehicleBrandsDto);
        }
        /// <inheritdoc/>

        [RemoteService(IsEnabled = false)]
        public async Task<Result<List<VehicleBrandDto>>> GetbrandsAsync()
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var brands = await _repository.GetListAsync();
                if (brands.Count == 0)
                    return Result<List<VehicleBrandDto>>.NotFound();

                var brandsMap = ObjectMapper.Map<List<VehicleBrand>, List<VehicleBrandDto>>(brands);
                return Result<List<VehicleBrandDto>>.Success(brandsMap);
            }
        }
        /// <inheritdoc/>

        public async Task<List<VehicleBrandDto>> GetAllVehicleBrandsAsync()
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var brands = await _repository.GetListAsync();
                return ObjectMapper.Map<List<VehicleBrand>, List<VehicleBrandDto>>(brands);
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Validates if the vehicle brand name is unique.
        /// </summary>
        /// <param name="input">The vehicle brand create or update DTO containing the details for the brand name.</param>
        /// <param name="currentId">The ID of the current vehicle brand (used for update scenarios to ignore the current brand itself in the check).</param>
        /// <exception cref="InvalidOperationException">Thrown when the vehicle brand name already exists (in either English or Arabic).</exception>
        private async Task ValidateVehicleBrandNameUniquenessAsync(CreateUpdateVehicleBrandDto input, int? currentId = null)
        {
            var vehicleBrand = await _repository.FirstOrDefaultAsync(
                brand =>
                    brand.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                    brand.ArabicName.ToLower().Contains(input.ArabicName.ToLower())
            );

            if (vehicleBrand != null)
            {
                if (vehicleBrand.EnglishName == input.EnglishName.Trim() && vehicleBrand.Id != currentId)
                {
                    throw new InvalidOperationException("Vehicle brand English name already exists");
                }
                else if (vehicleBrand.ArabicName == input.ArabicName.Trim() && vehicleBrand.Id != currentId)
                {
                    throw new InvalidOperationException("Vehicle brand Arabic name already exists");
                }
            }
        }


        #endregion
    }
}
