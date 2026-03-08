using MudBlazor;
using SmartParking.Blazor.Components.State;
using SmartParking.CustomerPackages.Dtos;
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
            createUpdatePackage.DurationOfPackage = normalDto.DurationOfPackage;
        }
    }
    public static CustomerPackageStatus GetDisplayStatus(CustomerPackageDto package)
    {
        var currentDate = DateOnly.FromDateTime(DateTime.Now);
        var currentTime = TimeOnly.FromDateTime(DateTime.Now);

        // If the package status is not Active in DB, return the DB status
        if (package.CustomerPackageStatus != CustomerPackageStatus.Active)
        {
            return package.CustomerPackageStatus;
        }

        // Check if dates are available
        if (!package.ActivationDateOfPackage.HasValue || package.ExpiryDate ==null)
        {
            return package.CustomerPackageStatus;
        }

        var activationDate = DateOnly.FromDateTime(package.ActivationDateOfPackage.Value);
        var expirationDate = DateOnly.FromDateTime(package.ExpiryDate);

        // Check if before activation date
        if (currentDate < activationDate)
        {
            return CustomerPackageStatus.InActive;
        }

        // Check if after expiration date
        if (currentDate > expirationDate)
        {
            return CustomerPackageStatus.Expired;
        }

        // We are within the valid date range
        // Now check if we're on the activation date or expiration date

        // If on activation date, check if time has started
        if (currentDate == activationDate)
        {
            // 24-hour access - active immediately on activation date
            if (package.publishedOfPackage.HasValue && package.publishedOfPackage.Value)
            {
                return CustomerPackageStatus.Active;
            }

            // Check if time has started
            if (package.PublishFromOfPackage.HasValue)
            {
                if (currentTime < package.PublishFromOfPackage.Value)
                {
                    // Time hasn't started yet on activation date
                    return CustomerPackageStatus.InActive;
                }
            }

            return CustomerPackageStatus.Active;
        }

        // If on expiration date, check if time has passed
        if (currentDate == expirationDate)
        {
            // 24-hour access - active until end of expiration date
            if (package.publishedOfPackage.HasValue && package.publishedOfPackage.Value)
            {
                return CustomerPackageStatus.Active;
            }

            // Check if time has passed
            if (package.PublishToOfPackage.HasValue)
            {
                if (currentTime > package.PublishToOfPackage.Value)
                {
                    // Time has passed on expiration date
                    return CustomerPackageStatus.Expired;
                }
            }

            return CustomerPackageStatus.Active;
        }

        // We are between activation and expiration dates (not on either boundary)
        // Package is active regardless of time
        return CustomerPackageStatus.Active;
    }

   
}
