using Scalar.AspNetCore;

namespace Bookify;

public static class DevelopmentMode
{
    public static IEndpointRouteBuilder DevelopmentModeOpenApi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapOpenApi();
        endpoints.MapScalarApiReference(options =>
        {
            options
                .WithTitle("Bookify API")
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithOpenApiRoutePattern("/openapi/v1.json");
        });
        return endpoints;
    }

}