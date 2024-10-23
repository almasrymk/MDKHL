using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Lookups.PlateColor;
using SmartParking.Lookups.VehicleBrand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
namespace SmartParking.Vehicles.LookUps
{
    [Authorize]
	public class VehicleBrandsAppService : CrudAppService<VehicleBrand, VehicleBrandDto, int, PagedAndSortedResultRequestDto, CreateUpdateVehicleBrandDto>, IVehicleBrandsAppService
	{
        private readonly IRepository<VehicleBrand, int> _repository;

        public VehicleBrandsAppService(IRepository<VehicleBrand, int> repository) : base(repository)
		{
           _repository = repository;
        }
        ///<inheritdoc/>
        public override async Task DeleteAsync(int id)
        {
            var vehicleBrand = await _repository.FindAsync(id);

            if (vehicleBrand is null)
            {
                throw new EntityNotFoundException(typeof(VehicleModel), id);
            }


            await base.DeleteAsync(id);
        }
        public async Task<PagedResultDto<VehicleBrandDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
        {
            // Get the IQueryable<camera> from the repository
            var queryable = await _repository.GetQueryableAsync();
            // Apply search filtering
            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {

                queryable = queryable.Where(or =>
                    or.ArabicName.StartsWith(input.SearchingTerm) ||
                    or.EnglishName.StartsWith(input.SearchingTerm) ||
                    or.Notes.StartsWith(input.SearchingTerm)
                    

                );
            }
            // Get the total count of filtered records
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // Apply pagination
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
            // Execute the query and get the list of camera entities
            var vehicleBrands = await AsyncExecuter.ToListAsync(queryable);

            // Map the hotel entities to cameraDto objects
            var vehicleBrandsDto = ObjectMapper.Map<List<VehicleBrand>, List<VehicleBrandDto>>(vehicleBrands);
            // Return the paginated result
            return new PagedResultDto<VehicleBrandDto>(
                totalCount,
                vehicleBrandsDto
            );
        }


        public async Task<PagedResultDto<VehicleBrandDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
        {
            // Get the IQueryable<camera> from the repository
            var queryable = await _repository.GetQueryableAsync();
            // Apply filtering
            var filterExpression = BuildExpression<VehicleBrand>.BuildFilterExpression(input.filters);
            if (filterExpression is not null)
            {
                queryable = queryable.Where(filterExpression);
            }
            // Get the total count of filtered records
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // Apply pagination
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
            // Execute the query and get the list of organizations entities
            var vehicleBrands = await AsyncExecuter.ToListAsync(queryable);

            // Map the organizations entities to organizationsDto objects
            var vehicleBrandsDto = ObjectMapper.Map<List<VehicleBrand>, List<VehicleBrandDto>>(vehicleBrands);
            // Return the paginated result
            return new PagedResultDto<VehicleBrandDto>(
                totalCount,
                vehicleBrandsDto
            );
        }
    }
}
