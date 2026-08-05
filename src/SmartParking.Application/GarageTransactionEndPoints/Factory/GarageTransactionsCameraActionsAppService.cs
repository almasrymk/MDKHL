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
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.GarageTransactionEndPoints.Factory;

public class GarageTransactionsCameraActionsAppService : IGarageTransactionActionsAppService
{
    private readonly IRepository<GarageTransaction, int> _repository;
    private readonly IDataFilter _dataFilter;
    public GarageTransactionsCameraActionsAppService(IRepository<GarageTransaction, int> repository, IDataFilter dataFilter)
    {
        _repository = repository;
        _dataFilter = dataFilter;
    }

    [RemoteService(false)]
    public async Task UnMappingEntryTransactionForDeletedExit(GarageTransactionDto garageTransactionDto)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();

            var latestTransaction = await query
                    .Where(gt => gt.PlateNumber == garageTransactionDto.PlateNumber && gt.CameraType == WatcherType.Entry )
                    .OrderByDescending(gt => gt.AccurateTime)
                    .FirstOrDefaultAsync();
            if (latestTransaction == null) {
                return;
            }
            // Maintain invariant: reset all CanCheckout=true for this vehicle before restoring this entry
            var allActive = await query
                .Where(gt => gt.CanCheckout == true
                          && (gt.PlateNumber == garageTransactionDto.PlateNumber))
                .ToListAsync();
            allActive.ForEach(gt => gt.CanCheckout = false);
            if (allActive.Any())
                await _repository.UpdateManyAsync(allActive);

            latestTransaction.IsIn = true;
            latestTransaction.IsMapped = false;
            latestTransaction.CanCheckout = true;  // restore as new stack top
            latestTransaction.CheckoutMobile = null;
            await _repository.UpdateAsync(latestTransaction);
        }
    }

    [RemoteService(false)]
    public bool CanHandle(GarageTransactionDto garageTransactionDto)
    {
        return !string.IsNullOrEmpty(garageTransactionDto.PlateNumber) && string.IsNullOrEmpty(garageTransactionDto.TicketId);
    }


    [RemoteService(false)]
    public async Task<GarageTransactionDto> CheckIfCheckoutFromMobile(GarageTransactionDto key)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var query = await _repository.GetQueryableAsync();
            var latestTransaction = await query
                    .Where(gt => gt.PlateNumber == key.PlateNumber && gt.CameraType == WatcherType.Exit && gt.IsIn == true&&gt.CheckoutMobile != null && gt.OrganizationID == key.OrganizationID && gt.GarageId == key.GarageId)
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

    [RemoteService(false)]
    public bool CanHandle(InvoiceSearchDto invoiceSearchDto)
    {
        return !string.IsNullOrEmpty(invoiceSearchDto.PlateNumber) && string.IsNullOrEmpty(invoiceSearchDto.TicketId);

    }

    [RemoteService(false)]
    public async Task<GarageTransactionDto> GetGarageTransaction(InvoiceSearchDto invoiceSearchDto)
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
