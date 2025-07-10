using Cite.WebTools.Data.Transaction;
using DataGEMS.Gateway.App.Data;

namespace DataGEMS.Gateway.Api.Transaction
{
	public class AppTransactionFilter : TransactionFilter
	{
		public AppTransactionFilter(AppDbContext dbContext, ILogger<AppTransactionFilter> logger) : base(dbContext, logger) { }
	}
}
