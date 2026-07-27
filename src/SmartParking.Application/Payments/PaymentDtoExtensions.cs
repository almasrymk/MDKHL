using SmartParking.Enums;
using SmartParking.Entities; // Assuming Invoice is here, wait I need the exact namespace
using SmartParking.Payments.PaymentIntegrations;
using SmartParking.Entities.Invoices;
using System;
using SmartParking.CustomerPackages;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
using SmartParking.Packages;

namespace SmartParking.Payments;

public static class PaymentDtoExtensions
{
    public static PaymentCheckoutRequestDto ToPaymentCheckoutRequestDto(
        this Invoice invoice, 
        decimal amount, 
        PaymentMethod paymentMethod, 
        string? callbackUrl = null)
    {
        return new PaymentCheckoutRequestDto
        {
            InvoiceId = invoice.Id,
            OrganizationId = invoice.OrganizationId,
            Amount = amount,
            PaymentCategory = PaymentCategory.PaymentParking,
            PaymentMethod = paymentMethod,
            TenantId = invoice.TenantId.Value,
            CallbackUrl = callbackUrl,
        };
    }

    public static MobilePaymentSessionDto ToMobilePaymentSessionDto(this GatewayInitiateResult initiateResult)
    {
        return new MobilePaymentSessionDto
        {
            PaymentId = initiateResult.Gateway == GatewayKey.Geidea ? default : default,
            SessionId = initiateResult.GeideaSessionId ?? initiateResult.OppwaCheckoutId,
            SessionUrl = initiateResult.JsArguments.Length > 3 ? initiateResult.JsArguments[3]?.ToString() : null,
            MerchantReferenceId = initiateResult.PaymentNumber,
            ExpiresAt = null,
            GatewayResponse = initiateResult.GatewayResponseData
        };
    }

    public static PaymentCheckoutRequestDto ToPaymentCheckoutRequestDto(
        this SmartParking.Entities.Organizations.CustomerPackage customerPackage,
        decimal amount,
        PaymentMethod paymentMethod,
        string? callbackUrl = null)
    {
        return new PaymentCheckoutRequestDto
        {
            OrganizationId = customerPackage.OrganizationId ?? 0,
            PaymentCategory = PaymentCategory.Package,
            CustomerPackageId = customerPackage.Id,
            PackageId = customerPackage.PackageId,
            PaymentMethod = paymentMethod,
            Amount = amount,
            CustomerId = customerPackage.CustomerId,
            ArPlateNumber = customerPackage.ArPlateNumber,
            EnPlateNumber = customerPackage.EnPlateNumber,
            TenantId = customerPackage.TenantId ?? System.Guid.Empty,
            CallbackUrl = callbackUrl
        };
    }

    public static PaymentDetailDto ToPaymentDetailDto(this Payment payment, CustomerPackageDto customerPackageDto, Organization? organization)
    {
        return new PaymentDetailDto
        {
            Id = payment.Id,
            CreationTime = payment.CreationTime,
            CreatorId = payment.CreatorId,
            LastModificationTime = payment.LastModificationTime,
            LastModifierId = payment.LastModifierId,
            IsDeleted = payment.IsDeleted,
            DeleterId = payment.DeleterId,
            DeletionTime = payment.DeletionTime,
            InvoiceNumber = payment.PaymentNumber ?? customerPackageDto.PackageCode ?? $"PKG-{customerPackageDto.Id}",
            TotalHours = customerPackageDto.DurationOfPackage ?? (int)((customerPackageDto.CountOfDays ?? 0) * 24),
            InvoicePrice = payment.Amount,
            InvoiceType = PaymentCategory.Package.ToString(),
            OrganizationArabicName = customerPackageDto.Organization?.ArabicName ?? organization?.ArabicName ?? string.Empty,
            OrganizationEnglishName = customerPackageDto.Organization?.EnglishName ?? organization?.EnglishName ?? string.Empty,
            GarageArabicName = string.Empty,
            GarageEnglishName = string.Empty,
            PlateNumber = customerPackageDto.EnPlateNumber ?? customerPackageDto.ArPlateNumber ?? payment.EnPlateNumber ?? payment.ArPlateNumber ?? string.Empty,
            InvoiceTime = payment.CreationTime,
            EntranceTime = customerPackageDto.ActivationDateOfPackage ?? payment.CreationTime,
            CustomerArName = customerPackageDto.CustomerArabicName ?? customerPackageDto.Customer?.ArabicName ?? string.Empty,
            CustomerEnName = customerPackageDto.CustomerEnglishName ?? customerPackageDto.Customer?.FullName ?? string.Empty,
            IsPackage = true,
            CustomerId = customerPackageDto.CustomerId,
            EntryTransactionImage = string.Empty,
            ExitTransactionImage = string.Empty,
            QrCodeInvoice = payment.QrCodeInvoice ?? customerPackageDto.QRCodePath ?? string.Empty,
            InvoiceId = 0,
            OrganizationId = payment.OrganizationId,
            OrganizationAddress = organization?.Address ?? string.Empty,
            OrganizationTIN = organization?.TaxIdentificationNumber ?? string.Empty,
            GarageTransactionExitId = 0,
            Vat = customerPackageDto.Vat != 0 ? customerPackageDto.Vat : Math.Round((payment.Amount - customerPackageDto.Discount) * 0.15m, 2),
            Discount = customerPackageDto.Discount,
            TicketId = string.Empty,
            TenantId = payment.TenantId ?? customerPackageDto.TenantId,
            LicenseTypeImage = customerPackageDto.LicenseTypeImage,
            VehicleModelImage = customerPackageDto.VehicleModelImage
        };
    }

    public static PaymentDetailDto ToPaymentDetailDto(this Payment payment, PackageDto packageDto, Organization? organization)
    {
        return new PaymentDetailDto
        {
            Id = payment.Id,
            CreationTime = payment.CreationTime,
            CreatorId = payment.CreatorId,
            LastModificationTime = payment.LastModificationTime,
            LastModifierId = payment.LastModifierId,
            IsDeleted = payment.IsDeleted,
            DeleterId = payment.DeleterId,
            DeletionTime = payment.DeletionTime,
            InvoiceNumber = payment.PaymentNumber ?? $"PKG-{packageDto.Id}",
            TotalHours = packageDto.DurationOfPackage,
            InvoicePrice = payment.Amount,
            InvoiceType = PaymentCategory.Package.ToString(),
            OrganizationArabicName = packageDto.OrganizationArabicName ?? organization?.ArabicName ?? string.Empty,
            OrganizationEnglishName = packageDto.OrganizationEnglishName ?? organization?.EnglishName ?? string.Empty,
            GarageArabicName = string.Empty,
            GarageEnglishName = string.Empty,
            PlateNumber = payment.EnPlateNumber ?? payment.ArPlateNumber ?? string.Empty,
            InvoiceTime = payment.CreationTime,
            EntranceTime = payment.CreationTime,
            CustomerArName = string.Empty,
            CustomerEnName = string.Empty,
            IsPackage = true,
            CustomerId = payment.CustomerId,
            EntryTransactionImage = string.Empty,
            ExitTransactionImage = string.Empty,
            QrCodeInvoice = payment.QrCodeInvoice ?? string.Empty,
            InvoiceId = 0,
            OrganizationId = payment.OrganizationId,
            OrganizationAddress = organization?.Address ?? string.Empty,
            OrganizationTIN = organization?.TaxIdentificationNumber ?? string.Empty,
            GarageTransactionExitId = 0,
            Vat = Math.Round(payment.Amount * 0.15m, 2),
            Discount = 0,
            TicketId = string.Empty,
            TenantId = payment.TenantId ?? packageDto.TenantId,
            LicenseTypeImage = string.Empty,
            VehicleModelImage = string.Empty
        };
    }

    public static PaymentDetailDto ToPaymentDetailDto(this Payment payment, Organization? organization)
    {
        return new PaymentDetailDto
        {
            Id = payment.Id,
            CreationTime = payment.CreationTime,
            CreatorId = payment.CreatorId,
            LastModificationTime = payment.LastModificationTime,
            LastModifierId = payment.LastModifierId,
            IsDeleted = payment.IsDeleted,
            DeleterId = payment.DeleterId,
            DeletionTime = payment.DeletionTime,
            InvoiceNumber = payment.PaymentNumber ?? string.Empty,
            TotalHours = 0,
            InvoicePrice = payment.Amount,
            InvoiceType = payment.PaymentCategory.ToString(),
            OrganizationArabicName = organization?.ArabicName ?? string.Empty,
            OrganizationEnglishName = organization?.EnglishName ?? string.Empty,
            GarageArabicName = string.Empty,
            GarageEnglishName = string.Empty,
            PlateNumber = payment.EnPlateNumber ?? payment.ArPlateNumber ?? string.Empty,
            InvoiceTime = payment.CreationTime,
            EntranceTime = payment.CreationTime,
            CustomerArName = string.Empty,
            CustomerEnName = string.Empty,
            IsPackage = payment.PaymentCategory == PaymentCategory.Package,
            CustomerId = payment.CustomerId,
            EntryTransactionImage = string.Empty,
            ExitTransactionImage = string.Empty,
            QrCodeInvoice = payment.QrCodeInvoice ?? string.Empty,
            InvoiceId = 0,
            OrganizationId = payment.OrganizationId,
            OrganizationAddress = organization?.Address ?? string.Empty,
            OrganizationTIN = organization?.TaxIdentificationNumber ?? string.Empty,
            GarageTransactionExitId = 0,
            Vat = Math.Round(payment.Amount * 0.15m, 2),
            Discount = 0,
            TicketId = string.Empty,
            TenantId = payment.TenantId,
            LicenseTypeImage = string.Empty,
            VehicleModelImage = string.Empty
        };
    }
}

