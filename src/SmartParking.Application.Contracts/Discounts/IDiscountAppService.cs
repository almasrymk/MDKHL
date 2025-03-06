using SmartParking.DailyShifts;
using SmartParking.Results;
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
    Task<Result<List<DiscountDto>>> GetListMobileAsync();
    Task<DiscountDto> DeleteDiscountAsync(int id);
}
