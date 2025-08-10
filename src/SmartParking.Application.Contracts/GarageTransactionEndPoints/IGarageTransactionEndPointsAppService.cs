using SmartParking.CustomerPackages;
using SmartParking.GarageTransactions;
using SmartParking.Organizations;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.GarageTransactionEndPoints;

public interface IGarageTransactionEndPointsAppService : IApplicationService
{
    /// <summary>
    /// Method used to recieve transaction request from camera.
    /// </summary>
    /// <param name="input">Transaction dto</param>
    /// <returns>flag if true opens gate </returns>
    Task<Boolean> CreateAsync(CreateUpdateGarageTransactionEndPointDto input);
    Task<bool> HandleEntrance(CreateUpdateGarageTransactionEndPointDto input, bool _hasFreeSlots, bool _hasPackageOnly, bool DispenserTransaction , bool CameraTransaction, CreateUpdateGarageTransactionDto GarageTransaction ,   CustomerPackageTransactionDto? customerPackageDto=null);
    Task<bool> HandleExit(CreateUpdateGarageTransactionDto exitTransaction, TransactionOrganizationDto organization , CreateUpdateGarageTransactionEndPointDto input  , CustomerPackageTransactionDto? customerPackageDto = null);
}
