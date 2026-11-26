using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Labels = DataGEMS.Gateway.App.Common.AnalyticalPatternLabels;
using M = DataGEMS.Gateway.App.Model.DeconstructedDatasetAnalyticalPattern;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class DeconstructedDatasetAnalyticalPatternBuilder : PrimitiveBuilder<DeconstructedDatasetAnalyticalPattern, AnalyticalPattern>
	{
		public DeconstructedDatasetAnalyticalPatternBuilder(
			ILogger<DeconstructedDatasetAnalyticalPatternBuilder> logger) : base(logger) { }

		public override Task<List<DeconstructedDatasetAnalyticalPattern>> Build(IFieldSet fields, IEnumerable<AnalyticalPattern> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(DeconstructedDatasetAnalyticalPattern)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Task.FromResult(Enumerable.Empty<DeconstructedDatasetAnalyticalPattern>().ToList());

			IFieldSet datasetFields = fields.ExtractPrefixed(this.AsPrefix(nameof(M.Dataset)));
			IFieldSet distributionFields = fields.ExtractPrefixed(this.AsPrefix(nameof(M.Distributions)));
			IFieldSet recordSetFields = fields.ExtractPrefixed(this.AsPrefix(nameof(M.RecordSets)));

			List<DeconstructedDatasetAnalyticalPattern> models = new List<DeconstructedDatasetAnalyticalPattern>();
			foreach (AnalyticalPattern d in datas ?? Enumerable.Empty<AnalyticalPattern>())
			{
				if (d == null) continue;
				Graph graph = new Graph(d);
				AnalyticalPatternNode datasetNode = graph.Nodes.FirstOrDefault(x => x.HasLabel(Labels.Node.Dataset));

				DeconstructedDatasetAnalyticalPattern m = new DeconstructedDatasetAnalyticalPattern();
				if (!datasetFields.IsEmpty() && datasetNode != null) m.Dataset = this.BuildDataset(datasetFields, datasetNode);
				if (!distributionFields.IsEmpty()) m.Distributions = this.BuildDistributions(distributionFields, graph, datasetNode);
				if (!recordSetFields.IsEmpty()) m.RecordSets = this.BuildRecordSets(recordSetFields, graph, datasetNode);

				models.Add(m);
			}
			return Task.FromResult(models);
		}

		private M.DatasetInfo BuildDataset(IFieldSet fields, AnalyticalPatternNode node)
		{
			M.DatasetInfo m = new M.DatasetInfo();
			if (fields.HasField(nameof(M.DatasetInfo.Id))) m.Id = node.Id;
			if (fields.HasField(nameof(M.DatasetInfo.Labels))) m.Labels = node.Labels?.ToList();
			if (fields.HasField(nameof(M.DatasetInfo.Name))) m.Name = node.PropertyAsString("name");
			if (fields.HasField(nameof(M.DatasetInfo.Description))) m.Description = node.PropertyAsString("description");
			if (fields.HasField(nameof(M.DatasetInfo.Headline))) m.Headline = node.PropertyAsString("headline");
			if (fields.HasField(nameof(M.DatasetInfo.License))) m.License = node.PropertyAsString("license");
			if (fields.HasField(nameof(M.DatasetInfo.Url))) m.Url = node.PropertyAsString("url");
			if (fields.HasField(nameof(M.DatasetInfo.CiteAs))) m.CiteAs = node.PropertyAsString("citeAs");
			if (fields.HasField(nameof(M.DatasetInfo.Doi))) m.Doi = node.PropertyAsString("doi");
			if (fields.HasField(nameof(M.DatasetInfo.Version))) m.Version = node.PropertyAsString("version");
			if (fields.HasField(nameof(M.DatasetInfo.ConformsTo))) m.ConformsTo = node.PropertyAsString("conformsTo");
			if (fields.HasField(nameof(M.DatasetInfo.Access))) m.Access = node.PropertyAsString("access");
			if (fields.HasField(nameof(M.DatasetInfo.Status))) m.Status = node.PropertyAsString("status");
			if (fields.HasField(nameof(M.DatasetInfo.ArchivedAt))) m.ArchivedAt = node.PropertyAsString("archivedAt");
			if (fields.HasField(nameof(M.DatasetInfo.UploadedBy))) m.UploadedBy = node.PropertyAsGuid("uploadedBy");
			if (fields.HasField(nameof(M.DatasetInfo.DatePublished))) m.DatePublished = node.PropertyAsDateOnly("datePublished");
			if (fields.HasField(nameof(M.DatasetInfo.Keywords))) m.Keywords = node.PropertyAsStringList("keywords");
			if (fields.HasField(nameof(M.DatasetInfo.FieldOfScience))) m.FieldOfScience = node.PropertyAsStringList("fieldOfScience");
			if (fields.HasField(nameof(M.DatasetInfo.Language))) m.Language = node.PropertyAsStringList("inLanguage");
			if (fields.HasField(nameof(M.DatasetInfo.Country))) m.Country = node.PropertyAsStringList("country");
			return m;
		}

		private List<M.Distribution> BuildDistributions(IFieldSet fields, Graph graph, AnalyticalPatternNode datasetNode)
		{
			//linked from the dataset first (keeps the AP order), then any distribution-like node left unlinked
			IEnumerable<AnalyticalPatternNode> linked = datasetNode == null ? Enumerable.Empty<AnalyticalPatternNode>() : graph.Targets(datasetNode.Id, Labels.Edge.Distribution);
			IEnumerable<AnalyticalPatternNode> byLabel = graph.Nodes.Where(x => this.DistributionKindOf(x) != M.DistributionKind.Other);
			List<AnalyticalPatternNode> nodes = linked.Concat(byLabel).DistinctBy(x => x.Id).ToList();

			List<M.Distribution> models = new List<M.Distribution>();
			foreach (AnalyticalPatternNode node in nodes)
			{
				M.Distribution m = new M.Distribution();
				if (fields.HasField(nameof(M.Distribution.Id))) m.Id = node.Id;
				if (fields.HasField(nameof(M.Distribution.Kind))) m.Kind = this.DistributionKindOf(node);
				if (fields.HasField(nameof(M.Distribution.Labels))) m.Labels = node.Labels?.ToList();
				if (fields.HasField(nameof(M.Distribution.Type))) m.Type = node.PropertyAsString("type");
				if (fields.HasField(nameof(M.Distribution.Name))) m.Name = node.PropertyAsString("name");
				if (fields.HasField(nameof(M.Distribution.Description))) m.Description = node.PropertyAsString("description");
				if (fields.HasField(nameof(M.Distribution.ContentUrl))) m.ContentUrl = node.PropertyAsString("contentUrl");
				if (fields.HasField(nameof(M.Distribution.ContentSize))) m.ContentSize = node.PropertyAsString("contentSize");
				if (fields.HasField(nameof(M.Distribution.EncodingFormat))) m.EncodingFormat = node.PropertyAsString("encodingFormat");
				if (fields.HasField(nameof(M.Distribution.Sha256))) m.Sha256 = node.PropertyAsString("sha256");
				if (fields.HasField(nameof(M.Distribution.Includes))) m.Includes = node.PropertyAsString("includes");
				if (fields.HasField(nameof(M.Distribution.ContainedIn))) m.ContainedIn = graph.TargetIds(node.Id, Labels.Edge.ContainedIn);
				models.Add(m);
			}
			return models;
		}

		private M.DistributionKind DistributionKindOf(AnalyticalPatternNode node)
		{
			if (node.HasLabel(Labels.Node.DatabaseConnection)) return M.DistributionKind.DatabaseConnection;
			if (node.HasLabel(Labels.Node.FileSet)) return M.DistributionKind.FileSet;
			if (node.HasLabel(Labels.Node.FileObject)) return M.DistributionKind.FileObject;
			return M.DistributionKind.Other;
		}

		private List<M.RecordSet> BuildRecordSets(IFieldSet fields, Graph graph, AnalyticalPatternNode datasetNode)
		{
			IFieldSet fieldFields = fields.ExtractPrefixed(this.AsPrefix(nameof(M.RecordSet.Fields)));

			IEnumerable<AnalyticalPatternNode> linked = datasetNode == null ? Enumerable.Empty<AnalyticalPatternNode>() : graph.Targets(datasetNode.Id, Labels.Edge.RecordSet);
			IEnumerable<AnalyticalPatternNode> byLabel = graph.Nodes.Where(x => x.HasLabel(Labels.Node.RecordSet));
			List<AnalyticalPatternNode> nodes = linked.Concat(byLabel).DistinctBy(x => x.Id).ToList();

			List<M.RecordSet> models = new List<M.RecordSet>();
			foreach (AnalyticalPatternNode node in nodes)
			{
				M.RecordSet m = new M.RecordSet();
				if (fields.HasField(nameof(M.RecordSet.Id))) m.Id = node.Id;
				if (fields.HasField(nameof(M.RecordSet.Labels))) m.Labels = node.Labels?.ToList();
				if (fields.HasField(nameof(M.RecordSet.Name))) m.Name = node.PropertyAsString("name");
				if (fields.HasField(nameof(M.RecordSet.Examples))) m.Examples = this.ParseExamples(node.PropertyAsJToken("examples"));
				if (!fieldFields.IsEmpty()) m.Fields = graph.Targets(node.Id, Labels.Edge.Field).Select(x => this.BuildField(fieldFields, graph, x)).ToList();
				models.Add(m);
			}
			return models;
		}

		private Dictionary<String, List<Object>> ParseExamples(JToken token)
		{
			if (token is not JObject obj) return null;
			return obj.Properties().ToDictionary(x => x.Name, x => AnalyticalPatternExtensions.ToPlainList(x.Value));
		}

		private M.Field BuildField(IFieldSet fields, Graph graph, AnalyticalPatternNode node)
		{
			M.Field m = new M.Field();
			if (fields.HasField(nameof(M.Field.Id))) m.Id = node.Id;
			if (fields.HasField(nameof(M.Field.Labels))) m.Labels = node.Labels?.ToList();
			if (fields.HasField(nameof(M.Field.Name))) m.Name = node.PropertyAsString("name");
			if (fields.HasField(nameof(M.Field.Column))) m.Column = node.PropertyAsString("column");
			if (fields.HasField(nameof(M.Field.Description))) m.Description = node.PropertyAsString("description");
			if (fields.HasField(nameof(M.Field.DataType))) m.DataType = node.PropertyAsString("dataType");
			if (fields.HasField(nameof(M.Field.SemanticType))) m.SemanticType = node.PropertyAsString("semanticType");
			if (fields.HasField(nameof(M.Field.Sample))) m.Sample = AnalyticalPatternExtensions.ToPlainList(node.PropertyAsJToken("sample"));
			if (fields.HasField(nameof(M.Field.Sources))) m.Sources = graph.TargetIds(node.Id, Labels.Edge.SourceFileObject);
			return m;
		}

		private class Graph
		{
			private readonly Dictionary<Guid, AnalyticalPatternNode> _nodesById;
			private readonly Dictionary<Guid, List<AnalyticalPatternEdge>> _edgesFrom;

			public List<AnalyticalPatternNode> Nodes { get; }

			public Graph(AnalyticalPattern ap)
			{
				this.Nodes = ap.Nodes?.Where(x => x != null).DistinctBy(x => x.Id).ToList() ?? [];
				this._nodesById = this.Nodes.ToDictionary(x => x.Id);
				this._edgesFrom = (ap.Edges ?? []).Where(x => x != null).GroupBy(x => x.From).ToDictionary(x => x.Key, x => x.ToList());
			}

			//nodes reached from the given node over edges with the given label, in edge order, without duplicates
			public IEnumerable<AnalyticalPatternNode> Targets(Guid from, String edgeLabel)
			{
				if (!this._edgesFrom.TryGetValue(from, out List<AnalyticalPatternEdge> edges)) return Enumerable.Empty<AnalyticalPatternNode>();
				return edges
					.Where(x => x.HasLabel(edgeLabel) && this._nodesById.ContainsKey(x.To))
					.Select(x => this._nodesById[x.To])
					.DistinctBy(x => x.Id)
					.ToList();
			}

			//ids reached from the given node over edges with the given label, including ids not present as nodes
			public List<Guid> TargetIds(Guid from, String edgeLabel)
			{
				if (!this._edgesFrom.TryGetValue(from, out List<AnalyticalPatternEdge> edges)) return [];
				return edges.Where(x => x.HasLabel(edgeLabel)).Select(x => x.To).Distinct().ToList();
			}
		}
	}
}
