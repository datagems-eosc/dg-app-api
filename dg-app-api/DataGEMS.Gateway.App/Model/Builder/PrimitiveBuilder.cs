using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Builder;
using Cite.Tools.FieldSet;
using Cite.Tools.Time;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public abstract class PrimitiveBuilder<M, D> : IBuilder
	{
		public PrimitiveBuilder(
			ILogger logger)
		{
			this._logger = logger;
		}

		protected readonly ILogger _logger;

		public async Task<M> Build(IFieldSet directives, D data)
		{
			if (data == null) return default(M);
			List<M> models = await this.Build(directives, new D[] { data });
			return models.FirstOrDefault();
		}

		public abstract Task<List<M>> Build(IFieldSet directives, IEnumerable<D> datas);

		public async Task<Dictionary<K, M>> AsForeignKey<K>(IEnumerable<D> datas, IFieldSet directives, Func<M, K> keySelector)
		{
			List<M> models = await this.Build(directives, datas);
			Dictionary<K, M> map = models.ToDictionary(keySelector);
			return map;
		}

		public async Task<Dictionary<K, List<M>>> AsMasterKey<K>(IEnumerable<D> datas, IFieldSet directives, Func<M, K> keySelector)
		{
			List<M> models = await this.Build(directives, datas);
			Dictionary<K, List<M>> map = new Dictionary<K, List<M>>();
			foreach (M model in models ?? new List<M>())
			{
				K key = keySelector.Invoke(model);
				if (!map.ContainsKey(key)) map.Add(key, new List<M>());
				map[key].Add(model);
			}
			return map;
		}

		public Dictionary<FK, FM> AsEmpty<FK, FM>(IEnumerable<FK> keys, Func<FK, FM> mapper, Func<FM, FK> keySelector)
		{
			IEnumerable<FM> models = keys.Select(mapper);
			Dictionary<FK, FM> map = models.ToDictionary(keySelector);
			return map;
		}

		protected String HashValue(DateTime value)
		{
			return value.ToEpoch().ToString();
		}

		protected String AsPrefix(String name)
		{
			return name.AsIndexerPrefix();
		}

		protected String AsIndexer(params String[] names)
		{
			return names.AsIndexer();
		}
	}
}
