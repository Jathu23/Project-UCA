using Project_UCA.Repositories.Interfaces;
using Project_UCA.Repositories;

namespace Project_UCA.Extensions
{
    public static class RepositoryServiceExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            return services;
        }
    }
}
