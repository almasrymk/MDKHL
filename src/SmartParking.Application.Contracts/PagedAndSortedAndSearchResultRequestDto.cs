using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking;

public class PagedAndSortedAndSearchResultRequestDto : PagedAndSortedResultRequestDto
{
    public string SearchingTerm { get; set; }
}
