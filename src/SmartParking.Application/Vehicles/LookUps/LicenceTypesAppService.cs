using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups.VehiclesLookups;
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
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
namespace SmartParking.Vehicles.LookUps;

    [Authorize]
	public class LicenceTypesAppService: CrudAppService<LicenceType, LicenceTypeDto, int, PagedAndSortedResultRequestDto, CreateUpdateLicenceTypeDto>, ILicenceTypesAppService
	{
        private readonly IRepository<LicenceType, int> _repository;
        private readonly IRepository<PlateColor, int> _plateColorRepository;


    public LicenceTypesAppService(IRepository<LicenceType, int> repository, IRepository<PlateColor, int> plateColorRepository) : base(repository)
    {
        _repository = repository;
        _plateColorRepository = plateColorRepository;
    }

    ///<inheritdoc/>
    public override async Task<LicenceTypeDto> CreateAsync(CreateUpdateLicenceTypeDto input)
    {
        var licenceType = await _repository.FirstOrDefaultAsync
                (type =>
                type.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                type.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));

        var exisitingPlateColor = await _plateColorRepository.FindAsync(input.PlateColorId);

        if (exisitingPlateColor is null)
        {
            throw new InvalidOperationException("Plate color doesnot exist");
        }

        if (licenceType != null)
        {
            if (licenceType.EnglishName == input.EnglishName.Trim(' ') )
            {
                throw new InvalidOperationException("Licence type english name already exists");

            }
            else if (licenceType.ArabicName == input.ArabicName.Trim(' ') )
            {
                throw new InvalidOperationException("Licence type arabic name already exists");

            }


        }
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

        var exisitingPlateColor = await _plateColorRepository.FindAsync(input.PlateColorId);

        if (exisitingPlateColor is null)
        {
            throw new InvalidOperationException("Plate color doesnot exist");
        }

        var LicenceType = await _repository.FirstOrDefaultAsync
                (type =>
                type.EnglishName.ToLower().Contains(input.EnglishName.ToLower()) ||
                type.ArabicName.ToLower().Contains(input.ArabicName.ToLower()));


        if (LicenceType != null)
        {
            if (LicenceType.EnglishName == input.EnglishName.Trim(' ') && LicenceType.Id != id)
            {
                throw new InvalidOperationException("Licence type english name already exists");

            }
            else if (LicenceType.ArabicName == input.ArabicName.Trim(' ') && LicenceType.Id != id)
            {
                throw new InvalidOperationException("Licence type arabic name already exists");

            }


        }
        return await base.UpdateAsync(id, input);
    }
    public async Task<PagedResultDto<LicenceTypeDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
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
        var licenceTypes = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to cameraDto objects
        var licenceTypesDto = ObjectMapper.Map<List<LicenceType>, List<LicenceTypeDto>>(licenceTypes);
        // Return the paginated result
        return new PagedResultDto<LicenceTypeDto>(
            totalCount,
            licenceTypesDto
        );
    }

    /// <summary>
    /// method used to get list of cameraes
    /// </summary>
    /// <param name="input">specifies max number of camera and if it filtered or not</param>
    /// <returns>cameraeDto List</returns>
    public async Task<PagedResultDto<LicenceTypeDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        // Get the IQueryable<camera> from the repository
        var queryable = await _repository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<LicenceType>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of organizations entities
        var licenceTypes = await AsyncExecuter.ToListAsync(queryable);

        // Map the organizations entities to organizationsDto objects
        var licenceTypesDto = ObjectMapper.Map<List<LicenceType>, List<LicenceTypeDto>>(licenceTypes);
        // Return the paginated result
        return new PagedResultDto<LicenceTypeDto>(
            totalCount,
            licenceTypesDto
        );
    }

    public override async Task DeleteAsync(int id)
    {
        var licenceType = await _repository.FindAsync(id);

        if (licenceType is null)
        {
            throw new EntityNotFoundException(typeof(VehicleModel), id);
        }


        await base.DeleteAsync(id);
    }
}
