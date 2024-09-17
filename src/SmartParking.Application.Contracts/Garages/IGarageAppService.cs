using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Garages;

public interface IGarageAppService : ICrudAppService<GarageDto,int,PagedAndSortedResultRequestDto,CreateUpdateGarageDto>
{
}
