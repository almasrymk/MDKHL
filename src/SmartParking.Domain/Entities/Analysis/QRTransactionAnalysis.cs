using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Analysis
{
    public class QRTransactionAnalysis
    {
        public string PlateNumber { get; set; }
        public DateOnly? TransactionDate { get; set; }
        public TimeOnly? TransactionTime { get; set; }
        public string CustomerNameAr { get; set; }
        public string CustomerNameEn { get; set; }
        public string PackageNameAr { get; set; }
        public string PackageNameEn { get; set; }
        public YesNoEnum IsCarBelongToPackage { get; set; }
        public CustomerPackageStatus CustomerPackageStatus  { get; set; }
        public string PackageCode { get; set; }
        public string PackagePlateNumberAr { get; set; }
        public string PackagePlateNumberEn { get; set; }
        public WatcherType CameraType { get; set; }
    }
}
