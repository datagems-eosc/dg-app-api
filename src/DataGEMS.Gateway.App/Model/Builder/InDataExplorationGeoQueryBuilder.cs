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
	public class InDataExplorationGeoQueryBuilder : Builder<InDataGeoQueryExploration, Service.InDataExploration.Model.ExplorationGeoQueryResponse>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public InDataExplorationGeoQueryBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<InDataExplorationGeoQueryBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
		}

		public InDataExplorationGeoQueryBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }


		public override async Task<List<InDataGeoQueryExploration>> Build(IFieldSet fields, IEnumerable<Service.InDataExploration.Model.ExplorationGeoQueryResponse> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.InDataExploration.Model.ExplorationGeoQueryResponse)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Enumerable.Empty<InDataGeoQueryExploration>().ToList();

			List<InDataGeoQueryExploration> models = new List<InDataGeoQueryExploration>();
			foreach (Service.InDataExploration.Model.ExplorationGeoQueryResponse d in datas ?? new List<Service.InDataExploration.Model.ExplorationGeoQueryResponse>())
			{
				InDataGeoQueryExploration m = new InDataGeoQueryExploration();
				if (fields.HasField(nameof(InDataGeoQueryExploration.Place))) m.Place = d.Place;
				if (fields.HasField(nameof(InDataGeoQueryExploration.Oql))) m.Oql = d.Oql;
				if (fields.HasField(nameof(InDataGeoQueryExploration.MostRelevantWikidata)) && d.MostRelevantWikidata != null)
				{
					m.MostRelevantWikidata = new WikidataInfo
					{
						Id = d.MostRelevantWikidata.Id,
						Label = d.MostRelevantWikidata.Label,
						Description = d.MostRelevantWikidata.Description
					};
				}
				if (fields.HasField(nameof(InDataGeoQueryExploration.Results)) && d.Results != null)
				{
					m.Results = new GeoQueryResults
					{
						Points = d.Results.Points,
						BBox = d.Results.BBox,
						Centroid = d.Results.Centroid,
						Multipolygons = d.Results.Multipolygons
					};
				}

				models.Add(m);
			}
			return models;

		}

	}
}
