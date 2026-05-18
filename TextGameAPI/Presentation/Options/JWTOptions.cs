using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TextGame.Infrastructure.Token;
using TextGame.Infrastructure.Token.JWT;

namespace TextGame.Presentation.Options
{
    public class JWTOptions
    {
        public static void Configure(JwtBearerOptions options)
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = TokenParameters.Issuer,
                ValidateAudience = true,
                ValidAudience = TokenParameters.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = JwtKeyProvider.Instance
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.TryGetValue(TokenParameters.AccessToken, out var accessToken)) 
                        context.Token = accessToken;
                    return Task.CompletedTask;
                }
            };
        }
    }
}
