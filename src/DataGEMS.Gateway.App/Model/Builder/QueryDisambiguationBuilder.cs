using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class QueryDisambiguationBuilder : Builder<QueryDisambiguationViewModel, QueryDisambiguation>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public QueryDisambiguationBuilder(
			ILogger<QueryDisambiguationBuilder> logger,
			QueryFactory queryFactory,
			BuilderFactory builderFactory) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
		}

		public QueryDisambiguationBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override Task<List<QueryDisambiguationViewModel>> Build(IFieldSet fields, IEnumerable<QueryDisambiguation> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(QueryDisambiguationViewModel)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Task.FromResult(Enumerable.Empty<QueryDisambiguationViewModel>().ToList());

			List<QueryDisambiguationViewModel> models = [];
			foreach (QueryDisambiguation query in datas ?? [])
			{
				QueryDisambiguationViewModel m = new QueryDisambiguationViewModel();

				if (fields.HasField(nameof(QueryDisambiguationViewModel.Results))) m.Results = query.Results;
				if (fields.HasField(nameof(QueryDisambiguationViewModel.Metadata))) m.Metadata = query.Metadata;

				models.Add(m);
			}

			return Task.FromResult(models);
		}
	}
}
