using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Query;
using Cite.Tools.Json;
using DataGEMS.Gateway.App.AccessToken;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Query;
using DataGEMS.Gateway.App.Service.DataManagement.Model;
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
