using AutoMapper.Internal.Mappers;
using SmartParking.Entities.Lookups;
using SmartParking.Filters;
using SmartParking.Localization;
using SmartParking.Lookups.CameraMainType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.LookupsAppServices;

public class CameraMainTypeAppService : CrudAppService<CameraMainType, CameraMainTypeDto, int, PagedAndSortedResultRequestDto, CreateUpdateCameraMainTypeDto>, ICameraMainTypeAppService
{
    private readonly IRepository<CameraMainType, int> _repository;
    private readonly IDataFilter _dataFilter;

    public CameraMainTypeAppService(IRepository<CameraMainType, int> repository, IDataFilter dataFilter) : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
    }
    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var cameraMainType = await _repository.FindAsync(id);

        if (cameraMainType is null)
        {
            throw new EntityNotFoundException(typeof(CameraMainType), id);
        }

        await _repository.HardDeleteAsync(cameraMainType);
    }

    ///<inheritdoc/>
    public override async Task<CameraMainTypeDto> CreateAsync(CreateUpdateCameraMainTypeDto input)
    {
        var cameraMainType = await _repository.FirstOrDefaultAsync
           (camera =>
           camera.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
           camera.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));


        if (cameraMainType != null)
        {
            if (cameraMainType.EnglishName == input.EnglishName.Trim(' '))
            {
                throw new InvalidOperationException(
                                                        $"{LocalizationHelper.GetLocalizedString("EnglishName")} {LocalizationHelper.GetLocalizedString("DuplicateValidation")}");
            }
            else if (cameraMainType.ArabicName == input.ArabicName.Trim(' '))
            {
                throw new InvalidOperationException(
                                            $"{LocalizationHelper.GetLocalizedString("ArabicName")} {LocalizationHelper.GetLocalizedString("DuplicateValidation")}");
            }


        }
        return await base.CreateAsync(input);

    }

    ///<inheritdoc/>
    public override async Task<CameraMainTypeDto> UpdateAsync(int id, CreateUpdateCameraMainTypeDto input)
    {
        var existingCameraMainType = await _repository.GetAsync(id);

        if (existingCameraMainType is null)
        {
            throw new InvalidOperationException("Camera maintype not found");
        }

        var cameraMainType = await _repository.FirstOrDefaultAsync
                (camera =>
                camera.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                camera.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));


        if (cameraMainType != null)
        {
            if (cameraMainType.EnglishName == input.EnglishName.Trim(' ') && cameraMainType.Id != id)
            {
                throw new InvalidOperationException(
                                        $"{LocalizationHelper.GetLocalizedString("EnglishName")} {LocalizationHelper.GetLocalizedString("DuplicateValidation")}");
            }
            else if (cameraMainType.ArabicName == input.ArabicName.Trim(' ') && cameraMainType.Id != id)
            {
                throw new InvalidOperationException(
                            $"{LocalizationHelper.GetLocalizedString("ArabicName")} {LocalizationHelper.GetLocalizedString("DuplicateValidation")}");

            }


        }
        return await base.UpdateAsync(id, input);
    }
    public async Task<PagedResultDto<CameraMainTypeDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<CameraMainType>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of organizations entities
        var cameraMainTypes = await AsyncExecuter.ToListAsync(queryable);

        var cameraMainTypesDto = ObjectMapper.Map<List<CameraMainType>, List<CameraMainTypeDto>>(cameraMainTypes);
        // Return the paginated result
        return new PagedResultDto<CameraMainTypeDto>(
            totalCount,
            cameraMainTypesDto
        );
    }

    public async Task<List<CameraMainTypeDto>> GetAllCameraMainTypesAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var maintypes = await _repository.GetListAsync();
            return ObjectMapper.Map<List<CameraMainType>, List<CameraMainTypeDto>>(maintypes);
        }
    }

    public async Task<PagedResultDto<CameraMainTypeDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();
        // Apply search filtering
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower())


            );
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var cameraMainTypes = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to cameraDto objects
        var cameraMainTypesDto = ObjectMapper.Map<List<CameraMainType>, List<CameraMainTypeDto>>(cameraMainTypes);
        // Return the paginated result
        return new PagedResultDto<CameraMainTypeDto>(
            totalCount,
            cameraMainTypesDto
        );
    }
}