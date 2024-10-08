

using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Garages;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.PlateColor;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.Lookups.VehicleModel;
using SmartParking.OrganizationsGarages;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Customer;

public partial class AddVehicleDialog
{
    public CreateUpdateVehicleDto createUpdateVehicle = new CreateUpdateVehicleDto();
    IReadOnlyList<PlateColorDto> plateColors = new List<PlateColorDto>();
    IReadOnlyList<LicenceTypeDto> LicenceTypes = new List<LicenceTypeDto>();
    IReadOnlyList<VehicleBrandDto> VehicleBrands = new List<VehicleBrandDto>();
    IReadOnlyList<VehicleModelDto> VehicleModels = new List<VehicleModelDto>();
    bool _clearable;
    [CascadingParameter]
    private MudDialogInstance MudDialogAdd { get; set; }

	[Parameter]
	public Guid CustomerId { get; set; }


	protected override async Task OnInitializedAsync()
    {
        createUpdateVehicle.BelongToCompany = false;
        var resultplate = await PlateColorsAppService.GetListAsync(
            new Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100,
                SkipCount = 0
            });
        plateColors= resultplate.Items;


        var resultLicence = await LicenceTypesAppService.GetListAsync(
            new Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100,
                SkipCount = 0
            });
        LicenceTypes= resultLicence.Items;

        var resultVehicleBrand = await VehicleBrandAppService.GetListAsync(new Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto
        {
            MaxResultCount = 100,
            SkipCount = 0
        });
        VehicleBrands = resultVehicleBrand.Items;

        var resultVehicleModel = await VehicleModelAppService.GetListAsync(new Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto
        {
            MaxResultCount = 100,
            SkipCount = 0
        });
        VehicleModels = resultVehicleModel.Items;

    }

    private async void SubmitCreate()
    {
        try
        {
            createUpdateVehicle.CustomerId = CustomerId;
            var vehichle = await VehicleAppService.CreateAsync(createUpdateVehicle);
            if (vehichle != null)
            {

                ShowSnackbar($"Vehichle was created successfully and added to customer", Severity.Success);
                MudDialogAdd.Close(DialogResult.Ok(true));


            }


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

			ShowSnackbar($"Failed to add organizagaragetion: {ex.Code} + ", Severity.Error);
		}
		catch (AuthenticationException ex)
		{
			ShowSnackbar($"Failed to add garage: {ex.Message} + ", Severity.Error);
		}
		catch (Exception ex)
		{

			ShowSnackbar($"Failed to add garage: {ex.Message} + ", Severity.Error);
		}

	}

    private async Task CancelCreate()
    {

        MudDialogAdd.Cancel();

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
