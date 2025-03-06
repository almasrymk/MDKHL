using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SmartParking.SingletonService;

public interface ISingletonService<T, TKey> : ISingletonDependency,IAsyncDisposable
{
    Task<T> GetAsync(TKey key);
    Task<T> SetAsync(TKey key);
    void Refresh();
}
