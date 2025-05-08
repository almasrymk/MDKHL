using SmartParking.KeeperShifts;

namespace SmartParking.Blazor.Components.Pages.MyShifts
{
    partial class CurrentShift
    {
        public KeeperShiftDto KeeperShiftDto { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            KeeperShiftDto = await KeeperShiftsAppService.GetAsync(1);
            await base.OnInitializedAsync();
        }
    }
}
