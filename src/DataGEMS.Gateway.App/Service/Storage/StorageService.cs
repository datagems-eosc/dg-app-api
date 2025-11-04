using Cite.Tools.FieldSet;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Exception;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.Storage
{
	public class StorageService : IStorageService
	{
		private readonly StorageConfig _config;
		private readonly ILogger<StorageService> _logger;

		public StorageService(
			StorageConfig config, 
			ILogger<StorageService> logger)
		{
			this._config = config;
			this._logger = logger;

			this.Bootstrap();
		}

		private void Bootstrap()
		{
			if (this._config.Storages != null)
			{
				foreach (StorageTypeConfig storage in _config.Storages)
				{
					DirectoryInfo dir = new DirectoryInfo(storage.BasePath);
					if (!dir.Exists) dir.Create();

					DirectoryInfo subDir = new DirectoryInfo(Path.Combine(storage.BasePath, storage.SubPath));
					if (!subDir.Exists) subDir.Create();
				}
			}
		}

		public Task<List<String>> AllowedExtensions()
		{
			return Task.FromResult(this._config.UploadRules.AllowedExtensions);
		}
		
		public Task<long> MaxFileUploadSize()
		{
			return Task.FromResult(this._config.UploadRules.MaxFileSize);
		}

		public async Task<String> PersistAsync(StorageFile model, String payload, Encoding encoding)
		{
			byte[] bytes = encoding.GetBytes(payload);
			return await this.PersistAsync(model, bytes);

		}
		public async Task<String> PersistAsync(StorageFile model, byte[] payload)
		{
			String path = this.FilePath(model);
			await File.WriteAllBytesAsync(path, payload);
			return path;
		}

		public async Task<String> PersistAsync(StorageFile model, IFormFile formFile)
		{
			String path = this.FilePath(model);
			using FileStream fs = new FileStream(path, FileMode.CreateNew);
			await formFile.CopyToAsync(fs);
			return path;
		}

		public async Task<String> PersistZipAsync(StorageFile model, String payload, Encoding encoding)
		{
			byte[] bytes = encoding.GetBytes(payload);
			return await this.PersistZipAsync(model, bytes);
		}

		public Task<String> PersistZipAsync(StorageFile model, byte[] payload)
		{
			String nameWithExtension = this.AppendExtension(model.Name, model.Extension);

			String path = this.FilePath(model);

			using (FileStream fileStream = new FileStream(path, FileMode.Create))
			{
				using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
				{
					var zipArchiveEntry = archive.CreateEntry(nameWithExtension, CompressionLevel.Fastest);
					using (var zipStream = zipArchiveEntry.Open()) zipStream.Write(payload, 0, payload.Length);
				}
			}
			return Task.FromResult(path);
		}

		public async Task<String> MoveToStorage(String filePath, StorageType type, String subDirectory = null)
		{
			FileInfo file = new FileInfo(filePath);
			if(file.Exists) return await this.MoveFileToStorage(filePath, type, subDirectory);

			DirectoryInfo dir = new DirectoryInfo(filePath);
			if (dir.Exists) return await this.MoveDirectoryToStorage(filePath, type, subDirectory);

			throw new DGNotFoundException($"Could not locate file {filePath}");
		}

		private Task<String> MoveFileToStorage(String filePath, StorageType type, String subDirectory)
		{
			FileInfo sourceFile = new FileInfo(filePath);
			if (!sourceFile.Exists) throw new DGNotFoundException($"Could not locate file {filePath}");

			StorageFile targetFile = new StorageFile()
			{
				Name = Path.GetFileNameWithoutExtension(filePath),
				Extension = Path.GetExtension(filePath),
				StorageType = type
			};
			String targetFilePath = this.FilePath(targetFile, subDirectory);

			FileInfo targetFileInfo = new FileInfo(targetFilePath);
			targetFileInfo.Directory?.Create();

			sourceFile.MoveTo(targetFilePath, true);

			return Task.FromResult(targetFilePath);
		}

		private Task<String> MoveDirectoryToStorage(String directoryPath, StorageType type, String subDirectory)
		{
			DirectoryInfo sourceDir = new DirectoryInfo(directoryPath);
			if (!sourceDir.Exists) throw new DGNotFoundException($"Could not locate directory {directoryPath}");

			String targetDirectoryPath = this.DirectoryPath(type, subDirectory);

			DirectoryInfo targetDirectoryInfo = new DirectoryInfo(targetDirectoryPath);

			if (!targetDirectoryInfo.Exists) targetDirectoryInfo.Create();
			sourceDir.MoveTo(Path.Combine(targetDirectoryPath, Path.GetFileName(sourceDir.Name)));

			return Task.FromResult(targetDirectoryPath);
		}

		public async Task<byte[]> ReadByteSafeAsync(String path)
		{
			try
			{
				FileInfo file = new FileInfo(path);
				if (!file.Exists) return null;
				return await File.ReadAllBytesAsync(file.FullName);
			}
			catch (System.Exception ex)
			{
				this._logger.Warning(ex, "problem reading byte content of storage file {id}", path);
				return null;
			}
		}

		public Task<String> DirectoryOf(StorageType type, String subDirectory)
		{
			String path = this.DirectoryPath(type, subDirectory);
			return Task.FromResult(path);
		}

		private String FilePath(StorageFile model, String subDirectory = null)
		{
			StorageTypeConfig storageTypeConfig = this._config.Storages.FirstOrDefault(x => x.Type == model.StorageType);
			if (storageTypeConfig == null) throw new DGApplicationException($"Storage {model.StorageType} not found");

			String filename = storageTypeConfig.NamingConventions.NamePattern
				.Replace("{filename}", model.Name)
				.Replace("{nonce}", Guid.NewGuid().ToString("N"))
				.Replace("{extension}", this.GetExtensionWithoutDot(model.Extension));

			String path = Path.Combine(storageTypeConfig.BasePath, storageTypeConfig.SubPath);
			if(!String.IsNullOrEmpty(subDirectory)) path = Path.Combine(path, subDirectory);
			path = Path.Combine(path, filename);

			return path;
		}

		private String DirectoryPath(StorageType type, String subDirectory = null)
		{
			StorageTypeConfig storageTypeConfig = this._config.Storages.FirstOrDefault(x => x.Type == type);
			if (storageTypeConfig == null) throw new DGApplicationException($"Storage {type} not found");

			String path = Path.Combine(storageTypeConfig.BasePath, storageTypeConfig.SubPath);
			if (!String.IsNullOrEmpty(subDirectory)) path = Path.Combine(path, subDirectory);

			return path;
		}

		private String AppendExtension(String name, String extension)
		{
			if (string.IsNullOrEmpty(extension)) return name;
			String extensionToUse = this.GetExtensionWithoutDot(extension);
			return $"{name}.{extension}";
		}

		private String GetExtensionWithoutDot(String extension)
		{
			String current = extension;
			if (extension.StartsWith(".")) current = extension.Substring(1);

			if (current.StartsWith(".")) return this.GetExtensionWithoutDot(current);

			return current;
		}
	}
}
