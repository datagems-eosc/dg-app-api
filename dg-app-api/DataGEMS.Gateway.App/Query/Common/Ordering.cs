using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Query
{
	public class Ordering
	{
		private List<String> _items;
		public List<String> Items
		{
			get
			{
				return this._items;
			}
			set
			{
				if (value != null) this._items = new List<string>(value.Select(x => x.ToLower()));
				else this._items = value;
			}
		}

		public Ordering AddAscending(String property)
		{
			if (String.IsNullOrEmpty(property)) return this;
			if (this._items == null) this._items = new List<string>();
			this._items.Add($"+{property}");
			return this;
		}

		public Ordering AddDescending(String property)
		{
			if (String.IsNullOrEmpty(property)) return this;
			if (this._items == null) this._items = new List<string>();
			this._items.Add($"-{property}");
			return this;
		}

		public Boolean IsEmpty { get { return this.Items == null || this.Items.Count == 0; } }
	}
}
