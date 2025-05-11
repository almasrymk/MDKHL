using SmartParking.Entities.Lookups;
using SmartParking.Lookups.CameraBrands;
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
using System.Linq.Dynamic.Core;
using Volo.Abp.MultiTenancy;
using SmartParking.Filters;

namespace SmartParking.LookupsAppServices;

public class CameraBrandAppService : CrudAppService<CameraBrand, CameraBrandDto, int, PagedAndSortedResultRequestDto, CreateUpdateCameraBrandDto>, ICameraBrandAppService
{
    private readonly IRepository<CameraBrand, int> _repository;
    private readonly IDataFilter _dataFilter;

    public CameraBrandAppService(IRepository<CameraBrand, int> repository, IDataFilter dataFilter) : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
    }
    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var cameraBrand = await _repository.FindAsync(id);

        if (cameraBrand is null)
        {
            throw new EntityNotFoundException(typeof(CameraBrand), id);
        }

        
         await _repository.HardDeleteAsync(cameraBrand);
    }

    ///<inheritdoc/>
    public override async Task<CameraBrandDto> CreateAsync(CreateUpdateCameraBrandDto input)
    {
        await CheckCameraBrandUniqnuess(input);
        return await base.CreateAsync(input);

    }

    private async Task CheckCameraBrandUniqnuess(CreateUpdateCameraBrandDto input)
    {
        var cameraBrand = await _repository.FirstOrDefaultAsync
                   (camera =>
                   camera.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                   camera.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));


        if (cameraBrand != null)
        {
            if (cameraBrand.EnglishName == input.EnglishName.Trim(' '))
            {
                throw new InvalidOperationException("Camera brand english name already exists");

            }
            else if (cameraBrand.ArabicName == input.ArabicName.Trim(' '))
            {
                throw new InvalidOperationException("Camera brand  arabic name already exists");

            }


        }
    }

    ///<inheritdoc/>
    public override async Task<CameraBrandDto> UpdateAsync(int id, CreateUpdateCameraBrandDto input)
    {
        var existingCameraBrand = await _repository.GetAsync(id);

        if (existingCameraBrand is null)
        {
            throw new InvalidOperationException("Camera brand not found");
        }

        await CheckCameraBrandUniqnuess(input);
        return await base.UpdateAsync(id, input);
    }
    public async Task<PagedResultDto<CameraBrandDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<CameraBrand>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of organizations entities
        var cameraBrands = await AsyncExecuter.ToListAsync(queryable);

        var cameraBrandsDto = ObjectMapper.Map<List<CameraBrand>, List<CameraBrandDto>>(cameraBrands);
        // Return the paginated result
        return new PagedResultDto<CameraBrandDto>(
            totalCount,
            cameraBrandsDto
        );
    }

    public async Task<List<CameraBrandDto>> GetAllCameraBrandsAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var brands = await _repository.GetListAsync();
            return ObjectMapper.Map<List<CameraBrand>, List<CameraBrandDto>>(brands);
        }
    }

    public async Task<PagedResultDto<CameraBrandDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
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
        var cameraBrands = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to cameraDto objects
        var cameraBrandsDto = ObjectMapper.Map<List<CameraBrand>, List<CameraBrandDto>>(cameraBrands);
        // Return the paginated result
        return new PagedResultDto<CameraBrandDto>(
            totalCount,
            cameraBrandsDto
        );
    }
}
