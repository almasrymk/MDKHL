using SmartParking.Lookups.PlateColor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups.LicenceType
{
	public interface ILicenceTypesAppService : ICrudAppService<LicenceTypeDto, int, PagedAndSortedResultRequestDto, CreateUpdateLicenceTypeDto>
	{

	}
}
