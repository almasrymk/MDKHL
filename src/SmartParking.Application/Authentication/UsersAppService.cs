using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SmartParking.Cutomers;
using Microsoft.EntityFrameworkCore;
using SmartParking.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using IdentityRole = Volo.Abp.Identity.IdentityRole;
using QRCoder;
using SmartParking.Entities.Invoices;
using SmartParking.Invoices;
using System.IO;
using Volo.Abp.PermissionManagement;
using static Volo.Abp.Identity.IdentityPermissions;

namespace SmartParking.Authentication;

[Authorize]
public class UsersAppService : ApplicationService, IUsersAppService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMyUserRepository _userRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IPermissionManager _permissionManager;

    public UsersAppService(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager
        , IMyUserRepository userRepository, IDataFilter dataFilter, IPermissionManager  permissionManager)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _dataFilter = dataFilter;
        _permissionManager = permissionManager;
    }
    ///<inheritdoc/>
    public async Task<UserDto> CreateWithRoleAsync(UserCreateDto input, IdentityRole role)
    {
        var user = CreateUser(input);

        await CheckPhoneIsUnique(input.PhoneNumber);
        SetUserProperties(user, input);

        var creationResult = await _userManager.CreateAsync(user, input.Password);
        await _userManager.SetPhoneNumberAsync(user, input.PhoneNumber);

        if (!creationResult.Succeeded)
        {
            throw new Exception(creationResult.Errors.FirstOrDefault()?.Description ?? "User  creation failed.");
        }

        await AssignRoleAndPermissionsAsync(user, role);

        return ObjectMapper.Map<IdentityUser, UserDto>(user);
    }

    private IdentityUser CreateUser(UserCreateDto input)
    {
        return new IdentityUser(GuidGenerator.Create(), input.UserName, input.Email, input.TenantId);
    }

    private void SetUserProperties(IdentityUser user, UserCreateDto input)
    {
        user.Name = input.UserName;
        user.Surname = input.Surname;
        user.SetProperty("OtpCode", null);
        user.SetProperty("OtpExpirationDate", DateTime.Now);
        user.SetProperty("UserTypeId", input.UserType);
        user.SetProperty("OrganizationId", input.OrganizationId);
    }

    private async Task AssignRoleAndPermissionsAsync(IdentityUser user, IdentityRole role)
    {
        user.AddRole(role.Id);
        await _permissionManager.SetAsync("U", user.Id.ToString(), "AbpIdentity.Users.ManagePermissions", true);
    }

    /// <summary>
    /// Checks if the provided phone number is unique and associated with a user.
    /// </summary>
    /// <param name="phone">The phone number to check for uniqueness.</param>
    /// <returns>The <see cref="IdentityUser"/> associated with the phone number if it exists.</returns>
    /// <exception cref="ValidationException">Thrown when the phone number is not associated with any user.</exception>
    private async Task<IdentityUser> CheckPhoneIsUnique(string phone)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phone);

        if (user != null)
        {
            throw new Exception("User with this phone number does  exist.");
        }
        return user;

    }
    ///<inheritdoc/>
    public async Task<int> GetOrganizationIdByTenantAsync(Guid? tenantId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            var user = await _userRepository.FindByOrganizationByTenantAsync(tenantId);
            return user.GetProperty<int>("OrganizationId");
        }
    }

    ///<inheritdoc/>
    public async Task<List<UserDto>> GetUserByOrganizationId(int organizationId)
    {


        using (_dataFilter.Disable<IMultiTenant>())
        {

            var users = await _userRepository.FindByOrganizationAsync(organizationId);
            var userMap = ObjectMapper.Map<List<IdentityUser>, List<UserDto>>(users);
            return userMap;
        }

    }
    ///<inheritdoc/>
    public async Task<PagedResultDto<UserDto>> GetListAsync(PagedResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _userRepository.GetQueryableAsync();
            return await GetPagedResultAsync(query, input);
        }
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<UserDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _userRepository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {
                queryable = ApplySearchFilter(queryable, input.SearchingTerm);
            }

            if (!input.Sorting.IsNullOrEmpty())
            {
                queryable = ApplySorting(queryable, input.Sorting);
            }

            return await GetPagedResultAsync(queryable, input);
        }
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<UserDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _userRepository.GetQueryableAsync();
            var filterExpression = BuildExpression<IdentityUser>.BuildFilterExpression(input.filters);

            if (filterExpression is not null)
            {
                queryable = queryable.Where(filterExpression);
            }

            if (!input.Sorting.IsNullOrEmpty())
            {
                queryable = ApplySorting(queryable, input.Sorting);
            }

            return await GetPagedResultAsync(queryable, input);
        }
    }

    private async Task<PagedResultDto<UserDto>> GetPagedResultAsync(IQueryable<IdentityUser> queryable, PagedResultRequestDto input)
    {
        var totalCount = await AsyncExecuter.CountAsync(queryable);
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var users = await AsyncExecuter.ToListAsync(queryable);
        var usersDto = ObjectMapper.Map<List<IdentityUser>, List<UserDto>>(users);

        return new PagedResultDto<UserDto>
        {
            TotalCount = totalCount,
            Items = usersDto
        };
    }

    private IQueryable<IdentityUser> ApplySearchFilter(IQueryable<IdentityUser> queryable, string searchingTerm)
    {
        var lowerTerm = searchingTerm.ToLower();
        return queryable.Where(user =>
            user.UserName.ToLower().Contains(lowerTerm) ||
            user.Email.ToLower().Contains(lowerTerm) ||
            user.PhoneNumber.ToLower().Contains(lowerTerm));
    }

    private IQueryable<IdentityUser> ApplySorting(IQueryable<IdentityUser> queryable, string sorting)
    {
        if (sorting.Contains("DESC"))
        {
            var sortingParts = sorting.Split(' ').ToList();
            return queryable.OrderBy(sortingParts[0] + " descending");
        }
        return queryable.OrderBy(sorting);
    }

    public async Task<UserDto> AssignRoleToUser(Guid userId, string roleName)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            IdentityUser? user = await IsUserFound(userId);
            await _userManager.AddToRoleAsync(user, roleName);
            var userDto = ObjectMapper.Map<IdentityUser, UserDto>(user);
            return userDto;
        }
    }

    private async Task<IdentityUser?> IsUserFound(Guid? userId)
    {
        var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            throw new Exception("User not found");

        }

        return user;
    }

    /// <inheritdoc/>
    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto input)
    {
        using(_dataFilter.Disable<IMultiTenant>())
        {
            IdentityUser? user = await IsUserFound(id);
            user.Name = input.Name;
            user.Surname = input.Surname;
            await _userManager.SetEmailAsync(user,input.Email);
            await _userManager.SetUserNameAsync(user,input.UserName);
            var createdUser = await _userManager.UpdateAsync(user);
            var userDto = ObjectMapper.Map<IdentityUser, UserDto>(user);
            return userDto;
        }
    }

    public async Task<UserDto> GetAsync(Guid? id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            IdentityUser? user = await IsUserFound(id);
            var userDto = ObjectMapper.Map<IdentityUser, UserDto>(user);
            userDto.Image = user.GetProperty<string>("Image");
            return userDto;
        }
    }

    public async Task<UserDto> UpdateImageAsync(Guid? id, string image)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            IdentityUser? user = await IsUserFound(id);
            var imagePath =  SaveImageAsync(image);
            user?.SetProperty("Image", imagePath);

            await _userManager.UpdateAsync(user);
            return ObjectMapper.Map<IdentityUser, UserDto>(user);
        }
    }

    private static string SaveImageAsync(string image)
    {
        var base64Data = image.Substring(image.IndexOf(",") + 1);
        byte[] imageBytes = Convert.FromBase64String(base64Data);

        string fileName = $"{Guid.NewGuid()}{DateTime.Now:yyyyMMddHHmmssfff}.png";
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users", fileName);

        System.IO.File.WriteAllBytes(filePath, imageBytes);
        return $"images/users/{fileName}";
    }

    public async Task<IdentityRole?> FindRoleByTenantIdAsync(string roleName, Guid? tenantId)
    {
        return await _userRepository.FindRoleByTenantAsync(roleName, tenantId);
    }
}
