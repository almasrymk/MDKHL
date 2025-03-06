using AutoMapper.Internal.Mappers;
using Shouldly;
using SmartParking.DailyShifts;
using SmartParking.Entities.Shifts;
using SmartParking.KeeperShifts;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartParking.Shifts;

public abstract class DailyShiftsAppService_Tests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
//IRepository<Shift, int> repository, IDataFilter dataFilter, IRepository<Organization, int> organizationRepository, IRepository<Garage, int> garageRepository
{

    private readonly IShiftAppService _shiftAppService;
    private readonly IKeeperShiftsAppService _keeperShiftsAppService;
    private readonly IDailyShiftAppService _dailyShiftAppService;
    protected DailyShiftsAppService_Tests()
    {
        _dailyShiftAppService =  GetRequiredService<IDailyShiftAppService>();
        _keeperShiftsAppService = GetRequiredService<IKeeperShiftsAppService>();
        _shiftAppService = GetRequiredService<IShiftAppService>();

    }
    [Fact] 
    public async Task DailyShiftCheckStartMidNightShift()
    {
        var result = await _dailyShiftAppService.CreateAsync(new CreateUpdateDailyShiftDto
        {
            GarageId = 1,
            OrganizationId = 1,
            ShiftId = 1,
            KeeperId = new Guid("3a182b4a-115e-aa2c-4665-44084dbd9815"),
            TenantId = new Guid("3a177b65-6973-9fcf-cc7d-e988aea9085e") , 
            RequestTime = new DateTime(2025 , 2 , 27 ,14 , 0 , 1),
            KeeperShiftRequestType = Enums.KeeperShiftRequestType.StartShift
        });

        Assert.NotNull(result);
        
    }


    [Fact]
    public async Task TestSeedDataForShift()
    {
        var result = await _shiftAppService.GetListAsync(new PagedAndSortedResultRequestDto
        {
            MaxResultCount = 100,
            SkipCount = 0  ,
            Sorting = "Id"
        });

        Assert.NotNull(result);

    }

    [Fact]
    public async Task TestSeedDataForKeeperShift()
    {
        var result = await _keeperShiftsAppService.GetListAssignedShiftsToUserForMdkhal( null,new Guid("3a182b4a-115e-aa2c-4665-44084dbd9815") ,  new PagedAndSortedAndSearchResultRequestDto
        {
            MaxResultCount = 100,
            SkipCount = 0,
            Sorting = "Id"
        });


        Assert.Equal(2, result.TotalCount);
      //  Assert.NotNull(result);

    }
}