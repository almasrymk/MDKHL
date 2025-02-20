using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking;

public class PagedAndSortedAndSearchResultRequestDto : PagedAndSortedResultRequestDto
{
    public string SearchingTerm { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {


        return new List<ValidationResult>();
    }
}
