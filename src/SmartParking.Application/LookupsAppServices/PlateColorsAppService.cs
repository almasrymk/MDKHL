using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using SmartParking.Entities;
using SmartParking.Entities.Lookups;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Lookups;
using SmartParking.Lookups.PlateColor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.LookupsAppServices;

public class PlateColorsAppService : CrudAppService<PlateColor, PlateColorDto,
                                    int, PagedAndSortedResultRequestDto,
                                    CreateUpdatePlateColorDto>,
        IPlateColorsAppService

{
    public PlateColorsAppService(IRepository<PlateColor, int> repository) : base(repository)
    {
    }


}
