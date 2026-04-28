using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.Json;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Service.DatasetFileManagement.Model;
using DataGEMS.Gateway.App.Service.Storage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Service.DatasetFileManagement
{
	public class DatasetFileManagementService : IDatasetFileManagementService
	{
		private readonly IAccessTokenService _accessTokenService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<DatasetFileManagementService> _logger;
		private readonly QueryFactory _queryFactory;
		private readonly ErrorThesaurus _errors;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly BuilderFactory _builderFactory;
		private readonly IStorageService _storageService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly IAuthorizationService _authorizationService;
		private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

		public DatasetFileManagementService(
			IAccessTokenService accessTokenService,
			IHttpClientFactory httpClientFactory,
			ILogger<DatasetFileManagementService> logger,
			QueryFactory queryFactory,
			ErrorThesaurus errors,
			JsonHandlingService jsonHandlingService,
			BuilderFactory builderFactory,
			IStorageService storageService,
			IAuthorizationContentResolver authorizationContentResolver,
			IAuthorizationService authorizationService,
			IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer)
		{
			this._accessTokenService = accessTokenService;
			this._httpClientFactory = httpClientFactory;
			this._logger = logger;
			this._queryFactory = queryFactory;
			this._errors = errors;
			this._jsonHandlingService = jsonHandlingService;
			this._builderFactory = builderFactory;
			this._storageService = storageService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._authorizationService = authorizationService;
			this._localizer = localizer;
		}

		public async Task<DatasetObject> BrowseDatasetFilesAsync(Guid datasetId, Guid? fileSetNodeId)
		{
			HashSet<string> userDatasetRoles = await _authorizationContentResolver.EffectiveContextRolesForDatasetOfUser(datasetId);
			await _authorizationService.AuthorizeOrAffiliatedContextForce(new AffiliatedContextResource(userDatasetRoles), Permission.BrowseDatasetFiles);

			List<App.Service.DataManagement.Model.Dataset> datas = (await this._queryFactory.Query<DatasetHttpQuery>().Ids(datasetId).CollectAsync())?.Items ?? [];
			if (datas == null || datas.Count == 0) throw new DGNotFoundException(this._localizer["general_notFound", datasetId, nameof(App.Model.Dataset)]);
			if (datas.Count > 1) throw new DGFoundManyException(this._localizer["general_nonUnique", datasetId, nameof(App.Model.Dataset)]);
			if (datas.First().ProfileRaw == null) throw new DGApplicationException(this._localizer["dataset_noProfile", datasetId]);
			Profile profile = this._jsonHandlingService.FromJsonSafe<Profile>(this._jsonHandlingService.ToJsonSafe(datas.First().ProfileRaw));
			
			List<Profile.ProfileNode> fileSets = profile.Nodes?.Where(x => x.Labels.Contains("cr:FileSet") && x.Properties != null && x.Properties.ContainsKey("contentUrl")).ToList();
			List<DatasetFileSet> nodes = fileSets.Select(x => new DatasetFileSet
			{
				Id = x.Id,
				Path = this._storageService.NormalizePath((string)x.Properties["contentUrl"]),
				FileSets = [],
				Files = []
			})
			.OrderBy(x => x.Path.Count(y => y == Path.DirectorySeparatorChar)).ToList();

			var roots = new List<DatasetFileSet>();

			foreach (var node in nodes)
			{
				var parent = nodes.Where(candidate => candidate != node && IsSubPathOf(node.Path, candidate.Path))
					.OrderByDescending(candidate => candidate.Path.Length)
					.FirstOrDefault();
				if (parent == null)
				{
					roots.Add(node);
				}
				else
				{
					parent.FileSets.Add(node);
				}
			}
			var root = new DatasetFileSet
			{
				Id = datasetId,
				Files = [],
				FileSets = fileSets != null && fileSets.Count > 0 ? roots : [],
				Path = await this._storageService.DirectoryOf(StorageType.Dataset, datasetId.ToString())
			};

			IEnumerable<DatasetFile> fileObjects = profile.Nodes?.Where(x => x.Labels.Contains("cr:FileObject")).Select(x => new DatasetFile
			{
				Id = x.Id,
				Path = x.Properties != null && x.Properties.ContainsKey("contentUrl") ? (string)x.Properties["contentUrl"] : "",
				Size = x.Properties != null && x.Properties.ContainsKey("contentSize") ? (string)x.Properties["contentSize"] : "0 B",
			});

			foreach (var file in fileObjects)
			{
				var filePath = this._storageService.NormalizePath(Path.GetDirectoryName(file.Path));
				if (filePath == this._storageService.NormalizePath(root.Path))
				{
					root.Files.Add(file);
					continue;
				}
				foreach (var node in nodes)
				{
					if (filePath == this._storageService.NormalizePath(node.Path))
					{
						node.Files.Add(file);
						break;
					}
				}
			}

			if (fileSetNodeId != null)
			{
				DatasetFileSet targetNode = nodes.FirstOrDefault(x => x.Id == fileSetNodeId);
				if (targetNode == null)
				{
					var targetFile = fileObjects.FirstOrDefault(x => x.Id == fileSetNodeId);
					if (targetFile != null)
					{
						return targetFile;
					}
					throw new DGNotFoundException(this._localizer["general_notFound", fileSetNodeId, nameof(Profile.ProfileNode)]);
				}
				return targetNode;
			}
			else
			{
				return root;
			}
		}

		private static bool IsSubPathOf(string childPath, string parentPath)
		{
			return childPath.StartsWith(parentPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
		}

		public async Task<byte[]> DownloadDatasetFileAsync(Guid datasetId, Guid fileObjectNodeId)
		{
			HashSet<string> userDatasetRoles = await _authorizationContentResolver.EffectiveContextRolesForDatasetOfUser(datasetId);
			await _authorizationService.AuthorizeOrAffiliatedContextForce(new AffiliatedContextResource(userDatasetRoles), Permission.DownloadDatasetFile);

			List<App.Service.DataManagement.Model.Dataset> datas = (await this._queryFactory.Query<DatasetHttpQuery>().Ids(datasetId).CollectAsync())?.Items ?? [];
			if (datas == null || datas.Count == 0) throw new DGNotFoundException(this._localizer["general_notFound", datasetId, nameof(App.Model.Dataset)]);
			if (datas.Count > 1) throw new DGFoundManyException(this._localizer["general_nonUnique", datasetId, nameof(App.Model.Dataset)]);
			if (datas.First().ProfileRaw == null) throw new DGApplicationException(this._localizer["dataset_noProfile", datasetId]);

			Profile.ProfileNode node = this._jsonHandlingService.FromJsonSafe<Profile>(this._jsonHandlingService.ToJsonSafe(datas.First().ProfileRaw)).Nodes?.FirstOrDefault(x => x.Id == fileObjectNodeId);
			if (node == null) throw new DGNotFoundException(this._localizer["general_notFound", fileObjectNodeId, nameof(Profile.ProfileNode)]);
			if (node.Properties == null ||  node.Properties.Count == 0 || !node.Properties.ContainsKey("contentUrl")) throw new DGApplicationException(this._localizer["datasetFile_noContentUrl", datasetId, fileObjectNodeId]);

			string path = (string)node.Properties["contentUrl"];

			return await this._storageService.ReadByteSafeAsync(path);

		}
	}
}
