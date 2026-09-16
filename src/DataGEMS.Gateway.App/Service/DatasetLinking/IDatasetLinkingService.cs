namespace DataGEMS.Gateway.App.Service.DatasetLinking
{
	public interface IDatasetLinkingService
	{
		public Task<string> GetJobByIdAsync(Guid id);
	}
}
