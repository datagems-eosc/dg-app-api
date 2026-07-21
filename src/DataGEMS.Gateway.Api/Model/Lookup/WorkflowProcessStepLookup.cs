using Cite.Tools.Data.Query;
using DataGEMS.Gateway.App.Query;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class WorkflowProcessStepLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description: "Limit lookup to items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> Ids { get; set; }
		[SwaggerSchema(description: "Exclude from the lookup items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> ExcludedIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items belonging to specific process ids. If set, the list of ids must not be empty")]
		public List<Guid> ProcessIds { get; set; }

		public WorkflowProcessStepQuery Enrich(QueryFactory factory)
		{
			WorkflowProcessStepQuery query = factory.Query<WorkflowProcessStepQuery>();

			if (this.Ids != null) query.Ids(this.Ids);
			if (this.ProcessIds != null) query.ProcessIds(this.ProcessIds);
			if (this.ExcludedIds != null) query.ExcludedIds(this.ExcludedIds);

			this.EnrichCommon(query);
			return query;
		}
	}
}
