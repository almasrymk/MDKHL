using SmartParking.Entities.Lookups;
using SmartParking.Lookups.Region;
using SmartParking.Lookups.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

	public StateAppService(IRepository<State, int> repository)
	: base(repository)
	{
	}

}
