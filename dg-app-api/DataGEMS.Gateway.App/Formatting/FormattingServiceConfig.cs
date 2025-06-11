using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Formatting
{
	public class FormattingServiceConfig
	{
		public FormattingServiceOptions Default { get; set; }
		public FormattingServiceOptions Override { get; set; }

		public class FormattingServiceOptions
		{
			public TypeOption IntegerFormat { get; set; }
			public TypeOption DecimalFormat { get; set; }
			public TypeOption CurrencyFormat { get; set; }
			public TypeOption PercentageFormat { get; set; }
			public TypeOption TotalHoursFormat { get; set; }
			public TypeOption DateTimeFormat { get; set; }
			public TypeOption DateOnlyFormat { get; set; }
			public TypeOption TimeOnlyFormat { get; set; }

			public class TypeOption
			{
				public string Format { get; set; }
				public string Culture { get; set; }
			}
		}
	}
}
