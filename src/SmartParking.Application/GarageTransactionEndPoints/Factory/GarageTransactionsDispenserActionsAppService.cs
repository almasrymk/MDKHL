using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.GarageTransactionEndPoints.Factory;

public class GarageTransactionsDispenserActionsAppService : IGarageTransactionActionsAppService
{
    private readonly IRepository<GarageTransaction, int> _repository;

    public GarageTransactionsDispenserActionsAppService(IRepository<GarageTransaction, int> repository)
    {
        _repository = repository;
    }


    public async Task UnMappingEntryTransactionForDeletedExit(GarageTransactionDto garageTransactionDto)
    {
        var query = await _repository.GetQueryableAsync();

        var latestTransaction = query
                .Where(gt => gt.TicketId == garageTransactionDto.TicketId && gt.CameraType == WatcherType.Entry && gt.IsIn == false && gt.IsMapped == true)
                .OrderByDescending(gt => gt.AccurateTime)
                .SingleOrDefault();
        if (latestTransaction == null)
        {
            return;
        }
        latestTransaction.IsIn = true;
        latestTransaction.IsMapped = false;
    }

    public bool CanHandle(GarageTransactionDto garageTransactionDto)
    {
        return !string.IsNullOrEmpty(garageTransactionDto.TicketId);
    }

    public async Task<GarageTransactionDto> CheckIfCheckoutFromMobile(GarageTransactionDto key)
    {
        var query = await _repository.GetQueryableAsync();
        var latestTransaction = await query
                .Where(gt => gt.TicketId == key.TicketId && gt.CameraType == WatcherType.Exit && gt.IsIn == true &&gt.CheckoutMobile !=null)
                .OrderByDescending(gt => gt.AccurateTime)
                .Select(x => new GarageTransactionDto
                {
                    Id = x.Id,
                    CameraId = x.CameraId,
                    PlateNumber = x.PlateNumber,
                    SnapTime = (DateTime)x.SnapTime,
                    AccurateTime = (DateTime)x.AccurateTime,
                    OrganizationID = x.OrganizationID,
                    GarageId = x.GarageId,
                    VehiclePicture = x.VehiclePicture,
                    CameraType = x.CameraType,
                    TenantId = x.TenantId,
                    IsIn = x.IsIn,
                    CheckoutMobile = x.CheckoutMobile,
                    IsMapped = x.IsMapped,
                    RemainingTime = x.RemainingTime,
                    IsPending = x.IsPending,
                    CustomerCategory = x.CustomerCategory,
                    CanCheckout = x.CanCheckout,
                    DispenserId = x.DispenserId,
                    LaneNO = x.LaneNO,
                    TicketId = x.TicketId
                })
                .FirstOrDefaultAsync();
        return latestTransaction;
    }
}
