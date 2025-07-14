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

				if (fields.HasField(nameof(m.Status))) m.Status = d.Status;
				if (fields.HasField(nameof(m.Message))) m.Message = d.Message;
				if (fields.HasField(nameof(m.Params)) && d.Params != null)
				{
					m.Params = new Params{Results = d.Params.Results != null ? new Results{Points = d.Params.Results.Points?
						.Select(p => p.ToList()).ToList()} : null};
				}
				if (fields.HasField(nameof(m.Question))) m.Question = d.Question;
				if (fields.HasField(nameof(m.ModelName))) m.ModelName = d.ModelName;
				if (fields.HasField(nameof(m.SqlPattern))) m.SqlPattern = d.SqlPattern;
				if (fields.HasField(nameof(m.InputParams)) && d.InputParams != null)
				{
					m.InputParams = d.InputParams
						.Select(ip => new InputParam{Coordinates = ip.Coordinates?.Select(c => new CoordinateTuple{Tuple = c.Tuple?.ToList()}).ToList()}).ToList();
				}
				if (fields.HasField(nameof(m.OutputParams)) && d.OutputParams != null)
				{
					m.OutputParams = new OutputParams{Coordinates = d.OutputParams.Coordinates?.ToList()};
				}
				if (fields.HasField(nameof(m.Reasoning))) m.Reasoning = d.Reasoning;
				if (fields.HasField(nameof(m.SqlQuery))) m.SqlQuery = d.SqlQuery;
				if (fields.HasField(nameof(m.SqlResults)) && d.SqlResults != null)
				{
					m.SqlResults = new SqlResults{Status = d.SqlResults.Status, Message = d.SqlResults.Message};
				}

				models.Add(m);
			}
			return models;

		}


	}
}
