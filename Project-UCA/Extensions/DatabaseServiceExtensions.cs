using Microsoft.EntityFrameworkCore;
using Project_UCA.Data;

namespace Project_UCA.Extensions
{
    public static class DatabaseServiceExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            return services;
        }
    }


}
