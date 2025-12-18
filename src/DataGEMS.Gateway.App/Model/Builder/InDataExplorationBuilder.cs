using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public class InDataExplorationBuilder : Builder<InDataExplore, Service.InDataExploration.Model.InDataExplorationResponse>
	{
		private readonly QueryFactory _queryFactory;
		private readonly BuilderFactory _builderFactory;

		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public InDataExplorationBuilder(
			QueryFactory queryFactory,
			BuilderFactory builderFactory,
			ILogger<InDataExplorationBuilder> logger) : base(logger)
		{
			this._queryFactory = queryFactory;
			this._builderFactory = builderFactory;
		}

		public InDataExplorationBuilder Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		public override Task<List<InDataExplore>> Build(IFieldSet fields, IEnumerable<Service.InDataExploration.Model.InDataExplorationResponse> datas)
		{
			this._logger.Debug(new MapLogEntry("building").And("type", nameof(Service.InDataExploration.Model.InDataExplorationResponse)).And("fields", fields).And("dataCount", datas?.Count()));
			if (fields == null || fields.IsEmpty()) return Task.FromResult(Enumerable.Empty<InDataExplore>().ToList());

			List<InDataExplore> models = new List<InDataExplore>();
			foreach (Service.InDataExploration.Model.InDataExplorationResponse d in datas ?? new List<Service.InDataExploration.Model.InDataExplorationResponse>())
			{
				InDataExplore m = new InDataExplore();

				if (fields.HasField(nameof(InDataExplore.Question))) m.Question = d.Question;
				if (fields.HasField(nameof(InDataExplore.Data))) m.Data = new Dictionary<string, object>() { { nameof(d.InputParams), d.InputParams } };
				if (fields.HasField(nameof(InDataExplore.Status))) m.Status = this.ToResponseStatus(d);
				if (fields.HasField(nameof(InDataExplore.Entries))) m.Entries = new List<InDataExploreEntry>() { new InDataExploreEntry() { Status = this.ToEntryStatus(d), Process = this.BuildProcessEntry(d), Result = this.BuildResultEntry(d) } };

				models.Add(m);
			}
			return Task.FromResult(models);
		}

		private InDataExploreProcessEntry BuildProcessEntry(Service.InDataExploration.Model.InDataExplorationResponse item)
		{
			if (!String.IsNullOrEmpty(item.SqlPattern) ||
				!String.IsNullOrEmpty(item.SqlQuery) ||
				!String.IsNullOrEmpty(item.Reasoning)) return new InDataExploreProcessSqlEntry() { Sql = new InDataExploreProcessSqlEntry.SqlGeneration() { Pattern = item.SqlPattern, Reasoning = item.Reasoning, Query = item.SqlQuery } };
			else return new InDataExploreProcessNoneEntry();
		}

		private InDataExploreResultEntry BuildResultEntry(Service.InDataExploration.Model.InDataExplorationResponse item)
		{
			Boolean isSuccess = item?.SqlResults?.Status.Equals("success", StringComparison.OrdinalIgnoreCase) ?? false;
			if (!isSuccess || item.SqlResults.Data == null || item.SqlResults.Data.Count == 0) return new InDataExploreResultNoneEntry() { Message = item?.SqlResults?.Message };

			InDataExploreResultTableEntry table = new InDataExploreResultTableEntry() { Message = item.SqlResults.Message, Table = new InDataExploreResultTableEntry.TableInfo() };

			int columnCounter = 0;
			table.Table.Columns = item.SqlResults.Data.SelectMany(x => x.Keys).Distinct().Select(x => new InDataExploreResultTableEntry.TableInfo.Column() { ColumnNumber = columnCounter++, Name = x }).ToList();

			int rowCounter = 0;
			table.Table.Rows = item.SqlResults.Data.Select(x => new InDataExploreResultTableEntry.TableInfo.Row() { RowNumber = rowCounter++, Cells = x.Select(y => new InDataExploreResultTableEntry.TableInfo.Cell() { Column = y.Key, Value = y.Value }).ToList() }).ToList();

			return table;
		}

		private InDataExplore.ResponseStatus ToResponseStatus(Service.InDataExploration.Model.InDataExplorationResponse item)
		{
			Boolean isSuccess = item?.SqlResults?.Status.Equals("success", StringComparison.OrdinalIgnoreCase) ?? false;
			return isSuccess ? InDataExplore.ResponseStatus.Success : InDataExplore.ResponseStatus.Error;
		}

		private InDataExploreEntry.EntryStatus ToEntryStatus(Service.InDataExploration.Model.InDataExplorationResponse item)
		{
			Boolean isSuccess = item?.SqlResults?.Status.Equals("success", StringComparison.OrdinalIgnoreCase) ?? false;
			return isSuccess ? InDataExploreEntry.EntryStatus.Success : InDataExploreEntry.EntryStatus.Error;
		}
	}
}
