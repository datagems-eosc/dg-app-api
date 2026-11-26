using System.ComponentModel;

namespace DataGEMS.Gateway.App.Model
{
	/// <summary>
	/// Typed, hierarchical view of a dataset Analytical Pattern (profile graph).
	/// Dataset -> Distributions (files / sheets / tables / database connections)
	/// Dataset -> RecordSets -> Fields
	/// Every node carries the raw labels of its profile node, so consumers can tell node kinds apart.
	/// Column statistics and data quality nodes of the AP are intentionally ignored.
	/// </summary>
	[Description("The properties of a dataset, deconstructed from its profile (Analytical Pattern graph) into a typed hierarchy: the dataset metadata, its distributions (files, sheets, tables, database connections) and its record sets with their fields. Every item carries the labels of the profile node it was taken from, so that the kind of each node can be told apart. Column statistics and data quality findings present in the profile are not exposed.")]
	public class DeconstructedDatasetAnalyticalPattern
	{
		[Description("The dataset metadata, taken from the 'sc:Dataset' node of the profile")]
		public DatasetInfo Dataset { get; set; }
		[Description("The physical distributions of the dataset: every 'cr:FileObject', 'cr:FileSet' and 'dg:DatabaseConnection' node of the profile. Distributions linked to the dataset come first, in profile order, followed by any unlinked ones")]
		public List<Distribution> Distributions { get; set; }
		[Description("The logical record sets (tables / sheets) of the dataset: every 'cr:RecordSet' node of the profile. Record sets linked to the dataset come first, in profile order, followed by any unlinked ones")]
		public List<RecordSet> RecordSets { get; set; }

		[Description("The dataset metadata, as recorded in the dataset profile")]
		public class DatasetInfo
		{
			[Description("The id of the dataset")]
			public Guid? Id { get; set; }
			[Description("The raw labels of the profile node, identifying the kind of node (e.g. ['sc:Dataset'])")]
			public List<String> Labels { get; set; }
			[Description("The name of the dataset")]
			public String Name { get; set; }
			[Description("The full description of the dataset")]
			public String Description { get; set; }
			[Description("A short, one line summary of the dataset")]
			public String Headline { get; set; }
			[Description("The license under which the dataset is made available (e.g. 'CC BY 4.0')")]
			public String License { get; set; }
			[Description("A landing page for the dataset")]
			public String Url { get; set; }
			[Description("The preferred citation for the dataset")]
			public String CiteAs { get; set; }
			[Description("The DOI of the dataset, if one is assigned")]
			public String Doi { get; set; }
			[Description("The version of the dataset")]
			public String Version { get; set; }
			[Description("The standard or specification the dataset conforms to")]
			public String ConformsTo { get; set; }
			[Description("Access information of the dataset")]
			public String Access { get; set; }
			[Description("The processing status of the dataset (e.g. 'ready')")]
			public String Status { get; set; }
			[Description("The storage location where the dataset is archived")]
			public String ArchivedAt { get; set; }
			[Description("The id of the user that uploaded the dataset")]
			public Guid? UploadedBy { get; set; }
			[Description("The publication date of the dataset (yyyy-MM-dd)")]
			public DateOnly? DatePublished { get; set; }
			[Description("Free-text keywords describing the dataset")]
			public List<String> Keywords { get; set; }
			[Description("The fields of science the dataset relates to")]
			public List<String> FieldOfScience { get; set; }
			[Description("The languages of the dataset content (e.g. 'en', 'el')")]
			public List<String> Language { get; set; }
			[Description("The countries the dataset relates to (e.g. 'GR')")]
			public List<String> Country { get; set; }
		}

		[Description("The kind of a distribution, derived from the labels of its profile node")]
		public enum DistributionKind : short
		{
			[Description("Any other kind of distribution")]
			Other = 0,
			[Description("A single file or a part of one, such as a workbook sheet or a database table ('cr:FileObject')")]
			FileObject = 1,
			[Description("A set of files, typically a folder ('cr:FileSet')")]
			FileSet = 2,
			[Description("A connection to a database holding the dataset tables ('dg:DatabaseConnection')")]
			DatabaseConnection = 3,
		}

		[Description("A physical distribution of the dataset: a file, a part of a file (sheet), a file set (folder), a database table or a database connection")]
		public class Distribution
		{
			[Description("The id of the distribution node in the profile. File distributions can be downloaded through the storage endpoints using this id")]
			public Guid? Id { get; set; }
			[Description("The kind of the distribution")]
			public DistributionKind? Kind { get; set; }
			[Description("The raw labels of the profile node, identifying the kind of node (e.g. ['Data', 'cr:FileObject', 'Sheet'], ['Table', 'Data', 'cr:FileObject'] or ['RelationalDatabase', 'Data', 'dg:DatabaseConnection'])")]
			public List<String> Labels { get; set; }
			[Description("The type of the profile node (e.g. 'cr:FileObject')")]
			public String Type { get; set; }
			[Description("The name of the distribution (file name, sheet name, table name or database name)")]
			public String Name { get; set; }
			[Description("The description of the distribution")]
			public String Description { get; set; }
			[Description("The location of the distribution: a storage path for files, a qualified table name for database tables or a connection string for database connections")]
			public String ContentUrl { get; set; }
			[Description("The size of the content, as recorded in the profile (e.g. '24603 B')")]
			public String ContentSize { get; set; }
			[Description("The mime type of the content (e.g. 'text/csv', 'text/sql')")]
			public String EncodingFormat { get; set; }
			[Description("The SHA-256 checksum of the content, if available")]
			public String Sha256 { get; set; }
			[Description("For file sets, the pattern of the files included in the set")]
			public String Includes { get; set; }
			[Description("The ids of the distributions this one is contained in (e.g. a sheet in its workbook, a table in its database connection)")]
			public List<Guid> ContainedIn { get; set; }
		}

		[Description("A logical record set (table or sheet) of the dataset")]
		public class RecordSet
		{
			[Description("The id of the record set node in the profile")]
			public Guid? Id { get; set; }
			[Description("The raw labels of the profile node, identifying the kind of node (e.g. ['cr:RecordSet'])")]
			public List<String> Labels { get; set; }
			[Description("The name of the record set")]
			public String Name { get; set; }
			[Description("Example values per column, keyed by column name")]
			public Dictionary<String, List<Object>> Examples { get; set; }
			[Description("The fields (columns) of the record set, in profile order")]
			public List<Field> Fields { get; set; }
		}

		[Description("A field (column) of a record set")]
		public class Field
		{
			[Description("The id of the field node in the profile")]
			public Guid? Id { get; set; }
			[Description("The raw labels of the profile node, identifying the kind of node (e.g. ['Column', 'cr:Field'])")]
			public List<String> Labels { get; set; }
			[Description("The name of the field")]
			public String Name { get; set; }
			[Description("The name of the source column")]
			public String Column { get; set; }
			[Description("The description of the field")]
			public String Description { get; set; }
			[Description("The detected data type of the field (e.g. 'sc:Text', 'sc:Integer', 'sc:Float')")]
			public String DataType { get; set; }
			[Description("The detected semantic meaning of the field (e.g. 'identifier', 'geographic coordinate')")]
			public String SemanticType { get; set; }
			[Description("Sample values of the field")]
			public List<Object> Sample { get; set; }
			[Description("The ids of the distributions the field is sourced from")]
			public List<Guid> Sources { get; set; }
		}
	}
}
