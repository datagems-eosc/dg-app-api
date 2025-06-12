using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Query
{
	public class QueryFactory
	{
		public class QueryFactoryConfig
		{
			public QueryFactoryConfig() { }

			public QueryFactoryConfig(IEnumerable<Type> types)
			{
				this.Add(types);
			}

			public HashSet<Type> Queries { get; } = new HashSet<Type>();

			public QueryFactoryConfig Add(IEnumerable<Type> types)
			{
				foreach (Type t in types) this.Queries.Add(t);
				return this;
			}
		}

		private readonly IServiceProvider _serviceProvider;
		private readonly Dictionary<Type, Func<IQuery>> _queryMap = null;

		public QueryFactory(IServiceProvider serviceProvider, QueryFactoryConfig config)
		{
			this._serviceProvider = serviceProvider;

			this._queryMap = new Dictionary<Type, Func<IQuery>>();
			foreach (Type t in config?.Queries)
			{
				this._queryMap.Add(t, () => { return this._serviceProvider.GetRequiredService(t) as IQuery; });
			}
		}

		public T Query<T>() where T : IQuery
		{
			Type tt = typeof(T);
			if (this._queryMap.TryGetValue(tt, out Func<IQuery> obj)) return (T)obj();
			throw new ApplicationException("unrecognized query " + tt.FullName);
		}

		public IQuery this[Type t]
		{
			get
			{
				if (this._queryMap.TryGetValue(t, out Func<IQuery> obj)) return obj();
				throw new ApplicationException("unrecognized query " + t.FullName);
			}
		}
	}
}
