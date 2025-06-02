using Microsoft.Extensions.DependencyInjection;
using Project_UCA.Services;
using Project_UCA.Services.Interfaces;
using Project_UCA.Utilities.Interface;
using Project_UCA.Utilities.Services;

namespace Project_UCA.Extensions
{
    public static class AppServiceExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddHttpClient(); // For IHttpClientFactory in CloudinaryService
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IPositionService, PositionService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            return services;
        }
    }
}