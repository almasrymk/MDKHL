using iText.IO.Image;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartParking.Organizations;
using SmartParking.Entities;
using SmartParking.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using IdentityRole = Volo.Abp.Identity.IdentityRole;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using SmartParking.Enums;
using SmartParking.Utilities;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using SmartParking.Localization;
using Volo.Abp;
using SmartParking.Entities.Customers;
using SmartParking.Customers;
using Castle.Core.Resource;
using SmartParking.Cutomers;

namespace SmartParking.Authentication;

[Authorize]
public class UsersAppService : ApplicationService, IUsersAppService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMyUserRepository _userRepository;
    private readonly IDataFilter _dataFilter;
    private readonly ICustomRoleReository _roleReository;
    private readonly IOptions<IdentityOptions> _identityOptions;
    private readonly IAuthenticationUtility _authenticationUtility;
    private readonly IBaseCustomerAppService _baseCustomer;
    private readonly IRepository<Customer, Guid> _customerRepository;

    public UsersAppService(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager
        , IMyUserRepository userRepository, IDataFilter dataFilter,
        ICustomRoleReository roleReository,
        IOptions<IdentityOptions> identityOptions,
        IAuthenticationUtility authenticationUtility,
        IBaseCustomerAppService baseCustomer,
        IRepository<Customer, Guid> customerRepository
        )
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _dataFilter = dataFilter;
        _roleReository = roleReository;
        _identityOptions = identityOptions;
        _authenticationUtility = authenticationUtility;
        _baseCustomer = baseCustomer;
        _customerRepository = customerRepository;
    }
    ///<inheritdoc/>
    public async Task<UserDto> CreateWithRoleAsync(UserCreateDto input, Guid roleId)
    {
        IdentityUser user = await AddAndValidateUserAsync(input);

        await AssignRoleAndPermissionsAsync(user, roleId, input);

        return ObjectMapper.Map<IdentityUser, UserDto>(user);
    }

    [AllowAnonymous]
    public async Task<IdentityUser> AddAndValidateUserAsync (UserCreateDto input)
    {
        await _identityOptions.SetAsync();
        var user = CreateUser(input);
        await _authenticationUtility.CheckPhoneNumberIsValid(input.PhoneNumber);
        var result = await CheckPhoneIsUnique(input.PhoneNumber);
        if (result != null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("PhoneNumber") + " " +
                LocalizationHelper.GetLocalizedString("DuplicateValidation"));
        }
        SetUserProperties(user, input);
        await _authenticationUtility.CheckNationalId(input.IdentificationTypes, input.NationalId);
        await _authenticationUtility.CheckIfNationalIDIsUnique(input.NationalId);
        var creationResult = await _userManager.CreateAsync(user, input.Password);

        await _userManager.SetPhoneNumberAsync(user, input.PhoneNumber);
        return user;
    }

    private IdentityUser CreateUser(UserCreateDto input)
    {
        return new IdentityUser(GuidGenerator.Create(),input.UserName,input.Email,input.TenantId);
    }

    private void SetUserProperties(IdentityUser user, UserCreateDto input)
    {
        user.Name = input.Name;
        user.Surname = input.Surname;
        user.SetProperty("OtpCode", null);
        user.SetProperty("OtpExpirationDate", DateTime.Now);
        user.SetProperty("UserTypeId", input.UserType);
        user.SetProperty("OrganizationId", input.OrganizationId);
        user.SetProperty("IdentificationTypes", input.IdentificationTypes);
        user.SetProperty("NationalId", input.NationalId);
    }

    private async Task AddCustomer(IdentificationTypes identificationTypes, string NationalId,string ArabicName,Guid userId)
    {
        try
        {
            var customer = new Customer
            {
                IdentityUserId = userId,
                IdentificationTypes = identificationTypes,
                NationalId = NationalId,
                ArabicName = ArabicName,
                TenantId = null,

            };

            await _customerRepository.InsertAsync(customer, true);
        }
        catch (Exception ex)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.FAILED) + " " +
                LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.ADD) + " " +
                LocalizationHelper.GetLocalizedString("Customer"));

        }

    }
    private async Task AssignRoleAndPermissionsAsync(IdentityUser user, Guid roleId)
    {
        user.AddRole(roleId);
    }    
    private async Task AssignRoleAndPermissionsAsync(IdentityUser user, Guid roleId,UserCreateDto input)
    {
        if (input.IsCustomer)
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                await CreateCustomerWithRoleAsync(user, input);

            }


        }
        user.AddRole(roleId);
    }
    [AllowAnonymous]
    public async Task CreateCustomerWithRoleAsync(IdentityUser user, UserCreateDto input)
    {
        await AddCustomer(input.IdentificationTypes, input.NationalId, input.Surname, user.Id);
        await _userManager.AddToRoleAsync(user, RolesType.Customer.ToString());
    }

    /// <summary>
    /// Checks if the provided phone number is unique and associated with a user.
    /// </summary>
    /// <param name="phone">The phone number to check for uniqueness.</param>
    /// <returns>The <see cref="IdentityUser"/> associated with the phone number if it exists.</returns>
    /// <exception cref="ValidationException">Thrown when the phone number is not associated with any user.</exception>
    public async Task<UserDto> CheckPhoneIsUnique(string phone)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.PhoneNumber == phone);

            if (user != null)
            {
                return ObjectMapper.Map<IdentityUser, UserDto>(user);
            }
            return null;
        }

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
    public async Task<PagedResultDto<UserWithRole>> GetListMdklAsync(PagedResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            IQueryable<IdentityUser> query = await FindByUserType();
            var users = await _userRepository.FindUserWithRoleMdkhlAsync(input.SkipCount,input.MaxResultCount);
            
            return new PagedResultDto<UserWithRole>
            {
                Items = users.Queryable.ToList(),
                TotalCount = users.PageSize
            };
        }
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<UserWithRole>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            //IQueryable<IdentityUser> queryable = await FindByUserType();

            //if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            //{
            //    queryable = ApplySearchFilter(queryable, input.SearchingTerm);
            //}

            //if (!input.Sorting.IsNullOrEmpty())
            //{
            //    queryable = ApplySorting(queryable, input.Sorting);
            //}
            var users = await _userRepository.SearchUserWithRoleMdkhlAsync(input.SkipCount, input.MaxResultCount,input.SearchingTerm);
            return new PagedResultDto<UserWithRole>
            {
                Items = users.Queryable.ToList(),
                TotalCount = users.PageSize
            };
        }
    }

    public async Task<PagedResultDto<UserWithRole>> SearchForOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        var users = await _userRepository.SearchUserWithRoleAsync(input.SkipCount, input.MaxResultCount, input.SearchingTerm);
        return new PagedResultDto<UserWithRole> { Items = users.Queryable.ToList(), TotalCount = users.PageSize };

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

    public async Task<PagedResultDto<UserDto>> FilterForOrganizationAsync(PagedAndSortedAndFilteredResultRequestDto input)
    {
        IQueryable<IdentityUser> queryable = await FindByUserType(1, 4, 5);
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
            user.Name.ToLower().Contains(lowerTerm) ||
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

    public async Task<IdentityUser?> IsUserFound(Guid? userId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("PhoneNumber") +" "+ 
                    LocalizationHelper.GetLocalizedString("NotFoundValidation"));

            }

            return user;
        }
       
    }
    public async Task<UserDto> UpdateUserWithoutRoleChangeAsync(Guid? id, UpdateUserDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            IdentityUser? user = await IsUserFound(id);
            await UpdateUserProperties(user, input);
            var updatedUser = await _userManager.UpdateAsync(user);
            if (!updatedUser.Succeeded)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.FAILED) + " " +
                     LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.UPDATE) + " " +
                     LocalizationHelper.GetLocalizedString("User"));
            }

            var userDto = ObjectMapper.Map<IdentityUser, UserDto>(user);
            return userDto;
        }
    }
    /// <inheritdoc/>
    public async Task<UserDto> UpdateUserWithRoleChangeAsync(Guid? id, UpdateUserDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            IdentityUser? user = await IsUserFound(id);
            

            await ChangeRole(input, user);
            await UpdateUserProperties(user, input);
            var updatedUser = await _userManager.UpdateAsync(user);
            if (!updatedUser.Succeeded)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.FAILED) + " " +
                    LocalizationHelper.GetLocalizedString(AlertMessages.AlertMessages.UPDATE)+" "+
                    LocalizationHelper.GetLocalizedString("User"));
            }

            var userDto = ObjectMapper.Map<IdentityUser, UserDto>(user);
            return userDto;
        }
    }
    private async Task UpdateUserProperties(IdentityUser user, UpdateUserDto input)
    {
        await CheckNationalIdValid(user, input.IdentificationTypes,input.NationalId);
        await CheckUpdatePhoneNumber(user, input);
        user.Name = input.Name;
        user.Surname = input.Surname;

        ObjectHelper.TrySetProperty(
            user,
            x => x.Email,
            () => input.Email
        );
        user.SetIsActive(input.IsActive);
        user.SetProperty("IdentificationTypes", input.IdentificationTypes);
        user.SetProperty("NationalId", input.NationalId);
        ObjectHelper.TrySetProperty(
            user,
            x => x.PhoneNumber,
            () => input.PhoneNumber
            );
        user.SetConcurrencyStampIfNotNull(input.ConcurrencyStamp);
    }

    private async Task CheckUpdatePhoneNumber(IdentityUser user, UpdateUserDto input)
    {
        if (user.PhoneNumber != input.PhoneNumber)
        {
            var result = await CheckPhoneIsUnique(input.PhoneNumber);
            if (result is not null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("PhoneNumber") + " " +
                    LocalizationHelper.GetLocalizedString("DuplicateValidation"));

            }
        }
    }

    private async Task CheckNationalIdValid(IdentityUser user, IdentificationTypes identification, string nationalId)
    {
        var IdentificationTypes = user.GetProperty<IdentificationTypes>("IdentificationTypes");
        var NationalId = user.GetProperty<string>("NationalId");
        if (IdentificationTypes != identification || NationalId != nationalId)
        {
            await _authenticationUtility.CheckNationalId(identification, nationalId);
            await _authenticationUtility.CheckIfNationalIDIsUnique(nationalId);
        }
    }

    public async Task<UserDto> UpdateProfileDataAsync(Guid? id, UpdateUserDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            IdentityUser? user = await IsUserFound(id);

            ObjectHelper.TrySetProperty(
            user,
            x => x.UserName,
            () => input.UserName
             );
            await UpdateUserProperties(user, input);
            var createdUser = await _userManager.UpdateAsync(user);
            var userDto = ObjectMapper.Map<IdentityUser, UserDto>(user);
            return userDto;
        }
    }

    private  async Task ChangeRole(UpdateUserDto input, IdentityUser? user)
    {
        if (user.Roles.Count == 0)
        {
            await CustomerToUser(user);
        }
        if (user.Roles.Count > 0)
        {
            user.RemoveRole(user.Roles.First().RoleId);
        }
        user.AddRole(input.RoleId);
    }
    
    private async Task CustomerToUser(IdentityUser user)
    {
        var orgId =await GetOrganizationIdByTenantAsync(CurrentTenant.Id);
        user.SetProperty("OrganizationId", orgId);
        ObjectHelper.TrySetProperty(
                   user,
                   x => x.TenantId,
                   () => CurrentTenant.Id
               );


    }

    private static async void RemoveRole(Guid roleId, IdentityUser? user)
    {
        user.RemoveRole(roleId);
    }
    
    public async Task RemoveRoleMdkhl(Guid UserId , Guid RoleId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            IdentityUser? user = await IsUserFound(UserId);
            var role = await _roleReository.GetAsync(RoleId);
            if(role.Name == RolesType.Customer.ToString())
            {
                throw new Exception(
                   LocalizationHelper.GetLocalizedString("UserRole") + " " +
                   LocalizationHelper.GetLocalizedString("Customer"));
              
            }
            user.RemoveRole(RoleId);
            await _userManager.UpdateAsync(user);
        }
    }

    //private async Task ChangePassword(UpdateUserDto input, IdentityUser? user)
    //{
    //    await _userManager.RemovePasswordAsync(user);
    //    await _userManager.AddPasswordAsync(user, input.Password);
    //}

    public async Task<UserDto> GetAsync(Guid? id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            
            IdentityUser? user = await IsUserFound(id);
            
            var userDto = ObjectMapper.Map<IdentityUser, UserDto>(user);
            userDto.Image = user.GetProperty<string>("Image");
            userDto.IdentificationTypes = user.GetProperty<IdentificationTypes>("IdentificationTypes");
            userDto.NationalId = user.GetProperty<string>("NationalId");
            if (user.Roles.Any())
            {
                userDto.RoleId = user.Roles.FirstOrDefault().RoleId;
            }
            return userDto;
        }
    }

    public async Task<UserDto> UpdateImageAsync(Guid? id, string image)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            IdentityUser? user = await IsUserFound(id);
            var imagePath =await FileHelper.SaveImageFromBase64Async(image, PathHelper.UserFolder);
            user?.SetProperty("Image", imagePath);
            
            await _userManager.UpdateAsync(user);
            var userMap = ObjectMapper.Map<IdentityUser, UserDto>(user);
            userMap.Image = user.GetProperty<string>("Image");
            return userMap;
        }
    }

    public async Task<IdentityRole?> FindRoleByTenantIdAsync(string roleName, Guid? tenantId)
    {
        return await _roleReository.FindRoleByTenantAsync(roleName, tenantId);
    }
    ///<inheritdoc/>
    public async Task<PagedResultDto<UserWithRole>> GetListForOrganizationAsync(PagedResultRequestDto input)
    {
        //IQueryable<IdentityUser> query = await FindByUserType(1, 4, 5);
        //return await GetPagedResultAsync(query, input);
        var users = await _userRepository.FindUserWithRoleAsync(input.SkipCount, input.MaxResultCount);
        return new PagedResultDto<UserWithRole>
        {
            Items = users.Queryable.ToList(),
            TotalCount = users.PageSize
        };
    }

    private async Task<IQueryable<IdentityUser>> FindByUserType(params int[] userTypes)
    {
        var query = await _userRepository.GetQueryableAsync();
        query = query.Where(_ => _.Roles.Any());
        //if (userTypes.Any())
        //{
        //    query = query.Where(user => userTypes.Contains(EF.Property<int>(user, "UserTypeId")));
        //}
        return query;
    }

    public async Task<UserDto> UpdateUserWithNoRoleAsync(Guid? id, UpdateUserDto input)
    {
        IdentityUser? user = await IsUserFound(id);

       
        RemoveRole(input.RoleId, user);
        user?.SetProperty("UserTypeId", 3);
        user?.SetProperty("OrganizationId", 0);
        
        if (user.UserName != input.UserName)
        {
            await _userManager.SetUserNameAsync(user, input.UserName);
            await _userManager.SetPhoneNumberAsync(user, input.PhoneNumber);
        }
        await UpdateUserProperties(user, input);
        var createdUser = await _userManager.UpdateAsync(user);
        var userDto = ObjectMapper.Map<IdentityUser, UserDto>(user);
        return userDto;
    }

    public  async Task<List<UserRolesForMdkhl>> GetUserWithRolesForMdkhlAsync(Guid userId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            return await _userRepository.GetUserWithRolesForMdkhlAsync(userId);
        }
    }

    public async Task<string?> GetImage(Guid? userId)
    {
     using (_dataFilter.Disable<IMultiTenant>())
        {
            IdentityUser? user = await IsUserFound(userId);
            return user?.GetProperty<string>("Image");
        }
    }
}
