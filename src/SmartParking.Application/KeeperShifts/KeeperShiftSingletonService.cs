using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SmartParking.Entities.Shifts;
using SmartParking.SingletonService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.KeeperShifts;

public sealed class KeeperShiftSingletonService : ISingletonService<KeeperShiftDto,int>
{
    public static KeeperShiftDto? instance;
    private readonly IKeeperShiftsAppService _keeperShiftsAppService;

    public KeeperShiftSingletonService(IKeeperShiftsAppService keeperShiftsAppService)
    {
        _keeperShiftsAppService = keeperShiftsAppService;
       
    }

    public void Dispose()
    {
        instance = null;
    }

    public async Task<KeeperShiftDto> GetAsync()
    {
        await Task.Yield();
        return instance;
    }  

    public async Task<KeeperShiftDto> SetAsync(int key)
    {
       
        if (instance == null)
        {
            instance = await _keeperShiftsAppService.GetAsync(key);
        }
        return instance;
    }
  
}
