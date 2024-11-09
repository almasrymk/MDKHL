using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using SmartParking.Cameras;
using SmartParking.Entities;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.PlateColor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;

namespace SmartParking.Vehicles.LookUps;

[Authorize]
public class PlateColorsAppService : CrudAppService<PlateColor, PlateColorDto, int, PagedAndSortedResultRequestDto, CreateUpdatePlateColorDto>, IPlateColorsAppService
{
	private readonly Volo.Abp.Domain.Repositories.IRepository<PlateColor, int> _repository;
    private readonly IDataFilter _dataFilter;

    public PlateColorsAppService(Volo.Abp.Domain.Repositories.IRepository<PlateColor, int> repository,IDataFilter dataFilter) : base(repository)
    {
		_repository = repository;
        _dataFilter = dataFilter;
    }

    ///<inheritdoc/>
    public override async Task<PlateColorDto> CreateAsync(CreateUpdatePlateColorDto input)
    {
        var plateColor = await _repository.FirstOrDefaultAsync
               (plate =>
               plate.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
               plate.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));

        if (plateColor != null)
        {
            if (plateColor.EnglishName == input.EnglishName.Trim(' ') )
            {
                throw new InvalidOperationException("Plate color english name already exists");

            }
            else if (plateColor.ArabicName == input.ArabicName.Trim(' ') )
            {
                throw new InvalidOperationException("Plate color  arabic name already exists");

            }


        }
        return await base.CreateAsync(input);

    }

    ///<inheritdoc/>
    public override async Task<PlateColorDto> UpdateAsync(int id, CreateUpdatePlateColorDto input)
    {
        var existingPlate = await _repository.GetAsync(id);

        if (existingPlate is null)
        {
            throw new InvalidOperationException("Plate Color not found");
        }

        var plateColor = await _repository.FirstOrDefaultAsync
                (plate =>
                plate.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                plate.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));


        if (plateColor != null)
        {
            if (plateColor.EnglishName == input.EnglishName.Trim(' ') && plateColor.Id != id)
            {
                throw new InvalidOperationException("Plate Color english name already exists");

            }
            else if (plateColor.ArabicName == input.ArabicName.Trim(' ') && plateColor.Id != id)
            {
                throw new InvalidOperationException("Plate Color arabic name already exists");

            }


        }
        return await base.UpdateAsync(id, input);
    }
    public async Task<PagedResultDto<PlateColorDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
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
		var plateColors = await AsyncExecuter.ToListAsync(queryable);

		// Map the hotel entities to cameraDto objects
		var plateColorsDto = ObjectMapper.Map<List<PlateColor>, List<PlateColorDto>>(plateColors);
		// Return the paginated result
		return new PagedResultDto<PlateColorDto>(
			totalCount,
			plateColorsDto
		);
	}

	/// <summary>
	/// method used to get list of cameraes
	/// </summary>
	/// <param name="input">specifies max number of camera and if it filtered or not</param>
	/// <returns>cameraeDto List</returns>
	public async Task<PagedResultDto<PlateColorDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
	{
		// Get the IQueryable<camera> from the repository
		var queryable = await _repository.GetQueryableAsync();
		// Apply filtering
		var filterExpression = BuildExpression<PlateColor>.BuildFilterExpression(input.filters);
		if (filterExpression is not null)
		{
			queryable = queryable.Where(filterExpression);
		}
		// Get the total count of filtered records
		var totalCount = await AsyncExecuter.CountAsync(queryable);

		// Apply pagination
		queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
		// Execute the query and get the list of organizations entities
		var plateColors = await AsyncExecuter.ToListAsync(queryable);

		// Map the organizations entities to organizationsDto objects
		var plateColorsDto = ObjectMapper.Map<List<PlateColor>, List<PlateColorDto>>(plateColors);
		// Return the paginated result
		return new PagedResultDto<PlateColorDto>(
			totalCount,
			plateColorsDto
		);
	}

    public override async Task DeleteAsync(int id)
    {
        var plateColor = await _repository.FindAsync(id);

        if (plateColor is null)
        {
            throw new EntityNotFoundException(typeof(PlateColor), id);
        }
        plateColor.Status = Status.Deleted;
        await CurrentUnitOfWork.SaveChangesAsync();

        // await base.DeleteAsync(id);
    }

    public async Task<List<PlateColorDto>> GetAllAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var plateColors = await _repository.GetListAsync();
            return ObjectMapper.Map<List<PlateColor>, List<PlateColorDto>>(plateColors);
        }
    }
}
