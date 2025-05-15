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
    /// <summary>
    /// Retrieves a list of available discounts for mobile clients.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing a <see cref="Result{T}"/> with a list of <see cref="DiscountDto"/> objects.
    /// </returns>

    Task<Result<List<DiscountDto>>> GetListMobileAsync();
    /// <summary>
    /// Deletes a discount by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the discount to be deleted.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing the <see cref="DiscountDto"/> of the deleted discount.
    /// </returns>
    /// <exception cref="EntityNotFoundException">
    /// Thrown if a discount with the specified ID does not exist.
    /// </exception>

    Task<DiscountDto> DeleteDiscountAsync(int id);
}
