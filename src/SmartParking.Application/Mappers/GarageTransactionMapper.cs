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
        // Single source of truth for computed properties
        private static (DateTime? remainingTime, bool isPending, bool canCheckout)
            ComputeFields(DateTime? checkoutMobile, int? graceperiod, WatcherType cameraType, bool isIn, bool isMapped)
        {
            var now = DateTime.Now;
            var graceDeadline = checkoutMobile.HasValue
                ? checkoutMobile.Value.AddMinutes(graceperiod ?? 0)
                : (DateTime?)null;

            var remainingTime = graceDeadline.HasValue
                ? graceDeadline.Value <= now ? DateTime.MinValue : graceDeadline.Value
                : (DateTime?)null;

            var isPending = graceDeadline.HasValue && graceDeadline.Value > now;
            var canCheckout =  isIn
                              && !isMapped
                              && (!checkoutMobile.HasValue || graceDeadline!.Value <= now);

            return (remainingTime, isPending, canCheckout);
        }

        // Used by ToDto (entity → dto, full mapping with auditing fields)
        public static GarageTransactionDto ToDto(this GarageTransaction entity)
        {
            if (entity == null) return null;

            var (remainingTime, isPending, canCheckout) = ComputeFields(
                entity.CheckoutMobile,
                entity.Organization?.Graceperiod,
                entity.CameraType,
                entity.IsIn,
                entity.IsMapped);

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
        public static GarageTransactionDto ToDto(this GarageTransactionRawDto x)
        {
            var (remainingTime, isPending, canCheckout) = ComputeFields(
                x.CheckoutMobile,
                x.Graceperiod,
                x.CameraType,
                x.IsIn,
                x.IsMapped);

            return new GarageTransactionDto
            {
                Id = x.Id,
                PlateNumber = x.PlateNumber,
                AccurateTime = x.AccurateTime,
                CameraType = x.CameraType,
                SnapTime = x.SnapTime,
                TicketId = x.TicketId,
                CustomerCategory = x.CustomerCategory,
                IsMapped = x.IsMapped,
                IsIn = x.IsIn,
                CheckoutMobile = x.CheckoutMobile,
                GarageEnglishName = x.GarageEnglishName,
                GarageArabicName = x.GarageArabicName,
                OrganizationEnglishName = x.OrganizationEnglishName,
                OrganizationArabicName = x.OrganizationArabicName,
                ArabicName = x.ArabicName,
                // Computed
                RemainingTime = remainingTime,
                IsPending = isPending,
                CanCheckout = canCheckout,
            };
        }
    }
}