using MudBlazor;
using SmartParking.Enums;
using SmartParking.Organizations;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Organizations;

public partial class OrganizationAdd
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string UnifiedLicenseNum { get; set; }
    public int NumOfCameras { get; set; }
    public int MaxCapacity { get; set; }
    private int CountrySelectValue {  get; set; }
    private int CitySelectValue {  get; set; }

    public int EmaraaId { get; set; }
    public int StateId { get; set; }
    private DateTime? CRExpiryDate { get; set; }
    private DateTime? LicenseExpiryDate { get; set; }

    public bool Status { get; set; } = true;
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string ContactMail { get; set; }

    private void SelectCountry(int country)
    {
        CountrySelectValue = country;
    }
    private void SelectCity(int city)
    {
        CitySelectValue = city;
    }
    private async Task Save()
    {

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
            EmaraaId = EmaraaId,
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
            var organization = await OrganizationAppService.CreateAsync(createUpdateOrganiztion);
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
}
