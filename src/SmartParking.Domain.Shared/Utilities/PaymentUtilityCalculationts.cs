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
        var price = Math.Round((InvoicePrice + (InvoicePrice * 0.15m)), 2);
        if (DiscountPercent == default)
            return price;
        return Math.Round(price - DiscountPercent,2);
    }
}