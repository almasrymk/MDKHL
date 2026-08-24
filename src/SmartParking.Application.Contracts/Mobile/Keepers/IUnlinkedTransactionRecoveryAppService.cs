using SmartParking.GarageTransactions;
using SmartParking.Mobile.Keepers.Dtos;
using SmartParking.Results;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.Mobile.Keepers;

public interface IUnlinkedTransactionRecoveryAppService : IApplicationService
{
    /// <summary>
    /// Retrieves the latest unlinked camera snapshot transaction for a selected watcher/camera device.
    /// </summary>
    /// <param name="organizationId">The organization ID.</param>
    /// <param name="watcherCode">The watcher/device code.</param>
    /// <returns>A Result containing the <see cref="UnlinkedTransactionViewDto"/>.</returns>
    Task<Result<UnlinkedTransactionViewDto>> GetLatestUnlinkedTransactionByWatcherCodeAsync(
        int organizationId, 
        string watcherCode);

    /// <summary>
    /// Converts a pending unlinked transaction snapshot into an active ENTRY GarageTransaction and optionally triggers the barrier.
    /// </summary>
    /// <param name="input">The recovery input data.</param>
    /// <returns>A Result containing the created <see cref="GarageTransactionDto"/>.</returns>
    Task<Result<GarageTransactionDto>> SendUnlinkedTransactionAsEntryAsync(
        SendUnlinkedAsEntryDto input);
}
