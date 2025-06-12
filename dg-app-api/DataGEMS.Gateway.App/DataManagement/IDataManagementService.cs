using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.DataManagement
{
    public interface IDataManagementService
    {
        Task<List<DataManagement.Model.Dataset>> Collect();
        Task<int> Count();
    }
}
