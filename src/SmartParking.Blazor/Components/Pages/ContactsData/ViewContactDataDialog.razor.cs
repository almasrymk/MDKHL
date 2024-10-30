using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MudBlazor;
using SmartParking.Organizations;
using System.Threading.Tasks;
using System;
using Volo.Abp.MultiTenancy;
using Volo.Abp;

namespace SmartParking.Blazor.Components.Pages.ContactsData;

public partial class ViewContactDataDialog
{
    private MudForm form;

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;
    [Parameter] public ContactInfoDto ContactDataDto { get; set; }


    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();

    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
