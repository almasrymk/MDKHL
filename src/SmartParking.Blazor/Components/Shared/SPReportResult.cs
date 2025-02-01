using MudBlazor;
using SmartParking.Enums;

namespace SmartParking.Blazor.Components.Shared;

public class SPReportResult
{
 
        public string SearchingText { get; set; }
        public bool InRange { get; set; }
        public DateRange DateRange { get; set; }
        public CustomerCategoryStatus CustomerCategory { get; set; } 
        public bool HasBeenOpened { get; set; }




}
