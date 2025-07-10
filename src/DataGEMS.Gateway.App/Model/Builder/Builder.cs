using Cite.Tools.Data.Query;
using Cite.Tools.FieldSet;
using Cite.Tools.Logging.Extensions;
using Microsoft.Extensions.Logging;

namespace DataGEMS.Gateway.App.Model.Builder
{
	public abstract class Builder<M, D> : PrimitiveBuilder<M, D> where D : class
	{
		public Builder(ILogger logger) : base(logger) { }

		public async Task<Dictionary<K, M>> AsForeignKey<K>(Query<D> query, IFieldSet directives, Func<M, K> keySelector)
		{
			this._logger.Trace("Building references from query");
			List<D> datas = await query.CollectAsync(directives);
			this._logger.Debug("collected {count} items to build", datas?.Count);
			return await this.AsForeignKey(datas, directives, keySelector);
		}

		public async Task<Dictionary<K, List<M>>> AsMasterKey<K>(Query<D> query, IFieldSet directives, Func<M, K> keySelector)
		{
			this._logger.Trace("Building details from query");
			List<D> datas = await query.CollectAsync(directives);
			this._logger.Debug("collected {count} items to build", datas?.Count);
			return await this.AsMasterKey(datas, directives, keySelector);
		}
	}
}
