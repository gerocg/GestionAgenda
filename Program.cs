using GestionAgenda.Context;
using GestionAgenda.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql;

var builder = WebApplication.CreateBuilder(args);

// ==============================
// CONEXIÓN A BASE DE DATOS
// ==============================

string connectionString;

// Si estamos en Elastic Beanstalk (existe RDS_HOSTNAME)
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RDS_HOSTNAME")))
{
    var dbHost = Environment.GetEnvironmentVariable("RDS_HOSTNAME");
    var dbPort = Environment.GetEnvironmentVariable("RDS_PORT");
    var dbName = Environment.GetEnvironmentVariable("RDS_DB_NAME");
    var dbUser = Environment.GetEnvironmentVariable("RDS_USERNAME");
    var dbPass = Environment.GetEnvironmentVariable("RDS_PASSWORD");

    connectionString =
        $"Server={dbHost};Port={dbPort};Database={dbName};User={dbUser};Password={dbPass};";
}
else
{
    // Local (Visual Studio)
    connectionString = builder.Configuration.GetConnectionString("Connection");
}

builder.Services.AddDbContext<ContextBd>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// ==============================
// SERVICIOS
// ==============================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };
    });

builder.Services.AddScoped<JwtService>();

var app = builder.Build();

// ==============================
// PIPELINE
// ==============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// app.UseHttpsRedirection(); // comentado para AWS

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
