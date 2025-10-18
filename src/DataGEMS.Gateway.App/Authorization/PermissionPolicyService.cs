using Cite.Tools.Common.Extensions;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Authorization
{
	public class PermissionPolicyService : IPermissionPolicyService
	{
		private static ISet<String> _emptyRoleSet = new HashSet<String>();
		private static ISet<String> _emptyContextRoleSet = new HashSet<String>();
		private static ISet<String> _emptyClaimSet = new HashSet<String>();
		private static ISet<String> _emptyClientSet = new HashSet<String>();
		private static IList<String> _emptyRoleList = new List<String>();
		private static IList<String> _emptyContextRoleList = new List<String>();
		private static IList<String> _emptyClaimList = new List<String>();
		private static IList<String> _emptyClientList = new List<String>();

		private readonly PermissionPolicyConfig _config;
		private readonly ILogger<PermissionPolicyService> _logger;
		private Dictionary<String, HashSet<String>> _permissionRoleMap;
		private Dictionary<String, HashSet<String>> _permissionContextRoleMap;
		private Dictionary<String, Dictionary<String, HashSet<String>>> _permissionClaimMap;
		private Dictionary<String, HashSet<String>> _permissionClientMap;
		private Dictionary<String, Boolean> _permissionAnonymousMap;
		private Dictionary<String, Boolean> _permissionAuthenticatedMap;
		private Dictionary<String, HashSet<String>> _rolePermissionsMap;
		private Dictionary<String, HashSet<String>> _contextRolePermissionsMap;
		private Dictionary<String, Dictionary<String, HashSet<String>>> _claimPermissionsMap;

		public PermissionPolicyService(
			PermissionPolicyConfig config,
			ILogger<PermissionPolicyService> logger)
		{
			this._logger = logger;
			this._config = config;

			this._logger.Trace(new DataLogEntry("config", this._config));
			this.Refresh();
		}

		private void Refresh()
		{
			this._permissionRoleMap = new Dictionary<String, HashSet<String>>();
			foreach (var policyEntry in this._config.Policies)
			{
				if (!this._permissionRoleMap.ContainsKey(policyEntry.Key)) this._permissionRoleMap.Add(policyEntry.Key, new HashSet<String>());
				this._permissionRoleMap[policyEntry.Key].AddRange(policyEntry.Value.Roles ?? PermissionPolicyService._emptyRoleList);
			}
			this._permissionContextRoleMap = new Dictionary<String, HashSet<String>>();
			foreach (var policyEntry in this._config.Policies)
			{
				if (!this._permissionContextRoleMap.ContainsKey(policyEntry.Key)) this._permissionContextRoleMap.Add(policyEntry.Key, new HashSet<String>());
				this._permissionContextRoleMap[policyEntry.Key].AddRange(policyEntry.Value.ContextRoles ?? PermissionPolicyService._emptyContextRoleList);
			}
			this._permissionClaimMap = new Dictionary<String, Dictionary<String, HashSet<String>>>();
			foreach (var policyEntry in this._config.Policies)
			{
				if (!this._permissionClaimMap.ContainsKey(policyEntry.Key)) this._permissionClaimMap.Add(policyEntry.Key, new Dictionary<string, HashSet<string>>());
				if (policyEntry.Value.Claims == null || policyEntry.Value.Claims.Count == 0) continue;
				foreach (PermissionPolicyConfig.PermissionClaims policyClaim in policyEntry.Value.Claims)
				{
					if (!this._permissionClaimMap[policyEntry.Key].ContainsKey(policyClaim.Claim)) this._permissionClaimMap[policyEntry.Key].Add(policyClaim.Claim, new HashSet<string>());
					this._permissionClaimMap[policyEntry.Key][policyClaim.Claim].AddRange(policyClaim.Values ?? PermissionPolicyService._emptyClaimList);
				}
			}
			this._permissionClientMap = new Dictionary<String, HashSet<String>>();
			foreach (var policyEntry in this._config.Policies)
			{
				if (!this._permissionClientMap.ContainsKey(policyEntry.Key)) this._permissionClientMap.Add(policyEntry.Key, new HashSet<String>());
				this._permissionClientMap[policyEntry.Key].AddRange(policyEntry.Value.Clients ?? PermissionPolicyService._emptyClientList);
			}
			this._permissionAnonymousMap = new Dictionary<String, Boolean>();
			foreach (var policyEntry in this._config.Policies)
			{
				if (!this._permissionAnonymousMap.ContainsKey(policyEntry.Key)) this._permissionAnonymousMap.Add(policyEntry.Key, policyEntry.Value.AllowAnonymous);
				//if for the same permission we have multiple declerations, keep the most restrictive
				else this._permissionAnonymousMap[policyEntry.Key] = this._permissionAnonymousMap[policyEntry.Key] && policyEntry.Value.AllowAnonymous;
			}
			this._permissionAuthenticatedMap = new Dictionary<String, Boolean>();
			foreach (var policyEntry in this._config.Policies)
			{
				if (!this._permissionAuthenticatedMap.ContainsKey(policyEntry.Key)) this._permissionAuthenticatedMap.Add(policyEntry.Key, policyEntry.Value.AllowAuthenticated);
				//if for the same permission we have multiple declerations, keep the most restrictive
				else this._permissionAuthenticatedMap[policyEntry.Key] = this._permissionAuthenticatedMap[policyEntry.Key] && policyEntry.Value.AllowAuthenticated;
			}
			this._rolePermissionsMap = new Dictionary<String, HashSet<String>>();
			foreach (var policyEntry in this._config.Policies)
			{
				if (policyEntry.Value.Roles == null || policyEntry.Value.Roles.Count == 0) continue;
				foreach (String role in policyEntry.Value.Roles)
				{
					if (!this._rolePermissionsMap.ContainsKey(role)) this._rolePermissionsMap.Add(role, new HashSet<String>());
					this._rolePermissionsMap[role].Add(policyEntry.Key);
				}
			}
			this._contextRolePermissionsMap = new Dictionary<String, HashSet<String>>();
			foreach (var policyEntry in this._config.Policies)
			{
				if (policyEntry.Value.ContextRoles == null || policyEntry.Value.ContextRoles.Count == 0) continue;
				foreach (String contextRole in policyEntry.Value.ContextRoles)
				{
					if (!this._contextRolePermissionsMap.ContainsKey(contextRole)) this._contextRolePermissionsMap.Add(contextRole, new HashSet<String>());
					this._contextRolePermissionsMap[contextRole].Add(policyEntry.Key);
				}
			}
			this._claimPermissionsMap = new Dictionary<String, Dictionary<String, HashSet<String>>>();
			foreach (var policyEntry in this._config.Policies)
			{
				if (policyEntry.Value.Claims == null || policyEntry.Value.Claims.Count == 0) continue;
				foreach (PermissionPolicyConfig.PermissionClaims claimPolicy in policyEntry.Value.Claims)
				{
					if (!this._claimPermissionsMap.ContainsKey(claimPolicy.Claim)) this._claimPermissionsMap.Add(claimPolicy.Claim, new Dictionary<string, HashSet<string>>());
					foreach (String claimValue in claimPolicy.Values)
					{
						if (!this._claimPermissionsMap[claimPolicy.Claim].ContainsKey(claimValue)) this._claimPermissionsMap[claimPolicy.Claim].Add(claimValue, new HashSet<string>());
						this._claimPermissionsMap[claimPolicy.Claim][claimValue].Add(policyEntry.Key);
					}
				}
			}
		}

		public ISet<String> PermissionsOf(IEnumerable<String> roles)
		{
			HashSet<String> permissions = new HashSet<String>();
			if (roles == null || !roles.Any()) return permissions;

			foreach (String role in roles)
			{
				if (!this._rolePermissionsMap.ContainsKey(role)) continue;
				permissions.UnionWith(this._rolePermissionsMap[role]);
			}
			return permissions;
		}

		public ISet<String> PermissionsOfContext(IEnumerable<String> contextRoles)
		{
			HashSet<String> permissions = new HashSet<String>();
			if (contextRoles == null || !contextRoles.Any()) return permissions;

			foreach (String contextRole in contextRoles)
			{
				if (!this._contextRolePermissionsMap.ContainsKey(contextRole)) continue;
				permissions.UnionWith(this._contextRolePermissionsMap[contextRole]);
			}
			return permissions;
		}

		public ISet<String> RolesHaving(String permission)
		{
			if (!this._permissionRoleMap.ContainsKey(permission)) return PermissionPolicyService._emptyRoleSet;
			return this._permissionRoleMap[permission];
		}

		public ISet<String> ContextRolesHaving(String permission)
		{
			if (!this._permissionContextRoleMap.ContainsKey(permission)) return PermissionPolicyService._emptyContextRoleSet;
			return this._permissionContextRoleMap[permission];
		}

		public ISet<String> ClaimsHaving(String claim, String permission)
		{
			if (!this._permissionClaimMap.ContainsKey(permission)) return PermissionPolicyService._emptyClaimSet;
			if (!this._permissionClaimMap[permission].ContainsKey(claim)) return PermissionPolicyService._emptyClaimSet;
			return this._permissionClaimMap[permission][claim];
		}

		public ISet<String> ClientsHaving(String permission)
		{
			if (!this._permissionClientMap.ContainsKey(permission)) return PermissionPolicyService._emptyClientSet;
			return this._permissionClientMap[permission];
		}

		public Boolean AllowAnonymous(String permission)
		{
			if (!this._permissionAnonymousMap.ContainsKey(permission)) return false;
			return this._permissionAnonymousMap[permission];
		}

		public Boolean AllowAuthenticated(String permission)
		{
			if (!this._permissionAuthenticatedMap.ContainsKey(permission)) return false;
			return this._permissionAuthenticatedMap[permission];
		}
	}
}
