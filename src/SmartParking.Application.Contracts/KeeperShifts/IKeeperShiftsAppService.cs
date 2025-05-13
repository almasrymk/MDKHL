using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartParking.Entities;
using SmartParking.KeeperShifts;
using SmartParking.Vehicles;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace SmartParking.KeeperShifts
{
    /// <summary>
    /// contract for managing Keeper Shifts, including creation, retrieval, and assignment operations.
    /// </summary>
    public interface IKeeperShiftsAppService : IApplicationService, ITransientDependency
    {
        /// <summary>
        /// Creates a new Keeper Shift.
        /// </summary>
        /// <param name="input">The data used to create the shift.</param>
        /// <returns>True if creation was successful; otherwise, false.</returns>
        Task<bool> CreateAsync(CreateUpdateKeeperShiftDto input);

        /// <summary>
        /// Retrieves the list of shifts assigned to the current user.
        /// </summary>
        /// <param name="TenantId">Optional tenant identifier.</param>
        /// <param name="input">Pagination, sorting, and search parameters.</param>
        /// <returns>Paged result of <see cref="KeeperShiftDto"/> for the current user.</returns>
        Task<PagedResultDto<KeeperShiftDto>> GetMyShiftsForCurrentUser(Guid? TenantId, PagedAndSortedAndSearchResultRequestDto input);

        /// <summary>
        /// Retrieves a single Keeper Shift by its ID.
        /// </summary>
        /// <param name="id">The ID of the shift to retrieve.</param>
        /// <returns>The matching <see cref="KeeperShiftDto"/>, or null if not found.</returns>
        Task<KeeperShiftDto?> GetAsync(int id);

        /// <summary>
        /// Deletes a Keeper Shift by its ID.
        /// </summary>
        /// <param name="id">The ID of the shift to delete.</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Retrieves a paginated list of Keeper Shifts assigned to a specific user for Mdakhal context.
        /// </summary>
        /// <param name="tenantId">Optional tenant identifier.</param>
        /// <param name="userId">The ID of the user to retrieve shifts for.</param>
        /// <param name="input">Pagination, sorting, and search parameters.</param>
        /// <returns>Paged result of <see cref="KeeperShiftDto"/> for Mdakhal.</returns>
        Task<PagedResultDto<KeeperShiftDto>> GetListAssignedShiftsToUserForMdkhal(Guid? tenantId, Guid? userId, PagedAndSortedAndSearchResultRequestDto input);

        /// <summary>
        /// Retrieves a paginated list of Keeper Shifts assigned to a specific user for organization context.
        /// </summary>
        /// <param name="tenantId">Optional tenant identifier.</param>
        /// <param name="userId">The ID of the user to retrieve shifts for.</param>
        /// <param name="input">Pagination, sorting, and search parameters.</param>
        /// <returns>Paged result of <see cref="KeeperShiftDto"/> for the organization.</returns>
        Task<PagedResultDto<KeeperShiftDto>> GetListAssignedShiftsToUserForOrganization(Guid? tenantId, Guid? userId, PagedAndSortedAndSearchResultRequestDto input);

        /// <summary>
        /// Retrieves a paginated list of users eligible to be assigned shifts in the organization context.
        /// </summary>
        /// <param name="input">Pagination, sorting, and search parameters.</param>
        /// <returns>Paged result of users with roles.</returns>
        Task<PagedResultDto<UserWithRole>> GetListUserCanAssignShiftsOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input);

        /// <summary>
        /// Retrieves a paginated list of users eligible to be assigned shifts in the Mdakhal context.
        /// </summary>
        /// <param name="tenantId">Optional tenant identifier.</param>
        /// <param name="input">Pagination, sorting, and search parameters.</param>
        /// <returns>Paged result of users with roles for Mdakhal.</returns>
        Task<PagedResultDto<UserWithRole>> GetListUserCanAssignShiftsMdkhlAsync(Guid? tenantId, PagedAndSortedAndSearchResultRequestDto input);
    }



}


