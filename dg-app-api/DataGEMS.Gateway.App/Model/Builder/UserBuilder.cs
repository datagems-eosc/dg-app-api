
using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class UserBuilder : Builder<User, Data.User>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public UserBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			IAuthorizationContentResolver authorizationContentResolver,
			ILogger<UserBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		public UserBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override async Task<List<User>> Build(IFieldSet fields, IEnumerable<Data.User> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(App.Model.User)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<User>().ToList();

			IFieldSet userCollectionFields = fields.ExtractPrefixed(this.AsPrefix(nameof(User.UserCollections)));
			Dictionary<Guid, List<UserCollection>> userCollectioneMap = await this.CollectUserCollections(userCollectionFields, datas);

			List<User> models = new List<User>();
			foreach (Data.User d in datas ?? new List<Data.User>())
			{
				User m = new User();
				if (fields.HasField(nameof(User.ETag))) m.ETag = d.UpdatedAt.ToETag();
				if (fields.HasField(nameof(User.Id))) m.Id = d.Id;
				if (fields.HasField(nameof(User.Name))) m.Name = d.Name;
				if (fields.HasField(nameof(User.Email))) m.Email = d.Email;
				if (fields.HasField(nameof(User.IdpSubjectId))) m.IdpSubjectId = d.IdpSubjectId;
				if (fields.HasField(nameof(User.CreatedAt))) m.CreatedAt = d.CreatedAt;
				if (fields.HasField(nameof(User.UpdatedAt))) m.UpdatedAt = d.UpdatedAt;
				if (!userCollectionFields.IsEmpty() && userCollectioneMap != null && userCollectioneMap.ContainsKey(d.Id)) m.UserCollections = userCollectioneMap[d.Id];
			}
			return models;
		}

		private async Task<Dictionary<Guid, List<UserCollection>>> CollectUserCollections(IFieldSet fields, IEnumerable<Data.User> datas)
		{
			if (fields.IsEmpty() || !datas.Any()) return null;
			this._logger.Debug(new MapLogEntry("building related").And("type", nameof(App.Model.UserCollection)).And("fields", fields).And("dataCount", datas?.Count()));

			Dictionary<Guid, List<UserCollection>> itemMap = null;
			IFieldSet clone = new FieldSet(fields.Fields).Ensure(this.AsIndexer(nameof(UserCollection.User), nameof(User.Id)));
			UserCollectionQuery query = this._queryFactory.Query<UserCollectionQuery>().DisableTracking().IsActive(Common.IsActive.Active).UserIds(datas.Select(x => x.Id).Distinct()).Authorize(this._authorize);
			itemMap = await this._builderFactory.Builder<UserCollectionBuilder>().Authorize(this._authorize).AsMasterKey(query, clone, x => x.User.Id.Value);

			if (!fields.HasField(this.AsIndexer(nameof(UserCollection.User), nameof(User.Id)))) itemMap.SelectMany(x => x.Value).Where(x => x != null && x.User != null).ToList().ForEach(x => x.User.Id = null);

			return itemMap;
		}
	}
}
