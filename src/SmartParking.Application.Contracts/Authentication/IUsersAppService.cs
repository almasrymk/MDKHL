

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace SmartParking.Authentication;

public interface IUsersAppService:  IApplicationService
{
 
    /// <summary>
    /// Asynchronously retrieves a list of users associated with a specific organization ID.
    /// </summary>
    /// <param name="organizationId">The unique identifier of the organization for which the users are to be retrieved.</param>
    /// <returns>A task representing the asynchronous operation, containing a list of <see cref="UserDto"/> objects for the specified organization.</returns>
    /// <remarks>
    /// This method retrieves the user data for all users that belong to the organization identified by the given <paramref name="organizationId"/>.
    /// If no users are found for the given organization, the returned list will be empty.
    /// </remarks>
    Task<List<UserDto>> GetUserByOrganizationId(int organizationId);
    /// <summary>
    /// Asynchronously retrieves the organization ID associated with a given tenant ID.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant for which the organization ID is to be retrieved. Can be null.</param>
    /// <returns>A task representing the asynchronous operation, containing the organization ID as an integer. If the tenant ID is not found, the result will be 0.</returns>
    /// <remarks>
    /// This method is useful for retrieving the organization ID linked to a specific tenant in a multi-tenant system. If the tenant ID is null or no corresponding organization is found, the method returns 0.
    /// </remarks>
    Task<int> GetOrganizationIdByTenantAsync(Guid? tenantId);
    /// <summary>
    /// Asynchronously retrieves a paginated list of users based on the provided input.
    /// </summary>
    /// <param name="input">The pagination and filtering criteria for retrieving the users.</param>
    /// <returns>A task representing the asynchronous operation, containing a <see cref="PagedResultDto{UserDto}"/> result with the list of users and pagination metadata.</returns>
    /// <remarks>
    /// This method returns a <see cref="PagedResultDto{UserDto}"/> that includes the list of user DTOs and information about 
    /// the total number of users available, allowing clients to handle pagination easily.
    /// </remarks>
    Task<PagedResultDto<UserDto>> GetListMdklAsync(PagedResultRequestDto input);    
    
    /// <summary>
    /// Asynchronously retrieves a paginated list of users based on the provided input.
    /// </summary>
    /// <param name="input">The pagination and filtering criteria for retrieving the users.</param>
    /// <returns>A task representing the asynchronous operation, containing a <see cref="PagedResultDto{UserDto}"/> result with the list of users and pagination metadata.</returns>
    /// <remarks>
    /// This method returns a <see cref="PagedResultDto{UserDto}"/> that includes the list of user DTOs and information about 
    /// the total number of users available, allowing clients to handle pagination easily.
    /// </remarks>
    Task<PagedResultDto<UserDto>> GetListForOrganizationAsync(PagedResultRequestDto input);

    /// <summary>
    /// Searches for a paged and sorted list of users based on the specified input parameters.
    /// </summary>
    /// <param name="input">The search criteria, including pagination, sorting, and search filters.</param>
    /// <returns>A <see cref="PagedResultDto{UserDto}"/> containing a list of users matching the search criteria, along with pagination details.</returns>
    Task<PagedResultDto<UserDto>> Search(PagedAndSortedAndSearchResultRequestDto input);
    Task<PagedResultDto<UserDto>> SearchForOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input);

    Task<PagedResultDto<UserDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
    Task<PagedResultDto<UserDto>> FilterForOrganizationAsync(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Updates an existing user with the specified ID using the provided input data.
    /// </summary>
    /// <param name="id">The unique identifier of the user to be updated.</param>
    /// <param name="input">The <see cref="UserCreateDto"/> object containing the updated user details.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the updated <see cref="UserDto"/> object.
    /// </returns>
    Task<UserDto> UpdateAsync(Guid? id, UpdateUserDto input);
    Task<UserDto> UpdateUserWithNoRoleAsync(Guid? id, UpdateUserDto input);
    Task<UserDto> UpdateProfileDataAsync(Guid? id, UpdateUserDto input);

    Task<UserDto> GetAsync(Guid? id);

    Task<UserDto> AssignRoleToUser(Guid userId,string roleName);
    Task<UserDto> UpdateImageAsync(Guid? id, string Image);

    /// <summary>
    /// Asynchronously creates a new user based on the provided user creation data.
    /// </summary>
    /// <param name="input">The data for the new user to be created, encapsulated in a <see cref="UserCreateDto"/> object.</param>
    /// <returns>A task representing the asynchronous operation, containing the created <see cref="UserDto"/> of the newly created user.</returns>
    /// <remarks>
    /// This method creates a new user in the system using the information provided in the <paramref name="input"/> parameter. 
    /// Upon successful creation, it returns the <see cref="UserDto"/> representing the newly created user, including its ID and other details.
    /// </remarks>
    Task<UserDto> CreateWithRoleAsync(UserCreateDto input, Guid roleId);
    Task<IdentityRole> FindRoleByTenantIdAsync(string roleName,Guid? tenantId);
    Task<UserDto> CheckPhoneIsUnique(string phone);
    Task<IdentityUser?> IsUserFound(Guid? userId);
}
