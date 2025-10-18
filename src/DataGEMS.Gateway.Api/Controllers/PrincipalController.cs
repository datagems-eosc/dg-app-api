using Cite.Tools.Common.Extensions;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using Cite.WebTools.CurrentPrincipal;
using DataGEMS.Gateway.Api.Model;
using DataGEMS.Gateway.Api.OpenApi;
using DataGEMS.Gateway.Api.Transaction;
using DataGEMS.Gateway.Api.Validation;
using DataGEMS.Gateway.App.Accounting;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Auth;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Service.AAI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace DataGEMS.Gateway.Api.Controllers
{
	[Route("api/principal")]
	public class PrincipalController : ControllerBase
	{
		private readonly ICurrentPrincipalResolverService _currentPrincipalResolverService;
		private readonly ILogger<PrincipalController> _logger;
		private readonly AccountBuilder _accountBuilder;
		private readonly IAAIService _aaiService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly App.Authorization.IAuthorizationService _authorizationService;
		private readonly ErrorThesaurus _errors;
		private readonly IAccountingService _accountingService;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

		public PrincipalController(
			ILogger<PrincipalController> logger,
			ICurrentPrincipalResolverService currentPrincipalResolverService,
			IAuthorizationContentResolver authorizationContentResolver,
			App.Authorization.IAuthorizationService authorizationService,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
			IAccountingService accountingService,
			IAAIService aaiService,
			ErrorThesaurus errors,
			AccountBuilder accountBuilder)
		{
			this._logger = logger;
			this._currentPrincipalResolverService = currentPrincipalResolverService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._authorizationService = authorizationService;
			this._accountingService = accountingService;
			this._accountBuilder = accountBuilder;
			this._aaiService = aaiService;
			this._errors = errors;
			this._localizer = localizer;
		}

		[HttpGet("me")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Retrieve information for the logged in user")]
		[SwaggerResponse(statusCode: 200, description: "The information available for the logged in user", type: typeof(Account))]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<Account> Me(
			[ModelBinder(Name = "f")]
			[SwaggerParameter(description: "The fields to include in the response model", Required = false)]
			[LookupFieldSetQueryStringOpenApi]
			IFieldSet fieldSet)
		{
			this._logger.Debug(new MapLogEntry("me").And("fields", fieldSet));
			if (fieldSet == null || fieldSet.IsEmpty()) fieldSet = new FieldSet(
				nameof(Account.IsAuthenticated),
				nameof(Account.Roles),
				nameof(Account.Permissions),
				nameof(Account.DeferredPermissions),
				nameof(Account.More),
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

		[HttpGet("me/context-grants")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Retrieve the assigned context grants for the logged in user")]
		[SwaggerResponse(statusCode: 200, description: "The context grants available for the logged in user", type: typeof(List<App.Common.Auth.ContextGrant>))]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<List<App.Common.Auth.ContextGrant>> ContextGrantsMe()
		{
			this._logger.Debug(new MapLogEntry("context-grants").And("subject", "me"));

			String subjectId = await this._authorizationContentResolver.SubjectIdOfCurrentUser();
			List<App.Common.Auth.ContextGrant> grants = await this._aaiService.UserContextGrants(subjectId);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.ContextGroupAssignment.AsAccountable());

			return grants;
		}

		[HttpGet("me/context-grants/collection")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Retrieve the assigned context grants for the logged in user relevant to the provided collections")]
		[SwaggerResponse(statusCode: 200, description: "The context grants available for the logged in user for the provided collection", type: typeof(Dictionary<String, HashSet<String>>))]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<Dictionary<Guid, HashSet<String>>> ContextGrantsCollectionMe(
			[FromQuery]
			[SwaggerParameter(description: "The collection id to retrieve context grants for", Required = true)]
			Guid[] id)
		{
			this._logger.Debug(new MapLogEntry("context-grants").And("subject", "me").And("target", "collection").And("id", id));

			if (id == null || id.Length == 0) return new Dictionary<Guid, HashSet<string>>();
			Dictionary<Guid, HashSet<String>> grants = await this._authorizationContentResolver.ContextRolesForCollection(id);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.ContextGroupAssignment.AsAccountable());

			return grants;
		}

		[HttpGet("me/context-grants/dataset")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Retrieve the assigned context grants for the logged in user relevant to the provided datasets")]
		[SwaggerResponse(statusCode: 200, description: "The context grants available for the logged in user for the provided dataset", type: typeof(Dictionary<String, HashSet<String>>))]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<Dictionary<Guid, HashSet<String>>> ContextGrantsDatasetMe(
			[FromQuery]
			[SwaggerParameter(description: "The dataset id to retrieve context grants for", Required = true)]
			Guid[] id)
		{
			this._logger.Debug(new MapLogEntry("context-grants").And("subject", "me").And("target", "dataset").And("id", id));

			if (id == null || id.Length == 0) return new Dictionary<Guid, HashSet<string>>();
			Dictionary<Guid, HashSet<String>> grants = await this._authorizationContentResolver.EffectiveContextRolesForDataset(id);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.ContextGroupAssignment.AsAccountable());

			return grants;
		}

		[HttpGet("{subjectId}/context-grants")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Retrieve the assigned context grants for the provided user")]
		[SwaggerResponse(statusCode: 200, description: "The context grants available for the logged in user", type: typeof(List<App.Common.Auth.ContextGrant>))]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<List<App.Common.Auth.ContextGrant>> ContextGrantsOther(
			[FromRoute]
			[SwaggerParameter(description: "The subject id of the user to retrieve context grants for", Required = true)]
			String subjectId)
		{
			this._logger.Debug(new MapLogEntry("context-grants").And("subject", subjectId));

			await this._authorizationService.AuthorizeForce(Permission.LookupContextGrantOther);

			List<App.Common.Auth.ContextGrant> grants = await this._aaiService.UserContextGrants(subjectId);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.ContextGroupAssignment.AsAccountable());

			return grants;
		}

		[HttpGet("{subjectId}/context-grants/collection")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Retrieve the assigned context grants for the provided user relevant to the provided collections")]
		[SwaggerResponse(statusCode: 200, description: "The context grants available for the provided user for the provided collection", type: typeof(Dictionary<String, HashSet<String>>))]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<Dictionary<Guid, HashSet<String>>> ContextGrantsCollectionOther(
			[FromRoute]
			[SwaggerParameter(description: "The subject id of the user to retrieve context grants for", Required = true)]
			String subjectId,
			[FromQuery]
			[SwaggerParameter(description: "The collection id to retrieve context grants for", Required = true)]
			Guid[] id)
		{
			this._logger.Debug(new MapLogEntry("context-grants").And("subject", subjectId).And("target", "collection").And("id", id));

			await this._authorizationService.AuthorizeForce(Permission.LookupContextGrantOther);

			if (id == null || id.Length == 0) return new Dictionary<Guid, HashSet<string>>();
			Dictionary<Guid, HashSet<String>> grants = await this._authorizationContentResolver.ContextRolesForCollection(subjectId, id);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.ContextGroupAssignment.AsAccountable());

			return grants;
		}

		[HttpGet("{subjectId}/context-grants/dataset")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Retrieve the assigned context grants for the provided user relevant to the provided datasets")]
		[SwaggerResponse(statusCode: 200, description: "The context grants available for the provided user for the provided dataset", type: typeof(Dictionary<String, HashSet<String>>))]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<Dictionary<Guid, HashSet<String>>> ContextGrantsDatasetOther(
			[FromRoute]
			[SwaggerParameter(description: "The subject id of the user to retrieve context grants for", Required = true)]
			String subjectId,
			[FromQuery]
			[SwaggerParameter(description: "The dataset id to retrieve context grants for", Required = true)]
			Guid[] id)
		{
			this._logger.Debug(new MapLogEntry("context-grants").And("subject", subjectId).And("target", "dataset").And("id", id));

			await this._authorizationService.AuthorizeForce(Permission.LookupContextGrantOther);

			if (id == null || id.Length == 0) return new Dictionary<Guid, HashSet<string>>();
			Dictionary<Guid, HashSet<String>> grants = await this._authorizationContentResolver.EffectiveContextRolesForDataset(subjectId, id);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.ContextGroupAssignment.AsAccountable());

			return grants;
		}

		[HttpGet("context-grants/of/{code}")]
		[Authorize]
		[ModelStateValidationFilter]
		[SwaggerOperation(Summary = "Retrieve the available context grants for the provided code (collection or dataset id)")]
		[SwaggerResponse(statusCode: 200, description: "The context grants available for the provided code", type: typeof(List<App.Common.Auth.ContextGrant>))]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task<List<App.Common.Auth.ContextGrant>> ContextGrantsMe(
			[FromRoute]
			[SwaggerParameter(description: "The subject id of the user to retrieve context grants for", Required = true)]
			String code)
		{
			this._logger.Debug(new MapLogEntry("context-grants").And("of", code));

			await this._authorizationService.AuthorizeForce(Permission.LookupContextGrantGroups);

			List<App.Common.Auth.ContextGrant> grants = await this._aaiService.LookupContextGrantGroups(code);

			this._accountingService.AccountFor(KnownActions.Query, KnownResources.ContextGroup.AsAccountable());

			return grants;
		}

		[HttpPost("context-grants/user/{userId}/group/{groupId}")]
		[Authorize]
		[ModelStateValidationFilter]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Add user to context grant group")]
		[SwaggerResponse(statusCode: 200, description: "The user was added to the context grant group")]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task AddUserToContextGrantGroup(
			[FromRoute]
			[SwaggerParameter(description: "The user id to add to the provided context grant group", Required = true)]
			Guid userId,
			[FromRoute]
			[SwaggerParameter(description: "The context grant group to add", Required = true)]
			String groupId)
		{
			this._logger.Debug(new MapLogEntry("adding").And("userId", userId).And("groupId", groupId));

			String subjectId = await this._authorizationContentResolver.SubjectIdOfUserId(userId);
			if (String.IsNullOrEmpty(subjectId)) throw new DGValidationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			ContextGrantGroupTarget assignmentTarget = await this._aaiService.TargetOfContextGrantGroup(groupId);
			if (assignmentTarget == null) throw new DGNotFoundException(this._localizer["general_notFound", groupId, nameof(ContextGrant)]);

			if (!Guid.TryParse(assignmentTarget.Code, out var targetId)) throw new DGValidationException(this._errors.ModelValidation.Code, this._errors.ModelValidation.Message);
			HashSet<string> contextRoles = null;
			switch (assignmentTarget.Type)
			{
				case ContextGrant.TargetType.Dataset: { contextRoles = await _authorizationContentResolver.EffectiveContextRolesForDataset(targetId); break; }
				case ContextGrant.TargetType.Group: { contextRoles = await _authorizationContentResolver.ContextRolesForCollection(targetId); break; }
				default: throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI);
			}
			await this._authorizationService.AuthorizeOrAffiliatedContextForce(new AffiliatedContextResource(contextRoles), Permission.AddUserToContextGrantGroup);

			await this._aaiService.AddUserToContextGrantGroup(subjectId, groupId);

			this._accountingService.AccountFor(KnownActions.Persist, KnownResources.ContextGroupAssignment.AsAccountable());
		}

		[HttpDelete("context-grants/user/{userId}/group/{groupId}")]
		[Authorize]
		[ModelStateValidationFilter]
		[ServiceFilter(typeof(AppTransactionFilter))]
		[SwaggerOperation(Summary = "Remove user from context grant group")]
		[SwaggerResponse(statusCode: 200, description: "The user was removed from the context grant group")]
		[SwaggerResponse(statusCode: 400, description: "Validation problem with the request")]
		[SwaggerResponse(statusCode: 401, description: "The request is not authenticated")]
		[SwaggerResponse(statusCode: 403, description: "The requested operation is not permitted based on granted permissions")]
		[SwaggerResponse(statusCode: 500, description: "Internal error")]
		[SwaggerResponse(statusCode: 503, description: "An underpinning service indicated failure")]
		[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
		public async Task RemoveUserFromContextGrantGroup(
			[FromRoute]
			[SwaggerParameter(description: "The user id to remove from the provided context grant group", Required = true)]
			Guid userId,
			[FromRoute]
			[SwaggerParameter(description: "The context grant group to remove from", Required = true)]
			String groupId)
		{
			this._logger.Debug(new MapLogEntry("removing").And("userId", userId).And("groupId", groupId));

			String subjectId = await this._authorizationContentResolver.SubjectIdOfUserId(userId);
			if (String.IsNullOrEmpty(subjectId)) throw new DGValidationException(this._errors.UserSync.Code, this._errors.UserSync.Message);

			ContextGrantGroupTarget assignmentTarget = await this._aaiService.TargetOfContextGrantGroup(groupId);
			if (assignmentTarget == null) throw new DGNotFoundException(this._localizer["general_notFound", groupId, nameof(ContextGrant)]);

			if (!Guid.TryParse(assignmentTarget.Code, out var targetId)) throw new DGValidationException(this._errors.ModelValidation.Code, this._errors.ModelValidation.Message);
			HashSet<string> contextRoles = null;
			switch (assignmentTarget.Type)
			{
				case ContextGrant.TargetType.Dataset: { contextRoles = await _authorizationContentResolver.EffectiveContextRolesForDataset(targetId); break; }
				case ContextGrant.TargetType.Group: { contextRoles = await _authorizationContentResolver.ContextRolesForCollection(targetId); break; }
				default: throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.AAI);
			}
			await this._authorizationService.AuthorizeOrAffiliatedContextForce(new AffiliatedContextResource(contextRoles), Permission.RemoveUserFromContextGrantGroup);

			await this._aaiService.RemoveUserFromContextGrantGroup(subjectId, groupId);

			this._accountingService.AccountFor(KnownActions.Delete, KnownResources.ContextGroupAssignment.AsAccountable());
		}
	}
}
