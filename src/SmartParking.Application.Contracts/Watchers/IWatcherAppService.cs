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
        public Task<Camera> GetcameraByWatherCodeAsync(string watcherCode);
        public Task<Dispenser> GetDispenserByWatherCodeAsync(string watcherCode);
    }
}
