using SmartParking.Entities.Organizations;
using SmartParking.EnumDescriptions;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
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

namespace SmartParking.Garages;

public class GarageAppService : CrudAppService<Garage, GarageDto, int, PagedAndSortedResultRequestDto, CreateUpdateGarageDto>, IGarageAppService
{
    private readonly IRepository<Garage> _repository;
    private readonly OrganizationsAppService _organizationsAppService;

    public GarageAppService(IRepository<Garage, int> repository, OrganizationsAppService organizationsAppService) : base(repository)
    {
        _repository = repository;
        _organizationsAppService = organizationsAppService;
    }


    

    /// <summary>
    /// Method that used to create new garage to specific organization
    /// </summary>
    /// <param name="input">input is createupdateGarageDto type
    /// holds data from presentation layer
    /// </param>
    /// <returns>Dto of created garage</returns>
    /// <exception cref="EntityNotFoundException"> exception is returned when garage id isnot exists </exception>
    public override async Task<GarageDto> CreateAsync(CreateUpdateGarageDto input)
    {
        var organization = await _organizationsAppService.GetAsync(input.OrganizationId);
        if (organization == null)
        {
            throw new EntityNotFoundException(typeof(Organization), input.OrganizationId);
        }
        string code = organization.Code;
        var numberOfGaragesForOrganization = await _repository.GetListAsync(x => x.OrganizationId == input.OrganizationId);
        // Checking criteria for package that organization subscribe.
        input.Code = code + GetOrganizationCode(numberOfGaragesForOrganization.Count);
        return await base.CreateAsync(input);
    }
    /// <summary>
    /// Method is used to create a code for garage
    /// </summary>
    /// <param name="input">input is garage id which is related to specific organization </param>
    /// <returns></returns>
    /// <exception cref="BusinessException">it throws exception when organization tries to add more than 99 garage</exception>

    public string GetOrganizationCode(int input)
    {
        if (input == 99)
        {
            throw new BusinessException("Can't add more garages for that organization");
        }
        string Code = "-G-";
        if (input < 9)
        {
            Code += '0';
        }
        Code += (input + 1);
        return Code;
    }
    /// <summary>
    /// methods that deletes garage.
    /// it takes garage id as parameter , checks if it exists
    /// if don't it throws an exception
    /// if exists it deletes it , changes its status to deleted
    /// </summary>
    /// <param name="id">garage id</param>
    /// <returns>string</returns>
    /// <exception cref="EntityNotFoundException">exception thrown when garage isn't exist</exception>
    public new async Task<string> DeleteByIdAsync(int id)
    {
        var garage = await base.GetAsync(id);

        if (garage is null)
            throw new EntityNotFoundException(typeof(Garage), id);
        else
        {
            await base.DeleteByIdAsync(id);
            garage.Status = Enums.Status.Deleted;

            return EnumValues.GetEnumDescription(GarageValidations.GarageDeleteion);
        }
    }
    /// <summary>
    /// A method used to update Garage
    /// </summary>
    /// <param name="id">Id of garage</param>
    /// <param name="input">Dto of updated garage</param>
    /// <returns></returns>
    /// <exception cref="EntityNotFoundException">An exception will be thrown if no organization id found in database</exception>

    public async override Task<GarageDto> UpdateAsync(int id, CreateUpdateGarageDto input)
    {
        var organization = await _organizationsAppService.GetAsync(input.OrganizationId);
        if (organization == null)
        {
            throw new EntityNotFoundException(typeof(Organization), input.OrganizationId);
        }
        return await base.UpdateAsync(id, input);
    }
}
