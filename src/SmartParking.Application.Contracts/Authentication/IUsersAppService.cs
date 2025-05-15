

using SmartParking.Entities;
using SmartParking.Entities.Subscriptions;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace SmartParking.Authentication;

/// <summary>
/// interface for user service
/// </summary>
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
    Task<PagedResultDto<UserWithRole>> GetListMdklAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Asynchronously retrieves a paginated list of users based on the provided input.
    /// </summary>
    /// <param name="input">The pagination and filtering criteria for retrieving the users.</param>
    /// <returns>A task representing the asynchronous operation, containing a <see cref="PagedResultDto{UserDto}"/> result with the list of users and pagination metadata.</returns>
    /// <remarks>
    /// This method returns a <see cref="PagedResultDto{UserDto}"/> that includes the list of user DTOs and information about 
    /// the total number of users available, allowing clients to handle pagination easily.
    /// </remarks>
    Task<PagedResultDto<UserWithRole>> GetListForOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input);
    /// <summary>
    /// Searches for a paged and sorted list of users based on the specified input parameters.
    /// </summary>
    /// <param name="input">The search criteria, including pagination, sorting, and search filters.</param>
    /// <returns>A <see cref="PagedResultDto{UserDto}"/> containing a list of users matching the search criteria, along with pagination details.</returns>
    Task<PagedResultDto<UserWithRole>> Search(PagedAndSortedAndSearchResultRequestDto input);
    /// <summary>
    /// Searches for users within an organization with pagination, sorting, and search functionality.
    /// </summary>
    /// <param name="input">The input DTO containing the search, pagination, and sorting parameters.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="PagedResultDto{UserWithRole}"/> as the result containing a paged list of users with roles.</returns>
    Task<PagedResultDto<UserWithRole>> SearchForOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Filters users based on specified filtering, pagination, and sorting parameters.
    /// </summary>
    /// <param name="input">The input DTO containing the filter, pagination, and sorting parameters.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="PagedResultDto{UserDto}"/> as the result containing a paged list of filtered users.</returns>
    Task<PagedResultDto<UserDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Filters users within an organization based on specified filtering, pagination, and sorting parameters.
    /// </summary>
    /// <param name="input">The input DTO containing the filter, pagination, and sorting parameters specific to the organization.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="PagedResultDto{UserDto}"/> as the result containing a paged list of filtered users within the organization.</returns>
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
    Task<UserDto> UpdateUserWithRoleChangeAsync(Guid? id, UpdateUserDto input);
    /// <summary>
    /// Updates a user's information without changing their roles.
    /// </summary>
    /// <param name="id">The ID of the user to be updated.</param>
    /// <param name="input">The data transfer object containing the updated user information.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="UserDto"/> as the result containing the updated user information.</returns>
    Task<UserDto> UpdateUserWithNoRoleAsync(Guid? id, UpdateUserDto input);

    /// <summary>
    /// Updates the profile data of a user.
    /// </summary>
    /// <param name="id">The ID of the user whose profile data is to be updated.</param>
    /// <param name="input">The data transfer object containing the updated profile information.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="UserDto"/> as the result containing the updated user profile data.</returns>
    Task<UserDto> UpdateProfileDataAsync(Guid? id, UpdateUserDto input);

    /// <summary>
    /// Updates the profile data for a user in the mobile app context.
    /// </summary>
    /// <param name="id">The ID of the user whose profile data is to be updated.</param>
    /// <param name="input">The data transfer object containing the updated profile information for mobile use.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="Result{UserDataDto}"/> as the result containing the updated user profile data or an error message.</returns>
    Task<Result<UserDataDto>> UpdateProfileDataMobileAsync(Guid? id, UserDataDto input);

    /// <summary>
    /// Retrieves a user's information based on their ID.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="UserDto"/> as the result containing the user's information.</returns>
    Task<UserDto> GetAsync(Guid? id);

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="userId">The ID of the user to whom the role will be assigned.</param>
    /// <param name="roleName">The name of the role to assign to the user.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="UserDto"/> as the result containing the user’s updated information.</returns>
    Task<UserDto> AssignRoleToUser(Guid userId, string roleName);

    /// <summary>
    /// Updates the image associated with a user's profile.
    /// </summary>
    /// <param name="id">The ID of the user whose profile image is to be updated.</param>
    /// <param name="image">The new image (as a string representing the image path or URL).</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="UserDto"/> as the result containing the updated user information.</returns>
    Task<UserDto> UpdateImageAsync(Guid? id, string image);


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
    /// <summary>
    /// Finds the role associated with a given role name and tenant ID.
    /// </summary>
    /// <param name="roleName">The name of the role to search for.</param>
    /// <param name="tenantId">The ID of the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, with an <see cref="IdentityRole"/> as the result.</returns>
    Task<IdentityRole> FindRoleByTenantIdAsync(string roleName, Guid? tenantId);

    /// <summary>
    /// Checks if the provided phone number is unique.
    /// </summary>
    /// <param name="phone">The phone number to check for uniqueness.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="UserDto"/> as the result if the phone is unique.</returns>
    Task<UserDto> CheckPhoneIsUnique(string phone);

    /// <summary>
    /// Checks if a user is found based on their ID.
    /// </summary>
    /// <param name="userId">The ID of the user to search for.</param>
    /// <returns>A task that represents the asynchronous operation, with an <see cref="IdentityUser"/> as the result if the user is found, otherwise null.</returns>
    Task<IdentityUser?> IsUserFound(Guid? userId);

    /// <summary>
    /// Retrieves a list of roles assigned to a user for MDKHL (if applicable).
    /// </summary>
    /// <param name="userId">The ID of the user for whom the roles are being retrieved.</param>
    /// <returns>A task that represents the asynchronous operation, with a list of <see cref="UserRolesForMdkhl"/> as the result.</returns>
    Task<List<UserRolesForMdkhl>> GetUserWithRolesForMdkhlAsync(Guid userId);

    /// <summary>
    /// Removes a specific role from a user for MDKHL.
    /// </summary>
    /// <param name="userId">The ID of the user from whom the role will be removed.</param>
    /// <param name="roleId">The ID of the role to be removed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveRoleMdkhl(Guid userId, Guid roleId);

    /// <summary>
    /// Updates a user's information without changing their roles.
    /// </summary>
    /// <param name="id">The ID of the user to be updated.</param>
    /// <param name="input">The data transfer object containing updated user information.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="UserDto"/> as the result.</returns>
    Task<UserDto> UpdateUserWithoutRoleChangeAsync(Guid? id, UpdateUserDto input);

    /// <summary>
    /// Adds a new user and validates their information.
    /// </summary>
    /// <param name="input">The data transfer object containing the user's information to be validated and added.</param>
    /// <returns>A task that represents the asynchronous operation, with an <see cref="IdentityUser"/> as the result.</returns>
    Task<IdentityUser> AddAndValidateUserAsync(UserCreateDto input);

    /// <summary>
    /// Creates a customer and assigns a role to the user.
    /// </summary>
    /// <param name="user">The user to whom the customer role will be assigned.</param>
    /// <param name="input">The data transfer object containing user-related information.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateCustomerWithRoleAsync(IdentityUser user, UserCreateDto input);

    /// <summary>
    /// Retrieves the image associated with a user.
    /// </summary>
    /// <param name="userId">The ID of the user whose image is being retrieved.</param>
    /// <returns>A task that represents the asynchronous operation, with the image URL as a string, or null if not found.</returns>
    Task<string?> GetImage(Guid? userId);

    /// <summary>
    /// Creates a user without a customer role but with a specified role.
    /// </summary>
    /// <param name="input">The data transfer object containing the user's information.</param>
    /// <param name="roleId">The ID of the role to be assigned to the user.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="UserDto"/> as the result.</returns>
    Task<UserDto> CreateWithRoleWithoutCustomerAsync(UserCreateDto input, Guid roleId);

    /// <summary>
    /// Checks if the current user has a shift assigned.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, with a boolean result indicating if the user has a shift assigned.</returns>
    Task<bool> IsCurrentUserHasShift();

    /// <summary>
    /// Checks if the number of users in an organization exceeds the subscription limits.
    /// </summary>
    /// <param name="organizationId">The ID of the organization to check against the subscription limits.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CheckSubscriptionUsersLimitsAsync(int organizationId);

    /// <summary>
    /// Updates a user’s information with role changes for MDKHL.
    /// </summary>
    /// <param name="id">The ID of the user to be updated.</param>
    /// <param name="input">The data transfer object containing the updated user information.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="UserDto"/> as the result.</returns>
    Task<UserDto> UpdateUserWithRoleChangeMdkhlAsync(Guid? id, UpdateUserDto input);

}
