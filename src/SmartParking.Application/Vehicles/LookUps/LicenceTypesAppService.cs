using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.Filters;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.PlateColor;
using SmartParking.Lookups.VehicleModel;
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
namespace SmartParking.Vehicles.LookUps;

[Authorize]
public class LicenceTypesAppService : CrudAppService<LicenceType, LicenceTypeDto, int, PagedAndSortedResultRequestDto, CreateUpdateLicenceTypeDto>, ILicenceTypesAppService
{
    private readonly IRepository<LicenceType, int> _repository;
    private readonly IRepository<PlateColor, int> _plateColorRepository;
    private readonly IDataFilter _dataFilter;

    public LicenceTypesAppService(
        IRepository<LicenceType, int> repository,
        IRepository<PlateColor, int> plateColorRepository,
        IDataFilter dataFilter
    ) : base(repository)
    {
        _repository = repository;
        _plateColorRepository = plateColorRepository;
        _dataFilter = dataFilter;
    }

    #region Public Methods

    public override async Task<LicenceTypeDto> CreateAsync(CreateUpdateLicenceTypeDto input)
    {
        await ValidatePlateColorExistsAsync(input.PlateColorId);
        await ValidateLicenceTypeUniquenessAsync(input);
        return await base.CreateAsync(input);
    }

    public override async Task<LicenceTypeDto> UpdateAsync(int id, CreateUpdateLicenceTypeDto input)
    {
        var existingLicence = await _repository.GetAsync(id);
        if (existingLicence is null)
        {
            throw new InvalidOperationException("Licence type not found");
        }

        await ValidatePlateColorExistsAsync(input.PlateColorId);
        await ValidateLicenceTypeUniquenessAsync(input, id);
        return await base.UpdateAsync(id, input);
    }

    public async Task<PagedResultDto<LicenceTypeDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.ArabicName.StartsWith(input.SearchingTerm) ||
                or.EnglishName.StartsWith(input.SearchingTerm) ||
                or.Notes.StartsWith(input.SearchingTerm)
            );
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var licenceTypes = await AsyncExecuter.ToListAsync(queryable);
        var licenceTypesDto = ObjectMapper.Map<List<LicenceType>, List<LicenceTypeDto>>(licenceTypes);

        return new PagedResultDto<LicenceTypeDto>(totalCount, licenceTypesDto);
    }

    public async Task<PagedResultDto<LicenceTypeDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();
        var filterExpression = BuildExpression<LicenceType>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var licenceTypes = await AsyncExecuter.ToListAsync(queryable);
        var licenceTypesDto = ObjectMapper.Map<List<LicenceType>, List<LicenceTypeDto>>(licenceTypes);

        return new PagedResultDto<LicenceTypeDto>(totalCount, licenceTypesDto);
    }

    public override async Task DeleteAsync(int id)
    {
        var licenceType = await _repository.FindAsync(id);

        if (licenceType is null)
        {
            throw new EntityNotFoundException(typeof(LicenceType), id);
        }

        await _repository.HardDeleteAsync(licenceType);
    }

    public async Task<List<LicenceTypeDto>> GetAllLicencesAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var licenceType = await _repository.GetListAsync();
            return ObjectMapper.Map<List<LicenceType>, List<LicenceTypeDto>>(licenceType);
        }
    }

    [RemoteService(IsEnabled = false)]
    public async Task<Result<List<LicenceTypeDto>>> GetAllLicenceTypesAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var licenceTypes = await _repository.GetListAsync();
            var mappedLicenceType = ObjectMapper.Map<List<LicenceType>, List<LicenceTypeDto>>(licenceTypes);
            return Result<List<LicenceTypeDto>>.Success(mappedLicenceType);
        }
    }

    #endregion

    #region Private Methods

    private async Task ValidatePlateColorExistsAsync(int plateColorId)
    {
        var existingPlateColor = await _plateColorRepository.FindAsync(plateColorId);
        if (existingPlateColor is null)
        {
            throw new InvalidOperationException("Plate color does not exist");
        }
    }

    private async Task ValidateLicenceTypeUniquenessAsync(CreateUpdateLicenceTypeDto input, int? currentId = null)
    {
        var licenceType = await _repository.FirstOrDefaultAsync(
            type =>
                type.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                type.ArabicName.ToLower().Contains(input.ArabicName.ToLower())
        );

        if (licenceType != null)
        {
            if (licenceType.EnglishName == input.EnglishName.Trim() && licenceType.Id != currentId)
            {
                throw new InvalidOperationException("Licence type English name already exists");
            }
            else if (licenceType.ArabicName == input.ArabicName.Trim() && licenceType.Id != currentId)
            {
                throw new InvalidOperationException("Licence type Arabic name already exists");
            }
        }
    }

    #endregion
}

