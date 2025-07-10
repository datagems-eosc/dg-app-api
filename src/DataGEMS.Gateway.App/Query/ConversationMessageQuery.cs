using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Data;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Query
{
	public class ConversationMessageQuery : Query<ConversationMessage>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _conversationIds { get; set; }
		private List<ConversationMessageKind> _kind { get; set; }
		private ConversationQuery _conversationQuery { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public ConversationMessageQuery(
			AppDbContext dbContext,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly AppDbContext _dbContext;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public ConversationMessageQuery Ids(IEnumerable<Guid> ids) { this._ids = this.ToList(ids); return this; }
		public ConversationMessageQuery Ids(Guid id) { this._ids = this.ToList(id.AsArray()); return this; }
		public ConversationMessageQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = this.ToList(excludedIds); return this; }
		public ConversationMessageQuery ExcludedIds(Guid excludedId) { this._excludedIds = this.ToList(excludedId.AsArray()); return this; }
		public ConversationMessageQuery ConversationIds(IEnumerable<Guid> conversationIds) { this._conversationIds = this.ToList(conversationIds); return this; }
		public ConversationMessageQuery ConversationIds(Guid conversationId) { this._conversationIds = this.ToList(conversationId.AsArray()); return this; }
		public ConversationMessageQuery ConversationSubQuery(ConversationQuery subquery) { this._conversationQuery = subquery; return this; }
		public ConversationMessageQuery EnableTracking() { base.NoTracking = false; return this; }
		public ConversationMessageQuery DisableTracking() { base.NoTracking = true; return this; }
		public ConversationMessageQuery AsDistinct() { base.Distinct = true; return this; }
		public ConversationMessageQuery AsNotDistinct() { base.Distinct = false; return this; }
		public ConversationMessageQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._conversationIds) || this.IsEmpty(this._kind) ||
				this.IsEmpty(this._excludedIds) || this.IsFalseQuery(this._conversationQuery);
		}

		public async Task<ConversationMessage> Find(Guid id, Boolean tracked = true)
		{
			if (tracked) return await this._dbContext.ConversationMessages.FindAsync(id);
			else return await this._dbContext.ConversationMessages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		protected override IQueryable<ConversationMessage> Queryable()
		{
			IQueryable<ConversationMessage> query = this._dbContext.ConversationMessages.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<ConversationMessage>> ApplyAuthzAsync(IQueryable<ConversationMessage> query)
		{
			if (this._authorize.HasFlag(AuthorizationFlags.None)) return query;
			if (this._authorize.HasFlag(AuthorizationFlags.Permission))
			{
				if (await this._authorizationContentResolver.HasPermission(Permission.BrowseConversationMessage)) return query;
			}
			if (this._authorize.HasFlag(AuthorizationFlags.Owner))
			{
				String currentUser = this._authorizationContentResolver.CurrentUser();
				if (!String.IsNullOrEmpty(currentUser)) return query.Where(x => x.Conversation.User.IdpSubjectId == currentUser);
			}
			//AuthorizationFlags.Context not applicable
			return query.Where(x => false);
		}

		protected override async Task<IQueryable<ConversationMessage>> ApplyFiltersAsync(IQueryable<ConversationMessage> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._conversationIds != null) query = query.Where(x => this._conversationIds.Contains(x.ConversationId));
			if (this._kind != null) query = query.Where(x => this._kind.Contains(x.Kind));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			if (this._conversationQuery != null)
			{
				IQueryable<Guid> subQuery = await this.BindSubQueryAsync(this._conversationQuery, this._dbContext.Conversations, y => y.Id);
				subQuery = subQuery.Distinct();
				query = query.Where(x => subQuery.Contains(x.ConversationId));
			}
			return query;
		}

		protected override IOrderedQueryable<ConversationMessage> OrderClause(IQueryable<ConversationMessage> query, OrderingFieldResolver item)
		{
			IOrderedQueryable<ConversationMessage> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<ConversationMessage>;

			if (item.Match(nameof(Model.ConversationMessage.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(Model.ConversationDataset.Conversation), nameof(Model.Conversation.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Conversation.Id);
			else if (item.Match(nameof(Model.ConversationDataset.Conversation), nameof(Model.Conversation.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Conversation.Name);
			else if (item.Match(nameof(Model.ConversationDataset.Conversation), nameof(Model.Conversation.User.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Conversation.User.Id);
			else if (item.Match(nameof(Model.ConversationDataset.Conversation), nameof(Model.Conversation.User.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Conversation.User.Name);
			else if (item.Match(nameof(Model.ConversationMessage.Kind))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Kind);
			else if (item.Match(nameof(Model.ConversationMessage.CreatedAt))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.CreatedAt);
			else return null;

			return orderedQuery;
		}

		protected override List<String> FieldNamesOf(IEnumerable<FieldResolver> items)
		{
			HashSet<String> projectionFields = new HashSet<String>();
			foreach (FieldResolver item in items)
			{
				if (item.Match(nameof(Model.ConversationMessage.Id))) projectionFields.Add(nameof(ConversationMessage.Id));
				else if (item.Match(nameof(Model.ConversationMessage.Kind))) projectionFields.Add(nameof(ConversationMessage.Kind));
				else if (item.Prefix(nameof(Model.ConversationMessage.Conversation))) projectionFields.Add(nameof(ConversationMessage.ConversationId));
				else if (item.Match(nameof(Model.ConversationMessage.Data))) projectionFields.Add(nameof(ConversationMessage.Data));
				else if (item.Match(nameof(Model.ConversationMessage.CreatedAt))) projectionFields.Add(nameof(ConversationMessage.CreatedAt));
			}
			return projectionFields.ToList();
		}
	}
}
