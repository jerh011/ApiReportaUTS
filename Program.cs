//using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.OpenApi.Writers;
using Npgsql;
using ReportaUTS.Conexion;
using ReportaUTS.Controllers;
using ReportaUTS.Interfaces;
using ReportaUTS.Repository;





var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var PostgreSQLConnectionConfiguration = new PostgreSQLConnection(Environment.GetEnvironmentVariable("CONNECTION_STRING"));
builder.Services.AddSingleton(PostgreSQLConnectionConfiguration);

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

builder.Services.AddScoped<ICategoria, CategoriaRepository>();
builder.Services.AddScoped<IReportes, ReportesRepository>();
builder.Services.AddScoped<IVotos, VotoRepository>();

var app = builder.Build();
app.UseCors("CORS_ENABLED");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
