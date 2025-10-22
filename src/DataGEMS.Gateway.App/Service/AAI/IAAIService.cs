using DataGEMS.Gateway.App.Common.Auth;

namespace DataGEMS.Gateway.App.Service.AAI
{
	public interface IAAIService
	{
		Task BootstrapUserContextGrants(String userSubjectId);
		Task BootstrapGroupContextGrants(String userGroupId);
		Task AddUserToGroup(String userSubjectId, String userGroupId);
		Task<List<ContextGrant>> LookupPrincipalContextGrants(String principalId);
		Task<List<ContextGrant>> LookupUserEffectiveContextGrants(String userSubjectId);
		Task AssignCollectionGrantTo(String principalId, Guid collectionId, String role);
		Task AssignCollectionGrantTo(String principalId, Guid collectionId, List<String> roles);
		Task UnassignCollectionGrantFrom(String principalId, Guid collectionId, String role);
		Task AssignDatasetGrantTo(String principalId, Guid datasetId, String role);
		Task AssignDatasetGrantTo(String principalId, Guid datasetId, List<String> roles);
		Task UnassignDatasetGrantFrom(String principalId, Guid datasetId, String role);
		Task DeleteCollectionGrants(Guid collectionId);
		Task DeleteDatasetGrants(Guid datasetId);
	}
}
