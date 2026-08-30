using SmartParking.Entities.GarageTransactions;
using SmartParking.Results;
using SmartParking.Validations;
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
        if (string.IsNullOrWhiteSpace(plateNumber))
        {
            return Result.Invalid(new ValidationError
            {
                ErrorMessage = "Identifier is required."
            });
        }

        string trimmedValue = plateNumber.Trim();
        bool isTicket = VehicleValidation.IsValidTicketNumber(trimmedValue);
        bool isPlate = !isTicket && VehicleValidation.IsValidSaudiPlate(trimmedValue);

        if (!isTicket && !isPlate)
        {
            return Result.Invalid(new ValidationError
            {
                ErrorMessage = "Supplied value is neither a valid Saudi plate number nor a valid ticket number."
            });
        }

        GarageTransaction? transaction;
        using (DataFilter.Disable<IMultiTenant>())
        {
            transaction = await _garageTransactionRepository.FindAsync(garageTransactionId);
        }

        if (transaction is null)
        {
            return Result.NotFound("transaction not found");
        }

        if (isTicket)
        {
            transaction.TicketId = trimmedValue;
        }
        else
        {
            transaction.PlateNumber = trimmedValue;
        }

        try
        {
            await _garageTransactionRepository.UpdateAsync(transaction);
            return Result.SuccessWithMessage(isTicket ? "TicketNumber corrected successfully" : "PlateNumber corrected successfully");
        }
        catch (Exception ex)
        {
            return Result.Error("Transaction not corrected: " + ex.Message);
        }
    }
}

