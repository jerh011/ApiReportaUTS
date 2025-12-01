using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReportaUTS.Conexion;
using ReportaUTS.Interfaces;
using ReportaUTS.Repository;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Validar variables de entorno críticas (fallar pronto y con mensaje claro) ---
string jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("La variable de entorno JWT_SECRET no está configurada. Defínela antes de arrancar la aplicación.");
}

string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("La variable de entorno CONNECTION_STRING no está configurada. Defínela antes de arrancar la aplicación.");
}

// Opcional: base url pública para construir ImagenUrl si decides hacerlo en backend
// string apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5000";

// JWT Authentication
builder.Services.AddAuthentication("Bearer").AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;

    SymmetricSecurityKey signingKey = new(Encoding.UTF8.GetBytes(jwtSecret));

    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,    // importante cuando defines IssuerSigningKey
        IssuerSigningKey = signingKey,
        LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters) =>
        {
            return expires.HasValue && expires > DateTime.UtcNow;
        }
    };
});

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config =>
{
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

// Configuración de la conexión (validada arriba)
var PostgreSQLConnectionConfiguration = new PostgreSQLConnection(connectionString);
builder.Services.AddSingleton(PostgreSQLConnectionConfiguration);

// Registrar IHttpContextAccessor por si quieres construir URLs dinámicas desde controllers/repos
builder.Services.AddHttpContextAccessor();

// DI de repositorios / servicios
builder.Services.AddScoped<ILogin, LoginRepository>();
builder.Services.AddScoped<ICategoria, CategoriaRepository>();
builder.Services.AddScoped<IReportes, ReportesRepository>();
builder.Services.AddScoped<IVotos, VotoRepository>();
builder.Services.AddScoped<IEdificio, EdificioRepository>();

// CORS
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CORS_ENABLED", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// --- Middlewares (orden recomendado) ---
app.UseRouting();

// Habilitar archivos estáticos para servir wwwroot (ej: /imagenes/archivo.png)
app.UseStaticFiles();

app.UseCors("CORS_ENABLED");

// Habilitar Swagger siempre para pruebas (si lo quieres solo en dev, envuelve con env.IsDevelopment())
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Escuchar en 0.0.0.0 para que Docker sea accesible desde fuera del contenedor
app.Run("http://0.0.0.0:5000");
