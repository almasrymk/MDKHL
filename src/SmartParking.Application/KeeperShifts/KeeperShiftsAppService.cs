using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Shifts;
using SmartParking.Localization;
using SmartParking.Repositories;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace SmartParking.KeeperShifts;

public class KeeperShiftsAppService : ApplicationService, IKeeperShiftsAppService
{
    private readonly IRepository<KeeperShift,int> _keeperShiftRepository;
    private readonly IRepository<Garage,int> _garageRepository;
    private readonly IRepository<Organization,int> _organizationRepository;
    private readonly IRepository<Shift,int> _shiftRepository;
    private readonly IMyUserRepository _userRepository;
    private readonly IDataFilter _dataFilter;

    public KeeperShiftsAppService(IRepository<KeeperShift, int> repository
       ,  IRepository<Garage, int> garagerepository, IRepository<Organization, int> orgrepository,
       IRepository<Shift, int> shiftrepository, IMyUserRepository userrepository,
        IDataFilter dataFilter)

    {
        _keeperShiftRepository = repository;
        _garageRepository = garagerepository;
        _organizationRepository = orgrepository;
        _shiftRepository = shiftrepository;
        _userRepository = userrepository;
        _dataFilter = dataFilter;
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
    public async Task<KeeperShiftDto?> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryKeeperShift = await _keeperShiftRepository.GetQueryableAsync();
            var queryGarage = await _garageRepository.GetQueryableAsync();
            var queryOrganization = await _organizationRepository.GetQueryableAsync();
            var queryUser = await _userRepository.GetQueryableAsync();
            var queryShift = await _shiftRepository.GetQueryableAsync();

            var result = from keepershift in queryKeeperShift
                         where keepershift.Id == id
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
        var users = await _userRepository.FindUserShiftsOrganizationAsync(input.SkipCount,input.MaxResultCount,input.Sorting,input.SearchingTerm);
        return new PagedResultDto<UserWithRole>
        {
            TotalCount = users.PageSize,
            Items = users.Queryable.ToList()
        };
    } 
    
    public async Task<PagedResultDto<UserWithRole>> GetListUserCanAssignShiftsMdkhlAsync(Guid? tenantId,PagedAndSortedAndSearchResultRequestDto input)
    {

        using (_dataFilter.Disable<IMultiTenant>())
        {
            var users = await _userRepository.FindUserShiftsMdkhlAsync(input.SkipCount, input.MaxResultCount, input.Sorting, input.SearchingTerm, tenantId);
            return new PagedResultDto<UserWithRole>
            {
                TotalCount = users.PageSize,
                Items = users.Queryable.ToList()
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

    private static IQueryable<UserWithRole> ApplySortingForUser(PagedAndSortedResultRequestDto input, IQueryable<UserWithRole> result)
    {
        var sorting = input.Sorting;
        if (!string.IsNullOrEmpty(sorting) && sorting.Contains("DESC"))
        {
            var sortingSplit = sorting.Split(' ').ToList();
            sorting = sortingSplit[0] + " descending";
        }

        result = result.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount);
        return result;
    }

}
