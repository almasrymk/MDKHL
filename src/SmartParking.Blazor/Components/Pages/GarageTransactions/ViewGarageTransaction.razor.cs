using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.GarageTransactions;
using SmartParking.Organizations;
using System.Globalization;

namespace SmartParking.Blazor.Components.Pages.GarageTransactions
{
    partial class ViewGarageTransaction
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter] public GarageTransactionDto GarageTransaction { get; set; }
        string cameraCode = "";
        string OrgName = "";
        string GarageName = "";
        string CustomerCategory = "";
        bool _clearable;
        private MudForm form;
        private OrganizationDto _organizationDto = new OrganizationDto();

        private string TransformOrigin { get; set; } = "center center";
        private string Scale { get; set; } = "scale(1)";

        private void SetZoomOrigin(MouseEventArgs e)
        {
            // Calculate the position of the mouse relative to the container
            double xPercent = e.OffsetX / 400 * 100; // 400 is the container width
            double yPercent = e.OffsetY / 300 * 100; // 300 is the container height

            TransformOrigin = $"{xPercent}% {yPercent}%";
            Scale = "scale(2)"; // Zoom level when hovered
        }

        private void ResetZoom(MouseEventArgs e)
        {
            TransformOrigin = "center center";
            Scale = "scale(1)"; // Reset zoom when mouse leaves
        }

        protected override async Task OnInitializedAsync()
        {
            _organizationDto = await OrganizationsAppService.GetAsync(GarageTransaction.OrganizationID);

            var Garage = await GaragesAppService.GetAsync(GarageTransaction.GarageId);
            if(CultureInfo.CurrentCulture.Name == "en")
    
        {
                OrgName = _organizationDto.EnglishName;
                GarageName = Garage.NameEn;
            }else
            {
                OrgName = _organizationDto.ArabicName;
                GarageName = Garage.NameAr;
            }
            var Camera = await CamerasAppService.GetAsync(GarageTransaction.CameraId);
            cameraCode = Camera.DeviceId;
            CustomerCategory = GarageTransaction.CustomerCategory.ToString();
            await base.OnInitializedAsync();
        }

        private void PreCheckout()
        {
            Navigation.NavigateTo($"/preinvoice/{GarageTransaction.Id}");
        }

        private void OnCloseViewOnly()
        {
            MudDialogAdd.Cancel();

        }
    }
}
