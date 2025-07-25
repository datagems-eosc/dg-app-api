using Cite.Tools.FieldSet;

namespace DataGEMS.Gateway.App.Service.Conversation
{
	public interface IConversationService
	{
		Task<String> GenerateConversationName(Guid? conversationId, String currentQuery);
		Task<App.Model.Conversation> PersistAsync(App.Model.ConversationPersist model, IFieldSet fields = null);
		Task<App.Model.Conversation> PersistAsync(App.Model.ConversationPersistDeep model, IFieldSet fields = null);
		Task<App.Model.Conversation> PatchAsync(App.Model.ConversationDatasetPatch model, IFieldSet fields = null);
		Task<App.Model.Conversation> AddAsync(Guid conversationId, Guid datasetId, IFieldSet fields = null);
		Task<App.Model.Conversation> RemoveAsync(Guid conversationId, Guid datasetId, IFieldSet fields = null);
		Task DeleteAsync(Guid id);
		Task AppendToConversation(Guid conversationId, params Common.Conversation.ConversationEntry[] entries);
		Task SetConversationDatasets(Guid conversationId, IEnumerable<Guid> datasetIds);
	}
}
