using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Deleter;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Json;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Event;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using static DataGEMS.Gateway.App.Query.DatasetHttpQuery;

namespace DataGEMS.Gateway.App.Service.UserFavorite
{
	public class UserFavoriteService : IUserFavoriteService
	{
		private readonly Data.AppDbContext _dbContext;
		private readonly BuilderFactory _builderFactory;
		private readonly DeleterFactory _deleterFactory;
		private readonly QueryFactory _queryFactory;
		private readonly IStringLocalizer<Resources.MySharedResources> _localizer;
		private readonly IAuthorizationService _authorizationService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly ILogger<UserFavoriteService> _logger;
		private readonly ErrorThesaurus _errors;
		private readonly EventBroker _eventBroker;
		private readonly JsonHandlingService _jsonHandlingService;

		public UserFavoriteService(Data.AppDbContext dbContext,
			BuilderFactory builderFactory,
			DeleterFactory deleterFactory,
			QueryFactory queryFactory,
			IStringLocalizer<Resources.MySharedResources> localizer,
			IAuthorizationService authorizationService,
			IAuthorizationContentResolver authorizationContentResolver,
			ILogger<UserFavoriteService> logger,
			ErrorThesaurus errors,
			EventBroker eventBroker,
			JsonHandlingService jsonHandlingService)
		{
			this._dbContext = dbContext;
			this._builderFactory = builderFactory;
			this._deleterFactory = deleterFactory;
			this._queryFactory = queryFactory;
			this._localizer = localizer;
			this._authorizationService = authorizationService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._logger = logger;
			this._errors = errors;
			this._eventBroker = eventBroker;
			this._jsonHandlingService = jsonHandlingService;
		}

		private async Task AuthorizeForce(Guid? id)
		{
			if (!id.HasValue) return;

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			Data.UserFavorite data = await this._dbContext.UserFavorites.Where(x=> (x.Id == id.Value || x.DatasetId == id.Value) && x.UserId == userId && x.IsActive == IsActive.Active).FirstOrDefaultAsync();
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", id.Value, nameof(Model.UserFavorite)]);

			String subjectId = await this._authorizationContentResolver.SubjectIdOfUserId(data.UserId);
			await this._authorizationService.AuthorizeOwnerForce(!String.IsNullOrEmpty(subjectId) ? new OwnedResource(subjectId) : null);
		}

		public async Task<Model.UserFavorite> PersistAsync(Model.UserFavoritePersist model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.UserFavoritePersist)).And("model", model).And("fields", fields));

			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			if (model == null || !model.DatasetId.HasValue)
				throw new DGNotFoundException(this._localizer["general_notFound", Guid.Empty, nameof(Model.Dataset)]);

			QueryResult queryResult = await this._queryFactory.Query<DatasetHttpQuery>()
				.Ids(model.DatasetId.Value)
				.Permissions(Permission.BrowseDataset)
				.CollectAsync();

			if (queryResult?.Items == null || queryResult.Items.Count == 0)
				throw new DGNotFoundException(this._localizer["general_notFound", model.DatasetId.Value, nameof(Model.Dataset)]);

			Data.UserFavorite data = await this.PatchAndSave(model, userId.Value);
			Model.UserFavorite persisted = await this._builderFactory.Builder<Model.Builder.UserFavoriteBuilder>().Build(FieldSet.Build(fields, nameof(Model.UserFavorite.Id)), data);
			return persisted;
		}

		private async Task<Data.UserFavorite> PatchAndSave(Model.UserFavoritePersist model, Guid userId)
		{
			Data.UserFavorite data = null;

			var existingData = await this._dbContext.UserFavorites.FirstOrDefaultAsync(x => x.DatasetId == model.DatasetId.Value && x.IsActive == IsActive.Active && x.UserId == userId);
			if (existingData != null) return existingData;

			data = new Data.UserFavorite
			{
				Id = Guid.NewGuid(),
				DatasetId = model.DatasetId.Value,
				IsActive = IsActive.Active,
				UserId = userId,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
			};
			this._dbContext.Add(data);
			await this._dbContext.SaveChangesAsync();

			this._eventBroker.EmitUserFavoriteTouched(data.Id);

			return data;
		}

		public async Task DeleteByIdOrDatasetIdAsync(Guid id)
		{
			await this.AuthorizeForce(id);

			await this._deleterFactory.Deleter<Deleter.UserFavoriteDeleter>().DeleteAndSave([id]);
		}
	}
}
