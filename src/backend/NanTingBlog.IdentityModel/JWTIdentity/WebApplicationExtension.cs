using Microsoft.AspNetCore.Builder;

namespace NanTingBlog.IdentityModel.JWTIdentity;

public static class WebApplicationExtension
{
    public static WebApplication AddJWTMiddleware(this WebApplication webApplication)
    {
        webApplication.UseMiddleware<JWTMiddleware>();
        return webApplication;
    }
}