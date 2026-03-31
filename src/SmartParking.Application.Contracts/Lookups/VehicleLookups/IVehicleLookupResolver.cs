using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Lookups.VehicleLookups;

public interface IVehicleLookupResolver
{
     Task<int> ResolveBrandIdAsync(string brandName);
     Task<int> ResolveModelIdAsync(string modelName);
     Task<int> ResolvePlateColorIdAsync(string plateColorName);
     Task<int> ResolveLicenceTypeIdAsync(string licenceTypeName);
    
}
