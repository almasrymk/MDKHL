using SmartParking.Entities;
using SmartParking.Lookups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices
{
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
    }
}
