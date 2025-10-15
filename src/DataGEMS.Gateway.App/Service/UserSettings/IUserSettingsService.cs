using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Model;

namespace DataGEMS.Gateway.App.Service.UserSettings
{
	public interface IUserSettingsService
	{
		Task<Model.UserSettings> PersistAsync(UserSettingsPersist model, IFieldSet fields = null);
		Task DeleteAsync(Guid id);
		Task DeleteAsync(List<Guid> ids);
	}
}
