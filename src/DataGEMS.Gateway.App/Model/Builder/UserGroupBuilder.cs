
using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class UserGroupBuilder : Builder<UserGroup, Service.AAI.Model.Group>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly DataGEMS.Gateway.App.Service.AAI.AAIConfig _aaiConfig;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public UserGroupBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver,
			ILogger<UserGroupBuilder> logger,
			Service.AAI.AAIConfig aAIConfig) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._authorizationContentResolver = authorizationContentResolver;
			this._aaiConfig = aAIConfig;
		}

		public UserGroupBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override Task<List<UserGroup>> Build(IFieldSet fields, IEnumerable<Service.AAI.Model.Group> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(App.Model.UserGroup)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Task.FromResult(Enumerable.Empty<UserGroup>().ToList());

			List<UserGroup> models = new List<UserGroup>();
			foreach (Service.AAI.Model.Group d in datas ?? new List<Service.AAI.Model.Group>())
			{
				UserGroup m = new UserGroup();
				if (fields.HasField(nameof(UserGroup.Id))) m.Id = d.Id;
				if (fields.HasField(nameof(UserGroup.Name))) m.Name = d.Name;
				if (fields.HasField(nameof(UserGroup.Semantics)))
				{
					m.Semantics = d.Attributes != null && d.Attributes.ContainsKey(this._aaiConfig.ContextSemanticsAttributeName) ?
						d.Attributes[this._aaiConfig.ContextSemanticsAttributeName] :
						null;
				}

				models.Add(m);
			}
			return Task.FromResult(models);
		}
	}
}
