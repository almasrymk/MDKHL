using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups;

public interface IFeaturesAppService : ICrudAppService<FeatureDto,int, PagedAndSortedResultRequestDto, CreateUpdateFeatureDto>
{



    /// <summary>
    /// Method used to delete exists feature
    /// </summary>
    /// <param name="id">Id of feature that needed to be deleted</param>
    /// <returns>No Content</returns>
    public Task DeleteAsync(int id);
}
