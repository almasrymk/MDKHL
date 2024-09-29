
using SmartParking.Entities.Lookups;
using SmartParking.Lookups.State;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.LookupsAppServices;

public class StateAppService : CrudAppService<State, StateDto,
									int, PagedAndSortedResultRequestDto,
									CreateUpdateStateDto>,
		IStateAppService
{

    private readonly IRepository<Region, int> _regionRepository;
    private readonly IRepository<State, int> _stateRepository;

    public StateAppService(IRepository<State, int> stateRepository, IRepository<Region, int> regionRepository)
	: base(stateRepository)
    {
        _stateRepository = stateRepository;
        _regionRepository = regionRepository;
    }

    public async Task<List<RegionLookupDto>> GetRegionLookupAsync()
    {
        var regions = await _regionRepository.GetListAsync();
        var regionsList = ObjectMapper.Map<List<Region>, List<RegionLookupDto>>(regions);
        return regionsList;
    }

    public async Task<List<StateLookupDto>> GetStateLookupAsync()
    {
        var states = await _stateRepository.GetListAsync();
        var statesList = ObjectMapper.Map<List<State>, List<StateLookupDto>>(states);
        return statesList;
    }
}
