using Cite.Tools.Data.Builder;
using Cite.Tools.Data.Deleter;
using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Event;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.Query;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Service.UserSettings
{
	public class UserSettingsService : IUserSettingsService
	{
		private readonly Data.AppDbContext _dbContext;
		private readonly BuilderFactory _builderFactory;
		private readonly DeleterFactory _deleterFactory;
		private readonly QueryFactory _queryFactory;
		private readonly IStringLocalizer<Resources.MySharedResources> _localizer;
		private readonly IAuthorizationService _authorizationService;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;
		private readonly ILogger<UserSettingsService> _logger;
		private readonly ErrorThesaurus _errors;
		private readonly EventBroker _eventBroker;

		public UserSettingsService(
			ILogger<UserSettingsService> logger,
			IAuthorizationService authorizationService,
			IAuthorizationContentResolver authorizationContentResolver,
			Data.AppDbContext dbContext,
			BuilderFactory builderFactory,
			DeleterFactory deleterFactory,
			QueryFactory queryFactory,
			IStringLocalizer<Resources.MySharedResources> localizer,
			ErrorThesaurus errors,
			EventBroker eventBroker)
		{
			this._logger = logger;
			this._authorizationService = authorizationService;
			this._authorizationContentResolver = authorizationContentResolver;
			this._dbContext = dbContext;
			this._builderFactory = builderFactory;
			this._queryFactory = queryFactory;
			this._deleterFactory = deleterFactory;
			this._localizer = localizer;
			this._errors = errors;
			this._eventBroker = eventBroker;
		}

		private async Task AuthorizeForce(Guid? userSettingsId)
		{
			if (!userSettingsId.HasValue) return;

			Data.UserSettings data = await this._dbContext.UserSettings.FindAsync(userSettingsId);
			if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", userSettingsId.Value, nameof(Model.UserSettings)]);

			String subjectId = await this._authorizationContentResolver.SubjectIdOfUserId(data.UserId);
			await this._authorizationService.AuthorizeOwnerForce(!String.IsNullOrEmpty(subjectId) ? new OwnedResource(subjectId) : null);
		}

		public async Task<Model.UserSettings> PersistAsync(Model.UserSettingsPersist model, IFieldSet fields = null)
		{
			this._logger.Debug(new MapLogEntry("persisting").And("type", nameof(App.Model.UserSettingsPersist)).And("model", model).And("fields", fields));

			await this.AuthorizeForce(model.Id);

			Data.UserSettings data = await this.PatchAndSave(model);

			Model.UserSettings persisted = await this._builderFactory.Builder<Model.Builder.UserSettingsBuilder>().Build(FieldSet.Build(fields, nameof(Model.UserSettings.Id), nameof(Model.UserSettings.ETag)), data);
			return persisted;
		}

		private async Task<Data.UserSettings> PatchAndSave(Model.UserSettingsPersist model)
		{
			Guid? userId = await this._authorizationContentResolver.CurrentUserId();
			if (!userId.HasValue) throw new DGForbiddenException(this._errors.Forbidden.Code, this._errors.Forbidden.Message);

			Boolean isUpdate = model.Id.HasValue && model.Id.Value != Guid.Empty;

			Data.UserSettings data = null;
			if (isUpdate)
			{
				data = await this._queryFactory.Query<UserSettingsQuery>().Find(model.Id.Value);
				if (data == null) throw new DGNotFoundException(this._localizer["general_notFound", model.Id.Value, nameof(Model.UserSettings)]);
				if (!String.Equals(model.ETag, data.UpdatedAt.ToETag())) throw new DGValidationException(this._errors.ETagConflict.Code, string.Format(this._errors.ETagConflict.Message, data.Id, nameof(Data.UserCollection)));
			}
			else
			{
				data = new Data.UserSettings
				{
					Id = Guid.NewGuid(),
					Key = model.Key,
					CreatedAt = DateTime.UtcNow,
					UserId = userId.Value
				};
			}

			if (isUpdate &&
				String.Equals(data.Key, model.Key) &&
				String.Equals(data.Value, model.Value)
				) return data;

			data.Key = model.Key;
			data.Value = model.Value;
			data.UpdatedAt = DateTime.UtcNow;

			if (isUpdate) this._dbContext.Update(data);
			else await this._dbContext.AddAsync(data);

			await this._dbContext.SaveChangesAsync();

			this._eventBroker.EmitUserSettingsTouched(data.Id);

			return data;
		}

		public async Task DeleteAsync(Guid id)
		{
			await this.AuthorizeForce(id);

			await this._deleterFactory.Deleter<Deleter.UserSettingsDeleter>().DeleteAndSave([id]);
		}

		public async Task DeleteAsync(List<Guid> ids)
		{
			foreach(Guid id in ids) await this.AuthorizeForce(id);

			await this._deleterFactory.Deleter<Deleter.UserSettingsDeleter>().DeleteAndSave(ids);
		}
	}
}
