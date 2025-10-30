using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Service.Storage
{
	public class StorageConfig
	{
		public IEnumerable<StorageTypeConfig> Storages { get; set; }
		public FileRestrictions UploadRules { get; set; }
	}

	public class FileRestrictions
	{
		public long MaxFileSize { get; set; }
		public List<string> AllowedExtensions { get; set; }
	}

	public class FileConventions
	{
		public String NamePattern { get; set; }
	}

	public class StorageTypeConfig
	{
		public StorageType Type { get; set; }
		public String BasePath { get; set; }
		public String SubPath { get; set; }
		public FileConventions NamingConventions { get; set; }
	}
}
