using Cite.Tools.FieldSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.Conversation
{
	public interface IConversationService
	{
		Task<App.Model.Conversation> PersistAsync(App.Model.ConversationPersist model, IFieldSet fields = null);
		/*Task<App.Model.Conversation> PersistAsync(App.Model.ConversationPersistDeep model, IFieldSet fields = null);
		Task<App.Model.Conversation> PatchAsync(App.Model.Conversation___DatasetPatch model, IFieldSet fields = null);*/
		Task<App.Model.Conversation> AddAsync(Guid conversationId, Guid datasetId, IFieldSet fields = null);
		Task<App.Model.Conversation> RemoveAsync(Guid conversationId, Guid datasetId, IFieldSet fields = null);
		Task DeleteAsync(Guid id);
	}
}
