
namespace DataGEMS.Gateway.App.Accounting
{
	public interface IAccountingService
	{
		Boolean IsEnabled { get; }

		void AccountFor(KnownActions action);
		void AccountFor(KnownActions action, String resource);
		void AccountFor(AccountingInfo info);
	}
}
