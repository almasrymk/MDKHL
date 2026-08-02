using SmartParking.Entities.GarageTransactions;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartParking.Mappers
{
    public static class GarageTransactionMapper
    {
        // 1. Entity -> GarageTransactionDto (Read)
        // Single source of truth for computed properties (matches GarageTransactionWorkerRepository background job logic)
        private static (DateTime? remainingTime, bool isPending, bool canCheckout)
            ComputeFields(DateTime? checkoutMobile, int? graceperiod)
        {
            if (!checkoutMobile.HasValue)
            {
                return (null, false, true);
            }

            var now = DateTime.Now;
            var graceDeadline = checkoutMobile.Value.AddMinutes(graceperiod ?? 0);

            var remainingTime = graceDeadline <= now ? DateTime.MinValue : graceDeadline;
            var isPending = graceDeadline > now;
            var canCheckout = graceDeadline <= now;

            return (remainingTime, isPending, canCheckout);
        }

        // Used by ToDto (entity → dto, full mapping with auditing fields)
        public static GarageTransactionDto ToDto(this GarageTransaction entity)
        {
            if (entity == null) return null;

            var (remainingTime, isPending, canCheckout) = ComputeFields(
                entity.CheckoutMobile,
                entity.Organization?.Graceperiod);

            return new GarageTransactionDto
            {
                Id = entity.Id,
                CameraId = entity.CameraId,
                PlateNumber = entity.PlateNumber,
                SnapTime = entity.SnapTime,
                AccurateTime = entity.AccurateTime,
                OrganizationID = entity.OrganizationID,
                GarageId = entity.GarageId,
                VehiclePicture = entity.VehiclePicture,
                CameraType = entity.CameraType,
                TenantId = entity.TenantId,
                IsIn = entity.IsIn,
                CheckoutMobile = entity.CheckoutMobile,
                IsMapped = entity.IsMapped,
                CustomerCategory = entity.CustomerCategory,
                DispenserId = entity.DispenserId,
                LaneNO = entity.LaneNO,
                TicketId = entity.TicketId,
                OrganizationArabicName = entity.Organization?.ArabicName,
                OrganizationEnglishName = entity.Organization?.EnglishName,
                GarageArabicName = entity.Garage?.NameAr,
                GarageEnglishName = entity.Garage?.NameEn,
                ArabicName = entity.Garage?.NameAr,
                CreationTime = entity.CreationTime,
                CreatorId = entity.CreatorId,
                LastModificationTime = entity.LastModificationTime,
                LastModifierId = entity.LastModifierId,
                DeleterId = entity.DeleterId,
                DeletionTime = entity.DeletionTime,
                IsDeleted = entity.IsDeleted,
                // Computed
                RemainingTime = remainingTime,
                IsPending = isPending,
                CanCheckout = canCheckout,
            };
        }

        // 2. CreateUpdateDto -> Entity (Create)
        public static GarageTransaction ToEntity(this CreateUpdateGarageTransactionDto dto)
        {
            if (dto == null) return null;

            return new GarageTransaction
            {
                CameraId = dto.CameraId,
                PlateNumber = dto.PlateNumber,
                SnapTime = dto.SnapTime,
                AccurateTime = dto.AccurateTime,
                OrganizationID = dto.OrganizationID,
                GarageId = dto.GarageId,
                VehiclePicture = dto.VehiclePicture,
                CameraType = dto.CameraType,
                TenantId = dto.TenantId,
                IsIn = dto.IsIn,
                CheckoutMobile = dto.CheckoutMobile,
                IsMapped = dto.IsMapped,
                RemainingTime = dto.RemainingTime,
                IsPending = dto.IsPending,
                CustomerCategory = dto.CustomerCategory,
                CanCheckout = dto.CanCheckout,
                DispenserId = dto.DispenserId,
                LaneNO = dto.LaneNO,
                TicketId = dto.TicketId
                // Note: PackageCode is in DTO but not in Entity; handle separately if needed.
            };
        }

        // 3. Update existing Entity from DTO (Update)
        public static void MapToEntity(this CreateUpdateGarageTransactionDto dto, GarageTransaction entity)
        {
            if (dto == null || entity == null) return;

            entity.PlateNumber = dto.PlateNumber;
            entity.SnapTime = dto.SnapTime;
            entity.AccurateTime = dto.AccurateTime;
            entity.CheckoutMobile = dto.CheckoutMobile;
            entity.IsIn = dto.IsIn;
            entity.IsMapped = dto.IsMapped;
            entity.IsPending = dto.IsPending;
            entity.CustomerCategory = dto.CustomerCategory;
            entity.CanCheckout = dto.CanCheckout;
            entity.DispenserId = dto.DispenserId;
            entity.LaneNO = dto.LaneNO;
            entity.TicketId = dto.TicketId;
            // Add other fields you want to allow updating
        }

        // 4. List Mappings
        public static List<GarageTransactionDto> ToDtoList(this IEnumerable<GarageTransaction> entities)
        {
            return entities?.Select(e => e.ToDto()).ToList() ?? new List<GarageTransactionDto>();
        }

        public static List<GarageTransactionDto> ToDtoList(this IEnumerable<GarageTransactionRawDto> dtos)
        {
            return dtos?.Select(x => x.ToDto()).ToList() ?? new List<GarageTransactionDto>();
        }

        public static GarageTransactionDto ToDto(this GarageTransactionRawDto x)
        {
            if (x == null) return null;

            var (remainingTime, isPending, canCheckout) = ComputeFields(
                x.CheckoutMobile,
                x.Graceperiod);

            return new GarageTransactionDto
            {
                Id = x.Id,
                CameraId = x.CameraId,
                PlateNumber = x.PlateNumber,
                SnapTime = x.SnapTime,
                AccurateTime = x.AccurateTime,
                OrganizationID = x.OrganizationID,
                GarageId = x.GarageId,
                VehiclePicture = x.VehiclePicture,
                CameraType = x.CameraType,
                TenantId = x.TenantId,
                IsIn = x.IsIn,
                CheckoutMobile = x.CheckoutMobile,
                IsMapped = x.IsMapped,
                CustomerCategory = x.CustomerCategory,
                OrganizationArabicName = x.OrganizationArabicName,
                OrganizationEnglishName = x.OrganizationEnglishName,
                GarageArabicName = x.GarageArabicName,
                GarageEnglishName = x.GarageEnglishName,
                DispenserId = x.DispenserId,
                LaneNO = x.LaneNO,
                TicketId = x.TicketId,
                ArabicName = x.ArabicName,
                // Computed
                RemainingTime = remainingTime,
                IsPending = isPending,
                CanCheckout = canCheckout,
            };
        }
    }
}