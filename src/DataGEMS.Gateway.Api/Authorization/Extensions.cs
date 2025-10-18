using Cite.Tools.Auth.Claims;
using Cite.Tools.Configuration.Extensions;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace DataGEMS.Gateway.Api.Authorization
{
    public static class Extensions
    {
        public static IServiceCollection AddClaimExtractorServices(
            this IServiceCollection services,
            IConfigurationSection claimExtractorSection)
        {
            services.ConfigurePOCO<ClaimExtractorConfig>(claimExtractorSection);
            services.AddSingleton<ClaimExtractor>();

            return services;
        }

        public static IServiceCollection AddAuthenticationServices(
            this IServiceCollection services,
            IConfigurationSection idpClientSection)
        {
            services.ConfigurePOCO<IdpClientConfig>(idpClientSection);

            IdpClientConfig idpClientConfig = new IdpClientConfig();
            idpClientSection.Bind(idpClientConfig);

            services
                .AddAuthentication()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
            {
                jwtOptions.Authority = idpClientConfig.Authority;
                jwtOptions.Audience = idpClientConfig.ClientId;
                jwtOptions.RequireHttpsMetadata = idpClientConfig.RequireHttps;
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = idpClientConfig.ValidateIssuer,
                    ValidateAudience = idpClientConfig.ValidateAudience,
                    ValidateIssuerSigningKey = idpClientConfig.ValidateIssuerSigningKey
                };
                jwtOptions.MapInboundClaims = idpClientConfig.MapInboundClaims;
            });

            return services;
        }

        public static IServiceCollection AddPermissionsAndPolicies(this IServiceCollection services, IConfigurationSection permissionsConfigurationSection)
        {
            services.ConfigurePOCO<PermissionPolicyConfig>(permissionsConfigurationSection);
            //GOTCHA: this can be singleton because it reads the permissions from config
            services.AddSingleton<IPermissionPolicyService, PermissionPolicyService>();
            services.AddScoped<DataGEMS.Gateway.App.Authorization.IAuthorizationService, DataGEMS.Gateway.Api.Authorization.AuthorizationService>();
            services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionRoleAuthorizationHandler>();
            services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, AffiliatedContextAuthorizationHandler>();
			services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionClaimAuthorizationHandler>();
            services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionClientAuthorizationHandler>();
            services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, OwnedResourceAuthorizationHandler>();
            services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionAnonymousAuthorizationHandler>();
            services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionAuthenticatedAuthorizationHandler>();

            return services;
        }


        public static IServiceCollection AddAuthorizationContentResolverServices(this IServiceCollection services)
        {
			services.AddScoped<IAuthorizationContentResolver, AuthorizationContentResolver>();

			return services;
		}
	}
}
