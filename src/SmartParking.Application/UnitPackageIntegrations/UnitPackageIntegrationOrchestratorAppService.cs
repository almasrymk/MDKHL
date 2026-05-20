using SmartParking.UnitPackageIntegrations.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.UnitPackageIntegrations;

public class UnitPackageIntegrationOrchestratorAppService : ApplicationService, IUnitPackageIntegrationOrchestratorAppService
{
    private readonly IUnitPackageIntegrationAppService _unitPackageIntegrationAppService;
    private readonly IUnitPackageIntegrationGetAppService _unitPackageIntegrationGetAppService;

    public UnitPackageIntegrationOrchestratorAppService(IUnitPackageIntegrationAppService unitPackageIntegrationAppService, IUnitPackageIntegrationGetAppService unitPackageIntegrationGetAppService)
    {
        _unitPackageIntegrationAppService = unitPackageIntegrationAppService;
        _unitPackageIntegrationGetAppService = unitPackageIntegrationGetAppService;
    }


    public async Task<UnitPackageIntegrationDto> CreateAsync(CreateUpdateUnitPackageIntegrationDto input)
    {
        return await _unitPackageIntegrationAppService.CreateAsync(input);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _unitPackageIntegrationAppService.DeleteAsync(id);
    }

    public async Task<ICollection<UnitPackageIntegrationDto>> GetAllByBookingIdAsync(string bookingId)
    {
        var result = await _unitPackageIntegrationGetAppService.GetAllByBookingIdAsync(bookingId);
        return result.ToList();
    }

    public async Task<UnitPackageIntegrationDto> GetAsync(Guid id)
    {
        return await _unitPackageIntegrationAppService.GetAsync(id);
    }

    public async Task<PagedResultDto<UnitPackageIntegrationDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        return await _unitPackageIntegrationAppService.GetListAsync(input);
    }

    public async Task<UnitPackageIntegrationDto> UpdateAsync(Guid id, CreateUpdateUnitPackageIntegrationDto input)
    {
        return await _unitPackageIntegrationAppService.UpdateAsync(id, input);
    }
    public async Task<bool> BookingIdExist(string bookingId)
    {
        return await _unitPackageIntegrationGetAppService.BookingIdExist(bookingId);
    }

}
