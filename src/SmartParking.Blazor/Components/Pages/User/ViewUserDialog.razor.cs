using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Authentication;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.User;

partial class ViewUserDialog
{
    [Inject] IRoleAppService RoleAppService { get; set; }
    [Parameter]
    public string userId { get; set; }
    [Parameter]
    public UserDto userDto { get; set; }
    private string RoleName { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var role = await RoleAppService.GetRoleById(userDto.RoleId);
        RoleName = role.Name;

    }
    public void CancelEdit()
    {
        Navigation.NavigateTo("/users", true);

    }

}
