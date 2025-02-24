using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SmartParking.SingletonService;

public interface ISingletonService<T, TKey> : ISingletonDependency,IDisposable
{
    Task<T> GetAsync();
    Task<T> SetAsync(TKey key);
}
