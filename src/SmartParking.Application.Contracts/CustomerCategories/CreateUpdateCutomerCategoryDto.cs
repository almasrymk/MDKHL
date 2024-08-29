using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartParking.CustomerCategories;

public class CreateUpdateCutomerCategoryDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Default { get; set; }
    public int OrganizationId { get; set; }


    [JsonIgnore]
    public bool CreatedByOrganization { get; set; } = false;
}
