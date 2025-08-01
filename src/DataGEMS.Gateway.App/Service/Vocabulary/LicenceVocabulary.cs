using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Service.Vocabulary
{
	public class LicenseVocabulary
	{
		public List<License> Licenses { get; set; }

		public class License
		{
			public string Name { get; set; }
			public List<string> Url { get; set; }
			public string Code { get; set; }

		}

	}
}
