using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Myapp.Settings;
using Myapp.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyApp.Authentification;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using MyApp.Products;
using Myapp.Clients;
using Myapp.Transactions;
using Myapp.Billings;

var builder = WebApplication.CreateBuilder(args);

// Add JWT settings
var jwtSettings = builder.Configuration.GetSection("Jwt");

// Ensure required JWT settings are present
var key = jwtSettings["Key"] ?? throw new ArgumentNullException("Jwt:Key is missing in appsettings.json");
var issuer = jwtSettings["Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer is missing in appsettings.json");
var audience = jwtSettings["Audience"] ?? throw new ArgumentNullException("Jwt:Audience is missing in appsettings.json");
var expiryInMinutesValue = jwtSettings["ExpiryInMinutes"];
if (expiryInMinutesValue == null)
{    throw new ArgumentNullException("Jwt:ExpiryInMinutes is missing in appsettings.json"); }
if (!int.TryParse(expiryInMinutesValue, out var expiryInMinutes))
{    throw new ArgumentException("Jwt:ExpiryInMinutes must be a valid integer."); }

// Add JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
    };
});


// Register the JwtService
builder.Services.AddSingleton<JwtService>(provider =>
{
    return new JwtService(
        key,
        issuer,
        audience,
        expiryInMinutes
    );
});

// Configure Guid serialization in order to save mongo ID in bytes
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

// Add MongoDB settings to the DI container -> nwaliw najmou ninjectiwha fi ay service/repository 
// bind the class "MongoDBSettings.cs" with the configuration section "MongoDBSettings" in appsettings.json)
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

// Register the MongoDB client as a singleton service (instantiated once and reused for the entire application)
builder.Services.AddSingleton<IMongoClient>(s =>
{
    // This retrieves the MongoDBSettings configuration object from the DI container.
    var settings = s.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    // Create and return a new MongoClient instance
    return new MongoClient(settings.ConnectionString);
});

// Register the UserService
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddSingleton<ClientService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<BillingService>();


// Add controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
// Add Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyApp API", Version = "v1" });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://example.com",
                                           "http://localhost:5173").AllowAnyHeader().AllowAnyMethod();
                      });
});

var app = builder.Build();



app.UseCors(MyAllowSpecificOrigins);
// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();