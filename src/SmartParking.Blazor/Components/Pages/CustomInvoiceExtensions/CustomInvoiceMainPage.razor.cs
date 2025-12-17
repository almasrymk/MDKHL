using Microsoft.AspNetCore.Components;

namespace SmartParking.Blazor.Components.Pages.CustomInvoiceExtensions
{
    public partial class CustomInvoiceMainPage
    {
        [Parameter] public string WatcherCode { get; set; }
        [Parameter] public string InvoiceNumber { get; set; }
        [Parameter] public DateTime InvoiceDate { get; set; }
        [Parameter] public double InvoicePrice { get; set; }
        [Parameter] public double Vat { get; set; }
        [Parameter] public DateTime Checkout { get; set; }
        [Parameter] public DateTime CheckIn { get; set; }
        [Parameter] public string Duration { get; set; }
        [Parameter] public string EntryPicture { get; set; }
        [Parameter] public string ExitPicture { get; set; }
        [Parameter] public string PlateNumber { get; set; }
        [Parameter] public double InvoiceAmount { get; set; }
        [Parameter] public int VatRate { get; set; }
        [Parameter] public string PaymentMethod { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var invoice = await invoiceAppService.GetLatestInvoiceForOrganizationByWatcher(WatcherCode);
            InvoiceNumber = invoice.InvoiceId.ToString();
            InvoiceDate = (DateTime)invoice.InvoiceTime;
            InvoicePrice = (double)invoice.InvoicePrice;
            Vat = (double)invoice.Vat;
            var duration = invoice.InvoiceTime.Value - invoice.EntranceTime.Value;
            Duration = $"{(int)duration.TotalHours} hours {duration.Minutes} minutes";
            CheckIn = (DateTime)invoice.EntranceTime;
            Checkout = (DateTime)invoice.InvoiceTime;
            EntryPicture = invoice.EntryTransactionImage;
            ExitPicture = invoice.ExitTransactionImage;
            PlateNumber = invoice.PlateNumber;
            InvoiceAmount = (double)(invoice.InvoicePrice - invoice.Vat);
            var rate = (Vat / InvoicePrice * 100);
            VatRate = (int)rate;
            //PaymentMethod = SelectedPaymentMethod
        }



    }
}
