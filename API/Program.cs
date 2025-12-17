using APPLICATION;
using INFRASTRUCTURE;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add service information
var serviceName = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "AuthService";
var serviceVersion = Environment.GetEnvironmentVariable("SERVICE_VERSION") ?? "1.0.0";

Console.WriteLine($"Starting {serviceName} v{serviceVersion}");

// Add services to the container.
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    // Example: ignore reference loops
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    // Example: ignore null values
    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Auth Service API",
        Version = serviceVersion,
        Description = "Authentication and Authorization Gateway Service",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Auth Service",
            Email = "notifications@chanuthperera.com"
        }
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
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
});

// Add infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Register application services
builder.Services.AddApplicationServices();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://stage-myfinance.onrender.com", "https://myfinance.chanuthperera.com", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Allow credentials for authenticated requests
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSecretKey"] ?? throw new InvalidOperationException("JwtSecretKey is not configured")))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments (you can restrict this if needed)
    app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", $"Auth Service API v{serviceVersion}");
    options.RoutePrefix = "swagger"; // Swagger UI will be available at /swagger
    options.DisplayRequestDuration();
    options.EnableDeepLinking();
    options.EnableFilter();
    options.EnableValidator();
});

app.UseRouting();

// Apply CORS policy globally (before Authorization)
app.UseCors("AllowFrontend");

// HTTPS redirection is handled by Cloudflare, so we don't need this
// app.UseHttpsRedirection();

app.UseAuthentication();  // Add Authentication middleware
app.UseAuthorization();   // Add Authorization middleware

app.MapControllers();

app.Run();
