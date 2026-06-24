using XrmMcp.Core;
using XrmMcp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "XrmMCP API",
        Version = "v1",
        Description = "Model Context Protocol Server for Dynamics 365 / Dataverse"
    });
});

builder.Services.AddSingleton<IXrmConnectionService, XrmConnectionService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "XrmMCP API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "0.2.0-phase1"
}))
.WithName("HealthCheck")
.WithTags("System");

app.MapPost("/api/connections/test", async (XrmConnectionOptions request, IXrmConnectionService connectionService, CancellationToken cancellationToken) =>
{
    var result = await connectionService.TestConnectionAsync(request, cancellationToken);
    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
})
.WithName("TestConnection")
.WithTags("Connections");

app.Run();
