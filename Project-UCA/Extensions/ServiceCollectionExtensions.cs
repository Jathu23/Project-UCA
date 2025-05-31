namespace Project_UCA.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabase(configuration);
            services.AddIdentityServices();
            services.ConfigureJwtAuthentication(configuration);
            services.ConfigureAuthorizationPolicies();
            services.AddRepositories();
            services.AddAppServices();
            services.AddSwaggerAndMvc(configuration);

            return services;
        }
    }
}
