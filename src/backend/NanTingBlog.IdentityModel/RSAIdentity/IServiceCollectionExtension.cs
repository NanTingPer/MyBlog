using Microsoft.Extensions.DependencyInjection;

namespace NanTingBlog.IdentityModel.RSAIdentity;

public static class IServiceCollectionExtension
{
    public static IServiceCollection AddRSAService(this IServiceCollection services)
    {
        services.AddSingleton<RSAService>();
        return services;
    }
}
