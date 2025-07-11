using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging.Extensions;
using Cite.Tools.Logging;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class InDataExplorationTextToSqlBuilder : Builder<InDataTextToSqlExploration, Service.InDataExploration.Model.ExplorationTextToSqlResponse>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public InDataExplorationTextToSqlBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<InDataExplorationTextToSqlBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
		}

		public InDataExplorationTextToSqlBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override async Task<List<InDataTextToSqlExploration>> Build(IFieldSet fields, IEnumerable<Service.InDataExploration.Model.ExplorationTextToSqlResponse> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.InDataExploration.Model.ExplorationTextToSqlResponse)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<InDataTextToSqlExploration>().ToList();

			List<InDataTextToSqlExploration> models = new List<InDataTextToSqlExploration>();
			foreach (Service.InDataExploration.Model.ExplorationTextToSqlResponse d in datas ?? new List<Service.InDataExploration.Model.ExplorationTextToSqlResponse>())
			{
				InDataTextToSqlExploration m = new InDataTextToSqlExploration();
				if (fields.HasField(nameof(InDataTextToSqlExploration.Sql))) m.Sql = d.Sql;

				models.Add(m);
			}
			return models;

		}


	}
}
