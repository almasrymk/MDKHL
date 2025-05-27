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
    ///<inheritdoc/>

    public override async Task<LicenceTypeDto> CreateAsync(CreateUpdateLicenceTypeDto input)
    {
        await ValidatePlateColorExistsAsync(input.PlateColorId);
        await ValidateLicenceTypeUniquenessAsync(input);
        return await base.CreateAsync(input);
    }
    ///<inheritdoc/>

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
    public async Task<PagedResultDto<LicenceTypeDto>> GetListLicenceTypesAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        var query =  await _repository.GetQueryableAsync();
        var queryPlate =  await _plateColorRepository.GetQueryableAsync();

        var queryable = from licenceType in query
                        join plateColor in queryPlate on licenceType.PlateColorId equals plateColor.Id
                        select new LicenceTypeDto
                        {
                            Id = licenceType.Id,
                            ArabicName = licenceType.ArabicName,
                            EnglishName = licenceType.EnglishName,
                            PlateColorArabicName = plateColor.ArabicName,
                            PlateColorEnglishName = plateColor.EnglishName
                        };
        if(!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.PlateColorArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.PlateColorEnglishName.ToLower().Contains(input.SearchingTerm.ToLower())
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
        var licenceTypes = await AsyncExecuter.ToListAsync(queryable);
        return new PagedResultDto<LicenceTypeDto>(totalCount, licenceTypes);
    }
    ///<inheritdoc/>

    public async Task<PagedResultDto<LicenceTypeDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        var queryable = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.Notes.ToLower().Contains(input.SearchingTerm.ToLower())
            );
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var licenceTypes = await AsyncExecuter.ToListAsync(queryable);
        var licenceTypesDto = ObjectMapper.Map<List<LicenceType>, List<LicenceTypeDto>>(licenceTypes);

        return new PagedResultDto<LicenceTypeDto>(totalCount, licenceTypesDto);
    }
    ///<inheritdoc/>

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
    ///<inheritdoc/>

    public override async Task DeleteAsync(int id)
    {
        var licenceType = await _repository.FindAsync(id);

        if (licenceType is null)
        {
            throw new EntityNotFoundException(typeof(LicenceType), id);
        }

        await _repository.HardDeleteAsync(licenceType);
    }
    ///<inheritdoc/>

    public async Task<List<LicenceTypeDto>> GetAllLicencesAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var licenceType = await _repository.GetListAsync();
            return ObjectMapper.Map<List<LicenceType>, List<LicenceTypeDto>>(licenceType);
        }
    }
    ///<inheritdoc/>

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

    /// <summary>
    /// Validates whether a plate color with the specified ID exists in the repository.
    /// </summary>
    /// <param name="plateColorId">The ID of the plate color to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when the plate color with the given ID does not exist.</exception>
    private async Task ValidatePlateColorExistsAsync(int plateColorId)
    {
        var existingPlateColor = await _plateColorRepository.FindAsync(plateColorId);
        if (existingPlateColor is null)
        {
            throw new InvalidOperationException("Plate color does not exist");
        }
    }

    /// <summary>
    /// Validates the uniqueness of a license type by checking if the English or Arabic names already exist in the repository, excluding the current license type (if specified by ID).
    /// </summary>
    /// <param name="input">The DTO containing the license type details to validate.</param>
    /// <param name="currentId">The ID of the license type to exclude from the check (optional).</param>
    /// <exception cref="InvalidOperationException">Thrown when the English or Arabic name of the license type already exists in the repository.</exception>
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

