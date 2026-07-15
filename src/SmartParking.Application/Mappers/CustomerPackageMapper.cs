using System;
using System.Collections.Generic;
using System.Linq;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerPackages.Inputs;
using SmartParking.Entities.Organizations;

namespace SmartParking.Mappers
{
    public static class CustomerPackageMapper
    {
        // 1. Entity -> DTO (For Reading)
        public static CustomerPackageDto ToDto(this CustomerPackage entity)
        {
            if (entity == null) return null;

            return new CustomerPackageDto
            {
                Id = entity.Id,
                CustomerId = entity.CustomerId,
                PackageId = entity.PackageId ?? 0,
                OrganizationId = entity.OrganizationId ?? 0,
                CustomerPackageStatus = entity.CustomerPackageStatus,
                TenantId = entity.TenantId,

                // Package Details
                DurationOfPackage = entity.DurationOfPackage,
                PriceOfPackage = entity.PriceOfPackage,
                PriceOfPackagePerDay = entity.PriceOfPackagePerDay,
                CountOfDays = entity.CountOfDays,
                DescriptionOfPackage = entity.DescriptionOfPackage,
                publishedOfPackage = entity.publishedOfPackage,
                PublishFromOfPackage = entity.PublishFromOfPackage,
                PublishToOfPackage = entity.PublishToOfPackage,

                // Dates & Time Logic
                ActivationDateOfPackage = entity.ActivationDateOfPackage,
                DectivationDateOfPackage = entity.DectivationDateOfPackage,
                ExpiryDate = entity.ExpiryDate,

                // Splitting DateTime for DTO specific fields
                ExpiryDateOfPackage = DateOnly.FromDateTime(entity.ExpiryDate),
                ExpiryTimeOfPackage = TimeOnly.FromDateTime(entity.ExpiryDate),

                ActivationDate = entity.ActivationDateOfPackage.HasValue
                    ? DateOnly.FromDateTime(entity.ActivationDateOfPackage.Value) : null,
                ActivationTime = entity.ActivationDateOfPackage.HasValue
                    ? TimeOnly.FromDateTime(entity.ActivationDateOfPackage.Value) : null,

                // Metadata
                PackageArabicName = entity.PackageArabicName,
                PackageEnglishName = entity.PackageEnglishName,
                CustomerCategory = entity.CustomerCategory,
                EnPlateNumber = entity.EnPlateNumber,
                ArPlateNumber = entity.ArPlateNumber,
                IsCustomPackage = entity.IsCustomPackage,
                PackageCode = entity.PackageCode,
                QRCodePath = entity.QRCodePath,
                Discount = entity.Discount,
                Vat = entity.Vat,

                // Navigation Mapping (Requires .Include in Repository)
                CustomerArabicName = entity.Customer?.ArabicName,
            };
        }

        // 2. Create/Update DTO -> Entity
        public static void MapToEntity(this CreateUpdateCustomerPackageDto dto, CustomerPackage entity)
        {
            if (dto == null || entity == null) return;

            entity.CustomerId = dto.CustomerId;
            entity.PackageId =
                dto.PackageId.HasValue && dto.PackageId.Value > 0
                    ? dto.PackageId
                    : null; entity.OrganizationId = dto.OrganizationId;

            entity.CustomerPackageStatus = dto.CustomerPackageStatus;
            entity.TenantId = dto.TenantId;
            entity.EnPlateNumber = dto.EnPlateNumber;
            entity.ArPlateNumber = dto.ArPlateNumber;
            entity.DurationOfPackage = dto.DurationOfPackage;
            entity.PriceOfPackage = dto.PriceOfPackage;
            entity.PriceOfPackagePerDay = dto.PriceOfPackagePerDay;
            entity.CountOfDays = dto.CountOfDays;
            entity.DescriptionOfPackage = dto.DescriptionOfPackage;
            entity.publishedOfPackage = dto.PublishedOfPackage;
            entity.PublishFromOfPackage = dto.PublishFromOfPackage;
            entity.PublishToOfPackage = dto.PublishToOfPackage;
            entity.ActivationDateOfPackage = dto.ActivationDateOfPackage;
            entity.DectivationDateOfPackage = dto.DectivationDateOfPackage;
            entity.ExpiryDate = dto.ExpiryDate;
            entity.PackageArabicName = dto.PackageArabicName;
            entity.PackageEnglishName = dto.PackageEnglishName;
            entity.CustomerCategory = dto.CustomerCategory;
            entity.IsCustomPackage = dto.IsCustomPackage;
            entity.PackageCode = dto.PackageCode;
            entity.QRCodePath = dto.QRCodePath;
            entity.Discount = dto.Discount;
            entity.Vat = dto.Vat;
        }

        // 3. Collection Mappings (Lists)
        public static List<CustomerPackageDto> ToDtoList(this IEnumerable<CustomerPackage> entities)
        {
            return entities?.Select(e => e.ToDto()).ToList() ?? new List<CustomerPackageDto>();
        }
    }
}