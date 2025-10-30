using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Common;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace DataGEMS.Gateway.App.Service.Storage
{
	public interface IStorageService
	{
		Task<List<String>> AllowedExtensions();
		Task<long> MaxFileUploadSize();
		Task<String> PersistAsync(StorageFile model, String payload, Encoding encoding);
		Task<String> PersistAsync(StorageFile model, byte[] payload);
		Task<String> PersistAsync(StorageFile model, IFormFile formFile);
		Task<String> PersistZipAsync(StorageFile model, String payload, Encoding encoding);
		Task<String> PersistZipAsync(StorageFile model, byte[] payload);
		Task<String> MoveToStorage(String filePath, StorageType type, String subDirectory = null);
		Task<byte[]> ReadByteSafeAsync(String path);
	}

	public class StorageFile
	{
		public String Name { get; set; }
		public StorageType StorageType { get; set; }
		public String Extension { get; set; }
		public String MimeType { get; set; }
	}
}
