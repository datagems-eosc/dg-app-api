
using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;
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

		public override Task<List<User>> Build(IFieldSet fields, IEnumerable<Data.User> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(App.Model.User)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Task.FromResult(Enumerable.Empty<User>().ToList());

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

				models.Add(m);
			}
			return Task.FromResult(models);
		}
	}
}
