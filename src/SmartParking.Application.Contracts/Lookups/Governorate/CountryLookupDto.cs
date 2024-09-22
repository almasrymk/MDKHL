using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Lookups.Governorate;

public class CountryLookupDto : FullAuditedEntityDto<int>
{
    public string Name { get; set; }
}
