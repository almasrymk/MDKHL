using Microsoft.CodeAnalysis.Host.Mef;
using SmartParking.Currency;
using SmartParking.CustomerPackages;
using SmartParking.DataTable;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Payments;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Helpers;

public static class CreateTableHelper
{

    #region Public Methods
    /// <summary>
    /// Creates a table for garage transaction analysis with localized column names and rows based on the provided data.
    /// </summary>
    /// <param name="data">
    /// A list of <see cref="GarageTransactionAnalysis"/> objects representing the garage transaction data.
    /// </param>
    /// <param name="currentTenant">
    /// An optional tenant identifier (if any) for filtering tenant-specific data.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation, with a <see cref="TableData"/> object
    /// containing the localized table columns and rows.
    /// </returns>
    public static async Task<TableData> CreateDataTableForGarageTransaction(List<GarageTransactionAnalysis> data, Guid? currentTenant)
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
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PlateNumber"), Name = "PlateNumber", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "PlateNumber", IsFilterable = true });
        if (CultureInfo.CurrentCulture.Name == "en")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("GarageName"), Name = "GarageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "GarageName", IsFilterable = true });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("GarageName"), Name = "GarageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "GarageNameAr", IsFilterable = true });

        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CameraType"), Name = "CameraType", TypeFilter = TypeFilter.Enum, IsSortable = true, TypeOfColumn = typeof(WatcherType), IsFilterable = true, PropertyName = "CameraType" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AccurateDate"), Name = "AccurateDate", TypeFilter = TypeFilter.DateOnly, IsSortable = true, PropertyName = "TransactionDate", IsFilterable = true, TypeOfColumn = typeof(DateOnly) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AccurateTime"), Name = "AccurateTime", TypeFilter = TypeFilter.Time, IsSortable = true, PropertyName = "TransactionTime", IsFilterable = true, TypeOfColumn = typeof(TimeOnly) });
        if (currentTenant == null)
        {
            if (CultureInfo.CurrentCulture.Name == "en")
            {

                tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "OrganizationName", IsFilterable = true });
            }
            else
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "OrganizationNameAr", IsFilterable = true });

            }
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("customer category"), Name = "CustomerCategory", TypeFilter = TypeFilter.Enum, IsSortable = true, PropertyName = "GarageTransaction.CustomerCategory", IsFilterable = true, TypeOfColumn = typeof(CustomerCategoryStatus) });
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
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CameraType"), LocalizationHelper.GetLocalizedString(item.CameraType.ToString())));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AccurateDate"), item.TransactionDate.Value.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AccurateTime"), item.TransactionTime.Value.ToString()));
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
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Name"), item.UserName));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("customer category"), LocalizationHelper.GetLocalizedString(item.GarageTransaction.CustomerCategory.ToString())));

            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();

        return tableData;
    }
    /// <summary>
    /// Creates a table for displaying customer transaction data with localized column names and rows based on the provided data.
    /// </summary>
    /// <param name="data">
    /// A list of <see cref="GarageTransactionAnalysis"/> objects representing the garage transaction data for customers.
    /// </param>
    /// <param name="currentTenant">
    /// An optional tenant identifier (if any) for filtering tenant-specific data.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation, with a <see cref="TableData"/> object
    /// containing the localized table columns and rows.
    /// </returns>
    public static async Task<TableData> CreateDataTableForGarageTransactionCustomer(List<GarageTransactionAnalysis> data, Guid? currentTenant)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("TotalCustomersAndTheirTransactions"),
            ShowHeader = true
        };
        if (currentTenant == null)
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "OrganizationName" });
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PlateNumber"), Name = "PlateNumber", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "PlateNumber" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("GarageName"), Name = "GarageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "GarageName" });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CameraType"), Name = "CameraType", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "CameraType" });

        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AccurateTime"), Name = "AccurateTime", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "AccurateTime" });

        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("customer category"), Name = "CustomerCategory", TypeFilter = TypeFilter.Enum, IsSortable = true, PropertyName = "CustomerCategory", IsFilterable = true, TypeOfColumn = typeof(CustomerCategoryStatus) });

        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Shift"), Name = "Shift", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "ShiftArabicName", IsFilterable = true });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Shift"), Name = "Shift", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "ShiftEnglishName", IsFilterable = true });
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CreatorName"), Name = "CreatorName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "CreatorName", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CreationDate"), Name = "CreationDate", TypeFilter = TypeFilter.DateOnly, IsSortable = true, PropertyName = "CreationDate", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CreationTime"), Name = "CreationTiem", TypeFilter = TypeFilter.Time, IsSortable = true, PropertyName = "CreationTime", IsFilterable = true });

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
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("customer category"), LocalizationHelper.GetLocalizedString(item.GarageTransaction.CustomerCategory.ToString())));

            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Shift"), item.ShiftArabicName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Shift"), item.ShiftEnglishName));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CreatorName"), item.CreatorName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CreationDate"), item.CreationDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CreationTime"), item.CreationTime.ToString()));

            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();

        return tableData;
    }
    /// <summary>
    /// Creates a table for displaying garage data with localized column names and rows based on the provided list of garages.
    /// </summary>
    /// <param name="garages">
    /// A list of <see cref="GarageAnalysis"/> objects representing the garage data to be displayed in the table.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation, with a <see cref="TableData"/> object
    /// containing the localized table columns and rows.
    /// </returns>
    public static async Task<TableData> CreateDataTableForGarages(List<GarageAnalysis> garages)
    {
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("GaragesReports"),
            ShowHeader = true
        };

        if (CultureInfo.CurrentCulture.Name == "en")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("EnglishName"), Name = "NameEn", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "Garage.NameEn", IsFilterable = true });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ArabicName"), Name = "NameAr", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "Garage.NameAr", IsFilterable = true });

        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CurrnetCapacity"), Name = "CurrnetCapacity", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "Garage.CurrnetCapicity", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CurrnetCapacityForCustomers"), Name = "CurrnetCapacityForCustomers", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "Garage.CurrnetCapacityForCustomers", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CurrnetCapacityEmployees"), Name = "CurrnetCapacityEmployees", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "Garage.CurrnetCapacityEmployees", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CurrnetCapacityForFree"), Name = "CurrnetCapacityForFree", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "Garage.CurrnetCapacityForFree", IsFilterable = true });

        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Status"), Name = "Status", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "Status", IsFilterable = true, TypeOfColumn = typeof(Status) });
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
    /// <summary>
    /// Creates a table for displaying customer package data with localized column names and rows based on the provided list of customer packages.
    /// </summary>
    /// <param name="customerPackages">
    /// A list of <see cref="CustomerPackageDto"/> objects representing the customer package data to be displayed in the table.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation, with a <see cref="TableData"/> object
    /// containing the localized table columns and rows.
    /// </returns>
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
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CustomerName"), Name = "CustomerName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "CustomerArabicName", IsFilterable = true });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PlateNumber"), Name = "PlateNumber", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "ArPlateNumber", IsFilterable = true });

        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PackageName"), Name = "PackageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "PackageEnglishName", IsFilterable = true });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CustomerName"), Name = "CustomerName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "CustomerEnglishName", IsFilterable = true });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PlateNumber"), Name = "PlateNumber", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "EnPlateNumber", IsFilterable = true });


        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{LocalizationHelper.GetLocalizedString("PriceOfPackage")}",
            Name = "PriceOfPackage", TypeFilter = TypeFilter.Number  , IsSortable = true, PropertyName = "PriceOfPackage", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{LocalizationHelper.GetLocalizedString("PackageDuration")} ({LocalizationHelper.GetLocalizedString("Hour")})",
            Name = "PackageDuration", TypeFilter = TypeFilter.Number  , IsSortable = true , PropertyName = "DurationOfPackage", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("SubscriptionDate"), Name = "SubscriptionDate", TypeFilter = TypeFilter.DateOnly , IsSortable = true, PropertyName = "ActivationDate", IsFilterable = true, TypeOfColumn = typeof(DateOnly) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("SubscriptionTime"), Name = "SubscriptionTime", TypeFilter = TypeFilter.Time , IsSortable = true, PropertyName = "Activationtime", IsFilterable = true,TypeOfColumn = typeof(TimeOnly) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ExpiryDate"), Name = "ExpiryDate", TypeFilter = TypeFilter.DateOnly , IsSortable = true, PropertyName = "ExpiryDateOfPackage", IsFilterable = true,TypeOfColumn = typeof(DateOnly) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ExpiryTime"), Name = "Expirytime", TypeFilter = TypeFilter.Time , IsSortable = true, PropertyName = "ExpiryTimeOfPackage", IsFilterable = true,TypeOfColumn = typeof(TimeOnly) });

        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("customer category"), Name = "CustomerCategory", TypeFilter = TypeFilter.Enum, IsSortable = true, PropertyName = "CustomerCategory", IsFilterable = true, TypeOfColumn = typeof(CustomerCategoryStatus) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Status"), Name = "Status", TypeFilter = TypeFilter.Enum, IsSortable = true, PropertyName = "CustomerPackageStatus", IsFilterable = true, TypeOfColumn = typeof(Status) });
        // Loop through the customer packages data and add rows to the table
        foreach (var customerPackage in customerPackages)
        {
            var rowDef = new RowDef();
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PackageName"), customerPackage.PackageArabicName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CustomerName"), customerPackage.CustomerArabicName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PlateNumber"), customerPackage.ArPlateNumber));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PackageName"), customerPackage.PackageEnglishName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CustomerName"), customerPackage.CustomerEnglishName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PlateNumber"), customerPackage.EnPlateNumber));
            }



            var iconHtml =CurrencyIcon.ToHtml("icon-Saudi_Riyal_Symbolsvg", "15px");
            rowDef.Cells.Add(new KeyValuePair<string, string>(
                $"{LocalizationHelper.GetLocalizedString("PriceOfPackage")}",
                customerPackage.PriceOfPackage != null ?
                $"{customerPackage.PriceOfPackage?.ToString()} {iconHtml}" :$"0 {iconHtml}"));
            rowDef.Cells.Add(new KeyValuePair<string, string>($"{LocalizationHelper.GetLocalizedString("PackageDuration")} ({LocalizationHelper.GetLocalizedString("Hour")})", customerPackage.DurationOfPackage?.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("SubscriptionDate"), customerPackage.ActivationDate?.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("SubscriptionTime"), customerPackage.ActivationTime?.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ExpiryDate"), customerPackage.ExpiryDateOfPackage.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ExpiryTime"), customerPackage.ExpiryTimeOfPackage.ToString()));

            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("customer category"), LocalizationHelper.GetLocalizedString(customerPackage.CustomerCategory.ToString())));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Status"), LocalizationHelper.GetLocalizedString(customerPackage.CustomerPackageStatus.ToString())));
            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();
        return tableData;

    }

    /// <summary>
    /// Creates a data table for payments with dynamic columns based on localization and the provided payments data.
    /// </summary>
    /// <param name="payments">The list of payments to be displayed in the table.</param>
    /// <returns>A <see cref="Task{TableData}"/> representing the asynchronous operation. The result contains the generated table data.</returns>
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
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PaymentCategory"), Name = "PaymentCategory", IsFilterable = true, PropertyName = "PaymentCategory", IsSortable = true, TypeFilter = TypeFilter.Enum, TypeOfColumn = typeof(PaymentCategory) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("InvoiceId"), Name = "InvoiceId", IsFilterable = true, PropertyName = "Invoice.Id", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PaymentMethod"), Name = "PaymentMethod", IsFilterable = true, PropertyName = "PaymentMethod", IsSortable = true, TypeFilter = TypeFilter.Enum, TypeOfColumn = typeof(PaymentMethod) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{LocalizationHelper.GetLocalizedString("Discount")}", Name = "Discount", IsFilterable = true, PropertyName = "Invoice.Discount", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{LocalizationHelper.GetLocalizedString("Amount")}", Name = "Amount", IsFilterable = true, PropertyName = "Amount", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Date"), Name = "Date", IsFilterable = true, PropertyName = "Date", IsSortable = true, TypeFilter = TypeFilter.DateOnly , TypeOfColumn = typeof(DateOnly) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Time"), Name = "Time", IsFilterable = true, PropertyName = "Time", IsSortable = true, TypeFilter = TypeFilter.Time , TypeOfColumn = typeof(TimeOnly) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PaymentStatus"), Name = "PaymentStatus", IsFilterable = true, PropertyName = "PaymentStatus", IsSortable = true, TypeFilter = TypeFilter.Enum, TypeOfColumn = typeof(PaymentStatus) });
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
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CustomerName"), payment.Customers?.IdentityUser?.Name ?? LocalizationHelper.GetLocalizedString("NotCustomer")));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PlateNumber"), payment.InvoiceId.HasValue ? payment.PlateNumber : LocalizationHelper.GetLocalizedString("Pacakge")));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PaymentCategory"), LocalizationHelper.GetLocalizedString(payment.PaymentCategory.ToString())));
            rowDef.Cells.Add(new KeyValuePair<string, string>(
                LocalizationHelper.GetLocalizedString("InvoiceId"),
                payment.PaymentCategory == Enums.PaymentCategory.PaymentParking
                ? payment.InvoiceId.ToString()
                : payment.PackageId.ToString()
            ));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PaymentMethod"), LocalizationHelper.GetLocalizedString(payment.PaymentMethod.ToString())));
            var iconHtml = CurrencyIcon.ToHtml("icon-Saudi_Riyal_Symbolsvg", "15px");
            rowDef.Cells.Add(new KeyValuePair<string, string>(
        $"{LocalizationHelper.GetLocalizedString("Discount")}",
        $"{payment?.Invoice?.Discount} {iconHtml}"));
            rowDef.Cells.Add(new KeyValuePair<string, string>(
                $"{LocalizationHelper.GetLocalizedString("Amount")}",
                $"{payment.Amount.ToString()} {iconHtml}"));
    
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Date"), payment.Date.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Time"), payment.Time.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PaymentStatus"), LocalizationHelper.GetLocalizedString(payment.PaymentStatus.ToString())));

            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();
        return tableData;
    }
    /// <summary>
    /// Creates a data table for package payments, with columns and rows based on the provided payment data. 
    /// It adjusts the columns and their display based on the current UI culture (Arabic or English).
    /// </summary>
    /// <param name="payments">A list of <see cref="PackagePaymentAnalysis"/> objects representing the payment data to display in the table.</param>
    /// <returns>
    /// A <see cref="Task{TableData}"/> representing the asynchronous operation. 
    /// The result contains the generated table data with the package payment information.
    /// </returns>
    public static async Task<TableData> CreateDataTablePackagePayments(List<PackagePaymentAnalysis> payments)
    {
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("PackagePaymentReports"),
            ShowHeader = true
        };

        // Define the columns for the table
        if (CultureInfo.CurrentUICulture.Name == "ar")
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
        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{LocalizationHelper.GetLocalizedString("ValueOfPackage")}", Name = "ValueOfPackage", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "ValueOfPackage", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{LocalizationHelper.GetLocalizedString("ValueTotalPackage")}", Name = "ValueTotalPackage", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "ValueTotalPackage", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Date"), Name = "Date", TypeFilter = TypeFilter.DateOnly, TypeOfColumn = typeof(DateOnly), IsSortable = true, PropertyName = "Date", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Time"), Name = "Time", TypeFilter = TypeFilter.Time, TypeOfColumn = typeof(TimeOnly), IsSortable = true, PropertyName = "Time", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("customer category"), Name = "CustomerCategory", TypeFilter = TypeFilter.Enum, IsSortable = true, PropertyName = "Package.CustomerCategory", IsFilterable = true, TypeOfColumn = typeof(CustomerCategoryStatus) });
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
            var IconHtml = CurrencyIcon.ToHtml("icon-Saudi_Riyal_Symbolsvg", "15px");
            rowDef.Cells.Add(new KeyValuePair<string, string>(
                $"{LocalizationHelper.GetLocalizedString("ValueOfPackage")}",
                $"{payment.ValueOfPackage.ToString()} {IconHtml}"));

            rowDef.Cells.Add(new KeyValuePair<string, string>(
                $"{LocalizationHelper.GetLocalizedString("ValueTotalPackage")}",
                $"{payment.ValueTotalPackage.ToString()} {IconHtml}"));

            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Date"), payment.Date.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Time"), payment.Time.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("customer category"), LocalizationHelper.GetLocalizedString(payment.Package.CustomerCategory.ToString())));
            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();
        return tableData;
    }
    /// <summary>
    /// Creates a data table for keeper transactions report with localized column names based on the current UI culture.
    /// </summary>
    /// <param name="shiftAnalyses">A list of <see cref="ShiftAnalysis"/> objects containing shift transaction data.</param>
    /// <param name="CurrentTenant">The current tenant's identifier (if null, the organization name is included in the report).</param>
    /// <returns>A task representing the asynchronous operation, with a <see cref="TableData"/> object containing the generated table data.</returns>
    public static async Task<TableData> CreateDataTableForKeeperTransaction(List<ShiftAnalysis> shiftAnalyses, Guid? CurrentTenant)
    {
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("KeeperCountTransactionsReport"),
            ShowHeader = true
        };

        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("UserName"), Name = "UserName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "KeeperName", IsFilterable = true });
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ShiftName"), Name = "ShiftName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "ShiftNameAr", IsFilterable = true });
            if (CurrentTenant == null)
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "OrganizationNameAr", IsFilterable = true });

            }
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("GarageName"), Name = "GarageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "GarageNameAr", IsFilterable = true });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ShiftName"), Name = "ShiftName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "ShiftNameEn", IsFilterable = true });
            if (CurrentTenant == null)
            {

                tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "OrganizationNameEn", IsFilterable = true });
            }
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("GarageName"), Name = "GarageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "GarageNameEn", IsFilterable = true });
        }

        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("StartDate"), Name = "StartDate", TypeFilter = TypeFilter.DateOnly, TypeOfColumn = typeof(DateOnly), IsSortable = true, PropertyName = "StartDate", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("EndDate"), Name = "EndDate", TypeFilter = TypeFilter.DateOnly, TypeOfColumn = typeof(DateOnly), IsSortable = true, PropertyName = "EndDate", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("StartTime"), Name = "StartTime", TypeFilter = TypeFilter.Time, IsSortable = true, PropertyName = "StartTime", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("EndTime"), Name = "EndTime", TypeFilter = TypeFilter.Time, IsSortable = true, PropertyName = "EndTime", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("NumberOfTransactions"), Name = "NumberOfTransaction", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "NumberOfTransaction", IsFilterable = true });

        foreach (var shiftAnalysis in shiftAnalyses)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("UserName"), shiftAnalysis.KeeperName));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ShiftName"), shiftAnalysis.ShiftNameAr));
                if (CurrentTenant == null)
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), shiftAnalysis.OrganizationNameAr));
                }
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), shiftAnalysis.GarageNameAr));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ShiftName"), shiftAnalysis.ShiftNameEn));
                if (CurrentTenant == null)
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), shiftAnalysis.OrganizationNameEn));
                }
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
    /// <summary>
    /// Creates a data table for the payment user shift report with localized column names based on the current UI culture.
    /// </summary>
    /// <param name="paymentUserShifts">A list of <see cref="PaymentUserShift"/> objects containing payment and shift data for users.</param>
    /// <param name="CurrentTenant">The current tenant's identifier (if null, the organization name is included in the report).</param>
    /// <returns>A task representing the asynchronous operation, with a <see cref="TableData"/> object containing the generated table data.</returns>
    public static async Task<TableData> CreateDataTableForPaymentUserShift(List<PaymentUserShift> paymentUserShifts, Guid? CurrentTenant)
    {
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("PaymentUserShiftReport"),
            ShowHeader = true
        };
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("UserName"), Name = "UserName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "UserName", IsFilterable = true });

        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ShiftName"), Name = "ShiftName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "ShiftArabicName", IsFilterable = true });
            if (CurrentTenant == null)
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "OrganizationArabicName", IsFilterable = true });

            }

            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("GarageName"), Name = "GarageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "GarageArabicName", IsFilterable = true });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ShiftName"), Name = "ShiftName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "ShiftEnglishName", IsFilterable = true });
            if (CurrentTenant == null)
            {
                tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "OrganizationEnglishName", IsFilterable = true });
            }
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("GarageName"), Name = "GarageName", TypeFilter = TypeFilter.Text, IsSortable = true, PropertyName = "GarageEnglishName", IsFilterable = true });
        }

        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{LocalizationHelper.GetLocalizedString("Amount")}", Name = "TotalAmount", TypeFilter = TypeFilter.Number, IsSortable = true, PropertyName = "TotalAmount", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("StartDate"), Name = "StartDate", TypeFilter = TypeFilter.DateOnly, IsSortable = true, PropertyName = "StartDate", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("EndDate"), Name = "EndDate", TypeFilter = TypeFilter.DateOnly, IsSortable = true, PropertyName = "EndDate", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("StartTime"), Name = "StartTime", TypeFilter = TypeFilter.Time, IsSortable = true, PropertyName = "StartTime", IsFilterable = true });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("EndTime"), Name = "EndTime", TypeFilter = TypeFilter.Time, IsSortable = true, PropertyName = "EndTime", IsFilterable = true });

        foreach (var paymentUserShift in paymentUserShifts)
        {
            var rowDef = new RowDef();
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("UserName"), paymentUserShift.UserName));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ShiftName"), paymentUserShift.ShiftArabicName));
                if (CurrentTenant == null)
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), paymentUserShift.OrganizationArabicName));
                }
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), paymentUserShift.GarageArabicName));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ShiftName"), paymentUserShift.ShiftEnglishName));
                if (CurrentTenant == null)
                {
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), paymentUserShift.OrganizationEnglishName));
                }
                    rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), paymentUserShift.GarageEnglishName));
            }
            var iconHtml = CurrencyIcon.ToHtml("icon-Saudi_Riyal_Symbolsvg", "15px");
            rowDef.Cells.Add(new KeyValuePair<string, string>(
                $"{LocalizationHelper.GetLocalizedString("Amount")}",
                $"{paymentUserShift.TotalAmount.ToString("F2")} {iconHtml}"));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("StartDate"), paymentUserShift.StartDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("EndDate"), paymentUserShift.EndDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("StartTime"), paymentUserShift.StartTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("EndTime"), paymentUserShift.EndTime.ToString()));

            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();
        return tableData;
    }

    /// <summary>
    /// Asynchronously creates a data table for payment records, mapping the relevant properties of each payment to table columns and rows.
    /// </summary>
    /// <param name="discountPayments">A list of <see cref="DiscountPaymentAnalysis"/> objects representing payment records that will be displayed in the table.</param>
    /// <returns>
    /// A <see cref="TableData"/> object containing the columns and rows that represent the payment data, formatted for display in a table.
    /// </returns>
    /// <remarks>
    /// This method constructs a table with various columns, including payment details such as payment number, customer name, plate number, 
    /// payment category, invoice ID, payment method, amount, discount, payment date, and payment status. The columns are dynamically 
    /// adjusted based on the current culture (e.g., customer names are localized for Arabic). Each row in the table represents a single 
    /// payment record and is populated with its corresponding data from the provided <paramref name="discountPayments"/> list.
    /// </remarks>
    public static async Task<TableData> CreateDataTableForDiscountPayments(List<DiscountPaymentAnalysis> discountPayments)
    {
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("DiscountsReport"),
            ShowHeader = true
        };
        // Define the columns for the table
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Code"), Name = "PaymentNumber", IsFilterable = true, PropertyName = "PaymentNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("InvoiceId"), Name = "InvoiceId", IsFilterable = true, PropertyName = "InvoiceId", IsSortable = true, TypeFilter = TypeFilter.Number });

        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CustomerName"), Name = "CustomerName", IsFilterable = true, PropertyName = "CustomerArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        }

        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CustomerName"), Name = "CustomerName", IsFilterable = true, PropertyName = "CustomerEnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
        }

        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PlateNumber"), Name = "PlateNumber", IsFilterable = true, PropertyName = "PlateNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PaymentCategory"), Name = "PaymentCategory", IsFilterable = true, PropertyName = "PaymentCategory", IsSortable = true, TypeFilter = TypeFilter.Enum, TypeOfColumn = typeof(PaymentCategory) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PaymentMethod"), Name = "PaymentMethod", IsFilterable = true, PropertyName = "PaymentMethod", IsSortable = true, TypeFilter = TypeFilter.Enum, TypeOfColumn = typeof(PaymentMethod) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Currency"), Name = "Currency", IsFilterable = true, PropertyName = "Currency", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{LocalizationHelper.GetLocalizedString("Discount")}", Name = "Discount", IsFilterable = true, PropertyName = "Discount", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = $"{LocalizationHelper.GetLocalizedString("Amount")}", Name = "Amount", IsFilterable = true, PropertyName = "Amount", IsSortable = true, TypeFilter = TypeFilter.Number });

        
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Date"), Name = "Date", IsFilterable = true, PropertyName = "Date", IsSortable = true, TypeFilter = TypeFilter.DateOnly, TypeOfColumn = typeof(DateOnly) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Time"), Name = "Time", IsFilterable = true, PropertyName = "Time", IsSortable = true, TypeFilter = TypeFilter.Time, TypeOfColumn = typeof(TimeOnly) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PaymentStatus"), Name = "PaymentStatus", IsFilterable = true, PropertyName = "PaymentStatus", IsSortable = true, TypeFilter = TypeFilter.Enum, TypeOfColumn = typeof(PaymentStatus) });

        // Define the rows for the table
        foreach (var payment in discountPayments)
        {
            var rowDef = new RowDef();

            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Code"), payment.PaymentNumber?.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("InvoiceId"), payment.InvoiceId?.ToString()));
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CustomerName"), payment.CustomerArabicName ?? LocalizationHelper.GetLocalizedString("NotCustomer")));

            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CustomerName"), payment.CustomerEnglishName ?? LocalizationHelper.GetLocalizedString("NotCustomer")));

            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PlateNumber"), payment.PlateNumber));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PaymentCategory"), LocalizationHelper.GetLocalizedString(payment.PaymentCategory.ToString())));

            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PaymentMethod"), LocalizationHelper.GetLocalizedString(payment.PaymentMethod.ToString())));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Currency"), LocalizationHelper.GetLocalizedString(payment.Currency)));
            var IconHtml = CurrencyIcon.ToHtml("icon-Saudi_Riyal_Symbolsvg", "15px");

            rowDef.Cells.Add(new KeyValuePair<string, string>(
                $"{LocalizationHelper.GetLocalizedString("Discount")}",
                $"{payment.Discount.ToString()} {IconHtml}"));

            rowDef.Cells.Add(new KeyValuePair<string, string>(
                $"{LocalizationHelper.GetLocalizedString("Amount")}",
                $"{payment.Amount.ToString()} {IconHtml}"));

       
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Date"), payment.Date.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("Time"), payment.Time.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PaymentStatus"), LocalizationHelper.GetLocalizedString(payment.PaymentStatus.ToString())));

            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();
        return tableData;
    }

    /// <summary>
    /// Creates a data table for displaying customer vehicle information.
    /// </summary>
    /// <param name="vehicleCustomers">A list of customer vehicle data transfer objects to be displayed in the table.</param>
    /// <returns>A <see cref="TableData"/> object containing the table structure with columns and rows based on the provided data.</returns>
    public static async Task<TableData> CreateDataTableForCustomersVehicles(List<VehicleCustomerReportDto> vehicleCustomers)
    {
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("BeneficiariesVehicles"),
            ShowHeader = true
        };
        // Define the columns for the table
        //tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Id"), Name = "Id",  PropertyName = "Id",  TypeFilter = TypeFilter.Text});
        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("BeneficiaryName"), Name = "BeneficiaryName", IsFilterable = true, PropertyName = "CustomerArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PlateNumber"), Name = "PlateNumber", IsFilterable = true, PropertyName = "ArPlateNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("BrandName"), Name = "BrandName", IsFilterable = true, PropertyName = "BrandArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ModelName"), Name = "ModelName", IsFilterable = true, PropertyName = "ModelArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PackageName"), Name = "PackageName", IsFilterable = true, PropertyName = "PackageNameArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });


        }

        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("BeneficiaryName"), Name = "BeneficiaryName", IsFilterable = true, PropertyName = "CustomerEnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PlateNumber"), Name = "PlateNumber", IsFilterable = true, PropertyName = "EnPlateNumber", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("BrandName"), Name = "BrandName", IsFilterable = true, PropertyName = "BrandEnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ModelName"), Name = "ModelName", IsFilterable = true, PropertyName = "ModelEnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PackageName"), Name = "PackageName", IsFilterable = true, PropertyName = "PackageNameEnglishName", IsSortable = true, TypeFilter = TypeFilter.Text });


        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("customer category"), Name = "CustomerCategory", IsFilterable = true, PropertyName = "CustomerCategory", IsSortable = true, TypeFilter = TypeFilter.Enum, TypeOfColumn = typeof(CustomerCategoryStatus) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ModelYear"), Name = "ModelYear", IsFilterable = true, PropertyName = "ModelYear", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AddedBy"), Name = "AddedBy", IsFilterable = true, PropertyName = "CreatedBy", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AddedDate"), Name = "AddedDate", IsFilterable = true, PropertyName = "CreationDate", IsSortable = true, TypeFilter = TypeFilter.DateOnly, TypeOfColumn = typeof(DateOnly) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AddedTime"), Name = "AddedTime", IsFilterable = true, PropertyName = "CreationTime", IsSortable = true, TypeFilter = TypeFilter.Time, TypeOfColumn = typeof(TimeOnly) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ModifiedBy"), Name = "ModifiedBy", IsFilterable = true, PropertyName = "CreatedBy", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ModifiedDate"), Name = "ModifiedDate", IsFilterable = true, PropertyName = "CreationDate", IsSortable = true, TypeFilter = TypeFilter.DateOnly, TypeOfColumn = typeof(DateOnly) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ModifiedTime"), Name = "ModifiedTime", IsFilterable = true, PropertyName = "CreationTime", IsSortable = true, TypeFilter = TypeFilter.Time, TypeOfColumn = typeof(TimeOnly) });

        // Define the rows for the table
        foreach (var vehicleCustomer in vehicleCustomers)
        {
            var rowDef = new RowDef();

            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("BeneficiaryName"), vehicleCustomer.CustomerArabicName ?? LocalizationHelper.GetLocalizedString("NotCustomer")));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PlateNumber"), vehicleCustomer.ArPlateNumber));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("BrandName"), vehicleCustomer.BrandArabicName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ModelName"), vehicleCustomer.ModelArabicName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PackageName"), vehicleCustomer.PackageNameArabicName));

            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("BeneficiaryName"), vehicleCustomer.CustomerEnglishName ?? LocalizationHelper.GetLocalizedString("NotCustomer")));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PlateNumber"), vehicleCustomer.EnPlateNumber));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("BrandName"), vehicleCustomer.BrandEnglishName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ModelName"), vehicleCustomer.ModelEnglishName));
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PackageName"), vehicleCustomer.PackageNameEnglishName));

            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("customer category"), vehicleCustomer.CustomerCategory.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ModelYear"), vehicleCustomer.ModelYear));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AddedBy"), vehicleCustomer.CreatedBy));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AddedDate"), vehicleCustomer.CreationDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AddedTime"), vehicleCustomer.CreationTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ModifiedBy"), vehicleCustomer.ModifiedBy));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ModifiedDate"), vehicleCustomer.ModifiedDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ModifiedTime"), vehicleCustomer.ModifiedTime.ToString()));

            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();
        return tableData;
    }
    /// <summary>
    /// Creates a data table for beneficiaries' details, dynamically creating columns and rows based on the customer analysis data.
    /// The table is populated with either Arabic or English data based on the current culture.
    /// </summary>
    /// <param name="customers">A list of <see cref="CustomerAnalysis"/> objects containing the customer data to be displayed in the table.</param>
    /// <returns>A <see cref="TableData"/> object containing the table structure, including columns and rows with customer details.</returns>
    public static async Task<TableData> CreateDataTableForBeneficiariesDetails(List<CustomerAnalysis> customers)
    {
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("BeneficiariesDetailsReports"),
            ShowHeader = true
        };

        if (CultureInfo.CurrentCulture.Name == "ar")
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("BeneficiaryName"), Name = "BeneficiaryName", IsFilterable = true, PropertyName = "BeneficiaryNameAr", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        else
        {
            tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("BeneficiaryName"), Name = "BeneficiaryName", IsFilterable = true, PropertyName = "BeneficiaryNameEn", IsSortable = true, TypeFilter = TypeFilter.Text });
        }
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("customer category"), Name = "PackageCategory", IsFilterable = true, PropertyName = "PackageCategory", IsSortable = true, TypeFilter = TypeFilter.Enum, TypeOfColumn = typeof(CustomerCategoryStatus) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AddedBy"), Name = "AddedBy", IsFilterable = true, PropertyName = "CreatedBy", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AddedDate"), Name = "AddedDate", IsFilterable = true, PropertyName = "CreationDate", IsSortable = true, TypeFilter = TypeFilter.DateOnly, TypeOfColumn = typeof(DateOnly) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AddedTime"), Name = "AddedTime", IsFilterable = true, PropertyName = "CreationTime", IsSortable = true, TypeFilter = TypeFilter.Time, TypeOfColumn = typeof(TimeOnly) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ModifiedBy"), Name = "ModifiedBy", IsFilterable = true, PropertyName = "ModifiedBy", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ModifiedDate"), Name = "ModifiedDate", IsFilterable = true, PropertyName = "ModifiedDate", IsSortable = true, TypeFilter = TypeFilter.DateOnly, TypeOfColumn = typeof(DateOnly) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("ModifiedTime"), Name = "ModifiedTime", IsFilterable = true, PropertyName = "ModifiedTime", IsSortable = true, TypeFilter = TypeFilter.Time, TypeOfColumn = typeof(TimeOnly) });

        // Define the rows for the table
        foreach (var customer in customers)
        {
            var rowDef = new RowDef();

            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("BeneficiaryName"), customer.BeneficiaryNameAr ?? LocalizationHelper.GetLocalizedString("NotCustomer")));
            }
            else
            {
                rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("BeneficiaryName"), customer.BeneficiaryNameEn ?? LocalizationHelper.GetLocalizedString("NotCustomer")));
            }
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("customer category"), customer.PackageCategory.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AddedBy"), customer.CreatedBy));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AddedDate"), customer.CreationDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AddedTime"), customer.CreationTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ModifiedBy"), customer.ModifiedBy));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ModifiedDate"), customer.ModifiedDate.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("ModifiedTime"), customer.ModifiedTime.ToString()));
            tableData.Rows.Add(rowDef);
        }
        await Task.Yield();
        return tableData;
    }

    #endregion
    #region UnUsed Methods
    public static async Task<TableData> CreateDataTableForKepperPayments(List<PaymentAnalysis> payments)
    {
        var tableData = new TableData()
        {
            Name = LocalizationHelper.GetLocalizedString("PaymentReports"),
            ShowHeader = true
        };
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Code"), Name = "PaymentNumber", IsFilterable = true, PropertyName = "PaymentNumber", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PlateNumber"), Name = "PlateNumber", IsFilterable = false, PropertyName = "Invoice.PlateNumber", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PaymentCategory"), Name = "PaymentCategory", IsFilterable = true, PropertyName = "PaymentCategory", IsSortable = true, TypeFilter = TypeFilter.Enum, TypeOfColumn = typeof(PaymentCategory) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("InvoiceId"), Name = "InvoiceId", IsFilterable = false, PropertyName = "Invoice.Id", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PaymentMethod"), Name = "PaymentMethod", IsFilterable = true, PropertyName = "PaymentMethod", IsSortable = true, TypeFilter = TypeFilter.Enum, TypeOfColumn = typeof(PaymentMethod) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Amount"), Name = "Amount", IsFilterable = true, PropertyName = "Amount", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Currency"), Name = "Currency", IsFilterable = true, PropertyName = "Currency", IsSortable = true, TypeFilter = TypeFilter.Number });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CustomerName"), Name = "CustomerName", IsFilterable = true, PropertyName = "Customers.ArabicName", IsSortable = true, TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Date"), Name = "Date", IsFilterable = true, PropertyName = "CreationTime", IsSortable = true, TypeFilter = TypeFilter.Date, TypeOfColumn = typeof(DateTime) });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PaymentStatus"), Name = "PaymentStatus", IsFilterable = true, PropertyName = "PaymentStatus", IsSortable = true, TypeFilter = TypeFilter.Enum, TypeOfColumn = typeof(PaymentStatus) });
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
    #endregion
}

