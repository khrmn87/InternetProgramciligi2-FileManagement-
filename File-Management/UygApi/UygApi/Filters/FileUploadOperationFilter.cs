using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace UygApi.Filters
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParameterName = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile))
                .Select(p => p.Name)
                .FirstOrDefault();

            if (fileParameterName != null)
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Required = new HashSet<string> { fileParameterName },
                                Properties = context.MethodInfo.GetParameters().ToDictionary(
                                    p => p.Name,
                                    p => p.ParameterType == typeof(IFormFile) ?
                                        new OpenApiSchema { Type = "string", Format = "binary" } :
                                        new OpenApiSchema { Type = "string" }
                                )
                            }
                        }
                    }
                };
            }
        }
    }
}