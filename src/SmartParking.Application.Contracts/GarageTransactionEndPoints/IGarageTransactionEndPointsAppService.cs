using SmartParking.CustomerPackages;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Payments;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    Task<bool> HandleEntrance(CreateUpdateGarageTransactionEndPointDto input, bool _hasFreeSlots, bool _hasPackageOnly, CreateUpdateGarageTransactionDto GarageTransaction);
    Task<bool> HandleExit(CreateUpdateGarageTransactionDto exitTransaction, Organization organization);
}
