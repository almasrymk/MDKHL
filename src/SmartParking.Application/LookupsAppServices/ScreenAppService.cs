using SmartParking.Entities;
using SmartParking.Lookups;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices
{
    public class ScreenAppService : CrudAppService<Screen, 
                                                    ScreenDto, 
                                                    int, 
                                                    PagedAndSortedResultRequestDto,
                                                    CreateUpdateScreenDto>,
        IScreenAppService
    {
        public ScreenAppService(IRepository<Screen, int> repository) : base(repository)
        {
        }
    }
}
