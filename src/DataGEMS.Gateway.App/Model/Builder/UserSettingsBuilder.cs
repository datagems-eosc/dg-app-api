using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class UserSettingsBuilder : Builder<UserSettings, Data.UserSettings>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public UserSettingsBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver,
			ILogger<UserSettingsBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		public UserSettingsBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override async Task<List<UserSettings>> Build(IFieldSet fields, IEnumerable<Data.UserSettings> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(App.Model.UserSettings)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<UserSettings>().ToList();

			IFieldSet userFields = fields.ExtractPrefixed(this.AsPrefix(nameof(UserSettings.User)));
			Dictionary<Guid, User> userMap = await this.CollectUsers(userFields, datas);

			List<UserSettings> models = new List<UserSettings>();
			foreach (Data.UserSettings d in datas ?? new List<Data.UserSettings>())
			{
				UserSettings m = new UserSettings();
				if (fields.HasField(nameof(UserSettings.ETag))) m.ETag = d.UpdatedAt.ToETag();
				if (fields.HasField(nameof(UserSettings.Id))) m.Id = d.Id;
				if (fields.HasField(nameof(UserSettings.Key))) m.Key = d.Key;
				if (fields.HasField(nameof(UserSettings.Value))) m.Value = d.Value;
				if (fields.HasField(nameof(UserSettings.CreatedAt))) m.CreatedAt = d.CreatedAt;
				if (fields.HasField(nameof(UserSettings.UpdatedAt))) m.UpdatedAt = d.UpdatedAt;
				if (!userFields.IsEmpty() && userMap != null && userMap.ContainsKey(d.UserId)) m.User = userMap[d.UserId];

				models.Add(m);
			}
			return models;
		}

		private async Task<Dictionary<Guid, User>> CollectUsers(IFieldSet fields, IEnumerable<Data.UserSettings> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.User)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<Guid, User> itemMap = null;
			if (!fields.HasOtherField(this.AsIndexer(nameof(User.Id)))) itemMap = this.AsEmpty(datas.Select(x => x.UserId).Distinct(), x => new User() { Id = x }, x => x.Id.Value);
			else
			{
				IFieldSet clone = new FieldSet(fields.Fields).Ensure(nameof(User.Id));
				UserQuery q = this._queryFactory.Query<UserQuery>().DisableTracking().Ids(datas.Select(x => x.UserId).Distinct()).Authorize(this._authorize);
				itemMap = await this._builderFactory.Builder<UserBuilder>().Authorize(this._authorize).AsForeignKey(q, clone, x => x.Id.Value);
			}
			if (!fields.HasField(nameof(User.Id))) itemMap.Values.Where(x => x != null).ToList().ForEach(x => x.Id = null);

			return itemMap;
		}
	}
}
