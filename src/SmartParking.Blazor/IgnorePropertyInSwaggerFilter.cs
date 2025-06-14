using Microsoft.OpenApi.Models;
using SmartParking.DispenserAppService;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SmartParking.Blazor;

public class IgnorePropertyInSwaggerFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(BarrierRequest))
        {
            schema.Properties.Remove("method");
        }
    }
}
