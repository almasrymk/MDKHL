using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace SmartParking.Blazor.Filters
{
	public class CustomSwaggerFilter:IDocumentFilter
	{
		public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
		{
			//remove paths those start with /api/abp prefix
			swaggerDoc.Paths
				.Where(x => x.Key.ToLowerInvariant().StartsWith("/api/abp"))
				.ToList()
				.ForEach(x => swaggerDoc.Paths.Remove(x.Key));
		}
	}
}
