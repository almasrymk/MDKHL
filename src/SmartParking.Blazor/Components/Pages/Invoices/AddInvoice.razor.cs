using MudBlazor;
using SmartParking.Invoices;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using Volo.Abp.Validation;
using System.Linq;
using SmartParking.Enums;
using System.Collections.Generic;
using SmartParking.Organizations;
using SmartParking.Garages;
using SmartParking.Vehicles;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial class AddInvoice
{
    public CreateUpdateInvoiceDto input = new CreateUpdateInvoiceDto();

    public List<OrganizationDto> organizations = new List<OrganizationDto>();

    public List<GarageDto> garages = new List<GarageDto>();

    public List<VehicleDto> vehicles = new List<VehicleDto>();
    private MudForm form;


    public string code;
    protected override async Task OnInitializedAsync()
    {
        var organizationDto = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        var garageDto = await GarageAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        var vehicleDto = await VehiclesAppService.GetListAsync(new PagedAndSortedResultRequestDto());

        organizations = organizationDto.Items.ToList();
        garages = garageDto.Items.ToList();
        vehicles = vehicleDto.Items.ToList();

    }

    private async Task Submit()
    {
        var successedMessage = L["InvoiceCreatedSuccessfully"];
        var errorMessage = L["Can'tCreateInvoice"];
        var validationErrorMessage = L["PleaseFixTheValidationErrors"];


        // Call your service to create the invoice
        try
        {
            CreateUpdateInvoiceDto createUpdateInvoice = new CreateUpdateInvoiceDto()
            {
                InvoiceNumber = input.InvoiceNumber,
                TotalHours = input.TotalHours,
                InvoicePrice = input.InvoicePrice,
                InvoiceType = InvoiceType.UnPaid,
                OrganizationId = input.OrganizationId,
                GarageId = input.GarageId,
                PlateNumber = input.PlateNumber
            };

            var invoice = await InvoiceAppService.CreateAsync(createUpdateInvoice);

            if (invoice != null)
            {
                Snackbar.Add(successedMessage, Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/invoices",true);
               
            }

            else
            {
                Snackbar.Add(errorMessage, Severity.Error);

            }
        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                Snackbar.Add($"{validationErrorMessage} {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
        catch (BusinessException ex)
        {
            Snackbar.Add($"Failed to add invoice: {ex.Code} + ", Severity.Error);

        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to add invoice: {ex.Message} + ", Severity.Error);
        }

    }

    private void Back()
    {

        Navigation.NavigateTo("/invoices");
    }
}
