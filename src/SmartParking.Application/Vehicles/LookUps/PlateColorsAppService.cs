using SmartParking.Entities;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Lookups.PlateColor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Vehicles.LookUps;

public class PlateColorsAppService : CrudAppService<PlateColor, PlateColorDto, int, PagedAndSortedResultRequestDto, CreateUpdatePlateColorDto>, IPlateColorsAppService
{
    public PlateColorsAppService(IRepository<PlateColor, int> repository) : base(repository)
    {
    }
}
