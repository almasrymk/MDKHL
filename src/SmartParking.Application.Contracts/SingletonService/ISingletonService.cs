using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SmartParking.SingletonService;

public interface ISingletonService<T, TKey> : ISingletonDependency,IAsyncDisposable
{
    /// <summary>
    /// Retrieves an entity of type <typeparamref name="T"/> by its key.
    /// </summary>
    /// <param name="key">The key of the entity to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation, with the retrieved entity of type <typeparamref name="T"/> as the result.</returns>
    Task<T> GetAsync(TKey key);

    /// <summary>
    /// Sets or updates an entity of type <typeparamref name="T"/> with the provided key.
    /// </summary>
    /// <param name="key">The key of the entity to set or update.</param>
    /// <returns>A task that represents the asynchronous operation, with the updated entity of type <typeparamref name="T"/> as the result.</returns>
    Task<T> SetAsync(TKey key);

    /// <summary>
    /// Refreshes the current state of the data or cache.
    /// </summary>
    /// <remarks>
    /// This method can be used to refresh or reload data that may have been cached or fetched earlier.
    /// </remarks>
    void Refresh();

}
