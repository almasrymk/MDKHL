using SmartParking.Entities.Organizations;
using SmartParking.Organizations;
using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Garages;

public class GarageAppService : CrudAppService<Garage, GarageDto, int, PagedAndSortedResultRequestDto, CreateUpdateGarageDto>, IGarageAppService
{
    private readonly IRepository<Garage> _repository;
    private readonly IOrganizationsAppService _organizationsAppService;

    public GarageAppService(IRepository<Garage, int> repository, IOrganizationsAppService organizationsAppService) : base(repository)
    {
        _repository = repository;
        _organizationsAppService = organizationsAppService;
    }

    public IRepository<Garage, int> Repository { get; }

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
        var numberOfGaragesForOrganization = _repository.GetListAsync(x => x.OrganizationId == input.OrganizationId).Result.Count;
        // Checking criteria for package that organization subscribe.
        input.Code = code + GetOrganizationCode(numberOfGaragesForOrganization);
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
}
