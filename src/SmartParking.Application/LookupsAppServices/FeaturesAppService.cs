using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities;
using SmartParking.Lookups;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices;
[Authorize]

public class FeaturesAppService : CrudAppService<Feature, 
                                                FeatureDto,
                                                int, 
                                                PagedAndSortedResultRequestDto, 
                                                CreateUpdateFeatureDto>,
    IFeaturesAppService
{
    public FeaturesAppService(IRepository<Feature, int> repository) : base(repository)
    {
    }


    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var city = await GetAsync(id);
        if (city == null)
            throw new EntityNotFoundException(typeof(Feature), id);

        await base.DeleteAsync(id);
    }

}
