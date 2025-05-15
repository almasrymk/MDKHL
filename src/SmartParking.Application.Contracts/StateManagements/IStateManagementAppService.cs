using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.StateManagements;

public interface IStateManagementAppService : ICrudAppService<StateManagementDto, int, PagedAndSortedResultRequestDto, CreateUpdateStateManagementDto>
{
    /// <summary>
    /// Asynchronously retrieves the state management details for a specific user and shift.
    /// </summary>
    /// <param name="userId">The unique identifier of the user for whom the state management data is being fetched.</param>
    /// <param name="shiftId">The unique identifier of the shift for which state management data is being requested.</param>
    /// <returns>A task representing the asynchronous operation, with a <see cref="StateManagementDto"/> as the result containing the state management details.</returns>
    Task<StateManagementDto> GetStateManagementByUserAsync(Guid? userId, int shiftId);
}
