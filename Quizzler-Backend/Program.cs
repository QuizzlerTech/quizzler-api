using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quizzler_Backend.Controllers.Services;
using Quizzler_Backend.Data;
using Quizzler_Backend.Services;
using System.Text;

namespace Quizzler_Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<LessonService>();
            builder.Services.AddScoped<GlobalService>();
            builder.Services.AddScoped<FlashcardService>();
            builder.Services.AddControllers();
            builder.Services.AddMemoryCache();
            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JwtIssuer"],
                    ValidAudience = configuration["JwtIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtKey"]))
                };
            });
            builder.Services.AddDbContext<QuizzlerDbContext>(options =>
                options.UseMySQL(configuration["DbConnection"]));
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Quizzler Swagger", Version = "v0.9" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
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
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "QuizzlerCORS",
                                  builder =>
                                  {
                                      builder.WithOrigins("https://quizzler.tech",
                                                          "http://quizzler.tech",
                                                          "http://www.quizzler.tech",
                                                          "https://www.quizzler.tech",
                                                          "http://localhost:3000")
                                             .AllowAnyHeader()
                                             .AllowAnyMethod();
                                  });
            });

            if (!builder.Environment.IsDevelopment())
            {
                builder.WebHost.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.ListenAnyIP(5000);
                });

            }

            /*            Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Warning()
                            .Enrich.FromLogContext()
                            .WriteTo.File("logs/quizzler-api-.log", rollingInterval: RollingInterval.Day)
                            .CreateLogger();
                        builder.Host.UseSerilog();*/

            var app = builder.Build();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("QuizzlerCORS");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            app.MapControllers();
            app.UseStaticFiles();
            app.Run();

        }
    }
}
