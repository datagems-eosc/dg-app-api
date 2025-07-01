using Cite.Tools.FieldSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.Conversation
{
	public interface IConversationDatasetService
	{
		Task<List<Guid>> ApplyEditAccess(List<Guid> ids);
		Task<List<Guid>> ApplyDeleteAccess(List<Guid> ids);
		Task<Model.ConversationDataset> PersistAsync(Model.ConversationDatasetPersist model, IFieldSet fields = null);
		Task<List<Model.ConversationDataset>> PersistAsync(IEnumerable<Model.ConversationDatasetPersist> models, IFieldSet fields = null);
		Task DeleteAsync(Guid id);
		Task DeleteAsync(IEnumerable<Guid> ids);
	}
}
