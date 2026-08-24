using SmartParking.Results;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.Mobile.Keepers;

public interface IKeeperTransactionCorrectionAppService : IApplicationService
{
    /// <summary>
    /// Updates the plate number of a specific garage transaction.
    /// </summary>
    /// <param name="garageTransactionId">ID of the transaction to update.</param>
    /// <param name="plateNumber">Corrected plate number.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> CorrectTransactionByPlateNumberAsync(
        int garageTransactionId, 
        string plateNumber);
}
