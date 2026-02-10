using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.GarageTransactionEndPoints.Factory;

public class GarageTransactionsCameraActionsAppService : ApplicationService,IGarageTransactionActionsAppService
{
    private readonly IRepository<GarageTransaction, int> _repository;
    public GarageTransactionsCameraActionsAppService(IRepository<GarageTransaction, int> repository)
    {
        _repository = repository;
    }

    [RemoteService(false)]
    public async Task UnMappingEntryTransactionForDeletedExit(GarageTransactionDto garageTransactionDto)
    {
        var query = await _repository.GetQueryableAsync();

        var latestTransaction = query
                .Where(gt => gt.PlateNumber == garageTransactionDto.PlateNumber && gt.CameraType == WatcherType.Entry && gt.IsIn == false && gt.IsMapped == true)
                .OrderByDescending(gt => gt.AccurateTime)
                .FirstOrDefault();
        if (latestTransaction == null) {
            return;
        }
        latestTransaction.IsIn = true;
        latestTransaction.IsMapped = false;
    }

    [RemoteService(false)]
    public bool CanHandle(GarageTransactionDto garageTransactionDto)
    {
        return !string.IsNullOrEmpty(garageTransactionDto.PlateNumber) && string.IsNullOrEmpty(garageTransactionDto.TicketId);
    }


    [RemoteService(false)]
    public async Task<GarageTransactionDto> CheckIfCheckoutFromMobile(GarageTransactionDto key)
    {
        var query = await _repository.GetQueryableAsync();
        var latestTransaction = await query
                .Where(gt => gt.PlateNumber == key.PlateNumber && gt.CameraType == WatcherType.Exit && gt.IsIn == true&&gt.CheckoutMobile != null)
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

    [RemoteService(false)]
    public bool CanHandle(InvoiceSearchDto invoiceSearchDto)
    {
        return !string.IsNullOrEmpty(invoiceSearchDto.PlateNumber) && string.IsNullOrEmpty(invoiceSearchDto.TicketId);

    }

    [RemoteService(false)]
    public async Task<GarageTransactionDto> GetGarageTransaction(InvoiceSearchDto invoiceSearchDto)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            var latestTransaction = await query
                    .Where(gt => gt.PlateNumber == invoiceSearchDto.PlateNumber && gt.IsIn == true)
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
}
