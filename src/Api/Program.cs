using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using AzureWebContentShare.Api.Configuration;
using AzureWebContentShare.Api.Endpoints;
using AzureWebContentShare.Api.Services;
using AzureWebContentShare.Api.Authorization;
using AzureWebContentShare.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure logging  
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddApplicationInsights();

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry();

// Add configuration
builder.Services.Configure<AzureOptions>(builder.Configuration.GetSection(AzureOptions.SectionName));
builder.Services.Configure<EntraIdOptions>(builder.Configuration.GetSection(EntraIdOptions.SectionName));

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
builder.Services.AddScoped<ISiteService, SiteService>();

// Add HTTP context accessor for authorization
builder.Services.AddHttpContextAccessor();

// Add authentication and authorization
var entraIdConfig = builder.Configuration.GetSection(EntraIdOptions.SectionName).Get<EntraIdOptions>();
if (entraIdConfig != null)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = entraIdConfig.Authority;
            options.Audience = entraIdConfig.Audience;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = entraIdConfig.Issuer,
                ValidAudience = entraIdConfig.Audience,
                ClockSkew = TimeSpan.FromMinutes(5)
            };
            
            // Log authentication events for debugging
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("JWT authentication failed: {Exception}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogDebug("JWT token validated for user: {User}", context.Principal?.Identity?.Name);
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        // Default policy requires authentication
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
            
        // Administrator only policy
        options.AddPolicy(AuthorizationPolicies.AdministratorOnly, policy =>
            policy.Requirements.Add(new RoleRequirement(UserRole.Administrator)));
            
        // Content owner policy (includes administrators)
        options.AddPolicy(AuthorizationPolicies.ContentOwnerOnly, policy =>
            policy.Requirements.Add(new RoleRequirement(UserRole.ContentOwner)));
            
        // Authenticated user policy
        options.AddPolicy(AuthorizationPolicies.AuthenticatedUser, policy =>
            policy.RequireAuthenticatedUser());
    });

    // Register authorization handlers
    builder.Services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
}
else
{
    // Add basic authorization services for test scenarios
    builder.Services.AddAuthorization();
}

// Add CORS with proper configuration for authentication
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var frontendOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:5173", "https://localhost:5173" };
            
        policy.WithOrigins(frontendOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add OpenAPI/Swagger with security definitions
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Azure Web Content Share API",
        Version = "v1",
        Description = "Secure file sharing API built with .NET 8 and Azure services"
    });

    // Add JWT security definition
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
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

// Authentication and authorization middleware (only if authentication is configured)
if (entraIdConfig != null)
{
    app.UseAuthentication();
}
app.UseAuthorization();

// Map endpoints
app.MapFileShareEndpoints();
app.MapUserManagementEndpoints();
app.MapSiteEndpoints();
app.MapHealthEndpoints();

// Map health checks (allow anonymous access)
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

// Make Program accessible for testing
public partial class Program { }
