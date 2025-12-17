using Microsoft.AspNetCore.Components;

namespace SmartParking.Blazor.Components.Pages.CustomInvoiceExtensions
{
    public partial class CustomInvoiceMainPage
    {
        [Parameter] public string WatcherCode { get; set; }

        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public double InvoicePrice { get; set; }
        public double Vat { get; set; }
        public DateTime Checkout { get; set; }
        public DateTime CheckIn { get; set; }
        public string Duration { get; set; }
        public string EntryPicture { get; set; }
        public string ExitPicture { get; set; }
        public string PlateNumber { get; set; }
        public double InvoiceAmount { get; set; }
        public int VatRate { get; set; }
        private string PaymentMethod { get; set; } = "Mada";

        protected override async Task OnInitializedAsync()
        {
            var invoice = await invoiceAppService
                .GetLatestInvoiceForOrganizationByWatcher(WatcherCode);
            InvoiceNumber = invoice.InvoiceId.ToString();
            InvoiceDate = invoice.InvoiceTime!.Value;
            InvoicePrice = (double)invoice.InvoicePrice;
            Vat = (double)invoice.Vat;

            var duration = invoice.InvoiceTime.Value - invoice.EntranceTime.Value;
            Duration = $"{(int)duration.TotalHours} hours {duration.Minutes} minutes";

            CheckIn = invoice.EntranceTime.Value;
            Checkout = invoice.InvoiceTime.Value;

            EntryPicture = invoice.EntryTransactionImage;
            ExitPicture = invoice.ExitTransactionImage;
            PlateNumber = invoice.PlateNumber;

            InvoiceAmount = InvoicePrice - Vat;
            VatRate = (int)(Vat / InvoicePrice * 100);
        }

        private void OnPaymentMethodChanged(string method)
        {
            PaymentMethod = method;
        }

        private async Task HandleCheckout()
        {
      
        }
    }
}
