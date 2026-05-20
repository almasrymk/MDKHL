using SmartParking.Entities.UnitPackageIntegrations;
using SmartParking.Mappers;
using SmartParking.UnitPackageIntegrations.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.UnitPackageIntegrations;

[RemoteService(false)]
public class UnitPackageIntegrationGetAppService : ApplicationService, IUnitPackageIntegrationGetAppService
{
    private readonly IRepository<UnitPackageIntegration, Guid> _repository;

    public UnitPackageIntegrationGetAppService(IRepository<UnitPackageIntegration, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<ICollection<UnitPackageIntegrationDto>> GetAllByBookingIdAsync(string bookingId)
    {
        var unitPackageIntegrations = await _repository.GetListAsync(x => x.BookingId == bookingId);
        return unitPackageIntegrations.ToDtoList();
    }
    public async Task<bool> BookingIdExist(string bookingId) 
    { 
        return await _repository.AllAsync(x => x.BookingId == bookingId);
    }
}
