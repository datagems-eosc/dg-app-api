namespace DataGEMS.Gateway.Api.Model
{
	public class QueryResult<M>
	{
		public QueryResult() { }
		public QueryResult(List<M> items, int count)
		{
			this.Items = items;
			this.Count = count;
		}

		public List<M> Items { get; set; }
		public int Count { get; set; }

		public static QueryResult<M> Empty()
		{
			return new QueryResult<M>(Enumerable.Empty<M>().ToList(), 0);
		}
	}
}
