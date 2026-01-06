using SmartParking.Cameras.Watchers;
using SmartParking.Entities.Cameras.Watchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Watchers
{
    public interface IWatcherAppService
    {
        Task<Camera> GetcameraByWatherCodeAsync(string watcherCode);
        Task<Dispenser> GetDispenserByWatherCodeAsync(string watcherCode);
        Task<string> GetWatcherCodeByWatcherIdAsync(int watcherId);
        Task<ICollection<Watcher>> GetAllWatchersAsync();




        
    }
}
