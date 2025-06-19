
namespace DataGEMS.Gateway.App.ErrorCode
{
	public class ErrorThesaurus
	{
		public ErrorDescription Forbidden { get; set; }
		public ErrorDescription SystemError { get; set; }
		public ErrorDescription ModelValidation { get; set; }
		public ErrorDescription UnsupportedAction { get; set; }
		public ErrorDescription UnderpinningService { get; set; }
		public ErrorDescription TokenExchange { get; set; }
		public ErrorDescription UserSync { get; set; }
	}
}
