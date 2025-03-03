using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Discounts;

public class DiscountDto : FullAuditedEntityDto<int>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public decimal DiscountPercent { get; set; }
    public string Notes { get; set; }
}
