using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Lookups.PlateColor
{
    public class PlateColorDto : FullAuditedEntityDto<int>
    {
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }
    }
}
