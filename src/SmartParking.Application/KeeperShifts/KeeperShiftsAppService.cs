using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Shifts;
using SmartParking.Entities.Vehicles;
using SmartParking.Localization;
using SmartParking.Repositories;
using SmartParking.Vehicles;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
namespace SmartParking.KeeperShifts;

public class KeeperShiftsAppService :ApplicationService, IKeeperShiftsAppService
{
    private readonly IRepository<KeeperShift> _keeperShiftRepository;
    private readonly IRepository<Garage> _garageRepository;
    private readonly IRepository<Organization> _organizationRepository;
    private readonly IRepository<Shift> _shiftRepository;
    private readonly IMyUserRepository _userRepository;
    private IDataFilter _dataFilter;
    private readonly ICurrentUser _currentUser;

    public KeeperShiftsAppService(IRepository<KeeperShift, int> repository,
       IDataFilter dataFilter, ICurrentUser currentUser, IRepository<Garage, int> garagerepository ,IRepository<Organization, int> orgrepository, IRepository<Shift, int> shiftrepository, IMyUserRepository userrepository)
      
    {
        _keeperShiftRepository = repository;
        _garageRepository = garagerepository;
        _organizationRepository = orgrepository;
        _shiftRepository = shiftrepository;
        _userRepository = userrepository;
        _dataFilter = dataFilter;
        _currentUser = currentUser;
    }

    public  async Task<bool> CreateAsync(CreateUpdateKeeperShiftDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var shift = await _shiftRepository.FirstOrDefaultAsync(s => s.Id == input.ShiftId);
            if (shift == null)
            {
                throw new BusinessException(LocalizationHelper.GetLocalizedString("Shifts") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }

            await OverlappingValidation(input, shift);

            var keeperShift = new KeeperShift
            {
                TenantId = input.TenantId,
                KeeperId = input.KeeperId,
                OrganizationId = shift.OrganizationId,
                GarageId = shift.GarageId,
                ShiftId = input.ShiftId,
                StartDate = shift.StartDate,
                EndDate = shift.EndDate,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime
            };

            await _keeperShiftRepository.InsertAsync(keeperShift, true);
            return true;
        }
    }

    private async Task OverlappingValidation(CreateUpdateKeeperShiftDto input, Shift? shift)
    {
        bool isOverlapping = await _keeperShiftRepository.AnyAsync(ks =>
         ks.KeeperId == input.KeeperId &&
         ks.ShiftId != input.ShiftId &&  
         (
              (shift.StartDate == ks.StartDate && shift.EndDate == ks.EndDate) ||

              (shift.StartDate >= ks.StartDate && shift.EndDate <= ks.EndDate) ||

              (shift.StartDate <= ks.StartDate && shift.EndDate >= ks.StartDate)

         ) &&
         (
             (shift.StartTime < ks.EndTime && shift.EndTime > ks.StartTime) ||
             (shift.StartTime == ks.StartTime && shift.EndTime == ks.EndTime) 
         )
     );

        if (isOverlapping)
        {
            throw new BusinessException(LocalizationHelper.GetLocalizedString("KeeperAlreadyAssignedToShift"));
        }
    }

    public async Task<PagedResultDto<KeeperShiftDto>> GetListKeeperShiftAsync(Guid? TenantId, PagedAndSortedResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryKeeperShift = await _keeperShiftRepository.GetQueryableAsync();
            var queryGarage = await _garageRepository.GetQueryableAsync();
            var queryOrganization = await _organizationRepository.GetQueryableAsync();
            var queryUser = await _userRepository.GetQueryableAsync();
            var queryShift = await _shiftRepository.GetQueryableAsync();

            var result = from keepershift in queryKeeperShift
                         where TenantId.HasValue ? keepershift.TenantId == TenantId : true
                         join organization in queryOrganization
                         on keepershift.OrganizationId equals organization.Id
                         join garage in queryGarage
                         on keepershift.GarageId equals garage.Id
                         join user in queryUser
                         on keepershift.KeeperId equals user.Id
                         join shift in queryShift
                         on keepershift.ShiftId equals shift.Id

                         select new KeeperShiftDto
                         {
                             Id = keepershift.Id,
                             KeeperId = keepershift.KeeperId,
                             KeeperName = user.Name,
                             OrganizationId = keepershift.OrganizationId,
                             OrganizationNameAr = organization.ArabicName,
                             OrganizationNameEn = organization.EnglishName,
                             GarageNameAr = garage.NameAr,
                             GarageNameEn = garage.NameEn,
                             GarageId = keepershift.GarageId,
                             ShiftId = keepershift.ShiftId,
                             StartDate = keepershift.StartDate,
                             EndDate = keepershift.EndDate,
                             StartTime = keepershift.StartTime,
                             EndTime = keepershift.EndTime
                         };

            int total = ApplySorting(input, ref result);

            return new PagedResultDto<KeeperShiftDto>
            {
                TotalCount = total,
                Items = result.ToList()
            };
        }
    }

    private static int ApplySorting(PagedAndSortedResultRequestDto input, ref IQueryable<KeeperShiftDto> result)
    {
        var sorting = input.Sorting;
        if (!string.IsNullOrEmpty(sorting) && sorting.Contains("DESC"))
        {
            var sortingSplit = sorting.Split(' ').ToList();
            sorting = sortingSplit[0] + " descending";
        }

        var total = result.Count();
        result = result.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount);
        return total;
    }
    public Task<KeeperShiftDto> GetAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}
