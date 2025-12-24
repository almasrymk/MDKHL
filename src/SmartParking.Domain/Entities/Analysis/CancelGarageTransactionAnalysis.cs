using System;

namespace SmartParking.Entities.Analysis;

public class CancelGarageTransactionAnalysis
{
    public string CashierName { get; set; }
    public string ShiftArabicName { get; set; }
    public string ShiftEnglishName { get; set; }
    public DateOnly? TransactionDate { get; set; }
    public TimeOnly? TransactionTime { get; set; }
    public string PlateNumber { get; set; }
    public string CustomerNameAr { get; set; }
    public string CustomerNameEn { get; set; }
    public int Duration { get; set; }
    public decimal Price { get; set; }  
}