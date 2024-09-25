using SmartParking.CustomerCategories;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Customers.Vehicles;

public class VehicleAppService: CrudAppService<Vehicle, VehicleDto, int, PagedAndSortedResultRequestDto,CreatedUpdatedVehicleDto>, IVehicleAppService
{
    private readonly IRepository<Vehicle, int> _repository;
    private readonly IVehicleAppService _vehicleAppService;

    public VehicleAppService(IRepository<Vehicle, int> repository, IVehicleAppService vehicleAppService):base(repository)
    {
        _repository = repository;
        _vehicleAppService = vehicleAppService;
    }
}
