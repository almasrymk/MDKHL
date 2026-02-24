using SmartParking.Entities.Cameras.Watchers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Watchers;

/// <summary>
/// Provides application-level operations for managing and retrieving watcher-related resources such as cameras,
/// dispensers and watcher metadata.
/// </summary>
public interface IWatcherAppService
{
    /// <summary>
    /// Retrieves the <see cref="Camera"/> associated with the specified watcher code.
    /// </summary>
    /// <param name="watcherCode">The unique code that identifies the watcher. Expected to be non-null and non-empty.</param>
    /// <returns>
    /// A <see cref="Task{Camera}"/> that represents the asynchronous operation. The task result contains the
    /// <see cref="Camera"/> if found; implementations may return <c>null</c> if no camera is associated with the provided code.
    /// </returns>
    Task<Camera> GetcameraByWatherCodeAsync(string watcherCode);

    /// <summary>
    /// Retrieves the <see cref="Dispenser"/> associated with the specified watcher code.
    /// </summary>
    /// <param name="watcherCode">The unique code that identifies the watcher. Expected to be non-null and non-empty.</param>
    /// <returns>
    /// A <see cref="Task{Dispenser}"/> that represents the asynchronous operation. The task result contains the
    /// <see cref="Dispenser"/> if found; implementations may return <c>null</c> if no dispenser is associated with the provided code.
    /// </returns>
    Task<Dispenser> GetDispenserByWatherCodeAsync(string watcherCode);

    /// <summary>
    /// Retrieves the watcher code for the watcher identified by the given watcher Id.
    /// </summary>
    /// <param name="watcherId">The unique identifier of the watcher.</param>
    /// <returns>
    /// A <see cref="Task{String}"/> that represents the asynchronous operation. The task result contains the watcher code
    /// if the watcher exists; implementations may return <c>null</c> or an empty string when no watcher is found.
    /// </returns>
    Task<string> GetWatcherCodeByWatcherIdAsync(int watcherId);

    /// <summary>
    /// Retrieves all configured watchers.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{ICollection}"/> that represents the asynchronous operation. The task result contains a collection
    /// of <see cref="Watcher"/> instances. The collection may be empty if no watchers are configured.
    /// </returns>
    Task<ICollection<Watcher>> GetAllWatchersAsync();
}