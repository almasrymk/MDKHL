using Microsoft.AspNetCore.Components;
using SmartParking.Organizations;

namespace SmartParking.Blazor.Components.Pages.Organizations
{
    partial class OrganizationHoursAndPrice
    {
        [Parameter]
        public CreateUpdateOrganiztionDto CreateUpdateOrganiztion { get; set; }
        public TimeSpan? ClosingHoursStart { get; set; }
        public TimeSpan? ClosingHoursEnd { get; set; }



        protected override Task OnInitializedAsync()
        {
            ClosingHoursStart = ConvertTimeOnlyToTimeSpan(CreateUpdateOrganiztion.ClosingHoursStart);
            ClosingHoursEnd = ConvertTimeOnlyToTimeSpan(CreateUpdateOrganiztion.ClosingHoursEnd);
            return base.OnInitializedAsync();
        }
        private TimeSpan ConvertTimeOnlyToTimeSpan(TimeOnly? timeOnly)
        {
            return timeOnly?.ToTimeSpan() ?? TimeSpan.Zero;
        }
    }
}
