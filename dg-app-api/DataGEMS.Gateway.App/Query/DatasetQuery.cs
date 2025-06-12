using Cite.Tools.Common.Extensions;
using DataGEMS.Gateway.App.Common;

namespace DataGEMS.Gateway.App.Query
{
	public class DatasetQuery : IQuery
	{
		private Paging _page { get; set; }
		private Ordering _order { get; set; }

		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _collectionIds { get; set; }
		private String _like { get; set; }

		public DatasetQuery() { }

		public DatasetQuery Page(Paging page) { this._page = page; return this; }
		public DatasetQuery Order(Ordering order) { this._order = order; return this; }
		public DatasetQuery Ids(IEnumerable<Guid> ids) { this._ids = ids?.ToList(); return this; }
		public DatasetQuery Ids(Guid id) { this._ids = id.AsList(); return this; }
		public DatasetQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = excludedIds?.ToList(); return this; }
		public DatasetQuery ExcludedIds(Guid excludedId) { this._excludedIds = excludedId.AsList(); return this; }
		public DatasetQuery CollectionIds(IEnumerable<Guid> collectionIds) { this._collectionIds = collectionIds?.ToList(); return this; }
		public DatasetQuery CollectionIds(Guid collectionId) { this._collectionIds = collectionId.AsList(); return this; }
		public DatasetQuery Like(String like) { this._like = like; return this; }

		protected bool IsFalseQuery()
		{
			return this._ids.IsNotNullButEmpty() || this._excludedIds.IsNotNullButEmpty() || this._collectionIds.IsNotNullButEmpty();
		}

	}
}
