using Cite.Tools.Auth.Claims;
using Cite.Tools.Common.Extensions;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common.Auth;
using System.Security.Claims;

namespace DataGEMS.Gateway.Api.Model
{
	public class Account
	{
		public class PrincipalInfo
		{
			public String Subject { get; set; }
			[LogSensitive]
			public String Name { get; set; }
			[LogSensitive]
			public String Username { get; set; }
			[LogSensitive]
			public String GivenName { get; set; }
			[LogSensitive]
			public String FamilyName { get; set; }
			[LogSensitive]
			public String Email { get; set; }
		}

		public class TokenInfo
		{
			public String Client { get; set; }
			public String Issuer { get; set; }
			public String TokenType { get; set; }
			public String AuthorizedParty { get; set; }
			public List<String> Audience { get; set; }
			public DateTime? ExpiresAt { get; set; }
			public DateTime? IssuedAt { get; set; }
			public List<String> Scope { get; set; }
		}

		public Boolean? IsAuthenticated { get; set; }
		public PrincipalInfo Principal { get; set; }
		public TokenInfo Token { get; set; }
		public List<String> Roles { get; set; }
		public List<String> Permissions { get; set; }
		public List<String> DeferredPermissions { get; set; }
		[LogSensitive]
		public Dictionary<String, List<String>> More { get; set; }
	}

	public class AccountBuilder
	{
		private readonly IPermissionPolicyService _permissionPolicyService;
		private readonly ClaimExtractor _extractor;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public AccountBuilder(
			IPermissionPolicyService permissionPolicyService,
			ClaimExtractor extractor,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._permissionPolicyService = permissionPolicyService;
			this._extractor = extractor;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		public async Task<Account> Build(IFieldSet fields, ClaimsPrincipal principal)
		{
			Account model = new Account();

			Boolean isAuthenticated = principal != null;
			if(fields.HasField(nameof(Account.IsAuthenticated))) model.IsAuthenticated = isAuthenticated;
			if(!isAuthenticated) return model;

			IFieldSet principalFields = fields.ExtractPrefixed(nameof(Account.Principal).AsIndexerPrefix());
			if (!principalFields.IsEmpty()) model.Principal = new Account.PrincipalInfo();
			if (principalFields.HasField(nameof(Account.Principal.Subject))) model.Principal.Subject = this._extractor.SubjectString(principal);
			if (principalFields.HasField(nameof(Account.Principal.Name))) model.Principal.Name = this._extractor.Name(principal);
			if (principalFields.HasField(nameof(Account.Principal.Username))) model.Principal.Username = this._extractor.PreferredUsername(principal);
			if (principalFields.HasField(nameof(Account.Principal.GivenName))) model.Principal.GivenName = this._extractor.GivenName(principal);
			if (principalFields.HasField(nameof(Account.Principal.FamilyName))) model.Principal.FamilyName = this._extractor.FamilyName(principal);
			if (principalFields.HasField(nameof(Account.Principal.Email))) model.Principal.Email = this._extractor.Email(principal);

			IFieldSet tokenFields = fields.ExtractPrefixed(nameof(Account.Token).AsIndexerPrefix());
			if (!tokenFields.IsEmpty()) model.Token = new Account.TokenInfo();
			if (tokenFields.HasField(nameof(Account.Token.Client))) model.Token.Client = this._extractor.Client(principal);
			if (tokenFields.HasField(nameof(Account.Token.Issuer))) model.Token.Issuer = this._extractor.Issuer(principal);
			if (tokenFields.HasField(nameof(Account.Token.TokenType))) model.Token.TokenType = this._extractor.TokenType(principal);
			if (tokenFields.HasField(nameof(Account.Token.AuthorizedParty))) model.Token.AuthorizedParty = this._extractor.AuthorizedParty(principal);
			if (tokenFields.HasField(nameof(Account.Token.Audience))) model.Token.Audience = this._extractor.Audience(principal);
			if (tokenFields.HasField(nameof(Account.Token.ExpiresAt))) model.Token.ExpiresAt = this._extractor.ExpiresAt(principal);
			if (tokenFields.HasField(nameof(Account.Token.IssuedAt))) model.Token.IssuedAt = this._extractor.IssuedAt(principal);
			if (tokenFields.HasField(nameof(Account.Token.Scope))) model.Token.Scope = this._extractor.Scope(principal);

			if (fields.HasField(nameof(Account.Permissions))) model.Permissions = new List<string>(this._permissionPolicyService.PermissionsOf(this._extractor.Roles(principal)));
			if (fields.HasField(nameof(Account.DeferredPermissions)))
			{
				Guid? userId = _extractor.SubjectGuid(principal);
				if (userId.HasValue)
				{
					List<string> datasetRoles = await _authorizationContentResolver.ContextRolesOf();
					model.DeferredPermissions = new List<string>(_permissionPolicyService.PermissionsOfContext(datasetRoles));
				}
			}
			if (fields.HasField(nameof(Account.Roles))) model.Roles = this._extractor.Roles(principal);

			return model;
		}
	}
}
