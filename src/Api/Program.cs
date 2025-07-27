using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using AzureWebContentShare.Api.Configuration;
using AzureWebContentShare.Api.Endpoints;
using AzureWebContentShare.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddApplicationInsights();

// Add configuration
builder.Services.Configure<AzureOptions>(builder.Configuration.GetSection(AzureOptions.SectionName));

// Add Azure services using Managed Identity
var credential = new DefaultAzureCredential();

// Configure Azure Blob Storage
builder.Services.AddSingleton(serviceProvider =>
{
    var azureOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureOptions>>().Value;
    return new BlobServiceClient(new Uri(azureOptions.Storage.BlobEndpoint), credential);
});

// Configure Azure Cosmos DB
builder.Services.AddSingleton(serviceProvider =>
{
    var azureOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureOptions>>().Value;
    return new CosmosClient(azureOptions.CosmosDb.Endpoint, credential);
});

// Configure Azure Key Vault
builder.Services.AddSingleton(serviceProvider =>
{
    var azureOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureOptions>>().Value;
    return new SecretClient(new Uri(azureOptions.KeyVault.Uri), credential);
});

// Register application services
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IFileShareService, FileShareService>();
builder.Services.AddScoped<IUserService, UserService>();

// Background cleanup is now handled by a separate Container Apps job

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Azure Web Content Share API",
        Version = "v1",
        Description = "Secure file sharing API built with .NET 8 and Azure services"
    });

    // Include XML comments for better documentation
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Azure Web Content Share API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseHttpsRedirection();
app.UseCors();

// Map endpoints
app.MapFileShareEndpoints();
app.MapHealthEndpoints();

// Map health checks
app.MapHealthChecks("/health");

app.Run();

// Make Program accessible for testing
public partial class Program { }
