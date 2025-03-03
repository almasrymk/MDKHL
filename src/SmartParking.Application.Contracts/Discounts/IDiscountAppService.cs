using SmartParking.DailyShifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.Discounts;

public interface IDiscountAppService :
     ICrudAppService<
        DiscountDto,
        int,
        PagedAndSortedAndSearchResultRequestDto,
        CreateUpdateDiscountDto
        >
{
}
