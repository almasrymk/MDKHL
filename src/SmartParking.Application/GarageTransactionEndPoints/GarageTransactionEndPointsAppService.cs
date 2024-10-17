using Microsoft.Extensions.Logging;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.GarageTransactionEndPoints
{
    public class GarageTransactionEndPointsAppService : SmartParkingAppService, IGarageTransactionEndPointsAppService
    {
        private readonly IGarageTransactionsAppService _garageTransactionsAppService;
        private readonly IRepository<Camera, int> _cameraRepository;
        private readonly IRepository<Garage, int> _garageRepository;
        private readonly ILogger<GarageTransactionEndPointsAppService> _logger;
        private readonly IRepository<Customer, Guid> _customerRepoistory;
        private readonly IRepository<Vehicle, int> _vehicleRepository;
        private readonly ICustomerPackagesAppService _customerPackagesAppService;

        public GarageTransactionEndPointsAppService(IGarageTransactionsAppService garageTransactionsAppService, 
            IRepository<Camera, int> cameraRepository, IRepository<Garage, int> garageRepository
            , ILogger<GarageTransactionEndPointsAppService> logger
            ,IRepository<Customer,Guid>customerRepoistory,
            IRepository<Vehicle,int> vehicleRepository,
            ICustomerPackagesAppService customerPackagesAppService)
        {
            _garageTransactionsAppService = garageTransactionsAppService;
            _cameraRepository = cameraRepository;
            _garageRepository = garageRepository;
            _logger = logger;
            _customerRepoistory = customerRepoistory;
            _vehicleRepository = vehicleRepository;
            _customerPackagesAppService = customerPackagesAppService;
        }
        public async Task<bool> CreateAsync(CreateUpdateGarageTransactionEndPointDto input)
        {
           
            _logger.LogError($"{DateTime.Now} ====>  {JsonSerializer.Serialize( new {input.PlateNumber, input.AccurateTime, input.DeviceID, input.VehiclePictureName})}");
            var camera= GetCamera(input.DeviceID);
            input.PlateNumber ??= "123456";//input.PlateNnumber;

            _logger.LogError($"{DateTime.Now} ====>  Camera ID {camera.Id}");
            if (camera == null)
                return await Task.FromResult(false);
            var cameraId = camera.Id;
            var garageId =camera.GarageId;
            var organizationId = GetOrganizationId(garageId);
            var GarageTransaction = new CreateUpdateGarageTransactionDto
            {
                AccurateTime=input.AccurateTime,
                SnapTime=input.SnapTime,
                VehiclePicture=input.VehiclePicture,
                PlateNnumber = input.PlateNumber,
                CameraId=cameraId,
                GarageId=garageId,
                OrganizationID=organizationId,
                TenantId = camera.TenantId,
                CameraType=camera.CameraType
                
               // Type =camera.Type
            };
            //check if transaction is input
            if(GarageTransaction.CameraType==CameraType.Exit)
            {
                int succes= await CheckCustomerPackage(GarageTransaction.OrganizationID,GarageTransaction.PlateNnumber);
                if (succes == -1)
                {
					var garageTransactionDto = await _garageTransactionsAppService.CreateAsync(GarageTransaction);
					return await Task.FromResult(false);
                }

            }
			//Check Garage Capicity

			if (GarageTransaction.CameraType == CameraType.Entry)
			{
				var garage = await _garageRepository.FirstOrDefaultAsync(x => x.Id == garageId);
				if (garage.CurrnetCapicity == garage.MaxCapacity)
					return await Task.FromResult(false);
			}
			try
            {
                var  garageTransactionDto= await _garageTransactionsAppService.CreateAsync(GarageTransaction);
                return await Task.FromResult(true);

            }catch (Exception ex)
            {
				return await Task.FromResult(false);

			}
         

			//_logger.LogError($"{DateTime.Now} ====> transaction created {garageTransactionDto.Id}");

           
        }
        private Camera GetCamera(string CameraCode)
        {
            var camera = _cameraRepository.FirstOrDefaultAsync(x => x.Code.Equals(CameraCode)).Result;
           
            return camera;
        }
        private int GetOrganizationId(int GarageId)
        {
            return  _garageRepository.FirstAsync(x=>x.Id== GarageId).Result.OrganizationId;
        }

        private async Task<int> CheckCustomerPackage(int organizationId,string plateNumber)
        {
            int succes = 1;
            var Customer = await _vehicleRepository.FirstOrDefaultAsync(x => x.PlateNumber == plateNumber);

            if (Customer == null)
            {
                _logger.LogError($"{DateTime.Now} ====>  CustomerID ");

                return -1;
            }
            var CustomerId = Customer.CustomerId;

			var customerPackage=  _customerPackagesAppService.GetCustomerPackageByOrgIdAndCustomerId(organizationId, CustomerId);
            if(customerPackage == null)
            {
                _logger.LogError($"{DateTime.Now} ====>  customerPackage not found");

                return -1;
            }
            return succes;
        }
    }
}
