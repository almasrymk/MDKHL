using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Mobile.Banners;

public class BannerDto :  FullAuditedEntityDto<int>
{
    public string ImageUrl { get; set; }
}
