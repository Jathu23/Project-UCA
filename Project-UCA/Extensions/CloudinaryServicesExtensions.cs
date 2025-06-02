//namespace Project_UCA.Extensions
//{
//    public class CloudinaryServicesExtensions
//    {
//    }
//}

using Project_UCA.Services;
using Project_UCA.Services.Interfaces;
using Project_UCA.Utilities.Interface;
using Project_UCA.Utilities.Services;

namespace Project_UCA.Extensions
{
    public static class CloudinaryServicesExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IPositionService, PositionService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            return services;
        }
    }
}


