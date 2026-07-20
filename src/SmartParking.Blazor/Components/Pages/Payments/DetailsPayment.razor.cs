using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Pages.Invoices;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Blazor.Components.PosTerminal;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Enums.PosTerminal;
using SmartParking.Organizations;
using SmartParking.Payments;
using SmartParking.Payments.PaymentRelations;

namespace SmartParking.Blazor.Components.Pages.Payments;

public partial class DetailsPayment
{
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IOrganizationsAppService OrganizationAppService { get; set; } = default!;
    [Inject] private Volo.Abp.Domain.Repositories.IRepository<PosTerminalTransactionDetail, Guid> PosDetailRepository { get; set; } = default!;
    [Parameter] public string Id { get; set; }  // Capture string ID
    private PaymentGetResponseDto Payment {  get; set; } = new PaymentGetResponseDto();
    private string CreationTime { get; set; }
    private string Amount { get; set; }
    public string Vat {  get; set; }
    public string Discount {  get; set; }
    public string Total { get; set; }
    public string CashierName { get; set; }
    private string LocalizedPaymentCategory{ get; set; }
    private string LocalizedPaymentMethod{ get; set; }
    private string LocalizedPaymentStatus{ get; set; }
    private string LocalizedPaymentCurrency{ get; set; }
    private string LocalizedPaymentInvoiceType{ get; set; }
    private PaymentDetailsDto paymentRelation { get; set; } = new PaymentDetailsDto();


    private async Task FetchPayment()
    {
        Payment = await paymentWebAppService.GetMdkhlAsync(Guid.Parse(Id));
        //PaymentDetailsDto paymentRelation = new();
        if (Payment.customerPackageDto == null && Payment.Invoice==null)
             paymentRelation = await PaymentRelationAppService.GetByPaymentIdAsync(Payment.Id);
        Payment.customerPackageDto ??= new CustomerPackageDto();
        var dto = Payment.customerPackageDto;

        CreationTime = Payment?.CreationTime.ToString() ?? "";
        if (Payment.PaymentCategory == PaymentCategory.Package.ToString())
        {

            Total = Payment.Amount == 0 ? "0.0" : Payment.Amount.ToString();
            Amount = dto.PriceOfPackage?.ToString() ?? paymentRelation.Price.ToString();
            if (dto.Vat.ToString() is null || dto.Vat == 0)
                Vat = paymentRelation.Vat.ToString();
            else
                Vat = dto.Vat.ToString();
            if (dto.Discount.ToString() is null || dto.Discount == 0)
                Discount = paymentRelation.Discount.ToString();
            else
                Discount = dto.Discount.ToString();
            Payment?.customerPackageDto?.PackageArabicName = 
                Payment?.customerPackageDto?.PackageArabicName is not null ? Payment?.customerPackageDto?.PackageArabicName : paymentRelation.ArabicName;
            Payment?.customerPackageDto?.PackageEnglishName = 
                Payment?.customerPackageDto?.PackageEnglishName is not null ? Payment?.customerPackageDto?.PackageEnglishName : paymentRelation.EnglishName;

        }
        else
        {
            Total = Payment.Amount == 0 ? "0.0" : Payment.Amount.ToString();
            Amount = Payment.Invoice?.InvoicePrice.ToString();
            Vat =
                Payment.Invoice?.InvoicePrice is decimal price &&
                Payment.Invoice?.Discount is decimal discount &&
                (price - discount) != 0
                    ? Math.Round( (price - discount)*0.15m, 2).ToString()
                    : "0.0"; 
            Discount = Payment.Invoice?.Discount.ToString();
        }

        LocalizedPaymentCategory = Payment?.PaymentCategory != null ? L[Payment.PaymentCategory] : "";
        LocalizedPaymentMethod = Payment?.PaymentMethod != null ? L[Payment.PaymentMethod] : "";
        LocalizedPaymentStatus = Payment?.PaymentStatus != null ? L[Payment.PaymentStatus] : "";
        LocalizedPaymentCurrency = Payment?.Currency != null ? L[Payment.Currency] : "";

        LocalizedPaymentInvoiceType = Payment?.Invoice?.InvoiceType != null ? L[Payment.Invoice.InvoiceType] : "";


    }


    protected override async Task OnInitializedAsync()
    {
       await FetchPayment();
        
    }
    private async Task<Task<IDialogReference>> ViewInvoiceDetails()
    {
        Dictionary<string, Object> data = new();
        if (Payment.Invoice is not null)
        {
            
            data.Add("Cashier", Payment.CashierName);
            data.Add("OrganizationArName", Payment.Organization.ArabicName);
            data.Add("OrganizationAddress", Payment.Organization.Address);
            data.Add("PlateNumber", Payment.Invoice.PlateNumber);
            data.Add("EntranceTime", Payment.Invoice.EntranceTime.ToString());
            data.Add("ExitTime", Payment.Invoice.InvoiceTime.ToString());
            data.Add("Subscription", "Invoice");
            data.Add("Duration", Payment.Invoice.TotalHours.ToString());
            
        }

        else if(Payment.customerPackageDto is not null && Payment.customerPackageDto.CustomerId != Guid.Empty)
        {
            data.Add("Cashier", Payment.CashierName);
            data.Add("OrganizationArName", Payment.Organization.ArabicName);
            data.Add("OrganizationAddress", Payment.Organization.Address);
            data.Add("PlateNumber", Payment.EnPlateNumber);
            data.Add("EntranceTime", Payment.customerPackageDto.ActivationDateOfPackage.ToString());
            data.Add("ExitTime", Payment.customerPackageDto.ExpiryDate.ToString());
            data.Add("Subscription", "Package");
            data.Add("Duration", Payment.customerPackageDto.DurationOfPackage.ToString());
        }
        else
        {
            var customerUnitPackage = await CustomerUnitPackageAppService.GetAsync(paymentRelation.PayableId);

            data.Add("Cashier", Payment.CashierName);
            data.Add("OrganizationArName", Payment.Organization.ArabicName);
            data.Add("OrganizationAddress", Payment.Organization.Address);
            data.Add("UnitCode", customerUnitPackage.UnitCode);
            data.Add("EntranceTime", customerUnitPackage.ActivationDate.ToString());
            data.Add("ExitTime", customerUnitPackage.DectivationDate.ToString());
            data.Add("Subscription", "UnitPackage");
            data.Add("Duration", customerUnitPackage.DurationOfPackage.ToString());
        }

        //var parameters = new DialogParameters
        //{
        //    ["JsonString"] = System.Text.Json.JsonSerializer.Serialize(data)
        //};

        //return DialogService.ShowAsync<DynamicPrintDialog>("", parameters);

        var parameters = new DialogParameters
        {

            ["Invoice"] = Payment.Invoice is not null ? Payment.Invoice : null,
            ["PaymentId"] = Payment.Id,
            ["Package"] = Payment.customerPackageDto is not null ? Payment.customerPackageDto : null,
            ["CommercialRegisteration"] = Payment.Organization.TaxNumber,
            ["OrganizationAddress"] = Payment.Organization.Address,
            ["Code"] = Payment.PaymentNumber ?? "",
            ["PaymentPrice"] = Payment.Amount,
            ["CashierName"] = Payment.CashierName,
            ["OrganizationArabicName"] = Payment.Organization.ArabicName,
            ["OrganizationEnglishName"] = Payment.Organization.EnglishName,
            ["OrganizationPicture"] = Payment.Organization.OrganizationPicture,
            ["QRCode"] = Payment.QrCodeInvoice,
            ["Total"] = Total,
            ["Amount"] = Amount,
            ["Vat"] = Vat,
            ["Discount"] = Discount,
            ["PaymentDetails"] = paymentRelation,
            ["JsonString"] = System.Text.Json.JsonSerializer.Serialize(data)
        };
        return DialogService.ShowAsync<InvoiceDetailsViewDialog>("", parameters);


    }
    private void Back()
    {
        Navigation.NavigateTo("/payments");
    }

    private async Task ResendZatca()
    {
        var res = await paymentAppService.ResendZatca(Payment.Id);
        if (res)
            Navigation.NavigateTo("/payments");
        else
        {
            var uri = Navigation.Uri;
            Navigation.NavigateTo(uri, forceLoad: true);
        }
    }

    private async Task OpenAdminPosDialog(string operation)
    {
        try
        {
            var organization = await OrganizationAppService.GetOrganizationForCurrentTenatAsync();

            var details = await PosDetailRepository.GetListAsync(x => x.PaymentId == Payment.Id);
            var purchaseDetail = details.FirstOrDefault(x => x.TransactionType == PosTransactionType.Purchase) ?? details.FirstOrDefault();

            var parameters = new DialogParameters<PosAdminTransactionDialog>
            {
                { x => x.Operation, operation },
                { x => x.PaymentId, Payment.Id },
                { x => x.Amount, Payment.Amount },
                { x => x.Rrn, purchaseDetail?.RetrievalReferenceNumber },
                { x => x.Stan, purchaseDetail?.TransactionStan },
                { x => x.TxnDate, Payment.CreationTime },
                { x => x.OrgSettings, organization?.OrganizationSettings }
            };

            var options = new DialogOptions { CloseButton = false, MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await DialogService.ShowAsync<PosAdminTransactionDialog>($"POS {operation}", parameters, options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled && result.Data is PosTerminalStateSnapshot snapshot)
            {
                if (snapshot.Outcome == SamaResponseOutcome.Approved)
                {
                    SnackBar.ShowSnackbar(Snackbar, $"Transaction {operation} Approved", MudBlazor.Severity.Success);
                    await FetchPayment();
                }
                else
                {
                    SnackBar.ShowSnackbar(Snackbar, snapshot.SamaDescription ?? $"{operation} Declined", MudBlazor.Severity.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, ex.Message, MudBlazor.Severity.Error);
        }
    }

    private async Task OpenPosReconciliationDialog()
    {
        try
        {
            var organization = await OrganizationAppService.GetOrganizationForCurrentTenatAsync();

            var parameters = new DialogParameters<PosReconciliationDialog>
            {
                { x => x.OrgSettings, organization?.OrganizationSettings }
            };

            var options = new DialogOptions { CloseButton = false, MaxWidth = MaxWidth.Small, FullWidth = true };
            await DialogService.ShowAsync<PosReconciliationDialog>("POS Reconciliation", parameters, options);
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, ex.Message, MudBlazor.Severity.Error);
        }
    }
}