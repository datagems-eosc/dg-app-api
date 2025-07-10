using DataGEMS.Gateway.App.Data;
using Cite.Tools.Data.Query;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using Cite.Tools.Common.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Query
{
	public class ConversationQuery : Query<Conversation>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _userIds { get; set; }
		private String _like { get; set; }
		private List<IsActive> _isActive { get; set; }
		private ConversationDatasetQuery _conversationDatasetQuery { get; set; }
		private ConversationMessageQuery _conversationMessageQuery { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public ConversationQuery(
			AppDbContext dbContext,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly AppDbContext _dbContext;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public ConversationQuery Ids(IEnumerable<Guid> ids) { this._ids = this.ToList(ids); return this; }
		public ConversationQuery Ids(Guid id) { this._ids = this.ToList(id.AsArray()); return this; }
		public ConversationQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = this.ToList(excludedIds); return this; }
		public ConversationQuery ExcludedIds(Guid excludedId) { this._excludedIds = this.ToList(excludedId.AsArray()); return this; }
		public ConversationQuery UserIds(IEnumerable<Guid> userIds) { this._userIds = this.ToList(userIds); return this; }
		public ConversationQuery UserIds(Guid userId) { this._userIds = this.ToList(userId.AsArray()); return this; }
		public ConversationQuery Like(String like) { this._like = like; return this; }
		public ConversationQuery IsActive(IEnumerable<IsActive> isActive) { this._isActive = this.ToList(isActive); return this; }
		public ConversationQuery IsActive(IsActive isActive) { this._isActive = this.ToList(isActive.AsArray()); return this; }
		public ConversationQuery ConversationDatasetSubQuery(ConversationDatasetQuery subquery) { this._conversationDatasetQuery = subquery; return this; }
		public ConversationQuery ConversationMessageSubQuery(ConversationMessageQuery subquery) { this._conversationMessageQuery = subquery; return this; }
		public ConversationQuery EnableTracking() { base.NoTracking = false; return this; }
		public ConversationQuery DisableTracking() { base.NoTracking = true; return this; }
		public ConversationQuery AsDistinct() { base.Distinct = true; return this; }
		public ConversationQuery AsNotDistinct() { base.Distinct = false; return this; }
		public ConversationQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._excludedIds) || this.IsEmpty(this._userIds) ||
				this.IsEmpty(this._isActive) || this.IsFalseQuery(this._conversationDatasetQuery) || this.IsFalseQuery(this._conversationMessageQuery);
		}

		public async Task<Conversation> Find(Guid id, Boolean tracked = true)
		{
			if (tracked) return await this._dbContext.Conversations.FindAsync(id);
			else return await this._dbContext.Conversations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		protected override IQueryable<Conversation> Queryable()
		{
			IQueryable<Conversation> query = this._dbContext.Conversations.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<Conversation>> ApplyAuthzAsync(IQueryable<Conversation> query)
		{
			if (this._authorize.HasFlag(AuthorizationFlags.None)) return query;
			if (this._authorize.HasFlag(AuthorizationFlags.Permission))
			{
				if (await this._authorizationContentResolver.HasPermission(Permission.BrowseConversation)) return query;
			}
			if (this._authorize.HasFlag(AuthorizationFlags.Owner))
			{
				String currentUser = this._authorizationContentResolver.CurrentUser();
				if (!String.IsNullOrEmpty(currentUser)) return query.Where(x => x.User.IdpSubjectId == currentUser);
			}
			//AuthorizationFlags.Context not applicable
			return query.Where(x => false);
		}

		protected override async Task<IQueryable<Conversation>> ApplyFiltersAsync(IQueryable<Conversation> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._userIds != null) query = query.Where(x => this._userIds.Contains(x.UserId));
			if (this._isActive != null) query = query.Where(x => this._isActive.Contains(x.IsActive));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			if (!String.IsNullOrEmpty(this._like)) query = query.Where(x => EF.Functions.ILike(x.Name, this._like));
			if (this._conversationDatasetQuery != null)
			{
				IQueryable<Guid> subQuery = await this.BindSubQueryAsync(this._conversationDatasetQuery, this._dbContext.ConversationDatasets, y => y.ConversationId);
				subQuery = subQuery.Distinct();
				query = query.Where(x => subQuery.Contains(x.Id));
			}
			if (this._conversationMessageQuery != null)
			{
				IQueryable<Guid> subQuery = await this.BindSubQueryAsync(this._conversationMessageQuery, this._dbContext.ConversationMessages, y => y.ConversationId);
				subQuery = subQuery.Distinct();
				query = query.Where(x => subQuery.Contains(x.Id));
			}
			return query;
		}

		protected override IOrderedQueryable<Conversation> OrderClause(IQueryable<Conversation> query, OrderingFieldResolver item)
		{
			IOrderedQueryable<Conversation> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<Conversation>;

			if (item.Match(nameof(Model.Conversation.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(Model.Conversation.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Name);
			else if (item.Match(nameof(Model.Conversation.IsActive))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.IsActive);
			else if (item.Match(nameof(Model.Conversation.User), nameof(Model.Conversation.User.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UserId);
			else if (item.Match(nameof(Model.Conversation.User), nameof(Model.Conversation.User.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.User.Name);
			else if (item.Match(nameof(Model.Conversation.User), nameof(Model.Conversation.User.Email))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.User.Email);
			else if (item.Match(nameof(Model.Conversation.CreatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.CreatedAt);
			else if (item.Match(nameof(Model.Conversation.UpdatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.UpdatedAt);
			else return null;

			return orderedQuery;
		}

		protected override List<String> FieldNamesOf(IEnumerable<FieldResolver> items)
		{
			HashSet<String> projectionFields = new HashSet<String>();
			foreach (FieldResolver item in items)
			{
				if (item.Match(nameof(Model.Conversation.Id))) projectionFields.Add(nameof(Conversation.Id));
				else if (item.Match(nameof(Model.Conversation.Name))) projectionFields.Add(nameof(Conversation.Name));
				else if (item.Prefix(nameof(Model.Conversation.User))) projectionFields.Add(nameof(Conversation.UserId));
				else if (item.Prefix(nameof(Model.Conversation.Datasets))) projectionFields.Add(nameof(Conversation.Id));
				else if (item.Prefix(nameof(Model.Conversation.Messages))) projectionFields.Add(nameof(Conversation.Id));
				else if (item.Match(nameof(Model.Conversation.IsActive))) projectionFields.Add(nameof(Conversation.IsActive));
				else if (item.Match(nameof(Model.Conversation.CreatedAt))) projectionFields.Add(nameof(Conversation.CreatedAt));
				else if (item.Match(nameof(Model.Conversation.UpdatedAt))) projectionFields.Add(nameof(Conversation.UpdatedAt));
				else if (item.Match(nameof(Model.Conversation.ETag))) projectionFields.Add(nameof(Conversation.UpdatedAt));
			}
			return projectionFields.ToList();
		}
	}
}
