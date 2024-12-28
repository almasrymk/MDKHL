using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp;
using SmartParking.Subscriptions;
using SmartParking.SubscriptionTypes;
using Volo.Abp.Validation;
using System.Linq;
using SmartParking.Lookups;
using SmartParking.Blazor.Components.Shared;
using System.Security.Authentication;

namespace SmartParking.Blazor.Components.Pages.Subscriptions;

public partial class AddSubscriptionDialog
{
    private MudForm form;
    private DateTime? _activateDate = new DateTime();
    private DateTime? _inActivateDate = new DateTime();
    private DateTime? _creationDate = new DateTime();
    private DateTime? creationTime = DateTime.Now;
    public bool maximumNumberOfGarages { get; set; }
    public int UnlimitedMaximumNumber { get; set; } = 9999;
	public bool maximumNumberOfCameras { get; set; }
	public bool maximumNumberOfUsers { get; set; }
	public bool maximumNumberOfCustomers { get; set; }
	public bool maximumNumberOfParkingSlots { get; set; }
  


    public CreateUpdateSubscriptionDto input = new CreateUpdateSubscriptionDto();
    private int BoundNumberOfUsers;
    private int BoundNumberOfGarages;
    private int BoundNumberOfCustomers;
    private int BoundNumberOfCameras;
    private int BoundNumberOfParkingSlots;

    private int BoundNumberOfOrganization;

    public List<SubscriptionTypeLookupDto> subscriptions = new List<SubscriptionTypeLookupDto>();

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;

    protected override async Task OnInitializedAsync()
    {
        var result = await SubscriptionTypeAppService.GetSubscriptionTypeLookupAsync();
        subscriptions = result;
    }
   

    private async Task Submit()
    {
        
        try
        {
            input.NumbersOfGarages = BoundNumberOfGarages;
            input.NumberOfCustomers =  BoundNumberOfCustomers;
            input.NumberOfUsers =  BoundNumberOfUsers;
            input.NumberOfCameras = BoundNumberOfCameras;
            input.ParkingSlot = BoundNumberOfParkingSlots;
           
            input.ActivationDate = DateOnly.FromDateTime(_activateDate ?? DateTime.Now);

            input.InActivationDate = DateOnly.FromDateTime(_inActivateDate ?? DateTime.MinValue);
            input.Status = Enums.SubscriptionStatus.Draft;
            input.CreationTime = DateTime.Now;
            var subscription = await SubscriptionAppService.CreateAsync(input);

            if (subscription != null)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]} {input.ArabicName}", Severity.Success);

                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/subscriptions", true);
            }

            else
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} {input.ArabicName}", Severity.Error);

            }
        }
        catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message} ", Severity.Error);
        }

    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
