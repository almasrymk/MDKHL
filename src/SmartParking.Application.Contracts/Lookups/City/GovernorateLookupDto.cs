using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Lookups.City;

public class GovernorateLookupDto : FullAuditedEntityDto<int>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string ArabicCapital { get; set; }
    public string EnglishCapital { get; set; }

}
