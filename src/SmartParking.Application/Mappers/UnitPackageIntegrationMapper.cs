using SmartParking.Entities.UnitPackageIntegrations;
using SmartParking.UnitPackageIntegrations.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace SmartParking.Mappers;

public static class UnitPackageIntegrationMapper
{
    // 1. Entity -> UnitPackageIntegrationDto (Read)
    public static UnitPackageIntegrationDto ToDto(this UnitPackageIntegration entity)
    {
        if (entity == null) return null;

        return new UnitPackageIntegrationDto
        {
            Id = entity.Id,
            BookingId = entity.BookingId,
            OrganizationId = entity.OrganizationId,
            TenantId = entity.TenantId,
            UnitCode = entity.UnitCode,
            UnitVehiclePackageId = entity.UnitVehiclePackageId,
            CustomerId = entity.CustomerId
        };
    }

    // 2. CreateUpdateDto -> Entity (Create)
    public static UnitPackageIntegration ToEntity(this CreateUpdateUnitPackageIntegrationDto dto)
    {
        if (dto == null) return null;

        return new UnitPackageIntegration
        {
            BookingId = dto.BookingId,
            OrganizationId = dto.OrganizationId,
            TenantId = dto.TenantId,
            UnitCode = dto.UnitCode,
            UnitVehiclePackageId = dto.UnitVehiclePackageId,
            CustomerId = dto.CustomerId
        };
    }

    // 3. Update existing Entity from DTO (Update)
    public static void MapToEntity(this CreateUpdateUnitPackageIntegrationDto dto, UnitPackageIntegration entity)
    {
        if (dto == null || entity == null) return;

        entity.UnitVehiclePackageId = dto.UnitVehiclePackageId;
        entity.UnitCode = dto.UnitCode;
        entity.CustomerId = dto.CustomerId; 
        entity.BookingId = dto.BookingId;
        entity.OrganizationId = dto.OrganizationId;
        entity.TenantId = dto.TenantId;
    }

    // 4. List Mappings
    public static List<UnitPackageIntegrationDto> ToDtoList(this IEnumerable<UnitPackageIntegration> entities)
    {
        return entities?.Select(e => e.ToDto()).ToList() ?? new List<UnitPackageIntegrationDto>();
    }
}
