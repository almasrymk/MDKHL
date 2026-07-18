using SmartParking.CustomerUnitPackages;
using SmartParking.Entities.CustomerUnitPackages;
using System.Collections.Generic;
using System.Linq;

namespace SmartParking.Mappers;

public static class CustomerUnitPackageMapper
{
    // 1. Entity -> CustomerUnitPackageDto
    public static CustomerUnitPackageDto MapToDto(CustomerUnitPackage entity)
    {
        if (entity == null) return null;

        return new CustomerUnitPackageDto
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            UnitPackageId = entity.UnitPackageId,
            NumberOfVehicles = entity.NumberOfVehicles,
            NumberOfSlots = entity.NumberOfSlots,
            Price = entity.Price,
            Discount = entity.Discount,
            Vat = entity.Vat,
            DurationOfPackage = entity.DurationOfPackage,
            PublishFrom = entity.PublishFrom,
            PublishTo = entity.PublishTo,
            ActivationDate = entity.ActivationDate,
            DectivationDate = entity.DectivationDate,
            ArabicName = entity.ArabicName,
            EnglishName = entity.EnglishName,
            Status = entity.Status,
            OrganizationId = entity.OrganizationId,
            CustomerCategory = entity.CustomerCategory,
            UnitCode = entity.UnitCode,
            UnitPackageType = entity.UnitPackageType,
            CustomerArabicName=entity.Customer?.ArabicName??"",
            CustomerFullName=entity.Customer?.IdentityUser?.Name??"",



        };
    }

    // 2. CreateUpdateCustomerUnitPackageDto -> Entity (For Creation)
    public static CustomerUnitPackage MapToEntity(CreateUpdateCustomerUnitPackageDto dto)
    {
        if (dto == null) return null;

        var entity = new CustomerUnitPackage();
        UpdateEntityFromDto(dto, entity);
        return entity;
    }

    // 3. CreateUpdateCustomerUnitPackageDto -> Existing Entity (For Mutations / Updates)
    public static void UpdateEntityFromDto(CreateUpdateCustomerUnitPackageDto dto, CustomerUnitPackage entity)
    {
        if (dto == null || entity == null) return;

        entity.CustomerId = dto.CustomerId;
        entity.UnitPackageId = dto.UnitPackageId;
        entity.NumberOfVehicles = dto.NumberOfVehicles;
        entity.NumberOfSlots = dto.NumberOfSlots;
        entity.Price = dto.Price;
        entity.Discount = dto.Discount;
        entity.Vat = dto.Vat;
        entity.DurationOfPackage = dto.DurationOfPackage;
        entity.PublishFrom = dto.PublishFrom;
        entity.PublishTo = dto.PublishTo;
        entity.ActivationDate = dto.ActivationDate;
        entity.DectivationDate = dto.DectivationDate;
        entity.ArabicName = dto.ArabicName;
        entity.EnglishName = dto.EnglishName;
        entity.Status = dto.Status;
        entity.OrganizationId = dto.OrganizationId;
        entity.CustomerCategory = dto.CustomerCategory;
        entity.UnitCode = dto.UnitCode;
        entity.UnitPackageType = dto.UnitPackageType;
    }

    // 4. Entity -> CreateUpdateCustomerUnitPackageDto (Useful if you need to load an edit form)
    public static CreateUpdateCustomerUnitPackageDto MapToCreateUpdateDto(CustomerUnitPackage entity)
    {
        if (entity == null) return null;

        return new CreateUpdateCustomerUnitPackageDto
        {
            CustomerId = entity.CustomerId,
            UnitPackageId = entity.UnitPackageId,
            NumberOfVehicles = entity.NumberOfVehicles,
            NumberOfSlots = entity.NumberOfSlots,
            Price = entity.Price,
            Discount = entity.Discount,
            Vat = entity.Vat,
            DurationOfPackage = entity.DurationOfPackage,
            PublishFrom = entity.PublishFrom,
            PublishTo = entity.PublishTo,
            ActivationDate = entity.ActivationDate,
            DectivationDate = entity.DectivationDate,
            ArabicName = entity.ArabicName,
            EnglishName = entity.EnglishName,
            Status = entity.Status,
            OrganizationId = entity.OrganizationId,
            CustomerCategory = entity.CustomerCategory,
            UnitCode = entity.UnitCode,
            UnitPackageType = entity.UnitPackageType
        };
    }

    // 5. Collection Mapping: IEnumerable<Entity> -> List<CustomerUnitPackageDto>
    public static List<CustomerUnitPackageDto> MapToDtoList(IEnumerable<CustomerUnitPackage> entities)
    {
        return entities?.Select(MapToDto).ToList() ?? new List<CustomerUnitPackageDto>();
    }

    public static CreateUpdateCustomerUnitPackageDto ToCreateUpdateDto(this CustomerUnitPackageDto src)
    {
        if (src == null) return null;

        return new CreateUpdateCustomerUnitPackageDto
        {
            // Direct/Identical Mapping
            NumberOfVehicles = src.NumberOfVehicles,
            NumberOfSlots = src.NumberOfSlots,
            Price = src.Price,
            PublishFrom = src.PublishFrom,
            PublishTo = src.PublishTo,
            ActivationDate = src.ActivationDate,
            DectivationDate = src.DectivationDate,
            ArabicName = src.ArabicName,
            EnglishName = src.EnglishName,
            CustomerId = src.CustomerId,
            UnitPackageId = src.UnitPackageId,
            Status = src.Status,
            Discount = src.Discount,
            DurationOfPackage = src.DurationOfPackage,
            Vat = src.Vat,
            OrganizationId = src.OrganizationId,
            CustomerCategory = src.CustomerCategory,
            UnitCode = src.UnitCode,
            UnitPackageType = src.UnitPackageType
        };
    }
}
