
namespace DataGEMS.Gateway.App.Authorization
{
	public static class Permission
	{
		//Authorization
		public const String LookupContextGrantOther = "LookupContextGrantOther";
		public const String LookupContextGrantGroups = "LookupContextGrantGroups";
		public const String AddUserToContextGrantGroup = "AddUserToContextGrantGroup";
		public const String RemoveUserFromContextGrantGroup = "RemoveUserFromContextGrantGroup";
		//Collection
		public const String BrowseCollection = "BrowseCollection";
		public const String CreateCollection = "CreateCollection";
		public const String EditCollection = "EditCollection";
		public const String DeleteCollection = "DeleteCollection";
		//Conversation
		public const String BrowseConversation = "BrowseConversation";
		public const String EditConversation = "EditConversation";
		public const String DeleteConversation = "DeleteConversation";
		//ConversationDataset
		public const String BrowseConversationDataset = "BrowseConversationDataset";
		public const String EditConversationDataset = "EditConversationDataset";
		public const String DeleteConversationDataset = "DeleteConversationDataset";
		//ConversationMessage
		public const String BrowseConversationMessage = "BrowseConversationMessage";
		public const String EditConversationMessage = "EditConversationMessage";
		public const String DeleteConversationMessage = "DeleteConversationMessage";
		//CrossDatasetDiscovery
		public const String CanExecuteCrossDatasetDiscovery = "CanExecuteCrossDatasetDiscovery";
		//InDataExploration
		public const String CanExecuteInDataExploration = "CanExecuteInDataExploration";
		//Dataset
		public const String BrowseDataset = "BrowseDataset";
		//DatasetCollection
		public const String BrowseDatasetCollection = "BrowseDatasetCollection";
		//User
		public const String BrowseUser = "BrowseUser";
		//UserCollection
		public const String BrowseUserCollection = "BrowseUserCollection";
		public const String EditUserCollection = "EditUserCollection";
		public const String DeleteUserCollection = "DeleteUserCollection";
		//UserDatasetCollection
		public const String BrowseUserDatasetCollection = "BrowseUserDatasetCollection";
		public const String EditUserDatasetCollection = "EditUserDatasetCollection";
		public const String DeleteUserDatasetCollection = "DeleteUserDatasetCollection";
		//Vocabulary
		public const String BrowseFieldsOfScienceVocabulary = "BrowseFieldsOfScienceVocabulary";
		public const String BrowseLicenseVocabulary = "BrowseLicenseVocabulary";
		//Workflow
		public const String BrowseWorkflowDefinition = "BrowseWorkflowDefinition";
		public const String ExecuteWorkflow = "ExecuteWorkflow";
		public const String BrowseWorkflowExecution = "BrowseWorkflowExecution";
		public const String BrowseWorkflowTask = "BrowseWorkflowTask";
		public const String BrowseWorkflowTaskInstance = "BrowseWorkflowTaskInstance";
		public const String BrowseWorkflowXCom = "BrowseWorkflowXCom";
		public const String BrowseWorkflowTaskLog = "BrowseWorkflowTaskLog";
	}
}
