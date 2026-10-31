using Bookify;

var builder = WebApplication.CreateBuilder(args);

builder.AddDependencyInjectionServices();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.DevelopmentModeOpenApi();
}
app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();