using SmartParking.DataTable;
using SmartParking.Entities.Analysis;
using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Helpers;

public static class CreateTableHelper
{
    public static TableData CreateDataTableForGarageTransaction(List<GarageTransactionAnalysis> data)
    {
        // Initialize table data to be displayed, setting the table's name and whether to show the header
        var tableData = new TableData()
        {
            Name = "GarageTransactionAnalysis",
            ShowHeader = true
        };



        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("GarageName"), Name = "GarageName", TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("CameraType"), Name = "CameraType", TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("PlateNumber"), Name = "PlateNumber", TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AccurateTime"), Name = "AccurateTime", TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("OrganizationName"), Name = "OrganizationName", TypeFilter = TypeFilter.Text });


        foreach (var item in data)
        {
            var rowDef = new RowDef();

            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("GarageName"), item.GarageName));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("CameraType"), item.CameraType));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("PlateNumber"), item.PlateNumber));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("AccurateTime"), item.AccurateTime.ToString()));
            rowDef.Cells.Add(new KeyValuePair<string, string>(LocalizationHelper.GetLocalizedString("OrganizationName"), item.OrganizationName));


            tableData.Rows.Add(rowDef);
        }
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



        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("KeeperName"), Name = "KeeperName", TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AmountDaily"), Name = "AmountDaily", TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("AmountMonthly"), Name = "AmountMonthly", TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Day"), Name = "Day", TypeFilter = TypeFilter.Text });
        tableData.Columns.Add(new ColumnDef() { DisplayName = LocalizationHelper.GetLocalizedString("Month"), Name = "Month", TypeFilter = TypeFilter.Text });


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
}
