using Cite.Tools.FieldSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.Version
{
	public interface IVersionInfoService
	{
		Task<List<Model.VersionInfo>> CurrentAsync(IFieldSet fields = null);
	}
}
