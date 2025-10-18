using DataGEMS.Gateway.App.Common.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.AAI
{
	public interface IAAIService
	{
		Task<List<DatasetGrant>> LookupDatasetGrantGroups(String code);
		Task BootstrapDatasetGrant(DatasetGrant.TargetType type, String id);
		Task AddUserToGroup(String subjectId, String groupId);
		Task RemoveUserFromGroup(String subjectId, String groupId);
		Task<List<DatasetGrant>> UserDatasetGrants(String subjectId);
	}
}
