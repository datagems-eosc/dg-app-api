using Cite.Tools.Data.Query;
using Cite.Tools.Exception;
using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.Exception;
using System.Linq.Expressions;

namespace DataGEMS.Gateway.App.Query
{
	public abstract class ExtendedQuery<T> : AsyncQuery<T> where T : class
	{
		protected abstract Boolean RequiresInMemoryFiltering();
		protected abstract Boolean RequiresInMemoryOrdering();

		protected Boolean RequiresInMemoryProcessing() => RequiresInMemoryFiltering() || RequiresInMemoryOrdering();

		protected abstract String[] ProjectionEnsureInMemoryProcessing();
		protected abstract Task<List<T>> FilterAsync(List<T> items);
		protected virtual Task<List<T>> OrderAsync(List<T> items) { return Task.FromResult(items); }

		protected override async Task<IQueryable<K>> BindSubQueryAsync<D, K>(Query<D> subQuery, IQueryable<D> subQuerySource, Expression<Func<D, K>> projection) where D : class
		{
			//We cover this case with CanBindAsSubQuery. We should be OK with default behavior. In any case, the check would need to be on subQuery, not currenty query
			//if (this.RequiresInMemoryProcessing()) throw new MyApplicationException("Invalid use of filters");
			return await base.BindSubQueryAsync(subQuery, subQuerySource, projection);
		}

		protected override Boolean CanBindAsSubQuery() { return base.CanBindAsSubQuery() && !this.RequiresInMemoryProcessing(); }

		public override async Task<IQueryable<T>> ApplyAsync()
		{
			if (this.RequiresInMemoryProcessing()) throw new DGApplicationException("Invalid use of filters");
			return await base.ApplyAsync();
		}

		public override async Task<List<T>> CollectAsync()
		{
			if (!this.RequiresInMemoryProcessing()) return await base.CollectAsync();

			Paging page = this.Page;
			Ordering order = this.Order;
			this.Page = null;
			if (this.RequiresInMemoryOrdering()) this.Order = null;

			List<T> items = await base.CollectAsync();
			this.Order ??= order;
			if (this.RequiresInMemoryFiltering()) items = await this.FilterAsync(items);
			if (this.RequiresInMemoryOrdering()) items = await this.OrderAsync(items);
			items = this.ApplyPaging(page, items);

			this.Page = page;

			return items;
		}

		public override async Task<T> FirstAsync()
		{
			if (!this.RequiresInMemoryProcessing()) return await base.FirstAsync();

			Paging page = this.Page;
			Ordering order = this.Order;
			this.Page = null;
			if (this.RequiresInMemoryOrdering()) this.Order = null;

			List<T> items = await base.CollectAsync();
			this.Order ??= order;
			if (this.RequiresInMemoryFiltering()) items = await this.FilterAsync(items);
			if (this.RequiresInMemoryOrdering()) items = await this.OrderAsync(items);
			items = this.ApplyPaging(page, items);
			T result = items.FirstOrDefault();

			this.Page = page;

			return result;
		}

		public override async Task<List<R>> CollectAsync<R>(Expression<Func<T, R>> projection)
		{
			if (this.RequiresInMemoryProcessing()) throw new MyApplicationException("Invalid use of filters");
			return await base.CollectAsync(projection);
		}

		public override async Task<R> FirstAsync<R>(Expression<Func<T, R>> projection)
		{
			if (this.RequiresInMemoryProcessing()) throw new MyApplicationException("Invalid use of filters");
			return await base.FirstAsync(projection);
		}

		public override async Task<R> MaxAsync<R>(Expression<Func<T, R>> projection)
		{
			if (this.RequiresInMemoryProcessing()) throw new MyApplicationException("Invalid use of filters");
			return await base.MaxAsync(projection);
		}

		public override async Task<R> MinAsync<R>(Expression<Func<T, R>> projection)
		{
			if (this.RequiresInMemoryProcessing()) throw new MyApplicationException("Invalid use of filters");
			return await base.MinAsync(projection);
		}

		public override async Task<List<T>> CollectAsync(IFieldSet projection)
		{
			if (!this.RequiresInMemoryProcessing()) return await base.CollectAsync(projection);

			String[] ensureProjection = this.ProjectionEnsureInMemoryProcessing();
			IFieldSet enhancedProjection = new FieldSet(projection.Fields).Ensure(ensureProjection);

			Paging page = this.Page;
			Ordering order = this.Order;
			this.Page = null;
			if (this.RequiresInMemoryOrdering()) this.Order = null;

			List<T> items = await base.CollectAsync(enhancedProjection);
			this.Order ??= order;
			if (this.RequiresInMemoryFiltering()) items = await this.FilterAsync(items);
			if (this.RequiresInMemoryOrdering()) items = await this.OrderAsync(items);
			items = this.ApplyPaging(page, items);

			this.Page = page;


			return items;
		}

		public override async Task<T> FirstAsync(IFieldSet projection)
		{
			if (!this.RequiresInMemoryProcessing()) return await base.FirstAsync(projection);

			String[] ensureProjection = this.ProjectionEnsureInMemoryProcessing();
			IFieldSet enhancedProjection = new FieldSet(projection.Fields).Ensure(ensureProjection);

			Paging page = this.Page;
			Ordering order = this.Order;
			this.Page = null;
			if (this.RequiresInMemoryOrdering()) this.Order = null;

			List<T> items = await base.CollectAsync(enhancedProjection);
			this.Order ??= order;
			if (this.RequiresInMemoryFiltering()) items = await this.FilterAsync(items);
			if (this.RequiresInMemoryOrdering()) items = await this.OrderAsync(items);
			items = this.ApplyPaging(page, items);
			T result = items.FirstOrDefault();

			this.Page = page;

			return result;
		}

		public override async Task<int> CountAsync()
		{
			//Only checking for in-memory filtering. Paging and Ordering not applying
			if (!this.RequiresInMemoryFiltering()) return await base.CountAsync();

			Paging page = this.Page;
			Ordering order = this.Order;
			this.Page = null;
			this.Order = null;

			List<T> items = await base.CollectAsync();
			if (this.RequiresInMemoryFiltering()) items = await this.FilterAsync(items);

			this.Order = order;
			this.Page = page;

			return items.Count;
		}

		public override async Task<Boolean> AnyAsync()
		{
			//Only checking for in-memory filtering. Paging and Ordering not applying
			if (!this.RequiresInMemoryFiltering()) return await base.AnyAsync();

			Paging page = this.Page;
			Ordering order = this.Order;
			this.Page = null;
			this.Order = null;

			List<T> items = await base.CollectAsync();
			if (this.RequiresInMemoryFiltering()) items = await this.FilterAsync(items);

			this.Order = order;
			this.Page = page;

			return items.Count > 0;
		}

		private List<T> ApplyPaging(Paging page, List<T> items)
		{
			if (page == null) return items;

			IEnumerable<T> workingSet = items;
			if (page.Offset > 0) workingSet = workingSet.Skip(page.Offset);
			if (page.Size > 0) workingSet = workingSet.Take(page.Size);
			return workingSet.ToList();
		}
	}
}
