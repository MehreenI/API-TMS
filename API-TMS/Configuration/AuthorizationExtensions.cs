using Microsoft.AspNetCore.Authorization;

namespace API_TMS.Configuration
{
    public static class AuthorizationExtensions
    {
        public static void AddJwtAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));

                options.AddPolicy("UserOrAdmin", policy =>
                    policy.RequireRole("Admin", "User"));
            });
        }
    }
}
