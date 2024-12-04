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
    [Parameter]
    public string userId { get; set; }
    [Parameter]
    public UserDto userDto { get; set; }

    //protected override async Task OnInitializedAsync()
    //{
    //    await FetchAsync();
        
    //}

    //public async Task FetchAsync()
    //{
    //  userDto = await UsersAppService.GetAsync(Guid.Parse(userId));
    //}
    public void CancelEdit()
    {
        Navigation.NavigateTo("/users", true);

    }

}
