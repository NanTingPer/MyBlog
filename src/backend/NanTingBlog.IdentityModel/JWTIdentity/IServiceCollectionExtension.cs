using Microsoft.Extensions.DependencyInjection;

namespace NanTingBlog.IdentityModel.JWTIdentity;

public static class IServiceCollectionExtension
{
    public static IServiceCollection AddJWTService(this IServiceCollection services, Func<JWTServiceOptions>? options = null)
    {
        var jwtm = new JWTMiddleware();
        if(options != null) {
            var opt = options.Invoke();
            if(opt != null){
                jwtm.options = opt;
            }
        }
        services.AddSingleton<JWTMiddleware>(jwtm);
        return services;
    }
}
