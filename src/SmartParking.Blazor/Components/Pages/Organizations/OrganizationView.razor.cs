using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Threading.Tasks;
using System;
using System.Globalization;
using SmartParking.Subscriptions;
using SmartParking.Organizations;

namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class OrganizationView
{
    private string CityName { get; set; }
    private string CountryName{ get; set; }
    private string RegionName { get; set; }
    private string StateName { get; set; }
    private string Code { get; set; }
    [Parameter]
    public string Id { get; set; }  // Capture string ID
 
    private string CountrySelectValue { get; set; }
    private string CitySelectValue { get; set; }

    public string RegionSelectValue { get; set; }
    public string StateSelectValue { get; set; }
    private DateTime? CRExpiryDate { get; set; }
    public int Status { get; set; } = 1;//1-> active , 2-> inactive 3->deleted 
 
    public string Address { get; set; }


    OrganizationDto organization = new OrganizationDto();
    private async Task FetchData()
    {
        var id = int.Parse(Id);
        organization = await OrganizationsAppService.GetAsync(id);       
        if (organization.Status == Enums.Status.Active)
        {
            Status = 1;
        }
        else if (organization.Status == Enums.Status.Inactive)
        {
            Status = 2;
        }
        else
        {
            Status = 3;
        }
        if (CultureInfo.CurrentCulture.Name == "en")
        {
             CountryName = organization.CountryName;
             RegionName = organization.RegionName;
             StateName=organization.StateName;
             CityName = organization.CityName;
             Address = organization.Address + organization.CityName;
        }
        else
        {

            CountryName = organization.CountryNameAr;
            RegionName = organization.RegionNameAr;
            StateName = organization.StateNameAr;
            CityName = organization.CityNameAr;
            Address = organization.Address + organization.CityNameAr;
        }
    }
    protected override async Task OnInitializedAsync()
    {
        await FetchData();
        await base.OnInitializedAsync();
    }
    private void Back()
    {
        Navigation.NavigateTo("/organizations");
    }
    private async Task ViewCategories()
    {
        var id = int.Parse(Id);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<CategoriesDialog>("view garage", parameters);
    }
    private async Task ViewSubscription()
    {
        var id = int.Parse(Id);
        var org = await OrganizationsAppService.GetAsync(id);
        var subscription = new SubscriptionDto();
        if (org.SubscriptionId != 0)
        {
            subscription = await SunscriptionAppService.GetAsync(org.SubscriptionId);

        }
        else
        {
            subscription = null;
        }

        var parameters = new DialogParameters
        {
            ["subscription"] = subscription
        };
        await Dialog.ShowAsync<OrganizationSubscriptionDialog>("view DetailsSubscription", parameters);
    }


    private async Task ViewContact()
    {
        var id = int.Parse(Id);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<ContactDataDialog>("view contact", parameters);
    }
    private async Task ViewGarages()
    {
        var id = int.Parse(Id);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<GarageDialog>("view garage", parameters);
    }
    private async Task ViewCustomers()
    {
        var id = int.Parse(Id);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<CustomerDialog>("view customer", parameters);
    }
    private async Task ViewUsers()
    {
        var id = int.Parse(Id);
        var parameters = new DialogParameters
        {
            ["organizationId"] = id
        };
        var dialog = await Dialog.ShowAsync<UsersDialog>("view users", parameters);
    }
}