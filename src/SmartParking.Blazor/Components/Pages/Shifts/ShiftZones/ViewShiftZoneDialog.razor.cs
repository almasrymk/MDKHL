using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Shifts;
using SmartParking.Shifts.ShiftsMangers;
using SmartParking.Shifts.ShiftZones;
using System.Globalization;

namespace SmartParking.Blazor.Components.Pages.Shifts.ShiftZones;

public partial class ViewShiftZoneDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public ShiftZoneDto ShiftZone { get; set; } = new ShiftZoneDto();
    private bool IsArabic => CultureInfo.CurrentCulture.Name.StartsWith("ar", StringComparison.OrdinalIgnoreCase);

    private string GetLocalized(string en, string ar)
    {
        if (IsArabic)
            return !string.IsNullOrWhiteSpace(ar) ? ar : en ?? string.Empty;

        return !string.IsNullOrWhiteSpace(en) ? en : ar ?? string.Empty;
    }


    protected override Task OnParametersSetAsync()
    {
        ShiftZone ??= new ShiftZoneDto();
        ShiftZone.ShiftDto ??= new ShiftDto();
        ShiftZone.ShiftMangerDto ??= new ShiftMangerDto();

        return base.OnParametersSetAsync();
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}