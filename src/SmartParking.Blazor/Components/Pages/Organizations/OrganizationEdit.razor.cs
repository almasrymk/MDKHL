using Microsoft.AspNetCore.Identity;
using MudBlazor;
using SmartParking.Enums;
using SmartParking.Organizations;
using System.Threading.Tasks;
using System;
using Volo.Abp.Identity;
using Volo.Abp;
using Abp.Runtime.Validation;
using System.Linq;
using System.Security.Authentication;
using Microsoft.AspNetCore.Components;

namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class OrganizationEdit
{
    private bool CountryClicked = false;
    private string CityNameEdit { get; set; }
    private string CountryNameEdit { get; set; }
    [Parameter] 
    public string Id { get; set; }  // Capture string ID
    public string ArabicName { get; set; } 
    public string EnglishName { get; set; } 
    public string UnifiedLicenseNum { get; set; } 
    public int NumOfCameras { get; set; }
    public int MaxCapacity { get; set; }
    private int CountrySelectValue { get; set; }
    private int CitySelectValue { get; set; }

    public int RegionId { get; set; }
    public int StateId { get; set; }
    private DateTime? CRExpiryDate { get; set; }
    private DateTime? LicenseExpiryDate { get; set; }

    public bool Status { get; set; } = true;
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string ContactMail { get; set; }

    private void SelectCountry(string country)
    {
        CountrySelectValue = int.Parse(country);
    }
    private void SelectCity(string city)
    {
        CitySelectValue =int.Parse(city);
    }
    private async Task Update()
    {
        var id = int.Parse(Id);

        Status StatusValue;
        if (Status)
        {
            StatusValue = Enums.Status.Active;
        }
        else
        {
            StatusValue = Enums.Status.Inactive;
        }
        CreateUpdateOrganiztionDto createUpdateOrganiztion = new CreateUpdateOrganiztionDto()
        {
            ArabicName = ArabicName,
            EnglishName = EnglishName,
            UnifiedLicenseNum = UnifiedLicenseNum,
            NumOfCameras = NumOfCameras,
            MaxCapacity = MaxCapacity,
            CityId = CitySelectValue,
            RegionId = RegionId,
            StateId = StateId,
            ContactMail = ContactMail,
            CountryId = CountrySelectValue,
            CRExpiryDate = DateOnly.FromDateTime(CRExpiryDate ?? DateTime.MinValue),
            LicenseExpiryDate = DateOnly.FromDateTime(LicenseExpiryDate ?? DateTime.MinValue),
            Status = StatusValue,
            PhoneNum = PhoneNum,
            WhatsAppNum = WhatsAppNum,
        };
       

        try
        {
           
           
            await OrganizationAppService.UpdateAsync(id,createUpdateOrganiztion);

            ShowSnackbar("Add Successfully You Can Login Your Password Is P@ssw0rd", Severity.Success);
        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                ShowSnackbar($"Validation failed: {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {

            ShowSnackbar($"Failed to add organization: {ex.Code} + ", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            ShowSnackbar($"Failed to add organization: {ex.Message} + ", Severity.Error);
        }
        catch (Exception ex)
        {

            ShowSnackbar($"Failed to add organization: {ex.Message} + ", Severity.Error);
        }
    }
    /// <summary>
    /// Displays a snackbar notification with the specified message and severity.
    /// </summary>
    /// <param name="message">The message to display in the snackbar.</param>
    /// <param name="severity">The severity level of the snackbar (e.g., Success, Error).</param>
    void ShowSnackbar(string message, Severity severity)
    {
        Snackbar.Clear();
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;
        Snackbar.Add(message, severity, c => c.SnackbarVariant = Variant.Filled);
    }
    private void ClickCountry()
    {
        //CountryClicked = true;
        CountryNameEdit = "";
    }
    private async Task FetchData()
    {
         
        var id = int.Parse(Id);
        var organization = await OrganizationAppService.GetAsync(id);
         EnglishName = organization.EnglishName;
         NumOfCameras = organization.NumOfCameras;
        if(organization.Status == Enums.Status.Active)
        {
            Status = true;
        }
        else
        {
            Status = false;
        }
        MaxCapacity = organization.MaxCapacity;
        CountryNameEdit =  organization.CountryName;
        CityNameEdit = organization.CityName;
    }
    protected override async Task OnInitializedAsync()
    {
        
        await FetchData();
        await base.OnInitializedAsync();


    }
}
