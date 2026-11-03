using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class IntentDisambiguationBuilder : Builder<IntentDisambiguation, Service.IntentDisambiguation.Model.IntentDisambiguationResponse>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;
		public IntentDisambiguationBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<IntentDisambiguationBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
		}
		public IntentDisambiguationBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override Task<List<IntentDisambiguation>> Build(IFieldSet fields, IEnumerable<Service.IntentDisambiguation.Model.IntentDisambiguationResponse> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.IntentDisambiguation.Model.IntentDisambiguationResponse)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Task.FromResult(Enumerable.Empty<IntentDisambiguation>().ToList());
			List<IntentDisambiguation> models = [];
			foreach (Service.IntentDisambiguation.Model.IntentDisambiguationResponse d in datas ?? [])
			{
				IntentDisambiguation m = new IntentDisambiguation();
				if (fields.HasField(nameof(IntentDisambiguation.Intent))) m.Intent = d.Intent;
				if (fields.HasField(nameof(IntentDisambiguation.Confidence))) m.Confidence = d.Confidence;
				if (fields.HasField(nameof(IntentDisambiguation.SearchQuery))) m.SearchQuery = d.SearchQuery;
				if (fields.HasField(nameof(IntentDisambiguation.Degraded))) m.Degraded = d.Degraded;
				if (fields.HasField(nameof(IntentDisambiguation.Model))) m.Model = d.Model;
				if (fields.HasField(nameof(IntentDisambiguation.PromptVersion))) m.PromptVersion = d.PromptVersion;
				models.Add(m);
			}
			return Task.FromResult(models);
		}
	}
}
