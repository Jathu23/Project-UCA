namespace Project_UCA.Extensions
{
    public static class AuthorizationPolicyExtensions
    {
        public static IServiceCollection ConfigureAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("GenerateInvoice", policy => policy.RequireClaim("Permission", "GenerateInvoice"));
                options.AddPolicy("EditTemplate", policy => policy.RequireClaim("Permission", "EditTemplate"));
                options.AddPolicy("ManageUsers", policy => policy.RequireClaim("Permission", "ManageUsers"));
                options.AddPolicy("ManagePermissions", policy => policy.RequireClaim("Permission", "ManagePermissions"));
                options.AddPolicy("ManagePositions", policy => policy.RequireClaim("Permission", "ManagePositions"));
            });

            return services;
        }
    }
}
