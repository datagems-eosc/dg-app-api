
using Cite.Tools.Json.Inflater;
using System.ComponentModel;
using Newtonsoft.Json;

namespace DataGEMS.Gateway.App.Model
{
	public class InDataExplore
	{
		// GOTCHA: Any changes to this model should cause the version to change
		public static String ModelVersion = "V1";

		public String Question { get; set; }

		public Dictionary<String, Object> Data { get; set; }

		public ResponseStatus Status { get; set; }

		public List<InDataExploreEntry> Entries { get; set; }

		public enum ResponseStatus : short
		{
			[Description("Successful evaluation")]
			Success = 0,
			[Description("Problem in evaluation")]
			Error = 1,
		}
	}

	public class InDataExploreEntry
	{
		public InDataExploreProcessEntry Process { get; set; }
		public InDataExploreResultEntry Result { get; set; }

		public EntryStatus Status { get; set; }

		public enum EntryStatus : short
		{
			[Description("Successful evaluation")]
			Success = 0,
			[Description("Problem in evaluation")]
			Error = 1,
		}
	}

	[JsonConverter(typeof(SubTypeConverter))]
	[SubTypeConverterAnchor(nameof(InDataExploreProcessEntry.Kind), typeof(InDataExploreProcessEntry.InDataExploreProcessKind))]
	[SubTypeConverterMap(InDataExploreProcessEntry.InDataExploreProcessKind.None, typeof(InDataExploreProcessNoneEntry))]
	[SubTypeConverterMap(InDataExploreProcessEntry.InDataExploreProcessKind.Sql, typeof(InDataExploreProcessSqlEntry))]
	public abstract class InDataExploreProcessEntry
	{
		public enum InDataExploreProcessKind : short
		{
			[Description("None")]
			None = 0,
			[Description("Sql")]
			Sql = 1,
		}

		public abstract InDataExploreProcessKind Kind { get; }
	}

	public class InDataExploreProcessNoneEntry : InDataExploreProcessEntry
	{
		public override InDataExploreProcessKind Kind => InDataExploreProcessKind.None;
	}

	public class InDataExploreProcessSqlEntry : InDataExploreProcessEntry
	{
		public override InDataExploreProcessKind Kind => InDataExploreProcessKind.Sql;

		public SqlGeneration Sql { get; set; }

		public class SqlGeneration
		{
			public String Reasoning { get; set; }
			public String Pattern { get; set; }
			public String Query { get; set; }
		}
	}

	[JsonConverter(typeof(SubTypeConverter))]
	[SubTypeConverterAnchor(nameof(InDataExploreResultEntry.Kind), typeof(InDataExploreResultEntry.InDataExploreResultKind))]
	[SubTypeConverterMap(InDataExploreResultEntry.InDataExploreResultKind.None, typeof(InDataExploreResultNoneEntry))]
	[SubTypeConverterMap(InDataExploreResultEntry.InDataExploreResultKind.Table, typeof(InDataExploreResultTableEntry))]
	public abstract class InDataExploreResultEntry
	{
		public enum InDataExploreResultKind : short
		{
			[Description("None")]
			None = 0,
			[Description("Table")]
			Table = 1,
		}

		public abstract InDataExploreResultKind Kind { get; }
	}

	public class InDataExploreResultNoneEntry : InDataExploreResultEntry
	{
		public override InDataExploreResultKind Kind => InDataExploreResultKind.None;
	}

	public class InDataExploreResultTableEntry : InDataExploreResultEntry
	{
		public override InDataExploreResultKind Kind => InDataExploreResultKind.Table;

		public TableInfo Table { get; set; }

		public class TableInfo
		{
			public List<Column> Columns { get; set; }
			public List<Row> Rows { get; set; }

			public class Row
			{
				public int RowNumber { get; set; }
				public List<Cell> Cells { get; set; }
			}

			public class Cell
			{
				public String Column { get; set; }
				public Object Value { get; set; }
			}

			public class Column
			{
				public int ColumnNumber { get; set; }
				public String Name { get; set; }
			}
		}
	}
}
