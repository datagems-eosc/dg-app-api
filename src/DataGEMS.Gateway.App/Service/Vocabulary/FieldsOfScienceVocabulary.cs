
namespace DataGEMS.Gateway.App.Service.Vocabulary
{
	public class FieldsOfScienceVocabulary
	{
		public List<VocabularyItem> Hierarchy { get; set; }

		public class VocabularyItem
		{
			public int Ordinal { get; set; }

			public string Code { get; set; }

			public string Name { get; set; }

			public List<VocabularyItem> Children { get; set; }
		}
	}
}
