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
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
namespace SmartParking.Vehicles.LookUps;

public class LicenceTypesAppService: CrudAppService<LicenceType, LicenceTypeDto, int, PagedAndSortedResultRequestDto, CreateUpdateLicenceTypeDto>, ILicenceTypesAppService
{
    private readonly IRepository<LicenceType, int> _repository;

    public LicenceTypesAppService(IRepository<LicenceType, int> repository) : base(repository)
    {
        _repository = repository;
    }

    ///<inheritdoc/>
    public override async Task<LicenceTypeDto> CreateAsync(CreateUpdateLicenceTypeDto input)
    {
        var licenceType = await _repository.FirstOrDefaultAsync
                (type =>
                type.EnglishName == input.EnglishName ||
                type.ArabicName == input.ArabicName);

        if (licenceType != null)
        {
            if (licenceType.EnglishName == input.EnglishName)
            {
                throw new InvalidOperationException("licence type english name already exists");

            }
            else if (licenceType.ArabicName == input.ArabicName)
            {
                throw new InvalidOperationException("licence type arabic name already exists");

            }

            throw new InvalidOperationException("english name , arabic name already exist");
        }
        else

            return await base.CreateAsync(input);
        
    }

    ///<inheritdoc/>
    public override async Task<LicenceTypeDto> UpdateAsync(int id, CreateUpdateLicenceTypeDto input)
    {
        var licenceType = await _repository.FirstOrDefaultAsync
               (type =>
               type.EnglishName == input.EnglishName ||
               type.ArabicName == input.ArabicName);

        if (licenceType != null)
        {
            if (licenceType.EnglishName == input.EnglishName)
            {
                throw new InvalidOperationException("licence type english name already exists");

            }
            else if (licenceType.ArabicName == input.ArabicName)
            {
                throw new InvalidOperationException("licence type arabic name already exists");

            }

            throw new InvalidOperationException("english name , arabic name already exist");
        }
        else
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

}
