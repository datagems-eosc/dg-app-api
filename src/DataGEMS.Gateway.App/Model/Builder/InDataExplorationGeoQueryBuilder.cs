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
				if (fields.HasField(nameof(InDataGeoQueryExploration.MostRelevantWikidata)))
				{
					m.MostRelevantWikidata = d.MostRelevantWikidata != null ? new Dictionary<string, object>(d.MostRelevantWikidata) : null;
				}
				if (fields.HasField(nameof(InDataGeoQueryExploration.Oql)) && d.Oql != null) m.Oql = new OqlInfo{Reasoning = d.Oql.Reasoning, OqlText = d.Oql.OqlText};
				if (fields.HasField(nameof(InDataGeoQueryExploration.Results)) && d.Results != null)
					m.Results = new GeoQueryResults
					{
						Points = d.Results.Points?.Select(p => new Point { Lon = p.Lon, Lat = p.Lat }).ToList(),
						GeoJsonData = d.Results.GeoJsonData != null ? new Dictionary<string, object>(d.Results.GeoJsonData) : null,
						Bounds = d.Results.Bounds != null ? new Bounds
						{
							MinLat = d.Results.Bounds.MinLat,
							MinLon = d.Results.Bounds.MinLon,
							MaxLat = d.Results.Bounds.MaxLat,
							MaxLon = d.Results.Bounds.MaxLon
						} : null,
						Center = d.Results.Center != null ?	new List<decimal>(d.Results.Center) : null
					};

				models.Add(m);
			}
			return models;

		}
	}
}
