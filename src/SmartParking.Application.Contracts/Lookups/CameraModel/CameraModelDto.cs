using SmartParking.Lookups.CameraBrands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Lookups.CameraModel;

public class CameraModelDto : FullAuditedEntityDto<int>
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public  CameraBrandDto CameraBrand { get; set; }
}