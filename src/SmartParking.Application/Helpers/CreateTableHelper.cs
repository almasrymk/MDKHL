using SmartParking.CustomerPackages;
using SmartParking.DataTable;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Payments;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Helpers;

public static class CreateTableHelper
{
    public static async Task<TableData> CreateDataTableForGarageTransaction(List<GarageTransactionAnalysis> data)
    {
        var tableData = new TableData()
        {
            Name = "GarageTransactionAnalysis",
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("GarageName"), Name = "GarageName", TypeFilter = TypeFilter.Text , IsSortable = true , PropertyName = "GarageName", IsFilterable=true});
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CameraType"), Name = "CameraType", TypeFilter = TypeFilter.Enum  , IsSortable = true,EnumType = typeof(CameraType),IsFilterable=true, PropertyName = "CameraType" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PlateNumber"), Name = "PlateNumber", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "PlateNumber",IsFilterable=true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AccurateTime"), Name = "AccurateTime", TypeFilter = TypeFilter.Date , IsSortable = true, PropertyName = "AccurateTime",IsFilterable=true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "OrganizationName" , IsFilterable = true });
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ArabicName"), Name = "ArabicName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "ArabicName", IsFilterable = true });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("UserName"), Name = "UserName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "UserName", IsFilterable = true });
        }
      
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("customer category"), Name = "CustomerCategory", TypeFilter = TypeFilter.Enum, IsSortable = true, PropertyName = "GarageTransaction.CustomerCategory", IsFilterable = true, EnumType = typeof(CustomerCategoryStatus) });
        foreach (var item in data)
        {
            var rowDef = new RowDef();

            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), item.GarageName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CameraType"), item.CameraType.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PlateNumber"), item.PlateNumber));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AccurateTime"), item.AccurateTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), item.OrganizationName));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ArabicName"), item.ArabicName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("UserName"), item.UserName));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("customer category"), item.GarageTransaction.CustomerCategory.ToString()));

            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();

        return tableData;
    }
    public static async Task<TableData> CreateDataTableForGarageTransactionCustomer(List<GarageTransactionAnalysis> data)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "TotalCustomresAndTheirTransactions",
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("GarageName"), Name = "GarageName", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "GarageName" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CameraType"), Name = "CameraType", TypeFilter = TypeFilter.Text , IsSortable = true , PropertyName = "CameraType" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PlateNumber"), Name = "PlateNumber", TypeFilter = TypeFilter.Text , IsSortable = true  , PropertyName = "PlateNumber" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AccurateTime"), Name = "AccurateTime", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "AccurateTime" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "OrganizationName" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("customer category"), Name = "CustomerCategory", TypeFilter = TypeFilter.Enum, IsSortable = true, PropertyName = "CustomerCategory", IsFilterable = true, EnumType = typeof(CustomerCategoryStatus) });

        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ArabicName"), Name = "ArabicName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "ArabicName", IsFilterable = true });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("UserName"), Name = "UserName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "UserName" ,IsFilterable = true});
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CustomerId"), Name = "CustomerId", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "CustomerId" });

        foreach (var item in data)
        {
            var rowDef = new RowDef();

            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), item.GarageName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CameraType"), item.CameraType.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PlateNumber"), item.PlateNumber));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AccurateTime"), item.AccurateTime.ToString())); 
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), item.OrganizationName));

            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ArabicName"), item.ArabicName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("UserName"), item.UserName));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CustomerId"), item.CustomerId.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("customer category"), item.GarageTransaction.CustomerCategory.ToString()));
            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();

        return tableData;
    }

    public static TableData CreateDataTableForPaymentCollectedMoneyDailyAndMonthly(List<PaymentAnalysis> data)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "Payment Collected Money Daily And Monthly",
            ShowHeader = true
        };



        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("KeeperName"), Name = "KeeperName", TypeFilter = TypeFilter.Text , IsSortable = true , PropertyName = "KeeperName" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AmountDaily"), Name = "AmountDaily", TypeFilter = TypeFilter.Text ,IsSortable = true ,  PropertyName = "AmountDaily" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AmountMonthly"), Name = "AmountMonthly", TypeFilter = TypeFilter.Text ,  IsSortable = true, PropertyName = "AmountMonthly" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Day"), Name = "Day", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "Day" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Month"), Name = "Month", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "Month" });


        foreach (var item in data)
        {
            var rowDef = new RowDef();

            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("KeeperName"), item.KeeperName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AmountDaily"), item.AmountDaily.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AmountMonthly"), item.AmountMonthly.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Day"), item.Day));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Month"), item.Month));


            tableData.Rows.Add(rowDef);
        }
        return tableData;
    }

    public static TableData CreateDataTableForGarages(List<GarageAnalysis> garages)
    {
        var tableData = new TableData()
        {
            Name = "Garages Data",
            ShowHeader = true
        };

        // Define the columns for the table
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Code"), Name = "Code", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "Code" });
        if(CultureInfo.CurrentCulture.Name == "en")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("NameEn"), Name = "NameEn", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "NameEn" });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("NameAr"), Name = "NameAr", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "NameAr" });

        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("MaxCapacity"), Name = "MaxCapacity", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "MaxCapacity" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CurrnetCapacity"), Name = "CurrnetCapacity", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "CurrnetCapicity" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CurrnetCapacityForCustomers"), Name = "CurrnetCapacityForCustomers", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "CurrnetCapacityForCustomers" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CurrnetCapacityEmployees"), Name = "CurrnetCapacityEmployees", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "CurrnetCapacityEmployees" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CurrnetCapacityForFree"), Name = "CurrnetCapacityForFree", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "CurrnetCapacityForFree" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Status"), Name = "Status", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "Status" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ClosingHours"), Name = "ClosingHours", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "ClosingHours" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("MaxSlotsForCustomers"), Name = "MaxSlotsForCustomers", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "MaxSlotsForCustomers" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("MaxSlotsForEmployess"), Name = "MaxSlotsForEmployess", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "MaxSlotsForEmployess" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("MaxSlotsForFree"), Name = "MaxSlotsForFree", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "MaxSlotsForFree" });

        // Loop through the garages data and add rows to the table
        foreach (var garage in garages)
        {
            var rowDef = new RowDef();

            
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Code"), garage.Garage.Code));
            if (CultureInfo.CurrentCulture.Name == "en")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("NameEn"), garage.Garage.NameEn));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("NameAr"), garage.Garage.NameAr));

            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("MaxCapacity"), garage.Garage.MaxCapacity.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CurrnetCapacity"), garage.Garage.CurrnetCapicity.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CurrnetCapacityForCustomers"), garage.Garage.CurrnetCapacityForCustomers.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CurrnetCapacityEmployees"), garage.Garage.CurrnetCapacityEmployees.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CurrnetCapacityForFree"), garage.Garage.CurrnetCapacityForFree.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Status"), garage.Garage.Status.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ClosingHours"), garage.Garage.ClosingHours.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("MaxSlotsForCustomers"), garage.Garage.MaxSlotsForCustomers.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("MaxSlotsForEmployess"), garage.Garage.MaxSlotsForEmployess.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("MaxSlotsForFree"), garage.Garage.MaxSlotsForFree.ToString()));

            tableData.Rows.Add(rowDef);
        }

        return tableData;
    }
    public static async Task<TableData> CreateDataTableForCustomerPackages(List<CustomerPackageDto> customerPackages)
    {
        var tableData = new TableData()
        {
            Name = "Customer Packages Data",
            ShowHeader = true
        };
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PackageName"), Name = "PackageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "PackageEnglishName", IsFilterable=true});
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CustomerName"), Name = "CustomerName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "Customer.IdentityUser.Name", IsFilterable = true });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PackageName"), Name = "PackageArabicName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "PackageArabicName", IsFilterable = true });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CustomerName"), Name = "CustomerName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "Customer.ArabicName", IsFilterable = true });
        }

        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PriceOfPackage"), Name = "PriceOfPackage", TypeFilter = TypeFilter.Number  , IsSortable = true, PropertyName = "PriceOfPackage", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PackageDuration"), Name = "PackageDuration", TypeFilter = TypeFilter.Number  , IsSortable = true , PropertyName = "DurationOfPackage", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ActivationDate"), Name = "ActivationDateOfPackage", TypeFilter = TypeFilter.Date , IsSortable = true, PropertyName = "ActivationDateOfPackage", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ExpiryDate"), Name = "ExpiryDate", TypeFilter = TypeFilter.Date , IsSortable = true, PropertyName = "ExpiryDate", IsFilterable = true });

        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("customer category"), Name = "CustomerCategory", TypeFilter = TypeFilter.Enum , IsSortable = true, PropertyName = "CustomerCategory", IsFilterable = true ,EnumType = typeof(CustomerCategoryStatus) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Status"), Name = "Status", TypeFilter = TypeFilter.Enum, IsSortable = true, PropertyName = "CustomerPackageStatus", IsFilterable = true , EnumType = typeof(Status) });
        // Loop through the customer packages data and add rows to the table
        foreach (var customerPackage in customerPackages)
        {
            var rowDef = new RowDef();
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PackageName"), customerPackage.PackageArabicName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CustomerName"), customerPackage.Customer.ArabicName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PackageName"), customerPackage.PackageEnglishName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CustomerName"), customerPackage.Customer.FullName));
            }

            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PriceOfPackage"), customerPackage.PriceOfPackage?.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PackageDuration"), customerPackage.DurationOfPackage?.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ActivationDate"), customerPackage.ActivationDateOfPackage?.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ExpiryDate"), customerPackage.ExpiryDate.ToString()));
           
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("customer category"), customerPackage.CustomerCategory.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Status"), customerPackage.CustomerPackageStatus.ToString()));
            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();
        return tableData;
        
    }

    public static TableData CreateDataTableForPayments(List<PaymentDto> payments)
    {
        var tableData = new TableData()
        {
            Name = "Payments Data",
            ShowHeader = true
        };

        // Define the columns for the table
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Payment ID", Name = "Id", TypeFilter = TypeFilter.Text , IsSortable = true , PropertyName = "Id" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Customer ID", Name = "CustomerId", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "CustomerId" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Checkout ID", Name = "CheckoutId", TypeFilter = TypeFilter.Text  , IsSortable = true , PropertyName = "CheckoutId" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Payment Status", Name = "PaymentStatus", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "PaymentStatus" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Invoice ID", Name = "InvoiceId", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "InvoiceId" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Package ID", Name = "PackageId", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "PackageId" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Payment Category", Name = "PaymentCategory", TypeFilter = TypeFilter.Text , IsSortable = true , PropertyName = "PaymentCategory" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Organization ID", Name = "OrganizatioId", TypeFilter = TypeFilter.Text  , IsSortable = true, PropertyName = "OrganizatioId" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "QR Code Invoice", Name = "QrCodeInvoice", TypeFilter = TypeFilter.Text  , IsSortable = true , PropertyName = "QrCodeInvoice" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Payment Method", Name = "PaymentMethod", TypeFilter = TypeFilter.Text  , IsSortable = true, PropertyName = "PaymentMethod" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Amount", Name = "Amount", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "Amount" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Currency", Name = "Currency", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "Currency" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Creation Time", Name = "CreationTime", TypeFilter = TypeFilter.Text  , IsSortable = true , PropertyName = "CreationTime" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Payment Number", Name = "PaymentNumber", TypeFilter = TypeFilter.Text , IsSortable  = true, PropertyName = "PaymentNumber" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = "Plate Number", Name = "PlateNumber", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "PlateNumber" });

        // Loop through the payments data and add rows to the table
        foreach (var payment in payments)
        {
            var rowDef = new RowDef();

            rowDef.Cells.Add(new KeyValuePair<string, string>("Payment ID", payment.Id.ToString() ?? "N/A"));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Customer ID",payment.CustomerId.ToString() ?? "N/A"));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Checkout ID", payment.CheckoutId ?? "N/A"));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Payment Status", payment.PaymentStatus.ToString() ?? "N/A"));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Invoice ID", payment.InvoiceId?.ToString() ?? "N/A"));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Package ID", payment.PackageId?.ToString() ?? "N/A"));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Payment Category", payment.PaymentCategory.ToString() ?? "N/A"));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Organization ID", payment.OrganizatioId.ToString() ?? "N/A"));
            rowDef.Cells.Add(new KeyValuePair<string, string>("QR Code Invoice", payment.QrCodeInvoice ?? "N/A"));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Payment Method", payment.PaymentMethod ?? "N/A"));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Amount", payment.Amount.ToString("F2") ?? "N/A"));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Currency", payment.Currency ?? "N/A"));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Creation Time", payment.CreationTime ?? "N/A"));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Payment Number", payment.PaymentNumber ?? "N/A"));
            rowDef.Cells.Add(new KeyValuePair<string, string>("Plate Number", payment.PlateNumber ?? "N/A"));

            tableData.Rows.Add(rowDef);
        }

        return tableData;
    }


}
