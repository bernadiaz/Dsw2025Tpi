
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Dsw2025Ej15.Application.Services;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Helpers;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AuthenticationService = Dsw2025Tpi.Application.Services.AuthService;

namespace Dsw2025Tpi.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddLogging(config =>
        {
            config.AddConsole();
            //.AddFilter("Microsoft.EntityFramework", LogLevel.Error)//configurado desde appsettings
            //config.AddDebug();
            config.AddFile(builder.Configuration.GetSection("LogPath").Value, minimumLevel: LogLevel.Debug);
        });
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(o =>
        {
        o.SwaggerDoc("v1", new OpenApiInfo
        {
           Title = "TPI Desarrollo de SW",
           Version = "v1",
        });
        o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "Ingresar el token",
            Type = SecuritySchemeType.ApiKey
        });
            o.AddSecurityRequirement(new OpenApiSecurityRequirement
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
        builder.Services.AddHealthChecks();

        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password = new PasswordOptions
            {
                RequiredLength = 8
                //requisitos como caracteres especiales, mayusculas, etc. ya estan puestos como true por default
                //RequireDigit = true,
                //RequireUppercase = true,
                //RequireNonAlphanumeric = true
            };
        })
        .AddEntityFrameworkStores<AuthenticationContext>()
        .AddDefaultTokenProviders();

        var jwtConfig = builder.Configuration.GetSection("Jwt");
        var keyText = jwtConfig["Key"] ?? throw new ArgumentNullException("JWT Key");
        var key = Encoding.UTF8.GetBytes(keyText);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {//aca validamos el token
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig["Issuer"],
                    ValidAudience = jwtConfig["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    RoleClaimType = ClaimTypes.Role
                };
            });

        builder.Services.AddSingleton<JwtTokenService>();

        builder.Services.AddDbContext<Dsw2025TpiContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("Dsw2025TpiEntities"));
            options.UseSeeding((c, t) =>
            {
                ((Dsw2025TpiContext)c).SeedWork<Customer>("Sources\\customer.json");
                ((Dsw2025TpiContext)c).SeedWork<Product>("Sources\\product.json");
                /*((Dsw2025TpiContext)c).SeedWork<Order>("Sources\\order.json");
                ((Dsw2025TpiContext)c).SeedWork<OrderItem>("Sources\\orderitem.json");*/
            });
        });

        builder.Services.AddScoped<IRepository, EfRepository>();
        builder.Services.AddTransient<IProductsManagementService, ProductsManagementService>();
        builder.Services.AddTransient<IOrdersManagmentService, OrdersManagmentService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        builder.Services.AddDbContext<AuthenticationContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("Dsw2025TpiEntities"));
        });

        //Para cuando lo quieramos conectar con el front
        builder.Services.AddCors(options => 
        {
            options.AddPolicy("PermitirFrontend", policy =>
                policy.WithOrigins("http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });


        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            SeedData.SeedRoles(services).GetAwaiter().GetResult();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors("PermitirFrontend");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        
        app.MapHealthChecks("/healthcheck");

        app.Run();
    }
}
