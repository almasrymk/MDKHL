using SmartParking.Entities.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.KeeperShifts;

public sealed class KeeperShiftSingletonService
{
    public static KeeperShiftDto? instance;
    private readonly IKeeperShiftsAppService _keeperShiftsAppService;

    public KeeperShiftSingletonService(IKeeperShiftsAppService keeperShiftsAppService)
    {
        _keeperShiftsAppService = keeperShiftsAppService;
    }

    public async Task<KeeperShiftDto?> GetKeeperShiftInstance(int keeperShiftId)
    {
       
            if (instance == null)
            {
                instance = await _keeperShiftsAppService.GetAsync(keeperShiftId);
            }
            return instance;
        
    }    
    
    public void RemoveKeeperShiftInstance()
    {
        instance = null;
    }
}
