using Microsoft.Extensions.Logging;
using SmartParking.Entities.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Services.Vehicles;
public  class VehicleManager : DomainService
{
    private readonly IRepository<Vehicle, int> _vehicleRepository;
    private readonly ILogger<VehicleManager> _logger;
    private readonly IDataFilter _dataFilter;
    public VehicleManager(IRepository<Vehicle, int> vehicleRepository, IDataFilter dataFilter)
    {
        _vehicleRepository = vehicleRepository;
        _dataFilter = dataFilter;
    }

    public async Task<string> CheckPlateNumberBelongsToCustomerToGetHisIdAsync(string plateNumber)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _vehicleRepository.GetQueryableAsync();
            query = query.Where(x => x.ArPlateNumber == plateNumber || x.EnPlateNumber == plateNumber);
            var result = from q in query select q.CustomerId.ToString();
            var customerId = await AsyncExecuter.FirstOrDefaultAsync(result);
            if (customerId == null)
            {
                _logger.LogError($"{DateTime.Now} ====>  Vehicle Not Found ");
                return null;
            }
            return customerId;
        }
    }
}
