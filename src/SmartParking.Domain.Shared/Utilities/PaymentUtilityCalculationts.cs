using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Utilities;

public static class PaymentUtilityCalculationts
{
    public static decimal GetPriceForPayment(decimal InvoicePrice, decimal DiscountPercent)
    {
        var vat = Math.Round(InvoicePrice * 0.15m, 2);
        if (DiscountPercent == default)
            return Math.Round(InvoicePrice+vat,2);
        var _newPrice = InvoicePrice - DiscountPercent;
        return Math.Round((_newPrice) *1.15m,2);
    }
}