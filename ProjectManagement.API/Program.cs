using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using ProjectManagement.API.Extensions;
using ProjectManagement.API.Middleware;
using ProjectManagement.API.Services;
using ProjectManagement.Application;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---- Serilog ----
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: Path.Combine(AppContext.BaseDirectory, "logs", "log-.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14);
});

// ---- Layer DI ----
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---- HTTP / Auth ----
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// ---- API versioning ----
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// ---- MVC / Swagger ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwaggerInProduction"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                $"Project Management API {description.GroupName.ToUpperInvariant()}");
        }
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));

try
{
    Log.Information("Starting ProjectManagement.API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
