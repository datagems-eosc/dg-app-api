using Cite.Tools.Common.Extensions;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging.Extensions;
using Cite.WebTools.CurrentPrincipal;
using DataGEMS.Gateway.Api.Model;
using DataGEMS.Gateway.Api.Validation;
using DataGEMS.Gateway.App.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/principal")]
	public class PrincipalController : ControllerBase
	{
		private readonly ICurrentPrincipalResolverService _currentPrincipalResolverService;
		private readonly ILogger<PrincipalController> _logger;
		private readonly AccountBuilder _accountBuilder;

		public PrincipalController(
			ILogger<PrincipalController> logger,
			ICurrentPrincipalResolverService currentPrincipalResolverService,
			AccountBuilder accountBuilder)
		{
			this._logger = logger;
			this._currentPrincipalResolverService = currentPrincipalResolverService;
			this._accountBuilder = accountBuilder;
		}

		[HttpGet("me")]
		[Authorize]
		[ModelStateValidationFilter]
		public async Task<Account> Me([ModelBinder(Name = "f")] IFieldSet fieldSet)
		{
			this._logger.Debug("me");
			if (fieldSet == null || fieldSet.IsEmpty()) fieldSet = new FieldSet(
				nameof(Account.IsAuthenticated),
				nameof(Account.Roles),
				nameof(Account.Permissions),
				nameof(Account.DeferredPermissions),
				nameof(Account.More),
				nameof(Account.Datasets),
				nameof(Account.DatasetGrants),
				new String[] { nameof(Account.Principal), nameof(Account.PrincipalInfo.Subject) }.AsIndexer(),
				new String[] { nameof(Account.Principal), nameof(Account.PrincipalInfo.Name) }.AsIndexer(),
				new String[] { nameof(Account.Principal), nameof(Account.PrincipalInfo.Username) }.AsIndexer(),
				new String[] { nameof(Account.Principal), nameof(Account.PrincipalInfo.GivenName) }.AsIndexer(),
				new String[] { nameof(Account.Principal), nameof(Account.PrincipalInfo.FamilyName) }.AsIndexer(),
				new String[] { nameof(Account.Principal), nameof(Account.PrincipalInfo.Email) }.AsIndexer(),
				new String[] { nameof(Account.Token), nameof(Account.TokenInfo.Client) }.AsIndexer(),
				new String[] { nameof(Account.Token), nameof(Account.TokenInfo.Issuer) }.AsIndexer(),
				new String[] { nameof(Account.Token), nameof(Account.TokenInfo.TokenType) }.AsIndexer(),
				new String[] { nameof(Account.Token), nameof(Account.TokenInfo.AuthorizedParty) }.AsIndexer(),
				new String[] { nameof(Account.Token), nameof(Account.TokenInfo.Audience) }.AsIndexer(),
				new String[] { nameof(Account.Token), nameof(Account.TokenInfo.ExpiresAt) }.AsIndexer(),
				new String[] { nameof(Account.Token), nameof(Account.TokenInfo.IssuedAt) }.AsIndexer(),
				new String[] { nameof(Account.Token), nameof(Account.TokenInfo.Scope) }.AsIndexer());

			ClaimsPrincipal principal = this._currentPrincipalResolverService.CurrentPrincipal();

			Account me = await this._accountBuilder.Build(fieldSet, principal);

			return me;
		}
	}
}
