using SmartParking.CustomerPackages.Dtos;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;

namespace SmartParking.CustomerPackages.Inputs;

public class CreateUpdateCustomerPackageDto
{
    // Core relation fields
    public Guid CustomerId { get; set; }
    public int PackageId { get; set; }
    public int OrganizationId { get; set; }
    public CustomerPackageStatus CustomerPackageStatus { get; set; }
    public Guid? TenantId { get; set; }

    // Plate
    public string EnPlateNumber { get; set; } = default!;
    public string ArPlateNumber { get; set; } = default!;

    // ========= Package-related values =========
    public int? DurationOfPackage { get; set; }
    public decimal? PriceOfPackage { get; set; }
    public decimal? PriceOfPackagePerDay { get; set; }
    public decimal? CountOfDays { get; set; }
    public string DescriptionOfPackage { get; set; }
    public bool? PublishedOfPackage { get; set; } // renamed to PascalCase

    public TimeOnly? PublishFromOfPackage { get; set; }
    public TimeOnly? PublishToOfPackage { get; set; }

    public bool IsCustomPackage { get; set; }
    public string PackageCode { get; set; }
    public string QRCodePath { get; set; }

    public string PackageArabicName { get; set; }
    public string PackageEnglishName { get; set; }

    public decimal Discount { get; set; }
    public decimal Vat { get; set; }

    public string LicenseTypeImage { get; set; }
    public string VehicleModelImage { get; set; }

    // ========= All dates/times =========
    public DateTime? ActivationDateOfPackage { get; set; }
    public DateTime? DectivationDateOfPackage { get; set; } // keeping your spelling
    public DateTime ExpiryDate { get; set; } // non-null in source

    public DateOnly ExpiryDateOfPackage { get; set; }
    public TimeOnly ExpiryTimeOfPackage { get; set; }

    public DateOnly? ActivationDate { get; set; }
    public TimeOnly? ActivationTime { get; set; }
    public CustomerCategoryStatus CustomerCategory { get; set; }


    // Manual mapping
    public static CreateUpdateCustomerPackageDto From(CustomerPackageDto src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        return new CreateUpdateCustomerPackageDto
        {
            // core
            CustomerId = src.CustomerId,
            PackageId = src.PackageId,
            OrganizationId = src.OrganizationId,
            CustomerPackageStatus = src.CustomerPackageStatus,
            TenantId = src.TenantId,

            // plate
            EnPlateNumber = src.EnPlateNumber ?? string.Empty,
            ArPlateNumber = src.ArPlateNumber ?? string.Empty,

            // package values
            DurationOfPackage = src.DurationOfPackage,
            PriceOfPackage = src.PriceOfPackage,
            PriceOfPackagePerDay = src.PriceOfPackagePerDay,
            CountOfDays = src.CountOfDays,
            DescriptionOfPackage = src.DescriptionOfPackage,
            PublishedOfPackage = src.publishedOfPackage,
            CustomerCategory= src.CustomerCategory,
            PublishFromOfPackage = src.PublishFromOfPackage,
            PublishToOfPackage = src.PublishToOfPackage,

            IsCustomPackage = src.IsCustomPackage,
            PackageCode = src.PackageCode,
            QRCodePath = src.QRCodePath,

            PackageArabicName = src.PackageArabicName,
            PackageEnglishName = src.PackageEnglishName,

            Discount = src.Discount,
            Vat = src.Vat,

            LicenseTypeImage = src.LicenseTypeImage,
            VehicleModelImage = src.VehicleModelImage,

            // dates/times
            ActivationDateOfPackage = src.ActivationDateOfPackage,
            DectivationDateOfPackage = src.DectivationDateOfPackage,
            ExpiryDate = src.ExpiryDate,

            ExpiryDateOfPackage = src.ExpiryDateOfPackage,
            ExpiryTimeOfPackage = src.ExpiryTimeOfPackage,

            ActivationDate = src.ActivationDate,
            ActivationTime = src.ActivationTime
        };
    }

    public void MapFrom(CustomerPackageDto src)
    {
        var mapped = From(src);

        CustomerId = mapped.CustomerId;
        PackageId = mapped.PackageId;
        OrganizationId = mapped.OrganizationId;
        CustomerPackageStatus = mapped.CustomerPackageStatus;
        TenantId = mapped.TenantId;

        EnPlateNumber = mapped.EnPlateNumber;
        ArPlateNumber = mapped.ArPlateNumber;

        DurationOfPackage = mapped.DurationOfPackage;
        PriceOfPackage = mapped.PriceOfPackage;
        PriceOfPackagePerDay = mapped.PriceOfPackagePerDay;
        CountOfDays = mapped.CountOfDays;
        DescriptionOfPackage = mapped.DescriptionOfPackage;
        PublishedOfPackage = mapped.PublishedOfPackage;

        PublishFromOfPackage = mapped.PublishFromOfPackage;
        PublishToOfPackage = mapped.PublishToOfPackage;

        IsCustomPackage = mapped.IsCustomPackage;
        PackageCode = mapped.PackageCode;
        QRCodePath = mapped.QRCodePath;

        PackageArabicName = mapped.PackageArabicName;
        PackageEnglishName = mapped.PackageEnglishName;

        Discount = mapped.Discount;
        Vat = mapped.Vat;

        LicenseTypeImage = mapped.LicenseTypeImage;
        VehicleModelImage = mapped.VehicleModelImage;

        ActivationDateOfPackage = mapped.ActivationDateOfPackage;
        DectivationDateOfPackage = mapped.DectivationDateOfPackage;
        ExpiryDate = mapped.ExpiryDate;

        ExpiryDateOfPackage = mapped.ExpiryDateOfPackage;
        ExpiryTimeOfPackage = mapped.ExpiryTimeOfPackage;

        ActivationDate = mapped.ActivationDate;
        ActivationTime = mapped.ActivationTime;
    }

    public CustomerPackage ToEntity()
    {
        var entity = new CustomerPackage();
        MapTo(entity);
        return entity;
    }

    /// <summary>
    /// Maps DTO -> existing entity (best for Update scenarios).
    /// Does NOT touch navigation properties (Customer/Package/Organization).
    /// </summary>
    public void MapTo(CustomerPackage entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));

        entity.CustomerId = CustomerId;

        // Entity expects int? but dto has int
        entity.PackageId = PackageId;
        entity.OrganizationId = OrganizationId;

        entity.CustomerPackageStatus = CustomerPackageStatus;
        entity.TenantId = TenantId;

        // Non-nullable strings in entity
        entity.EnPlateNumber = EnPlateNumber ?? string.Empty;
        entity.ArPlateNumber = ArPlateNumber ?? string.Empty;

        entity.PackageArabicName = PackageArabicName ?? string.Empty;
        entity.PackageEnglishName = PackageEnglishName ?? string.Empty;

        // Dates / package values
        entity.ExpiryDate = ExpiryDate;

        entity.DurationOfPackage = DurationOfPackage;
        entity.PriceOfPackage = PriceOfPackage;
        entity.PriceOfPackagePerDay = PriceOfPackagePerDay;
        entity.CountOfDays = CountOfDays;

        entity.DescriptionOfPackage = DescriptionOfPackage;
        entity.publishedOfPackage = PublishedOfPackage;

        entity.PublishFromOfPackage = PublishFromOfPackage;
        entity.PublishToOfPackage = PublishToOfPackage;

        entity.ActivationDateOfPackage = ActivationDateOfPackage;
        entity.DectivationDateOfPackage = DectivationDateOfPackage;

        entity.CustomerCategory = CustomerCategory;

        entity.IsCustomPackage = IsCustomPackage;
        entity.PackageCode = PackageCode;
        entity.QRCodePath = QRCodePath;

        entity.Discount = Discount;
        entity.Vat = Vat;

    }

    public static CreateUpdateCustomerPackageDto FromEntity(CustomerPackage entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        return new CreateUpdateCustomerPackageDto
        {
            // Core
            CustomerId = entity.CustomerId,
            PackageId = entity.PackageId ?? 0,
            OrganizationId = entity.OrganizationId ?? 0,
            CustomerPackageStatus = entity.CustomerPackageStatus,
            TenantId = entity.TenantId,

            // Plate (non-nullable in DTO)
            EnPlateNumber = entity.EnPlateNumber ?? string.Empty,
            ArPlateNumber = entity.ArPlateNumber ?? string.Empty,

            // Package values
            DurationOfPackage = entity.DurationOfPackage,
            PriceOfPackage = entity.PriceOfPackage,
            PriceOfPackagePerDay = entity.PriceOfPackagePerDay,
            CountOfDays = entity.CountOfDays,
            DescriptionOfPackage = entity.DescriptionOfPackage,
            PublishedOfPackage = entity.publishedOfPackage,

            PublishFromOfPackage = entity.PublishFromOfPackage,
            PublishToOfPackage = entity.PublishToOfPackage,

            ActivationDateOfPackage = entity.ActivationDateOfPackage,
            DectivationDateOfPackage = entity.DectivationDateOfPackage,

            ExpiryDate = entity.ExpiryDate,

            PackageArabicName = entity.PackageArabicName ?? string.Empty,
            PackageEnglishName = entity.PackageEnglishName ?? string.Empty,

            CustomerCategory = entity.CustomerCategory,

            IsCustomPackage = entity.IsCustomPackage,
            PackageCode = entity.PackageCode,
            QRCodePath = entity.QRCodePath,

            Discount = entity.Discount,
            Vat = entity.Vat,

            // Derived fields (DTO only)
            ExpiryDateOfPackage = DateOnly.FromDateTime(entity.ExpiryDate),
            ExpiryTimeOfPackage = TimeOnly.FromDateTime(entity.ExpiryDate),

            ActivationDate = entity.ActivationDateOfPackage.HasValue
                ? DateOnly.FromDateTime(entity.ActivationDateOfPackage.Value)
                : null,

            ActivationTime = entity.ActivationDateOfPackage.HasValue
                ? TimeOnly.FromDateTime(entity.ActivationDateOfPackage.Value)
                : null,

            // Not موجودة في entity
            LicenseTypeImage = null,
            VehicleModelImage = null
        };
    }
}
