using DataGEMS.Gateway.App.Common.Auth;

namespace DataGEMS.Gateway.App.Service.AAI
{
	public interface IAAIService
	{
		Task BootstrapUserContextGrants(String userSubjectId);
		Task AddUserToGroup(String userSubjectId, String userGroupId);
		Task<List<ContextGrant>> LookupUserGroupContextGrants(String userGroupId);
		Task<List<ContextGrant>> LookupUserContextGrants(String userSubjectId);
		Task AssignCollectionGrantToUser(String subjectId, Guid collectionId, String role);
		Task AssignCollectionGrantToUser(String subjectId, Guid collectionId, List<String> roles);
		Task AssignCollectionGrantToUserGroup(String groupId, Guid collectionId, String role);
		Task AssignCollectionGrantToUserGroup(String groupId, Guid collectionId, List<String> roles);
		Task UnassignCollectionGrantFromUser(String subjectId, Guid collectionId, String role);
		Task UnassignCollectionGrantFromUserGroup(String groupId, Guid collectionId, String role);
		Task AssignDatasetGrantToUser(String subjectId, Guid datasetId, String role);
		Task AssignDatasetGrantToUser(String subjectId, Guid datasetId, List<String> roles);
		Task AssignDatasetGrantToUserGroup(String groupId, Guid datasetId, String role);
		Task AssignDatasetGrantToUserGroup(String groupId, Guid datasetId, List<String> roles);
		Task UnassignDatasetGrantFromUser(String subjectId, Guid datasetId, String role);
		Task UnassignDatasetGrantFromUserGroup(String groupId, Guid datasetId, String role);
		Task DeleteCollectionGrants(Guid collectionId);
		Task DeleteDatasetGrants(Guid datasetId);
	}
}
