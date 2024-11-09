using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
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
    private readonly IRepository<Feature, int> _repository;

    public FeaturesAppService(IRepository<Feature, int> repository) : base(repository)
    {
       _repository = repository;
    }


    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var feature = await _repository.FirstOrDefaultAsync(f => f.Id == id);
        if (feature == null)
            throw new EntityNotFoundException(typeof(Feature), id);

        //await base.DeleteAsync(id);
        feature.Status = Status.Deleted;
        await CurrentUnitOfWork.SaveChangesAsync();
    }

}
