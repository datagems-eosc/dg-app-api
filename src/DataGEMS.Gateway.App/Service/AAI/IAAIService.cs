using DataGEMS.Gateway.App.Common.Auth;
using static DataGEMS.Gateway.App.Common.Auth.ContextGrant;

namespace DataGEMS.Gateway.App.Service.AAI
{
	public interface IAAIService
	{
		Task<ContextGrantGroupTarget> TargetOfContextGrantGroup(String groupId);
		Task<List<ContextGrant>> LookupContextGrantGroups(String code);
		Task BootstrapContextGrantGroupsFor(ContextGrant.TargetType type, String code);
		Task DeleteContextGrantGroupsFor(String code);
		Task AddUserToContextGrantGroup(String subjectId, String groupId);
		Task RemoveUserFromContextGrantGroup(String subjectId, String groupId);
		Task<List<ContextGrant>> UserContextGrants(String subjectId);
	}

	public class ContextGrantGroupTarget
	{
		public ContextGrant.TargetType Type { get; set; }
		public String Code { get; set; }
	}
}
