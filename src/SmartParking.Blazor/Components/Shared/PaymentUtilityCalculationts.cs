using SmartParking.Discounts;

namespace SmartParking.Blazor.Components.Shared;

public static class PaymentUtilityCalculationts
{
    public static decimal GetPriceForPayment(decimal InvoicePrice , DiscountDto selectedItem)
    {
        var price = Math.Round((InvoicePrice + (InvoicePrice * 0.15m)), 2);
        if (selectedItem == null)
            return price;
        return price - Math.Round(price * ((selectedItem.DiscountPercent * 1m) / 100), 2);
    }
}
