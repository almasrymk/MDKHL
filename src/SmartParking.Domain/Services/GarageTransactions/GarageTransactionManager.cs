using SmartParking.Entities.GarageTransactions;
using SmartParking.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Services.GarageTransactions;
public class GarageTransactionManager : DomainService
{
    private readonly IRepository<GarageTransaction, int> _repository;
    private readonly IDataFilter _dataFilter;

    public GarageTransactionManager(IRepository<GarageTransaction, int> repository, IDataFilter dataFilter)
    {
        _repository = repository;
        _dataFilter = dataFilter;
    }
    public async Task<GarageTransaction> CheckIfCheckoutFromMobile(string plateNumber)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var query = await _repository.GetQueryableAsync();
            var latestTransaction = query
                    .Where(gt => gt.PlateNumber == plateNumber && gt.CameraType == WatcherType.Exit && gt.IsIn == true)
                    .OrderByDescending(gt => gt.AccurateTime)
                    .FirstOrDefault();

            return latestTransaction;
        }
    }
    public async Task CloseCheckoutMobileTransaction(string? plateNumber)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query.Where(gt => gt.CameraType == WatcherType.Exit);
            await SaveClosedEntryTransaction(plateNumber, query);
        }
    }

    public async Task CloseEntryTransaction(string? plateNumber, string TicketId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            if (!string.IsNullOrEmpty(TicketId))
            {
                await EntryTicketTransactions(TicketId, query);
                return;
            }
            await SaveClosedEntryTransaction(plateNumber, query);
        }
    }

    private async Task EntryTicketTransactions(string? TicketNumber, IQueryable<GarageTransaction> query)
    {
        if (TicketNumber == null)
            return ;
        var latestTransactions= query
                .Where(gt => gt.TicketId == TicketNumber && gt.IsIn == true)
                .OrderByDescending(gt => gt.AccurateTime)
                .ToList();
        latestTransactions.ForEach(gt =>
        {
            gt.IsIn = false;
            gt.IsMapped = true;
        });
        await _repository.UpdateManyAsync(latestTransactions);


    }



    private async Task SaveClosedEntryTransaction(string? plateNumber, IQueryable<GarageTransaction> query)
    {
        if (plateNumber == null)
            return ;
        var latestTransactions = query
                .Where(gt => gt.PlateNumber == plateNumber  && gt.IsIn == true)
                .OrderByDescending(gt => gt.AccurateTime)
                .ToList();
        latestTransactions.ForEach(gt =>
        {
            gt.IsIn = false;
            gt.IsMapped = true;
        });

        await _repository.UpdateManyAsync(latestTransactions);
    }


    public async Task<string> FindEntranceImage(string? plateNumber, string ticketId, int OrganiztionId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            var latestTransaction = query
                    .Where(gt => gt.PlateNumber == plateNumber && gt.CameraType == WatcherType.Entry && gt.IsIn == true)
                    .OrderByDescending(gt => gt.AccurateTime)
                    .FirstOrDefault();
            var img = latestTransaction?.VehiclePicture ?? null;

            if (img == null)
            {
                var query2 = await _repository.GetQueryableAsync();
                var latestTransaction2 = query
                        .Where(gt => gt.TicketId == ticketId && gt.CameraType == WatcherType.Entry && gt.IsIn == true)
                        .OrderByDescending(gt => gt.AccurateTime)
                        .FirstOrDefault();
                img = latestTransaction2?.VehiclePicture ?? null;
            }
            return img;
        }
    }





    public async Task<GarageTransaction?> GetTransactionByCamera(string plateNumber, int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            var latestTransaction = query
                    .Where(gt => gt.PlateNumber == plateNumber && gt.CameraType == WatcherType.Entry && gt.IsIn == true && gt.OrganizationID == organizationId)
                    .OrderByDescending(gt => gt.AccurateTime)
                    .FirstOrDefault();

            return latestTransaction;
        }
    }


    public async Task<GarageTransaction> GetTransactionById(int garagetransactionId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(transaction => transaction.Id == garagetransactionId);
            var transaction = await AsyncExecuter.FirstOrDefaultAsync(query);
            return transaction;
        }
    }

    public async Task<GarageTransaction?> GetTransactionByDispenser(string? TicketId, int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            var latestTransaction = query
                    .Where(gt => gt.TicketId == TicketId && gt.CameraType == WatcherType.Entry && gt.IsIn == true && gt.OrganizationID == organizationId)
                    .OrderByDescending(gt => gt.AccurateTime)
                    .FirstOrDefault();
            return latestTransaction;
        }
    }

    public async Task UpdateEntryTransactionCheckouFromMobile(int entryGarageTransactionId)
    {
        var entryGarageTransaction = await GetTransactionById(entryGarageTransactionId);
        entryGarageTransaction.IsPending = false;
        entryGarageTransaction.CanCheckout = false;
        await _repository.UpdateAsync(entryGarageTransaction, true);
    }

    //
}
