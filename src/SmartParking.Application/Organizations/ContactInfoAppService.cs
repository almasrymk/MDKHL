
using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Organizations;
[Authorize]

public class ContactInfoAppService : CrudAppService<ContactInfo, ContactInfoDto, int, PagedAndSortedResultRequestDto, ContactInfoCreateUpdateDto>, IContactInfoAppService
{
    private readonly IRepository<ContactInfo, int> _repository;

    public ContactInfoAppService(IRepository<ContactInfo, int> repository) : base(repository)
    {
        _repository = repository;
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<ContactInfoDto>> GetContactInfoByOrganization(PagedAndSortedResultRequestDto input, int organizationId)
    {

        var query = await _repository.GetQueryableAsync();

        query = query.Where(x => x.OrganizationId == organizationId);

        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                query = query.OrderBy(sorting[0] + " descending");
            }
            else
            {
                query = query.OrderBy(input.Sorting);
            }

        }
        var totalCount = await AsyncExecuter.CountAsync(query);

        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);

        query.OrderBy(x => x.Id);

        var contacts = await AsyncExecuter.ToListAsync(query);
        var contactDto = ObjectMapper.Map<List<ContactInfo>, List<ContactInfoDto>>(contacts);
        return new PagedResultDto<ContactInfoDto>(
           totalCount,
           contactDto
        );

    }
   
    /// <inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var contactInfo = await _repository.FindAsync(id);

        if (contactInfo is null)
        {
            throw new EntityNotFoundException(typeof(VehicleModel), id);
        }

        contactInfo.Status = Status.Deleted;
        await CurrentUnitOfWork.SaveChangesAsync();
        // await base.DeleteAsync(id);
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<ContactInfoDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            // Get the IQueryable<organization> from the repository
            var queryable = await _repository.GetQueryableAsync();
            // Apply search filtering
            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {

                queryable = queryable.Where(or =>
                    or.Name.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.ContactMail.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.WhatsAppNum.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.PhoneNum.ToLower().Contains(input.SearchingTerm.ToLower()));
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
            // Get the total count of filtered records
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // Apply pagination
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

            // Execute the query and get the list of ContactInfo entities
            var contactInfos = await AsyncExecuter.ToListAsync(queryable);

            // Map the ContactInfo entities to ContactInfoDto objects

            var contactInfoDto = ObjectMapper.Map<List<ContactInfo>, List<ContactInfoDto>>(contactInfos);

            Console.WriteLine(contactInfoDto.Count);
            // Return the paginated result
            return new PagedResultDto<ContactInfoDto>(
                totalCount,
                contactInfoDto
            );
        }
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<ContactInfoDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            // Get the IQueryable<ContactInfo> from the repository
            var queryable = await _repository.GetQueryableAsync();
            // Apply filtering
            var filterExpression = BuildExpression<ContactInfo>.BuildFilterExpression(input.filters);
            if (filterExpression is not null)
            {
                queryable = queryable.Where(filterExpression);
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
            // Get the total count of filtered records
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // Apply pagination
            queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

            // Execute the query and get the list of contactInfos entities
            var contactInfos = await AsyncExecuter.ToListAsync(queryable);

            // Map the contactInfos entities to contactInfosDto objects
            var contactInfosDto = ObjectMapper.Map<List<ContactInfo>, List<ContactInfoDto>>(contactInfos);
            Console.WriteLine(contactInfosDto.Count);
            // Return the paginated result
            return new PagedResultDto<ContactInfoDto>(
                totalCount,
                contactInfosDto
            );
        }
    }

}
