using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartParking.Authentication;
using SmartParking.Entities;
using SmartParking.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Shifts;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Repositories;
using SmartParking.Results;
using SmartParking.Vehicles;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;
namespace SmartParking.KeeperShifts;

public class KeeperShiftsAppService :ApplicationService, IKeeperShiftsAppService
{
    private readonly IRepository<KeeperShift> _keeperShiftRepository;
    private readonly IRepository<Garage> _garageRepository;
    private readonly IRepository<Organization> _organizationRepository;
    private readonly IRepository<Shift> _shiftRepository;
    private readonly IMyUserRepository _userRepository;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;
    private IDataFilter _dataFilter;
    private readonly ICurrentUser _currentUser;

    public KeeperShiftsAppService(IRepository<KeeperShift, int> repository,
       IDataFilter dataFilter, ICurrentUser currentUser, IRepository<Garage, int> garagerepository ,IRepository<Organization, int> orgrepository,
       IRepository<Shift, int> shiftrepository, IMyUserRepository userrepository,
       IRepository<IdentityRole, Guid> roleRepository)
      
    {
        _keeperShiftRepository = repository;
        _garageRepository = garagerepository;
        _organizationRepository = orgrepository;
        _shiftRepository = shiftrepository;
        _userRepository = userrepository;
        _roleRepository = roleRepository;
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

    public async Task<PagedResultDto<KeeperShiftDto>> GetMyShiftsForCurrentUser(Guid? userId, PagedAndSortedResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryKeeperShift = await _keeperShiftRepository.GetQueryableAsync();
            var queryGarage = await _garageRepository.GetQueryableAsync();
            var queryOrganization = await _organizationRepository.GetQueryableAsync();
            var queryUser = await _userRepository.GetQueryableAsync();
            var queryShift = await _shiftRepository.GetQueryableAsync();

            var result = from keepershift in queryKeeperShift
                         where userId.HasValue ? keepershift.KeeperId == userId : true
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
                             GarageNameAr = garage.NameAr,
                             GarageNameEn = garage.NameEn,
                             GarageId = keepershift.GarageId,
                             ShiftId = keepershift.ShiftId,
                             StartDate = keepershift.StartDate,
                             EndDate = keepershift.EndDate,
                             StartTime = keepershift.StartTime,
                             EndTime = keepershift.EndTime , 
                             ShiftNameAr = shift.ArabicName,
                             ShiftNameEn = shift.EnglishName
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
    public async Task<KeeperShiftDto?> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryKeeperShift = await _keeperShiftRepository.GetQueryableAsync();
            var queryGarage = await _garageRepository.GetQueryableAsync();
            var queryOrganization = await _organizationRepository.GetQueryableAsync();
            var queryUser = await _userRepository.GetQueryableAsync();
            var queryShift = await _shiftRepository.GetQueryableAsync();
            var IsFoundKeeperShift = await _keeperShiftRepository.FirstOrDefaultAsync(_ => _.Id == id);
            if (IsFoundKeeperShift is null)
                throw new Exception("NotFoundValidation");
            var result = from keepershift in queryKeeperShift
                         where keepershift.Id == id
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
                             GarageNameAr = garage.NameAr,
                             GarageNameEn = garage.NameEn,
                             GarageId = keepershift.GarageId,
                             ShiftId = keepershift.ShiftId,
                             StartDate = keepershift.StartDate,
                             EndDate = keepershift.EndDate,
                             StartTime = keepershift.StartTime,
                             EndTime = keepershift.EndTime,
                             ArabicName = shift.ArabicName,
                             EnglishName = shift.EnglishName
                             
                         };
            return await result.FirstOrDefaultAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var shift = await _keeperShiftRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (shift is null)
            {
                throw new BusinessException(LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }

            await _keeperShiftRepository.HardDeleteAsync(shift,true);
        }
    }

    public async Task<PagedResultDto<UserWithRole>> GetListUserCanAssignShiftsOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        var users = await _userRepository.GetQueryableAsync();
        var roles = await _roleRepository.GetQueryableAsync();

        var query = from user in users
                    where EF.Property<UserTypeCategory>(user, "UserTypeCategory") != 0
                    from roleId in user.Roles.Select(r => r.RoleId)
                    join role in roles on roleId equals role.Id
                    where BuildExperssionSearchInUser(input, user, role)
                    select new UserWithRole
                    {
                        User = user,
                        Role = role

                    };
        int total = ApplySortingForUser(input, ref query);
        var result = await AsyncExecuter.ToListAsync(query);
        return new PagedResultDto<UserWithRole>
        {
            TotalCount = total,
            Items = result
        };
    } 
    
    public async Task<PagedResultDto<UserWithRole>> GetListUserCanAssignShiftsMdkhlAsync(Guid? tenantId,PagedAndSortedAndSearchResultRequestDto input)
    {

        using (_dataFilter.Disable<IMultiTenant>())
        {
            var users = await _userRepository.GetQueryableAsync();
            var roles = await _roleRepository.GetQueryableAsync();

            var query = from user in users
                        where (EF.Property<UserTypeCategory>(user, "UserTypeCategory") != 0) &&
                        (tenantId.HasValue ? user.TenantId == tenantId : true)
                        from roleId in user.Roles.Select(r => r.RoleId)
                        join role in roles on roleId equals role.Id
                        where BuildExperssionSearchInUser(input, user, role)
                        select new UserWithRole
                        {
                            User = user,
                            Role = role

                        };
            int total = ApplySortingForUser(input, ref query);
            var result = await AsyncExecuter.ToListAsync(query);
            return new PagedResultDto<UserWithRole>
            {
                TotalCount = total,
                Items = result
            };
        }
    }

    private static bool BuildExperssionSearchInUser(PagedAndSortedAndSearchResultRequestDto input, IdentityUser user, IdentityRole role)
    {
        return !input.SearchingTerm.IsNullOrWhiteSpace() ?
                                    user.Name.ToLower().Contains(input.SearchingTerm.ToLower())
                                    || role.Name.ToLower().Contains(input.SearchingTerm.ToLower())
                                    || user.PhoneNumber.ToLower().Contains(input.SearchingTerm.ToLower())
                                    || user.UserName.ToLower().Contains(input.SearchingTerm.ToLower())
                                    || user.Email.ToLower().Contains(input.SearchingTerm.ToLower())
                                    : true;
    }
    private static int ApplySortingForUser(PagedAndSortedResultRequestDto input, ref IQueryable<UserWithRole> result)
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

}
