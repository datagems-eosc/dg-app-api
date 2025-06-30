using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGEMS.Gateway.App.Query
{
	public class ConversationDatasetQuery : Query<ConversationDataset>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _conversationIds { get; set; }
		private List<Guid> _datasetIds { get; set; }
		private List<IsActive> _isActive { get; set; }
		private ConversationQuery _conversationQuery { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public ConversationDatasetQuery(
			AppDbContext dbContext,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly AppDbContext _dbContext;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public ConversationDatasetQuery Ids(IEnumerable<Guid> ids) { this._ids = this.ToList(ids); return this; }
		public ConversationDatasetQuery Ids(Guid id) { this._ids = this.ToList(id.AsArray()); return this; }
		public ConversationDatasetQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = this.ToList(excludedIds); return this; }
		public ConversationDatasetQuery ExcludedIds(Guid excludedId) { this._excludedIds = this.ToList(excludedId.AsArray()); return this; }
		public ConversationDatasetQuery DatasetIds(IEnumerable<Guid> datasetIds) { this._datasetIds = this.ToList(datasetIds); return this; }
		public ConversationDatasetQuery DatasetIds(Guid datasetId) { this._datasetIds = this.ToList(datasetId.AsArray()); return this; }
		public ConversationDatasetQuery ConversationIds(IEnumerable<Guid> conversationIds) { this._conversationIds = this.ToList(conversationIds); return this; }
		public ConversationDatasetQuery ConversationIds(Guid conversationId) { this._conversationIds = this.ToList(conversationId.AsArray()); return this; }
		public ConversationDatasetQuery ConversationSubQuery(ConversationQuery subquery) { this._conversationQuery = subquery; return this; }
		public ConversationDatasetQuery IsActive(IEnumerable<IsActive> isActive) { this._isActive = this.ToList(isActive); return this; }
		public ConversationDatasetQuery IsActive(IsActive isActive) { this._isActive = this.ToList(isActive.AsArray()); return this; }
		public ConversationDatasetQuery EnableTracking() { base.NoTracking = false; return this; }
		public ConversationDatasetQuery DisableTracking() { base.NoTracking = true; return this; }
		public ConversationDatasetQuery AsDistinct() { base.Distinct = true; return this; }
		public ConversationDatasetQuery AsNotDistinct() { base.Distinct = false; return this; }
		public ConversationDatasetQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._datasetIds) || this.IsEmpty(this._conversationIds) ||
				this.IsEmpty(this._excludedIds) || this.IsEmpty(this._isActive) || this.IsFalseQuery(this._conversationQuery);
		}

		public async Task<ConversationDataset> Find(Guid id, Boolean tracked = true)
		{
			if (tracked) return await this._dbContext.ConversationDatasets.FindAsync(id);
			else return await this._dbContext.ConversationDatasets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		protected override IQueryable<ConversationDataset> Queryable()
		{
			IQueryable<ConversationDataset> query = this._dbContext.ConversationDatasets.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<ConversationDataset>> ApplyAuthzAsync(IQueryable<ConversationDataset> query)
		{
			if (this._authorize.HasFlag(AuthorizationFlags.None)) return query;
			if (this._authorize.HasFlag(AuthorizationFlags.Permission))
			{
				if (await this._authorizationContentResolver.HasPermission(Permission.BrowseConversationDataset)) return query;
			}
			if (this._authorize.HasFlag(AuthorizationFlags.Owner))
			{
				String currentUser = this._authorizationContentResolver.CurrentUser();
				if (!String.IsNullOrEmpty(currentUser)) return query.Where(x => x.Conversation.User.IdpSubjectId == currentUser);
			}
			//AuthorizationFlags.Context not applicable
			return query.Where(x => false);
		}

		protected override async Task<IQueryable<ConversationDataset>> ApplyFiltersAsync(IQueryable<ConversationDataset> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._datasetIds != null) query = query.Where(x => this._datasetIds.Contains(x.DatasetId));
			if (this._conversationIds != null) query = query.Where(x => this._conversationIds.Contains(x.ConversationId));
			if (this._isActive != null) query = query.Where(x => this._isActive.Contains(x.IsActive));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			if (this._conversationQuery != null)
			{
				IQueryable<Guid> subQuery = await this.BindSubQueryAsync(this._conversationQuery, this._dbContext.Conversations, y => y.Id);
				subQuery = subQuery.Distinct();
				query = query.Where(x => subQuery.Contains(x.ConversationId));
			}
			return query;
		}

		protected override IOrderedQueryable<ConversationDataset> OrderClause(IQueryable<ConversationDataset> query, OrderingFieldResolver item)
		{
			IOrderedQueryable<ConversationDataset> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<ConversationDataset>;

			if (item.Match(nameof(Model.ConversationDataset.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(Model.ConversationDataset.Dataset), nameof(Model.Dataset.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.DatasetId);
			else if (item.Match(nameof(Model.ConversationDataset.IsActive))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.IsActive);
			else if (item.Match(nameof(Model.ConversationDataset.Conversation), nameof(Model.Conversation.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Conversation.Id);
			else if (item.Match(nameof(Model.ConversationDataset.Conversation), nameof(Model.Conversation.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Conversation.Name);
			else if (item.Match(nameof(Model.ConversationDataset.Conversation), nameof(Model.Conversation.User.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Conversation.User.Id);
			else if (item.Match(nameof(Model.ConversationDataset.Conversation), nameof(Model.Conversation.User.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Conversation.User.Name);
			else if (item.Match(nameof(Model.ConversationDataset.CreatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.CreatedAt);
			else if (item.Match(nameof(Model.ConversationDataset.UpdatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UpdatedAt);
			else return null;

			return orderedQuery;
		}

		protected override List<String> FieldNamesOf(IEnumerable<FieldResolver> items)
		{
			HashSet<String> projectionFields = new HashSet<String>();
			foreach (FieldResolver item in items)
			{
				if (item.Match(nameof(Model.ConversationDataset.Id))) projectionFields.Add(nameof(ConversationDataset.Id));
				else if (item.Prefix(nameof(Model.ConversationDataset.Dataset))) projectionFields.Add(nameof(ConversationDataset.DatasetId));
				else if (item.Match(nameof(Model.ConversationDataset.IsActive))) projectionFields.Add(nameof(ConversationDataset.IsActive));
				else if (item.Prefix(nameof(Model.ConversationDataset.Conversation))) projectionFields.Add(nameof(ConversationDataset.ConversationId));
				else if (item.Match(nameof(Model.ConversationDataset.CreatedAt))) projectionFields.Add(nameof(ConversationDataset.CreatedAt));
				else if (item.Match(nameof(Model.ConversationDataset.UpdatedAt))) projectionFields.Add(nameof(ConversationDataset.UpdatedAt));
				else if (item.Match(nameof(Model.ConversationDataset.ETag))) projectionFields.Add(nameof(ConversationDataset.UpdatedAt));
			}
			return projectionFields.ToList();
		}
	}
}
