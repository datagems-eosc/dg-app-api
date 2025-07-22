using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class InDataExplorationSimpleExploreBuilder : Builder<InDataSimpleExploreExploration, Service.InDataExploration.Model.ExplorationSimpleExploreResponse>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public InDataExplorationSimpleExploreBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<InDataExplorationSimpleExploreBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
		}

		public InDataExplorationSimpleExploreBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }


		public override async Task<List<InDataSimpleExploreExploration>> Build(IFieldSet fields, IEnumerable<Service.InDataExploration.Model.ExplorationSimpleExploreResponse> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.InDataExploration.Model.ExplorationSimpleExploreResponse)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<InDataSimpleExploreExploration>().ToList();

			List<InDataSimpleExploreExploration> models = new List<InDataSimpleExploreExploration>();
			foreach (Service.InDataExploration.Model.ExplorationSimpleExploreResponse d in datas ?? new List<Service.InDataExploration.Model.ExplorationSimpleExploreResponse>())
			{
				InDataSimpleExploreExploration m = new InDataSimpleExploreExploration();

				if (fields.HasField(nameof(InDataSimpleExploreExploration.Question))) m.Question = d.Question;
				if (fields.HasField(nameof(InDataSimpleExploreExploration.SqlPattern))) m.SqlPattern = d.SqlPattern;
				if (fields.HasField(nameof(InDataSimpleExploreExploration.InputParams)) && d.InputParams != null)
				{
					m.InputParams = d.InputParams.Select(p => new SimpleExploreInputParam{Lon = p.Lon, Lat = p.Lat}).ToList();
				}
				if (fields.HasField(nameof(InDataSimpleExploreExploration.Reasoning))) m.Reasoning = d.Reasoning;
				if (fields.HasField(nameof(InDataSimpleExploreExploration.SqlQuery))) m.SqlQuery = d.SqlQuery;
				if (fields.HasField(nameof(InDataSimpleExploreExploration.SqlResults)) && d.SqlResults != null)
				{
					m.SqlResults = new SimpleExploreSqlResults
					{
						Status = d.SqlResults.Status,
						Data = d.SqlResults.Data != null ? new List<Dictionary<string, object>>(d.SqlResults.Data) : null
					};
				}

				models.Add(m);
			}
			return models;
		}
	}
}
