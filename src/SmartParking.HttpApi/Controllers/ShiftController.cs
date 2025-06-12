using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polly.Caching;
using SmartParking.DailyShifts;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Shifts;
using SmartParking.Enums;
using SmartParking.KeeperShifts;
using SmartParking.Repositories;
using SmartParking.Results;
using SmartParking.SingletonService;
using SmartParking.StateManagements;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Controllers
{
    [Route("api/mobile/shift")]
    [ApiController]
    public class ShiftController : AbpController
    {
        private readonly IRepository<DailyShift, int> _repository;
        private readonly IRepository<Garage, int> _garageRepository;
        private readonly IMyUserRepository _userRepository;
        private readonly IRepository<Organization, int> _organizationRepository;
        private readonly IRepository<Shift, int> _shiftRepository;
        private readonly IRepository<KeeperShift, int> _keeperShiftRepository;
        private readonly IDataFilter _dataFilter;
        private readonly IMapper _mapper;
        private readonly IDailyShiftAppService _dailyShiftAppService;
        private readonly IKeeperShiftsAppService _keeperShiftAppService;
        private readonly ISingletonService<StateManagementDto, int> _singletonService;

        public ShiftController(IRepository<KeeperShift, int> keeperrepository
           , IRepository<Garage, int> garagerepository, IRepository<Organization, int> orgrepository,
           IRepository<Shift, int> shiftrepository, IMyUserRepository userrepository,
            IDataFilter dataFilter, IMapper mapper, IRepository<DailyShift, int> repository, IDailyShiftAppService dailyShiftAppService , IKeeperShiftsAppService keeperShiftAppService, ISingletonService<StateManagementDto, int> singletonService)

        {
            _keeperShiftRepository = keeperrepository;
            _garageRepository = garagerepository;
            _organizationRepository = orgrepository;
            _shiftRepository = shiftrepository;
            _userRepository = userrepository;
            _dataFilter = dataFilter;
            _mapper = mapper;
            _repository = repository;
            _dailyShiftAppService = dailyShiftAppService;
            _singletonService = singletonService;
            _keeperShiftAppService = keeperShiftAppService;
        }

        [HttpPost("startshift")]
        [Authorize]
        public async Task<Result<DailyShiftDto>> StartShift([FromBody]CreateUpdateDailyShiftDto keeperShiftDto)
        {
            try
            {
                var result = await _dailyShiftAppService.CreateAsync(keeperShiftDto);
                var state = await _singletonService.SetAsync(result.Id);
                return Result.Success(result, "Started Succesfully");
            }
            catch (Exception)
            {
                return Result.Error("Can't  Start");
            }
        }
        [HttpPost("endshift")]
        [Authorize]
        public async Task<Result<DailyShiftDto>> EndShift([FromBody] CreateUpdateDailyShiftDto keeperShiftDto)
        {
            try
            {
                var result = await _dailyShiftAppService.CreateAsync(keeperShiftDto);
                await _singletonService.DisposeAsync();
                return Result.Success(result, "Ended Succesfully");
            }
            catch (Exception)
            {
                return Result.Error("Can't End");
            }
        }
        [HttpGet("getmyshifts")]
        [Authorize]
        public async Task<Result<List<KeeperShiftDto>>> GetMyShifts()
        {
            var userId = CurrentUser.Id;
            var shifts = await _keeperShiftAppService.GetAllMyShiftsForCurrentUser(userId);
            return Result<List<KeeperShiftDto>>.Success(shifts);
        }

    }
}
