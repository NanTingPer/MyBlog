using Microsoft.Extensions.DependencyInjection;

namespace NanTingBlog.IdentityModel.JWTIdentity;

public static class IServiceCollectionExtension
{
    public static IServiceCollection AddJWTService(this IServiceCollection services, Func<IServiceProvider, JWTServiceOptions>? options = null)
    {
        var jwtm = new JWTMiddleware();
        services.AddSingleton<JWTMiddleware>(serviceProvider => {
            if (options != null) {
                var opt = options.Invoke(serviceProvider);
                if (opt != null) {
                    jwtm.options = opt;
                }
            }
            return jwtm;
        });
        return services;
    }
}
