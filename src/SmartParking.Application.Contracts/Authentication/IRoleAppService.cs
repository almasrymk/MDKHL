using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace SmartParking.Authentication;

/// <summary>
/// Interface for role  service
/// </summary>
public interface IRoleAppService: IApplicationService
{
    /// <summary>
    /// Retrieves a list of <see cref="IdentityRoleDto"/> for the specified tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier (nullable).</param>
    /// <returns>A list of <see cref="IdentityRoleDto"/>.</returns>
    Task<List<IdentityRoleDto>> GetListByTenant(Guid? tenantId);

    /// <summary>
    /// Creates a new role using the provided <see cref="IdentityRoleCreateDto"/>.
    /// </summary>
    /// <param name="roleCreateDto">The data transfer object containing the information for the role to be created.</param>
    /// <param name="tenantId">The tenant identifier (optional).</param>
    /// <returns>The created <see cref="IdentityRoleDto"/>.</returns>
    Task<IdentityRoleDto> Create(IdentityRoleCreateDto roleCreateDto, [Optional] Guid? tenantId);

    /// <summary>
    /// Updates an existing role identified by the specified <paramref name="Id"/>.
    /// </summary>
    /// <param name="Id">The identifier of the role to update.</param>
    /// <param name="updateDto">The data transfer object containing the updated information for the role.</param>
    /// <returns>The updated <see cref="IdentityRoleDto"/>.</returns>
    Task<IdentityRoleDto> Update(Guid Id, IdentityRoleUpdateDto updateDto);

    /// <summary>
    /// Retrieves a paged list of <see cref="IdentityRoleDto"/> for organizations, 
    /// based on the specified search, sort, and paging criteria.
    /// </summary>
    /// <param name="input">The pagination, sorting, and search parameters.</param>
    /// <param name="tenantId">The tenant identifier (nullable).</param>
    /// <returns>A paged result of <see cref="IdentityRoleDto"/>.</returns>
    Task<PagedResultDto<IdentityRoleDto>> GetListAndPagedOrganization(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Retrieves a paged list of <see cref="RoleDto"/> for Mdkhl, 
    /// based on the specified search, sort, and paging criteria.
    /// </summary>
    /// <param name="input">The pagination, sorting, and search parameters.</param>
    /// <param name="tenantId">The tenant identifier (nullable).</param>
    /// <returns>A paged result of <see cref="RoleDto"/>.</returns>
    Task<PagedResultDto<RoleDto>> GetListAndPagedMdkhl(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Searches for roles in organizations based on the specified criteria, 
    /// and returns a paged list of <see cref="IdentityRoleDto"/>.
    /// </summary>
    /// <param name="input">The pagination, sorting, and search parameters.</param>
    /// <param name="tenantId">The tenant identifier (nullable).</param>
    /// <returns>A paged result of <see cref="IdentityRoleDto"/>.</returns>
    Task<PagedResultDto<IdentityRoleDto>> SearchOrganization(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Searches for roles in Mdkhl based on the specified criteria, 
    /// and returns a paged list of <see cref="RoleDto"/>.
    /// </summary>
    /// <param name="input">The pagination, sorting, and search parameters.</param>
    /// <returns>A paged result of <see cref="RoleDto"/>.</returns>
    Task<PagedResultDto<RoleDto>> SearchMdkhl(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Retrieves a role by its name for Mdkhl.
    /// </summary>
    /// <param name="roleName">The name of the role to retrieve.</param>
    /// <returns>The <see cref="IdentityRole"/> corresponding to the specified role name.</returns>
    Task<IdentityRole> GetRoleByNameMdkhlAsync(string roleName);

    /// <summary>
    /// Retrieves a role by its name.
    /// </summary>
    /// <param name="roleName">The name of the role to retrieve.</param>
    /// <returns>The <see cref="IdentityRole"/> corresponding to the specified role name.</returns>
    Task<IdentityRole> GetRoleByNameAsync(string roleName);

    /// <summary>
    /// Retrieves a role by its identifier.
    /// </summary>
    /// <param name="Id">The identifier of the role to retrieve.</param>
    /// <returns>The <see cref="IdentityRoleDto"/> corresponding to the specified role identifier.</returns>
    Task<IdentityRoleDto> GetRoleById(Guid Id);

    /// <summary>
    /// Retrieves an identity role by its identifier.
    /// </summary>
    /// <param name="Id">The identifier of the role to retrieve.</param>
    /// <returns>The <see cref="IdentityRole"/> corresponding to the specified role identifier.</returns>
    Task<IdentityRole> GetIdentityRoleById(Guid Id);

    /// <summary>
    /// Assigns a role to a user by their identifiers.
    /// </summary>
    /// <param name="UserId">The identifier of the user to assign the role to.</param>
    /// <param name="RoleId">The identifier of the role to assign to the user.</param>
    Task AssignRole(Guid UserId, Guid RoleId);

    /// <summary>
    /// Deletes a role by its identifier.
    /// </summary>
    /// <param name="RoleId">The identifier of the role to delete.</param>
    /// <returns>The deleted <see cref="IdentityRoleDto"/>.</returns>
    Task<IdentityRoleDto> Delete(Guid RoleId);

}
