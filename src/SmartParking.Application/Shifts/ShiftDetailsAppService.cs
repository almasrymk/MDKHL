using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Shifts;
using SmartParking.Shifts.ShiftDetail;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Shifts;

public class ShiftDetailsAppService : CrudAppService<ShiftDetails, ShiftDetailsDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateShiftDetailsDto>, IShiftDetailsAppService
{
    private readonly IRepository<ShiftDetails, Guid> _repository;
    private readonly IRepository<ShiftZone, Guid> _shiftZoneRepository;
    private readonly IDataFilter _dataFilter;
    public ShiftDetailsAppService(
        IRepository<ShiftDetails, Guid> repository,
        IRepository<ShiftZone, Guid> shiftZoneRepository,
        IDataFilter dataFilter) : base(repository)
    {
        _repository = repository;
        _shiftZoneRepository = shiftZoneRepository;
        _dataFilter = dataFilter;
    }

    public async Task<ShiftDetailsDto?> GetAllListAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var entities = await _repository.GetListAsync();
            var entity = entities.SingleOrDefault();
            if (entity == null)
                return null;
            var dto = ObjectMapper.Map<ShiftDetails, ShiftDetailsDto>(entity);
            return dto;
        }
    }

    public async Task<ShiftDetailsDto?> GetByShiftZoneAsync(Guid shiftZoneId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var shiftZone = await _shiftZoneRepository.SingleOrDefaultAsync(z => z.Id == shiftZoneId);

            if (shiftZone == null)
                return null;

            var queryable = await _repository.GetQueryableAsync();

            var details = await queryable
                .Include(sd => sd.ShiftStaffDetails)
                .Include(sd => sd.ShiftDevices)
                .FirstOrDefaultAsync(sd => sd.Id == shiftZone.ShiftDetailsId);

            if (details == null)
                return null;

            return ObjectMapper.Map<ShiftDetails, ShiftDetailsDto>(details);
        }
    }
}