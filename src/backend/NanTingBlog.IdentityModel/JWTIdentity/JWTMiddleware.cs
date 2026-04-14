using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace NanTingBlog.IdentityModel.JWTIdentity;

public class JWTMiddleware : IMiddleware
{
    public JWTServiceOptions options = new JWTServiceOptions();
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var jwt = new JWTAttribute();
        if (options.SecurityKey != null && 
            !await jwt.OnAuthorizationAsync(context, options.SecurityKey())) {
            if (!context.Response.HasStarted) {
                context.Response.StatusCode = options.UnauthorizedCode?.Invoke(context) ?? 401;
                await context.Response.WriteAsync(
                    options.UnauthorizedContent?.Invoke(context) ??    
                    JsonSerializer.Serialize("[401]")
                );
            }
            return;
        }
        await next.Invoke(context);
    }
}