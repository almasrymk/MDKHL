using SmartParking.Entities.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Analysis;

public class PaymentAnalysis
{
    public Payment Payment { get; set; }
    public string KeeperName { get; set; }
    public double AmountDaily { get; set; }
    public double AmountMonthly { get; set; }
    public string Day { get; set; }
    public string Month { get; set; }
}
