using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReportaUTS.Conexion;
using ReportaUTS.Interfaces;
using ReportaUTS.Repository;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Agregando la autenticacion de JWTBearer


builder.Services.AddAuthentication("Bearer").AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    string jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
    SymmetricSecurityKey signingKey = new(Encoding.UTF8.GetBytes(jwtSecret));

    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        IssuerSigningKey = signingKey,
        ValidateLifetime = true,
        LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) => {
            return expires.HasValue && expires > DateTime.UtcNow;
        }
    };

});


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(config =>
{

    // Configuracion de Swagger para poder inyectar en token en la UI que proporciona y asi acceder a rutas protegidas mediante esta
    config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autorizacion con JWT mediante el header Authorization Bearer.\n\"Authorization: Bearer {{token}}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
    });

    config.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme() {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

});

var PostgreSQLConnectionConfiguration = new PostgreSQLConnection(Environment.GetEnvironmentVariable("CONNECTION_STRING"));
builder.Services.AddSingleton(PostgreSQLConnectionConfiguration);

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CORS_ENABLED", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddScoped<ILogin, LoginRepository>();
builder.Services.AddScoped<ICategoria, CategoriaRepository>();
builder.Services.AddScoped<IReportes, ReportesRepository>();
builder.Services.AddScoped<IVotos, VotoRepository>();
builder.Services.AddScoped<IEdificio,EdificioRepository>();
var app = builder.Build();

app.UseCors("CORS_ENABLED");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthorization();

app.MapControllers();

app.Run();