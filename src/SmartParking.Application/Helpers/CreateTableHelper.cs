using Microsoft.CodeAnalysis.Host.Mef;
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
using Volo.Abp.MultiTenancy;

namespace SmartParking.Helpers;

public static class CreateTableHelper 
{
    public static async Task<TableData> CreateDataTableForGarageTransaction(List<GarageTransactionAnalysis> data , Guid? currentTenant)
    {
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("GarageTransactionAnalysis"),
            ShowHeader = true
        };
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Name"), Name = "Name", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "ArabicName", IsFilterable = true });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Name"), Name = "Name", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "UserName", IsFilterable = true });
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PlateNumber"), Name = "PlateNumber", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "PlateNumber",IsFilterable=true });
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("GarageName"), Name = "GarageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "GarageName", IsFilterable = true });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("GarageName"), Name = "GarageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "GarageNameAr", IsFilterable = true });

        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CameraType"), Name = "CameraType", TypeFilter = TypeFilter.Enum  , IsSortable = true,EnumType = typeof(CameraType),IsFilterable=true, PropertyName = "CameraType" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AccurateTime"), Name = "AccurateTime", TypeFilter = TypeFilter.Date , IsSortable = true, PropertyName = "AccurateTime",IsFilterable=true });
        if(currentTenant == null)
        {
            if(CultureInfo.CurrentCulture.Name == "en")
            {

            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "OrganizationName" , IsFilterable = true });
            }
            else
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "OrganizationNameAr", IsFilterable = true });

            }
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("customer category"), Name = "CustomerCategory", TypeFilter = TypeFilter.Enum, IsSortable = true, PropertyName = "GarageTransaction.CustomerCategory", IsFilterable = true, EnumType = typeof(CustomerCategoryStatus) });
        foreach (var item in data)
        {
            var rowDef = new RowDef();
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Name"), item.ArabicName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Name"), item.UserName));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PlateNumber"), item.PlateNumber));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), item.GarageNameAr));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), item.GarageName));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CameraType"),LocalizationHelper.GetLocalizedString(item.CameraType.ToString())));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AccurateTime"), item.AccurateTime.Value.ToString()));
            if (currentTenant == null)
            {
                if (CultureInfo.CurrentCulture.Name == "ar")
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), item.OrganizationNameAr));
                }
                else
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), item.OrganizationName));
                }
            }
            
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Name"), item.ArabicName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Name"), item.UserName) );
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("customer category"), LocalizationHelper.GetLocalizedString(item.GarageTransaction.CustomerCategory.ToString())));

            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();

        return tableData;
    }
    public static async Task<TableData> CreateDataTableForGarageTransactionCustomer(List<GarageTransactionAnalysis> data, Guid? currentTenant)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("TotalCustomersAndTheirTransactions"),
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("GarageName"), Name = "GarageName", TypeFilter = TypeFilter.Text , IsSortable = true, PropertyName = "GarageName" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CameraType"), Name = "CameraType", TypeFilter = TypeFilter.Text , IsSortable = true , PropertyName = "CameraType" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PlateNumber"), Name = "PlateNumber", TypeFilter = TypeFilter.Text , IsSortable = true  , PropertyName = "PlateNumber" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AccurateTime"), Name = "AccurateTime", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "AccurateTime" });
       if(currentTenant == null)
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "OrganizationName" });
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("customer category"), Name = "CustomerCategory", TypeFilter = TypeFilter.Enum, IsSortable = true, PropertyName = "CustomerCategory", IsFilterable = true, EnumType = typeof(CustomerCategoryStatus) });

        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ArabicName"), Name = "ArabicName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "ArabicName", IsFilterable = true });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("UserName"), Name = "UserName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "UserName" ,IsFilterable = true});
        }

        foreach (var item in data)
        {
            var rowDef = new RowDef();
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), item.GarageNameAr));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), item.GarageName));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CameraType"), LocalizationHelper.GetLocalizedString(item.CameraType.ToString())));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PlateNumber"), item.PlateNumber));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AccurateTime"), item.AccurateTime.ToString()));
            if (currentTenant == null)
            {
                if (CultureInfo.CurrentCulture.Name == "ar")
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), item.OrganizationNameAr));
                }
                else
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), item.OrganizationName));
                }
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("customer category"),LocalizationHelper.GetLocalizedString(item.GarageTransaction.CustomerCategory.ToString())));

            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ArabicName"), item.ArabicName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("UserName"), item.UserName));
            }
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
            Name =LocalizationHelper.GetLocalizedString("PaymentReports"),
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

    public static async Task<TableData> CreateDataTableForGarages(List<GarageAnalysis> garages)
    {
        var tableData = new TableData()
        {
            Name =LocalizationHelper.GetLocalizedString("GaragesReports"),
            ShowHeader = true
        };

        if(CultureInfo.CurrentCulture.Name == "en")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("EnglishName"), Name = "NameEn", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "Garage.NameEn",IsFilterable=true });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ArabicName"), Name = "NameAr", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "Garage.NameAr" ,IsFilterable=true});

        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CurrnetCapacity"), Name = "CurrnetCapacity", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "Garage.CurrnetCapicity" , IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CurrnetCapacityForCustomers"), Name = "CurrnetCapacityForCustomers", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "Garage.CurrnetCapacityForCustomers", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CurrnetCapacityEmployees"), Name = "CurrnetCapacityEmployees", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "Garage.CurrnetCapacityEmployees", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CurrnetCapacityForFree"), Name = "CurrnetCapacityForFree", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "Garage.CurrnetCapacityForFree", IsFilterable = true });
       
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Status"), Name = "Status", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "Status" ,IsFilterable=true,EnumType=typeof(Status) });
        // Loop through the garages data and add rows to the table
        foreach (var garage in garages)
        {
            var rowDef = new RowDef();

          
            if (CultureInfo.CurrentCulture.Name == "en")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("EnglishName"), garage.Garage.NameEn));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ArabicName"), garage.Garage.NameAr));

            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CurrnetCapacity"), garage.Garage.CurrnetCapicity.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CurrnetCapacityForCustomers"), garage.Garage.CurrnetCapacityForCustomers.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CurrnetCapacityEmployees"), garage.Garage.CurrnetCapacityEmployees.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CurrnetCapacityForFree"), garage.Garage.CurrnetCapacityForFree.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Status"), LocalizationHelper.GetLocalizedString(garage.Garage.Status.ToString())));

            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();
        return tableData;
    }
    public static async Task<TableData> CreateDataTableForCustomerPackages(List<CustomerPackageDto> customerPackages)
    {
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("PackagesReports"),
            ShowHeader = true
        };
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PackageName"), Name = "PackageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "PackageArabicName", IsFilterable = true });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CustomerName"), Name = "CustomerName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "Customer.ArabicName", IsFilterable = true });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PackageName"), Name = "PackageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "PackageEnglishName", IsFilterable = true });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CustomerName"), Name = "CustomerName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "Customer.IdentityUser.Name", IsFilterable = true });

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
           
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("customer category"), LocalizationHelper.GetLocalizedString(customerPackage.CustomerCategory.ToString())));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Status"), LocalizationHelper.GetLocalizedString(customerPackage.CustomerPackageStatus.ToString())));
            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();
        return tableData;
        
    }

    public static async Task<TableData> CreateDataTableForKepperPayments(List<PaymentAnalysis> payments)
    {
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("PaymentReports"),
            ShowHeader = true
        };
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Code"), Name = "PaymentNumber", IsFilterable = true, PropertyName = "PaymentNumber", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PlateNumber"), Name = "PlateNumber", IsFilterable = false, PropertyName = "Invoice.PlateNumber", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PaymentCategory"), Name = "PaymentCategory", IsFilterable = true, PropertyName = "PaymentCategory", IsSortable = true, TypeFilter = TypeFilter.Enum,EnumType=typeof(PaymentCategory) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("InvoiceId"), Name = "InvoiceId", IsFilterable = false, PropertyName = "Invoice.Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PaymentMethod"), Name = "PaymentMethod", IsFilterable = true, PropertyName = "PaymentMethod", IsSortable = true, TypeFilter = TypeFilter.Enum,EnumType = typeof(PaymentMethod) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Amount"), Name = "Amount", IsFilterable = true, PropertyName = "Amount", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Currency"), Name = "Currency", IsFilterable = true, PropertyName = "Currency", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CustomerName"), Name = "CustomerName", IsFilterable = true, PropertyName = "Customers.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Date"), Name = "Date", IsFilterable = true, PropertyName = "CreationTime", IsSortable = true, TypeFilter = TypeFilter.Date });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PaymentStatus"), Name = "PaymentStatus", IsFilterable = true, PropertyName = "PaymentStatus", IsSortable = true, TypeFilter = TypeFilter.Enum ,EnumType = typeof(PaymentStatus) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("KeeperName"), Name = "KeeperName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "KeeperName" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AmountDaily"), Name = "AmountDaily", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "AmountDaily" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AmountMonthly"), Name = "AmountMonthly", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "AmountMonthly" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Day"), Name = "Day", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "Day" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Month"), Name = "Month", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "Month" });

        foreach (var payment in payments)
        {
            var rowDef = new RowDef();

            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Code"), payment.Payment.PaymentNumber?.ToString()));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PlateNumber"), payment.Payment.InvoiceId.HasValue ? payment.Payment.EnPlateNumber : LocalizationHelper.GetLocalizedString("Pacakge")));

            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PlateNumber"), payment.Payment.InvoiceId.HasValue ? payment.Payment.ArPlateNumber : LocalizationHelper.GetLocalizedString("Pacakge")));

            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PaymentCategory"), LocalizationHelper.GetLocalizedString(payment.Payment.PaymentCategory.ToString())));
            rowDef.Cells.Add(new KeyValuePair<string, string>(
                LocalizationHelper.GetLocalizedString("InvoiceId"),
                payment.Payment.PaymentCategory == Enums.PaymentCategory.PaymentParking
                ? payment.Payment.InvoiceId.ToString()
                : payment.Payment.PackageId.ToString()
            ));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PaymentMethod"), LocalizationHelper.GetLocalizedString(payment.Payment.PaymentMethod.ToString())));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Amount"), payment.Payment.Amount.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Currency"), LocalizationHelper.GetLocalizedString(payment.Payment.Currency.ToString())));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CustomerName"), payment.Payment.Customers?.ArabicName ?? LocalizationHelper.GetLocalizedString("NotCustomer")));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CustomerName"), payment.Payment.Customers?.IdentityUser.Name ?? LocalizationHelper.GetLocalizedString("NotCustomer")));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Date"), payment.Payment.CreationTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PaymentStatus"), LocalizationHelper.GetLocalizedString(payment.Payment.PaymentStatus.ToString())));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("KeeperName"), payment.KeeperName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AmountDaily"), payment.AmountDaily.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AmountMonthly"), payment.AmountMonthly.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Day"), payment.Day));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Month"), payment.Month));

            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();
        return tableData;
    }
    public static async Task<TableData> CreateDataTableForPayments(List<PaymentDto> payments)
    {
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("PaymentReports"),
            ShowHeader = true
        };
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Code"), Name = "PaymentNumber", IsFilterable = true, PropertyName = "PaymentNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
        if (CultureInfo.CurrentCulture.Name == "ar")
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CustomerName"), Name = "CustomerName", IsFilterable = true, PropertyName = "Customers.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        else
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CustomerName"), Name = "CustomerName", IsFilterable = true, PropertyName = "Customers.IdentityUser.Name", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PlateNumber"), Name = "PlateNumber", IsFilterable = true, PropertyName = "Invoice.PlateNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PaymentCategory"), Name = "PaymentCategory", IsFilterable = true, PropertyName = "PaymentCategory", IsSortable = true, TypeFilter = TypeFilter.Enum, EnumType = typeof(PaymentCategory) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("InvoiceId"), Name = "InvoiceId", IsFilterable = true, PropertyName = "Invoice.Id", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PaymentMethod"), Name = "PaymentMethod", IsFilterable = true, PropertyName = "PaymentMethod", IsSortable = true, TypeFilter = TypeFilter.Enum, EnumType = typeof(PaymentMethod) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Amount"), Name = "Amount", IsFilterable = true, PropertyName = "Amount", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Currency"), Name = "Currency", IsFilterable = true, PropertyName = "Currency", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Date"), Name = "Date", IsFilterable = true, PropertyName = "CreationTime", IsSortable = true, TypeFilter = TypeFilter.Date });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PaymentStatus"), Name = "PaymentStatus", IsFilterable = true, PropertyName = "PaymentStatus", IsSortable = true, TypeFilter = TypeFilter.Enum, EnumType = typeof(PaymentStatus) });
        // Define the columns for the table

        foreach (var payment in payments)
        {
            var rowDef = new RowDef();

            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Code"), payment.PaymentNumber?.ToString()));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CustomerName"), payment.Customers?.ArabicName ?? LocalizationHelper.GetLocalizedString("NotCustomer")));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CustomerName"), payment.Customers?.FullName ?? LocalizationHelper.GetLocalizedString("NotCustomer")));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PlateNumber"), payment.InvoiceId.HasValue ? payment.PlateNumber : LocalizationHelper.GetLocalizedString("Pacakge")));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PaymentCategory"), LocalizationHelper.GetLocalizedString(payment.PaymentCategory.ToString())));
            rowDef.Cells.Add(new KeyValuePair<string, string>(
                LocalizationHelper.GetLocalizedString("InvoiceId"),
                payment.PaymentCategory == Enums.PaymentCategory.PaymentParking
                ? payment.InvoiceId.ToString()
                : payment.PackageId.ToString()
            ));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PaymentMethod"), LocalizationHelper.GetLocalizedString(payment.PaymentMethod)));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Amount"), payment.Amount.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Currency"), LocalizationHelper.GetLocalizedString(payment.Currency.ToString())));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Date"), payment.CreationTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PaymentStatus"), LocalizationHelper.GetLocalizedString(payment.PaymentStatus.ToString())));

            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();
        return tableData;
    }

    public static async Task<TableData> CreateDataTablePackagePayments(List<PackagePaymentAnalysis> payments)
    {
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("PackagePaymentReports"),
            ShowHeader = true
        };

        // Define the columns for the table
        if(CultureInfo.CurrentUICulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PackageName"), Name = "PackageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "Package.ArabicName", IsFilterable = true });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PackageTypeEnName"), Name = "PackageTypeName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "PackageType.ArabicName", IsFilterable = true });
        }        
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PackageName"), Name = "PackageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "Package.EnglishName", IsFilterable = true });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PackageTypeEnName"), Name = "PackageTypeName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "PackageType.EnglishName", IsFilterable = true });
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CountUsePackage"), Name = "CountUsePackage", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "CountUsePackage", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ValueOfPackage"), Name = "ValueOfPackage", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "ValueOfPackage", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ValueTotalPackage"), Name = "ValueTotalPackage", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "ValueTotalPackage", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Date"), Name = "Date", TypeFilter = TypeFilter.Date, IsSortable = true, PropertyName = "Package.CreationTime", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("customer category"), Name = "CustomerCategory", TypeFilter = TypeFilter.Enum, IsSortable = true, PropertyName = "Package.CustomerCategory", IsFilterable = true, EnumType = typeof(CustomerCategoryStatus) });
        // Loop through the payments data and add rows to the table
        foreach (var payment in payments)
        {
            var rowDef = new RowDef();
            if (CultureInfo.CurrentUICulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PackageName"), payment.Package.ArabicName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PackageTypeEnName"), payment.PackageType.ArabicName));
                
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PackageName"), payment.Package.EnglishName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PackageTypeEnName"), payment.PackageType.EnglishName));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CountUsePackage"), payment.CountUsePackage.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ValueOfPackage"), payment.ValueOfPackage.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ValueTotalPackage"), payment.ValueTotalPackage.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Date"), payment.Package.CreationTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("customer category"), LocalizationHelper.GetLocalizedString(payment.Package.CustomerCategory.ToString())));
            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();
        return tableData;
    }
    public static async Task<TableData> CreateDataTableForKeeperTransaction(List<ShiftAnalysis> shiftAnalyses ,Guid? CurrentTenant)
    {
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("KeeperTransactionReport"),
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("UserName"), Name = "UserName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "KeeperName", IsFilterable = true });
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ShiftName"), Name = "ShiftName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "ShiftNameAr", IsFilterable = true });
            if (CurrentTenant== null)
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "OrganizationNameAr", IsFilterable = true });

            }
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("GarageName"), Name = "GarageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "GarageNameAr", IsFilterable = true });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ShiftName"), Name = "ShiftName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "ShiftNameEn", IsFilterable = true });
            if (CurrentTenant ==null)
            {

                tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "OrganizationNameEn", IsFilterable = true });
            }
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("GarageName"), Name = "GarageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "GarageNameEn", IsFilterable = true });
        }

        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("StartDate"), Name = "StartDate", TypeFilter = TypeFilter.Date, IsSortable = true, PropertyName = "StartDate", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("EndDate"), Name = "EndDate", TypeFilter = TypeFilter.Date, IsSortable = true, PropertyName = "EndDate", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("StartTime"), Name = "StartTime", TypeFilter = TypeFilter.Date, IsSortable = true, PropertyName = "StartTime", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("EndTime"), Name = "EndTime", TypeFilter = TypeFilter.Date, IsSortable = true, PropertyName = "EndTime", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("NumberOfTransactions"), Name = "NumberOfTransaction", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "NumberOfTransaction", IsFilterable = true });

        foreach (var shiftAnalysis in shiftAnalyses)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("UserName"), shiftAnalysis.KeeperName));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ShiftName"), shiftAnalysis.ShiftNameAr));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), shiftAnalysis.OrganizationNameAr));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), shiftAnalysis.GarageNameAr));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ShiftName"), shiftAnalysis.ShiftNameEn));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), shiftAnalysis.OrganizationNameEn));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), shiftAnalysis.GarageNameEn));
            }

            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("StartDate"), shiftAnalysis.StartDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("EndDate"), shiftAnalysis.EndDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("StartTime"), shiftAnalysis.StartTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("EndTime"), shiftAnalysis.EndTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("NumberOfTransactions"), shiftAnalysis.NumberOfTransaction.ToString()));
            tableData.Rows.Add(rowDef);
        }

        await Task.Yield();
        return tableData;
    }



}
