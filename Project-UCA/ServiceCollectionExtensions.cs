using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Project_UCA.Models;
using Project_UCA.Services;
using System.Text;
using Microsoft.OpenApi.Models;
using Project_UCA.Data;
using Project_UCA.Services.Interfaces;
using Project_UCA.Repositories.Interfaces;
using Project_UCA.Repositories;

namespace Project_UCA
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Add Identity
            services.AddIdentity<ApplicationUser, IdentityRole<int>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Configure Identity options
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.SignIn.RequireConfirmedAccount = false;
            });

            // Add JWT Authentication
            var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing in configuration.");
            var key = Encoding.ASCII.GetBytes(jwtKey);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            // Add Authorization Policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("GenerateInvoice", policy => policy.RequireClaim("Permission", "GenerateInvoice"));
                options.AddPolicy("EditTemplate", policy => policy.RequireClaim("Permission", "EditTemplate"));
                options.AddPolicy("ManageUsers", policy => policy.RequireClaim("Permission", "ManageUsers"));
                options.AddPolicy("ManagePermissions", policy => policy.RequireClaim("Permission", "ManagePermissions"));
                options.AddPolicy("ManagePositions", policy => policy.RequireClaim("Permission", "ManagePositions"));
            });

            // Add Custom Services
            services.AddScoped<AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository,UserRepository>();

            // Add Controllers and Views
            services.AddControllersWithViews();

            // Add Swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Invoice Generator API",
                    Version = "v1",
                    Description = "API for managing invoices, users, positions, and permissions"
                });

                // Add JWT Authentication support
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by a space and the JWT token."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            return services;
        }
    }
}