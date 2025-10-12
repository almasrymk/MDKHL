using SmartParking.Localization;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PDF;
public class InvoiceLocalizationAppService :SmartParkingAppService, ILocalizationFactoryAppService
{

    public InvoiceLocalizationData Create(InvoiceLocalizationCreateDto dto)
    {
        var culture = CultureInfo.CurrentCulture.Name;
        bool isArabic = culture == "ar";

        return new InvoiceLocalizationData(
            CurrentPath: Path.Combine(Directory.GetCurrentDirectory(), $"{PathHelper.WwwRoot}{dto.OrganizationPicture}"),
            QRCodePath: Path.Combine(Directory.GetCurrentDirectory(), $"{PathHelper.WwwRoot}/{dto.ImagePath}"),
            InvoiceNumber: LocalizationHelper.GetLocalizedString(dto.InvoiceKey),
            TicketId: LocalizationHelper.GetLocalizedString(dto.TicketIdKey),
            SimplifiedTaxInvoice: LocalizationHelper.GetLocalizedString(dto.SimplifiedTaxInvoiceKey),
            HeaderVatReceiptPayment: LocalizationHelper.GetLocalizedString(dto.HeaderVatReceiptPaymentKey),
            Vats: isArabic ? "الضريبة (15%)" : "VAT",
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
            TotalPreVAT: isArabic ? "االمجموع" : "Total Before VAT",
            TotalIncludeVAT: isArabic ? "الإجمالي " : "Total After VAT",
            Discount: isArabic ? "الخصم" : "Discount",
            After: LocalizationHelper.GetLocalizedString(dto.AfterKey),
            InvoiceNumberView: LocalizationHelper.GetLocalizedString(dto.InvoiceNumberViewKey),
            Price: Math.Round(dto.Price + (dto.Price * 0.15m), 2),
            PaymentPrice: dto.PaymentAmount
        );
    }
    public PackageLocalizationData CreatePackageLocalization(PackageLocalizationCreateDto dto)
    {
        var culture = CultureInfo.CurrentCulture.Name;
        bool isArabic = culture == "ar";

        return new PackageLocalizationData(
            CurrentPath: Directory.GetCurrentDirectory(),
            QRCodePath: dto.QRCodePath,
            PackageNumber: isArabic ? "رقم الباقة" : "Package Number",
            TicketId: isArabic ? "رقم التذكرة" : "Ticket ID",
            SimplifiedTaxInvoice: isArabic ? "فاتورة ضريبية مبسطة" : "Simplified Tax Invoice",
            HeaderVatReceiptPayment: isArabic ? "سداد ضريبة القيمة المضافة" : "VAT Payment Receipt",
            Vats: isArabic ? "الضريبة (15%)" : "VAT",
            Sar: isArabic ? "ريال سعودي" : "SAR",
            CashierNo: isArabic ? "اسم الكاشير" : "Cashier Name",
            OrganizationName: isArabic ? "اسم المنشأة" : "Organization Name",
            OrganizationAddress: isArabic ? "عنوان المنشأة" : "Organization Address",
            PlateNumber: isArabic ? "رقم اللوحة" : "Plate Number",
            SubscriptionDate: isArabic ? "وقت الاشتراك" : "Subscription Time",
            ExpirationDate: isArabic ? "وقت الانتهاء" : "Expiration Time",
            Subscription: isArabic ? "الاشتراك" : "Subscription",
            Duration: isArabic ? "المدة" : "Duration",
            Days: isArabic ? "أيام" : "Days",
            TotalPreVAT: isArabic ? "االمجموع" : "Total Before VAT",
            TotalIncludeVAT: isArabic ? "الإجمالي " : "Total After VAT",
            Discount: isArabic ? "الخصم" : "Discount",
            After: isArabic ? "بعد" : "After",
            PackageNumberView: isArabic ? "باقة" : "Package",
            PaymentNumber : isArabic ? "رقم الفاتورة" : "Payment Number",
            Price: dto.Price,
            PaymentPrice: dto.PaymentAmount
        );
    }
}