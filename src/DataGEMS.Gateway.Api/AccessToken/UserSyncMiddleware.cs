using Cite.Tools.Auth.Claims;
using Cite.Tools.Logging.Extensions;
using Cite.WebTools.CurrentPrincipal;
using DataGEMS.Gateway.App.Data;
using DataGEMS.Gateway.App.Service.AAI;

namespace DataGEMS.Gateway.Api.AccessToken
{
	public class UserSyncMiddleware
	{
		private readonly RequestDelegate _next;

		public UserSyncMiddleware(RequestDelegate next)
		{
			this._next = next;
		}

		public async Task Invoke(
			HttpContext context, 
			ILogger<UserSyncMiddleware> logger, 
			AppDbContext dbContext, 
			ICurrentPrincipalResolverService currentPrincipalResolverService, 
			ClaimExtractor extractor, 
			IAAIService aaiService)
		{
			String idpSubjectId = extractor.SubjectString(currentPrincipalResolverService.CurrentPrincipal());
			if(String.IsNullOrEmpty(idpSubjectId))
			{
				await _next(context);
				return;
			}

			User user = dbContext.Users.FirstOrDefault(x=> x.IdpSubjectId == idpSubjectId);
			if (user == null)
			{
				user = new User() { Id = Guid.NewGuid(), IdpSubjectId = idpSubjectId, CreatedAt = DateTime.UtcNow, };
				dbContext.Users.Add(user);
			}

			String name = extractor.Name(currentPrincipalResolverService.CurrentPrincipal());
			String email = extractor.Email(currentPrincipalResolverService.CurrentPrincipal());
			String clientId = extractor.Client(currentPrincipalResolverService.CurrentPrincipal());
			String nameToUse = String.IsNullOrEmpty(name) ? clientId : name;
			logger.Information("synching user {idpSubjectId} with effectiveName {nameToUse}, name {name}, email {email}, client {clientId}", idpSubjectId, nameToUse, name, email, clientId);

			Boolean hasUpdate = false;
			if (!String.Equals(user.Name, nameToUse)) { user.Name = nameToUse; hasUpdate = true; }
			if (!String.Equals(user.Email, email)) { user.Email = email; hasUpdate = true; }

			if (hasUpdate)
			{
				user.UpdatedAt = DateTime.UtcNow;
				try
				{
					await dbContext.SaveChangesAsync();
				}
				catch (System.Exception ex)
				{
					logger.Warning(ex, "error synching user {idpSubjectId}. assuming race condition and continuing", idpSubjectId);
				}
			}

			await aaiService.BootstrapUserContextGrants(idpSubjectId);

			await _next(context);
		}
	}
}
