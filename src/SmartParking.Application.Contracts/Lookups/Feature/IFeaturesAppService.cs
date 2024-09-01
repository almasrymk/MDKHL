using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups
{
    public interface IFeaturesAppService : ICrudAppService<FeatureDto,int, PagedAndSortedResultRequestDto, CreateUpdateFeatureDto>
    {
    }
}
