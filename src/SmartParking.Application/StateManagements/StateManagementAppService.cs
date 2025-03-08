using SmartParking.Entities.Shifts;
using SmartParking.Entities.StateManagements;
using SmartParking.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.StateManagements;

public class StateManagementAppService : CrudAppService<StateManagement, StateManagementDto, int, PagedAndSortedResultRequestDto, CreateUpdateStateManagementDto>, IStateManagementAppService
{
    private readonly IRepository<StateManagement, int> _repository;

    public StateManagementAppService(IRepository<StateManagement, int> repository) : base(repository)
    {
        _repository = repository;
    }

    public async Task<StateManagementDto> GetStateManagementByUserAsync(Guid? userId,int shiftId)
    {
        var query = await _repository.GetQueryableAsync();
        var state = query.Where(x => x.UserId == userId && x.ShiftId == shiftId).ToList();
        if (state.Count > 1)
        {
            var statesToDelete = state.Skip(1).ToList();

            foreach (var record in statesToDelete)
            {
               await _repository.DeleteAsync(record);
            }

        }
        var result = ObjectMapper.Map<StateManagement, StateManagementDto>(state.FirstOrDefault());
        return result;
    }
    public override async Task DeleteAsync(int id)
    {
        var state = await _repository.FirstOrDefaultAsync(x => x.Id == id);
        if(state != null)
        await _repository.HardDeleteAsync(state);
    }
}
