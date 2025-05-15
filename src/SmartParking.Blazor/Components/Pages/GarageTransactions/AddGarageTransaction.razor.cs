using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.GarageTransactions;

namespace SmartParking.Blazor.Components.Pages.GarageTransactions
{
    partial class AddGarageTransaction
    {
        private MudForm form;
        private CreateUpdateGarageTransactionDto garageTransactionDto = new CreateUpdateGarageTransactionDto();
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }
        bool _clearable;

        protected override async Task OnInitializedAsync()
        {

            await base.OnInitializedAsync();
        }
        private async Task Submit()
        {
            if (form.IsValid)
            {
                // Call your service to create the camera
                try
                {
                    await GarageTransactionsAppService.CreateAsync(garageTransactionDto);
                }
                catch (Exception e)
                {
                    Snackbar.Add("Fill Requried Items", Severity.Error);
                    return;
                }
                Snackbar.Add("GarageTransaction created successfully!", Severity.Success);

                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/garageTransactions", forceLoad: true);
            }
            else
            {
                Snackbar.Add("Fill Requried Items", Severity.Error);
            }
        }
        private void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}
