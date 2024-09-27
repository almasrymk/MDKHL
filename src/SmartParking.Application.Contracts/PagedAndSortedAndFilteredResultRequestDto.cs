using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking;

public class PagedAndSortedAndFilteredResultRequestDto:PagedAndSortedAndSearchResultRequestDto
{
    public List<FilterRequestDto> filters { get; set; }
}
