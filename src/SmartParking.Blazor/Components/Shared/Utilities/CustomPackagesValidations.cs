using MudBlazor;
using SmartParking.Blazor.Components.State;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Packages;

namespace SmartParking.Blazor.Components.Shared.Utilities;

public static class BlazorPackageValidations
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

    public static void MapToCreateUpdatePackage(
        CreateUpdatePackageDto createUpdatePackage,
        CustomPackageUiDto? customDto,
        NormalPackagePropertiesDto? normalDto,
        ISnackbar snackbar)
    {
        if (createUpdatePackage.PackageCategoryType == PackageCategoryTypes.CustomPackage)
        {
            CheckIsCustomProperties(customDto, snackbar);

            createUpdatePackage.ActivationDate = customDto!.ActivationDate!.Value;
            createUpdatePackage.DeactivationDate = customDto.DectivationDate!.Value;
            createUpdatePackage.PublishFrom = TimeOnly.FromTimeSpan(customDto.startTime!.Value);
            createUpdatePackage.PublishTo = TimeOnly.FromTimeSpan(customDto.endTime!.Value);
            createUpdatePackage.DurationOfPackage = (int)customDto.CountOfDays!.Value;
            createUpdatePackage.Price = customDto.PriceOfPackage!.Value;
        }
        else
        {
            createUpdatePackage.ActivationDate = DateTime.Now;
            createUpdatePackage.Published = normalDto!.Published;
            createUpdatePackage.PublishFrom = normalDto.PublishFrom;
            createUpdatePackage.PublishTo = normalDto.PublishTo;
        }
    }
}
