using Microsoft.Extensions.Logging;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
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

        public GarageTransactionEndPointsAppService(IGarageTransactionsAppService garageTransactionsAppService, 
            IRepository<Camera, int> cameraRepository, IRepository<Garage, int> garageRepository, ILogger<GarageTransactionEndPointsAppService> logger)
        {
            _garageTransactionsAppService = garageTransactionsAppService;
            _cameraRepository = cameraRepository;
            _garageRepository = garageRepository;
            _logger = logger;
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
                Type =camera.Type
            };
            
                var  garageTransactionDto= await _garageTransactionsAppService.CreateAsync(GarageTransaction);

            _logger.LogError($"{DateTime.Now} ====> transaction created {garageTransactionDto.Id}");

            if (garageTransactionDto != null) 
                    return await Task.FromResult(true);
                else 
                    return await Task.FromResult(false);
           
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
    }
}
