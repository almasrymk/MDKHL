using SmartParking.CustomerPackages.Dtos;
using SmartParking.Entities.Organizations;
using SmartParking.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages.Mappings;
public static class CustomerPackageMapper
{
    public static CustomerPackageTransactionDto MapToCustomerPackageTransactionDto( this
        CustomerPackage entity)
    {
        if (entity == null)
            return null;

        return new CustomerPackageTransactionDto
        {
            Id = entity.Id,

            CustomerId = entity.CustomerId,

            PackageId = entity.PackageId ?? 0,

            OrganizationId = entity.OrganizationId ?? 0,

            Package = entity.Package == null
                ? null
                : new PackageDto
                {
                    EnglishName = entity.Package.EnglishName,
                    ArabicName = entity.Package.ArabicName,
                    Description = entity.Package.Description,
                    Id = entity.Package.Id,
                    Price = entity.Package.Price,
                    Published = entity.Package.published,
                    PublishFrom = entity.Package.PublishFrom,
                },

            CustomerPackageStatus = entity.CustomerPackageStatus,

            TenantId = entity.TenantId,

            DurationOfPackage = entity.DurationOfPackage,

            PriceOfPackage = entity.PriceOfPackage,

            DescriptionOfPackage = entity.DescriptionOfPackage,

            publishedOfPackage = entity.publishedOfPackage,

            PublishFromOfPackage = entity.PublishFromOfPackage,

            PublishToOfPackage = entity.PublishToOfPackage,

            ActivationDateOfPackage = entity.ActivationDateOfPackage,

            DectivationDateOfPackage = entity.DectivationDateOfPackage,

            ExpiryDate = entity.ExpiryDate,

            PackageArabicName = entity.PackageArabicName,

            PackageEnglishName = entity.PackageEnglishName,

            CustomerCategory = entity.CustomerCategory,

            CustomerArabicName = entity.Customer?.ArabicName,

            CustomerEnglishName = entity.Customer?.IdentityUser.Name,

            EnPlateNumber = entity.EnPlateNumber,

            ArPlateNumber = entity.ArPlateNumber,

            ExpiryDateOfPackage = DateOnly.FromDateTime(entity.ExpiryDate),

            ExpiryTimeOfPackage = TimeOnly.FromDateTime(entity.ExpiryDate),

            ActivationDate = entity.ActivationDateOfPackage.HasValue
                ? DateOnly.FromDateTime(entity.ActivationDateOfPackage.Value)
                : null,

            ActivationTime = entity.ActivationDateOfPackage.HasValue
                ? TimeOnly.FromDateTime(entity.ActivationDateOfPackage.Value)
                : null,

            IsCustomPackage = entity.IsCustomPackage,

            PackageCode = entity.PackageCode,

            QRCodePath = entity.QRCodePath
        };
    }

    public static CustomerPackageMobileDto MapToMobileDto(this CustomerPackageDto source)
    {
        if (source == null)
            return null;

        return new CustomerPackageMobileDto
        {
            Id = source.Id,
            CustomerPackageStatus = source.CustomerPackageStatus,

            DurationOfPackage = source.DurationOfPackage,
            PriceOfPackage = source.PriceOfPackage,
            PriceOfPackagePerDay = source.PriceOfPackagePerDay,
            CountOfDays = source.CountOfDays,

            PublishedOfPackage = source.publishedOfPackage,

            PackageArabicName = source.PackageArabicName,
            PackageEnglishName = source.PackageEnglishName,

            CustomerCategory = source.CustomerCategory,

            CustomerArabicName = source.CustomerArabicName,
            CustomerEnglishName = source.CustomerEnglishName,

            OrganizationArabicName = source.Organization?.ArabicName,
            OrganizationEnglishName = source.Organization?.EnglishName,

            EnPlateNumber = source.EnPlateNumber,
            ArPlateNumber = source.ArPlateNumber,

            ExpiryDateOfPackage = source.ExpiryDateOfPackage,
            ExpiryTimeOfPackage = source.ExpiryTimeOfPackage,

            ActivationDate = source.ActivationDate,
            ActivationTime = source.ActivationTime,

            IsCustomPackage = source.IsCustomPackage,

            PackageCode = source.PackageCode,
            QRCodePath = source.QRCodePath,

            Discount = source.Discount,
            Vat = source.Vat,

            LicenseTypeImage = source.LicenseTypeImage,
            VehicleModelImage = source.VehicleModelImage
        };
    }

    public static List<CustomerPackageMobileDto> MapToMobileDtos(this IEnumerable<CustomerPackageDto> source)
    {
        return source?.Select(x => x.MapToMobileDto()).ToList() ?? new List<CustomerPackageMobileDto>();
    }

}