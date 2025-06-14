//using Abp.Domain.Repositories;
//using SmartParking.DispenserAppService;
//using SmartParking.Entities.Cameras;
//using SmartParking.Enums;
//using SmartParking.GarageTransactionEndPoints;
//using SmartParking.GarageTransactions;
//using SmartParking.WebSockets;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SmartParking.Dispenser;

//public class DispenserAppService : SmartParkingAppService, IDispenserAppService
//{
//    private readonly IGarageTransactionEndPointsAppService _garageTransactionEndPointsAppService;
//    private readonly IWebSocketConnectionManager _websocketConnectionManager;
//    private readonly IBarrierAppService _barrierAppService;
//    private readonly IRepository<Dispenser>

//    public DispenserAppService(IGarageTransactionEndPointsAppService garageTransactionEndPointsAppService, IWebSocketConnectionManager websocketConnectionManager, IBarrierAppService barrierAppService)
//    {
//        _garageTransactionEndPointsAppService = garageTransactionEndPointsAppService;
//        _websocketConnectionManager = websocketConnectionManager;
//        _barrierAppService = barrierAppService;
//    }
//    #region public methods
//    public Task<bool> Entry(EntryDto request)
//    {
//        return Task.FromResult(true);
//    }

//    public Task<bool> Exit(ExitDto request)
//    {
//        _garageTransactionEndPointsAppService.Ha();

//    }

//    public Task<bool> PreEntry(PreEntryDto request)
//    {
//        _garageTransactionEndPointsAppService.HandleEntrance();
        
//    }
//    #endregion

//    #region private methods to initiate dtos
//    /// <summary>
//    /// Method used to initate a create garage transaction dto.
//    /// </summary>
//    /// <param name="input">CreateUpdateGarageTransaction EndPoint Dto</param>
//    /// <param name="camera">Camera which sends transaction</param>
//    /// <returns>create update garage transaction dto</returns>
//    private CreateUpdateGarageTransactionDto InitiateCreateGarageTransactionDto(CreateUpdateGarageTransactionEndPointDto input, )
//    {
//        return new CreateUpdateGarageTransactionDto
//        {
//            AccurateTime = input.AccurateTime,
//            SnapTime = input.SnapTime,
//            VehiclePicture = input.VehiclePicture,
//            PlateNumber = input.PlateNumber,
//            CameraId = camera.Id,
//            GarageId = camera.GarageId,
//            OrganizationID = camera.OrganizationId,
//            TenantId = camera.TenantId,
//            CameraType = camera.CameraType,
//            IsIn = camera.CameraType == CameraType.Exit ? false : true,
//            IsMapped = camera.CameraType == CameraType.Entry ? false : true,

//        };
//    }
//    #endregion


//}
