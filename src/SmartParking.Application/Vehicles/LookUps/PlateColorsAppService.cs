using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using SmartParking.Cameras;
using SmartParking.Entities;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.Filters;
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

    public PlateColorsAppService(Volo.Abp.Domain.Repositories.IRepository<PlateColor, int> repository, IDataFilter dataFilter)
        : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
    }

    #region Public Methods

    public override async Task<PlateColorDto> CreateAsync(CreateUpdatePlateColorDto input)
    {
        await ValidatePlateColorUniquenessAsync(input);
        return await base.CreateAsync(input);
    }

    public override async Task<PlateColorDto> UpdateAsync(int id, CreateUpdatePlateColorDto input)
    {
        var existingPlate = await _repository.GetAsync(id);
        if (existingPlate is null)
        {
            throw new InvalidOperationException("Plate Color not found");
        }

        await ValidatePlateColorUniquenessAsync(input, id);
        return await base.UpdateAsync(id, input);
    }

    public async Task<PagedResultDto<PlateColorDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower())
            );
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var plateColors = await AsyncExecuter.ToListAsync(queryable);
        var plateColorsDto = ObjectMapper.Map<List<PlateColor>, List<PlateColorDto>>(plateColors);

        return new PagedResultDto<PlateColorDto>(totalCount, plateColorsDto);
    }

    public async Task<PagedResultDto<PlateColorDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();
        var filterExpression = BuildExpression<PlateColor>.BuildFilterExpression(input.filters);

        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var plateColors = await AsyncExecuter.ToListAsync(queryable);
        var plateColorsDto = ObjectMapper.Map<List<PlateColor>, List<PlateColorDto>>(plateColors);

        return new PagedResultDto<PlateColorDto>(totalCount, plateColorsDto);
    }

    public override async Task DeleteAsync(int id)
    {
        var plateColor = await _repository.FindAsync(id);

        if (plateColor is null)
        {
            throw new EntityNotFoundException(typeof(PlateColor), id);
        }

        await _repository.HardDeleteAsync(plateColor);
    }

    public async Task<List<PlateColorDto>> GetAllPlateColorsAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var plateColors = await _repository.GetListAsync();
            return ObjectMapper.Map<List<PlateColor>, List<PlateColorDto>>(plateColors);
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Validates whether the English or Arabic name already exists in another plate color.
    /// </summary>
    private async Task ValidatePlateColorUniquenessAsync(CreateUpdatePlateColorDto input, int? currentId = null)
    {
        var plateColor = await _repository.FirstOrDefaultAsync(
            plate =>
                plate.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                plate.ArabicName.ToLower().Contains(input.ArabicName.ToLower())
        );

        if (plateColor != null)
        {
            if (plateColor.EnglishName == input.EnglishName.Trim() && plateColor.Id != currentId)
            {
                throw new InvalidOperationException("Plate color English name already exists");
            }
            else if (plateColor.ArabicName == input.ArabicName.Trim() && plateColor.Id != currentId)
            {
                throw new InvalidOperationException("Plate color Arabic name already exists");
            }
        }
    }

    #endregion
}
