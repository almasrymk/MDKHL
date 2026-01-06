using MudBlazor;
using SmartParking.Blazor.Components.State;
using SmartParking.Localization;

namespace SmartParking.Blazor.Components.Shared.Utilities;

public static class CustomPackageValidations
{
    public static void CheckIsCustomProperties(CustomPackageUiDto customerPackageDto , ISnackbar Snackbar )
    {
        if (customerPackageDto.ActivationDate == null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("StartDate") + " " + LocalizationHelper.GetLocalizedString("EmptyValidation"));
        }
        if (customerPackageDto.DectivationDate == null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("EndDate") + " " + LocalizationHelper.GetLocalizedString("EmptyValidation"));
        }
        if (customerPackageDto.startTime == null)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{LocalizationHelper.GetLocalizedString("StartTime")} , " +
          $"{LocalizationHelper.GetLocalizedString("EmptyValidation")} " +
          $"{LocalizationHelper.GetLocalizedString("TimeMustBeEntered")} ", Severity.Error);
        }
        if (customerPackageDto.endTime == null)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{LocalizationHelper.GetLocalizedString("EndTime")} , " +
                    $"{LocalizationHelper.GetLocalizedString("EmptyValidation")} " +
                    $"{LocalizationHelper.GetLocalizedString("TimeMustBeEntered")} ", Severity.Error);
        }
        if (customerPackageDto.PriceOfPackage == null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("Price") + " " + LocalizationHelper.GetLocalizedString("EmptyValidation"));
        }
    }
}
