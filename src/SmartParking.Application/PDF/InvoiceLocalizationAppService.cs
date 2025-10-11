using SmartParking.Localization;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PDF;
public class InvoiceLocalizationAppService :SmartParkingAppService, ILocalizationFactoryAppService
{

    public InvoiceLocalizationData Create(InvoiceLocalizationCreateDto dto)
    {
        return new InvoiceLocalizationData(
            CurrentPath: Path.Combine(Directory.GetCurrentDirectory(), $"{PathHelper.WwwRoot}{dto.OrganizationPicture}"),
            QRCodePath: Path.Combine(Directory.GetCurrentDirectory(), $"{PathHelper.WwwRoot}/{dto.ImagePath}"),
            InvoiceNumber: LocalizationHelper.GetLocalizedString(dto.InvoiceKey),
            TicketId: LocalizationHelper.GetLocalizedString(dto.TicketIdKey),
            SimplifiedTaxInvoice: LocalizationHelper.GetLocalizedString(dto.SimplifiedTaxInvoiceKey),
            HeaderVatReceiptPayment: LocalizationHelper.GetLocalizedString(dto.HeaderVatReceiptPaymentKey),
            Vats: LocalizationHelper.GetLocalizedString(dto.VatKey),
            Sar: LocalizationHelper.GetLocalizedString(dto.SarKey),
            CashierNo: LocalizationHelper.GetLocalizedString(dto.CashierNameKey),
            OrganizationName: LocalizationHelper.GetLocalizedString(dto.OrganizationNameKey),
            OrganizationAddress: LocalizationHelper.GetLocalizedString(dto.OrganizationAddressKey),
            PlateNumber: LocalizationHelper.GetLocalizedString(dto.PlateNumberKey),
            EntranceTime: LocalizationHelper.GetLocalizedString(dto.EntranceTimeKey),
            ExitTime: LocalizationHelper.GetLocalizedString(dto.ExitTimeKey),
            Subscription: LocalizationHelper.GetLocalizedString(dto.SubscriptionKey),
            Hour: LocalizationHelper.GetLocalizedString(dto.HourKey),
            Duration: LocalizationHelper.GetLocalizedString(dto.DurationKey),
            Hours: LocalizationHelper.GetLocalizedString(dto.HoursKey),
            TotalPreVAT: LocalizationHelper.GetLocalizedString(dto.TotalPreVatKey),
            TotalIncludeVAT: LocalizationHelper.GetLocalizedString(dto.TotalIncludeVatKey),
            Discount: LocalizationHelper.GetLocalizedString(dto.DiscountKey),
            After: LocalizationHelper.GetLocalizedString(dto.AfterKey),
            InvoiceNumberView: LocalizationHelper.GetLocalizedString(dto.InvoiceNumberViewKey),
            Price: Math.Round(dto.InvoicePrice + (dto.InvoicePrice * 0.15m), 2),
            PaymentPrice: dto.PaymentAmount
        );
    }
}