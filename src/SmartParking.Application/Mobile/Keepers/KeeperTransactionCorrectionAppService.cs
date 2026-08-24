using SmartParking.Entities.GarageTransactions;
using SmartParking.Results;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Mobile.Keepers;

[RemoteService(isEnabled: false)]
public class KeeperTransactionCorrectionAppService : SmartParkingAppService, IKeeperTransactionCorrectionAppService
{
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepository;

    public KeeperTransactionCorrectionAppService(
        IRepository<GarageTransaction, int> garageTransactionRepository)
    {
        _garageTransactionRepository = garageTransactionRepository;
    }

    /// <inheritdoc/>
    public async Task<Result> CorrectTransactionByPlateNumberAsync(int garageTransactionId, string plateNumber)
    {
        GarageTransaction? transaction;
        using (DataFilter.Disable<IMultiTenant>())
        {
            transaction = await _garageTransactionRepository.FindAsync(garageTransactionId);
        }

        if (transaction is null)
        {
            return Result.NotFound("transaction not found");
        }

        transaction.PlateNumber = plateNumber;
        try
        {
            await _garageTransactionRepository.UpdateAsync(transaction);
            return Result.SuccessWithMessage("PlateNumber corrected successfully");
        }
        catch (Exception ex)
        {
            return Result.Error("PlateNumber not corrected" + ex.Message);
        }
    }
}
