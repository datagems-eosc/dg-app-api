using Cite.Tools.FieldSet;

namespace DataGEMS.Gateway.App.Service.IntentDisambiguation
{
	public interface IIntentDisambiguationService
	{
		Task<App.Model.IntentDisambiguation> DisambiguateIntentAsync(Model.IntentDisambiguationRequest request, IFieldSet fieldSet);
	}
}