using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Service.QueryRecommender.Model;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class QueryRecommenderBuilder : Builder<QueryRecommendation, String>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public QueryRecommenderBuilder(ILogger<QueryRecommenderBuilder> logger,
			QueryFactory queryFactory,
			BuilderFactory builderFactory
			) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
		}

		public QueryRecommenderBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override async Task<List<QueryRecommendation>> Build(IFieldSet fields, IEnumerable<String> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(QueryRecommendation)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<QueryRecommendation>().ToList();

			List<QueryRecommendation> models = [];
			foreach (var query in datas)
			{
				QueryRecommendation m = new QueryRecommendation();
				if (fields.HasField(nameof(QueryRecommendation.Query))) m.Query = query;
				models.Add(m);
			}

			return models;
		}

	}
}
