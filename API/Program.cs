using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    "AllowFrontend",
                    policy => policy
                        .WithOrigins("http://localhost:3000", "https://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false; // develop only!

                    options.Authority = builder.Configuration["REPORTS_API_KEYCLOAK_AUTHORITY"];
                    options.Audience = builder.Configuration["REPORTS_API_KEYCLOAK_AUDIENCE"];

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidIssuer = builder.Configuration["REPORTS_API_KEYCLOAK_ISSUER"],
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = false
                    };
                    
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = OnTokenValidated
                    };
                });

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("ProtheticUsersPolicy", policy => policy.RequireRole("prothetic_user"));

            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseCors("AllowFrontend");

            app.UseAuthentication();            
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static Task OnTokenValidated(TokenValidatedContext context)
        {
            var identity = context.Principal?.Identity as ClaimsIdentity;
            var claim = context.Principal?.FindFirst("realm_access");

            if (identity != null && claim != null)
            {
                var realmAccess = JsonSerializer.Deserialize<RealmAccess>(claim.Value);

                if (realmAccess?.Roles != null)
                {
                    identity.AddClaims(realmAccess.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
                }
            }

            return Task.CompletedTask;
        }

        private class RealmAccess
        {
            [JsonPropertyName("roles")]
            public string[]? Roles { get; set; }
        }
    }
}
