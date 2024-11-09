using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
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
                throw new EntityNotFoundException(typeof(VehicleBrand), id);
            }

            vehicleBrand.Status = Status.Deleted;
            await CurrentUnitOfWork.SaveChangesAsync();
            // await base.DeleteAsync(id);
        }

        ///<inheritdoc/>
        public override async Task<VehicleBrandDto> CreateAsync(CreateUpdateVehicleBrandDto input)
        {
            var vehicleBrand = await _repository.FirstOrDefaultAsync
               (vehicle =>
               vehicle.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
               vehicle.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));


            if (vehicleBrand != null)
            {
                if (vehicleBrand.EnglishName == input.EnglishName.Trim(' '))
                {
                    throw new InvalidOperationException("Vehicle brand english name already exists");

                }
                else if (vehicleBrand.ArabicName == input.ArabicName.Trim(' '))
                {
                    throw new InvalidOperationException("Vehicle brand  arabic name already exists");

                }


            }
            return await base.CreateAsync(input);

        }

        ///<inheritdoc/>
        public override async Task<VehicleBrandDto> UpdateAsync(int id, CreateUpdateVehicleBrandDto input)
        {
            var existingVehicleBrand = await _repository.GetAsync(id);

            if (existingVehicleBrand is null)
            {
                throw new InvalidOperationException("Vehicle brand not found");
            }

            var vehicleBrand = await _repository.FirstOrDefaultAsync
                    (vehicle =>
                    vehicle.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                    vehicle.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));


            if (vehicleBrand != null)
            {
                if (vehicleBrand.EnglishName == input.EnglishName.Trim(' ') && vehicleBrand.Id != id)
                {
                    throw new InvalidOperationException("Vehicle brand english name already exists");

                }
                else if (vehicleBrand.ArabicName == input.ArabicName.Trim(' ') && vehicleBrand.Id != id)
                {
                    throw new InvalidOperationException("Vehicle brand arabic name already exists");

                }


            }
            return await base.UpdateAsync(id, input);
        }
        public async Task<PagedResultDto<VehicleBrandDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
        {
            // Get the IQueryable<camera> from the repository
            var queryable = await _repository.GetQueryableAsync();
            // Apply search filtering
            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {

                queryable = queryable.Where(or =>
                    or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) 
                    

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
        [RemoteService(IsEnabled =false)]
        public async Task<Result<List<VehicleBrandDto>>> GetbrandsAsync()
        {
            var brands = await _repository.GetListAsync();
            if (brands.Count == 0)
                return Result<List<VehicleBrandDto>>.NotFound();
            var brandsMap = ObjectMapper.Map<List<VehicleBrand>, List<VehicleBrandDto>>(brands);
            return Result<List<VehicleBrandDto>>.Success(brandsMap);
        }
    }
}
