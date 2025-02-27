using SmartParking.SingletonService;
using SmartParking.StateManagements;
using System;
using System.Threading.Tasks;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace SmartParking.KeeperShifts;

public sealed class KeeperShiftSingletonService : ISingletonService<StateManagementDto, int>
{
    public static KeeperShiftDto? instance;
    public static StateManagementDto? state;
    private readonly IKeeperShiftsAppService _keeperShiftsAppService;
    private readonly IStateManagementAppService _stateManagementApp;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public KeeperShiftSingletonService(IKeeperShiftsAppService keeperShiftsAppService,IStateManagementAppService stateManagementApp
        ,ICurrentTenant currentTenant, ICurrentUser currentUser)
    {
        _keeperShiftsAppService = keeperShiftsAppService;
        _stateManagementApp = stateManagementApp;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }
     
    public async ValueTask DisposeAsync()
    {

        await _stateManagementApp.DeleteAsync(state.Id);
        state = null;

    }

    public async Task<StateManagementDto> GetAsync(int key)
    {
        if (state == null)
        {
            instance = await _keeperShiftsAppService.GetAsync(key);
            state = await _stateManagementApp.GetStateManagementByUserAsync(_currentUser.Id, instance.ShiftId);
        }

        return state;
    }      
     

    public async  Task<StateManagementDto> SetAsync(int key)
    {
       

        if (instance == null)
        {
            instance = await _keeperShiftsAppService.GetAsync(key);
            
        }
            var input = new CreateUpdateStateManagementDto
            {
                IsStateActive = true,
                RemainingTime = DateTime.UtcNow,
                TenantId = instance.TenantId,
                UserId = instance.KeeperId,
                ShiftId = instance.ShiftId
            };

            state = await _stateManagementApp.CreateAsync(input);
        return state;
    }
  
}
