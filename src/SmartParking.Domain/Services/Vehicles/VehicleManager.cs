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
    public VehicleManager(IRepository<Vehicle, int> vehicleRepository, IDataFilter dataFilter, ILogger<VehicleManager> logger)
    {
        _vehicleRepository = vehicleRepository;
        _dataFilter = dataFilter;
        _logger = logger;
    }

    public async Task<Guid?> CheckPlateNumberBelongsToCustomerToGetHisIdAsync(string plateNumber)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _vehicleRepository.GetQueryableAsync();
            query = query.Where(x => x.ArPlateNumber == plateNumber || x.EnPlateNumber == plateNumber);
            var result = from q in query select q.CustomerId;
            var customerId = await AsyncExecuter.FirstOrDefaultAsync(result);
            if (customerId == null)
            {
                _logger.LogError($"{DateTime.Now} ====>  Vehicle Not Found ");
                return Guid.Empty;
            }
            return customerId;
        }
    }
}
