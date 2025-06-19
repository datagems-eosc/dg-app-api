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
	public class UserCollectionBuilder : Builder<UserCollection, Data.UserCollection>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public UserCollectionBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver,
			ILogger<UserCollectionBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		public UserCollectionBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override async Task<List<UserCollection>> Build(IFieldSet fields, IEnumerable<Data.UserCollection> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(App.Model.UserCollection)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<UserCollection>().ToList();

			IFieldSet userDatasetCollectionFields = fields.ExtractPrefixed(this.AsPrefix(nameof(UserCollection.UserDatasetCollections)));
			Dictionary<Guid, List<UserDatasetCollection>> userDatasetCollectionMap = await this.CollectUserDatasetCollections(userDatasetCollectionFields, datas);

			IFieldSet userFields = fields.ExtractPrefixed(this.AsPrefix(nameof(UserCollection.User)));
			Dictionary<Guid, User> userMap = await this.CollectUsers(userFields, datas);

			List<UserCollection> models = new List<UserCollection>();
			foreach (Data.UserCollection d in datas ?? new List<Data.UserCollection>())
			{
				UserCollection m = new UserCollection();
				if (fields.HasField(nameof(UserCollection.ETag))) m.ETag = d.UpdatedAt.ToETag();
				if (fields.HasField(nameof(UserCollection.Id))) m.Id = d.Id;
				if (fields.HasField(nameof(UserCollection.Name))) m.Name = d.Name;
				if (fields.HasField(nameof(UserCollection.CreatedAt))) m.CreatedAt = d.CreatedAt;
				if (fields.HasField(nameof(UserCollection.UpdatedAt))) m.UpdatedAt = d.UpdatedAt;
				if (!userDatasetCollectionFields.IsEmpty() && userDatasetCollectionMap != null && userDatasetCollectionMap.ContainsKey(d.Id)) m.UserDatasetCollections = userDatasetCollectionMap[d.Id];
				if (!userFields.IsEmpty() && userMap != null && userMap.ContainsKey(d.UserId)) m.User = userMap[d.UserId];
			}
			return models;
		}

		private async Task<Dictionary<Guid, List<UserDatasetCollection>>> CollectUserDatasetCollections(IFieldSet fields, IEnumerable<Data.UserCollection> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.UserDatasetCollection)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<Guid, List<UserDatasetCollection>> itemMap = null;
			IFieldSet clone = new FieldSet(fields.Fields).Ensure(this.AsIndexer(nameof(UserDatasetCollection.UserCollection), nameof(UserCollection.Id)));
			UserDatasetCollectionQuery query = this._queryFactory.Query<UserDatasetCollectionQuery>().DisableTracking().IsActive(Common.IsActive.Active).UserCollectionIds(datas.Select(x => x.Id).Distinct()).Authorize(this._authorize);
			itemMap = await this._builderFactory.Builder<UserDatasetCollectionBuilder>().Authorize(this._authorize).AsMasterKey(query, clone, x => x.UserCollection.Id.Value);

			if (!fields.HasField(this.AsIndexer(nameof(UserDatasetCollection.UserCollection), nameof(UserCollection.Id)))) itemMap.SelectMany(x => x.Value).Where(x => x != null && x.UserCollection != null).ToList().ForEach(x => x.UserCollection.Id = null);

			return itemMap;
		}

		private async Task<Dictionary<Guid, User>> CollectUsers(IFieldSet fields, IEnumerable<Data.UserCollection> datas)
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
