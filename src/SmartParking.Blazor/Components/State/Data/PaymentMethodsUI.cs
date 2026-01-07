namespace SmartParking.Blazor.Components.State.Data;

public enum PaymentMethodUI
{
    CreditCard,
    Mada,
    ApplePay
}
public static class PaymentMethodExtensions
{
    public static string ToHyperPayBrand(this PaymentMethodUI method)
    {
        return method switch
        {
            PaymentMethodUI.CreditCard => "VISA MASTER AMEX",
            PaymentMethodUI.Mada => "MADA",
            PaymentMethodUI.ApplePay => "APPLEPAY",
            _ => throw new ArgumentOutOfRangeException(nameof(method))
        };
    }
}