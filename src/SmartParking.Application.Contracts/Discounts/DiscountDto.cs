using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Discounts;

public class DiscountDto : BaseDto
{
    public decimal DiscountPercent { get; set; }
    public Status Status { get; set; }
}
