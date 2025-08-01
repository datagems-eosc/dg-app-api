using Cite.Tools.Common.Extensions;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.Common;
using Microsoft.EntityFrameworkCore;

namespace DataGEMS.Gateway.App.Query
{
	public class DatasetLocalQuery : Cite.Tools.Data.Query.Query<DataManagement.Data.Dataset>
	{
		private List<Guid> _ids { get; set; }
		private List<Guid> _excludedIds { get; set; }
		private List<Guid> _collectionIds { get; set; }
		private String _like { get; set; }
		private String _license { get; set; }
		private List<String> _fieldsOfScience { get; set; }
		private RangeOf<DateOnly?> _publishedRange { get; set; }
		private RangeOf<long?> _sizeRange { get; set; }
		private String _mimeType { get; set; }
		private AuthorizationFlags _authorize { get; set; } = AuthorizationFlags.None;

		public DatasetLocalQuery(
			DataManagement.Data.DataManagementDbContext dbContext,
			IAuthorizationContentResolver authorizationContentResolver)
		{
			this._dbContext = dbContext;
			this._authorizationContentResolver = authorizationContentResolver;
		}

		private readonly DataManagement.Data.DataManagementDbContext _dbContext;
		private readonly IAuthorizationContentResolver _authorizationContentResolver;

		public DatasetLocalQuery Ids(IEnumerable<Guid> ids) { this._ids = ids?.ToList(); return this; }
		public DatasetLocalQuery Ids(Guid id) { this._ids = id.AsList(); return this; }
		public DatasetLocalQuery ExcludedIds(IEnumerable<Guid> excludedIds) { this._excludedIds = excludedIds?.ToList(); return this; }
		public DatasetLocalQuery ExcludedIds(Guid excludedId) { this._excludedIds = excludedId.AsList(); return this; }
		public DatasetLocalQuery CollectionIds(IEnumerable<Guid> collectionIds) { this._collectionIds = collectionIds?.ToList(); return this; }
		public DatasetLocalQuery CollectionIds(Guid collectionId) { this._collectionIds = collectionId.AsList(); return this; }
		public DatasetLocalQuery Like(String like) { this._like = like; return this; }
		public DatasetLocalQuery License(String license) { this._license = license; return this; }
		public DatasetLocalQuery FieldsOfScience(IEnumerable<String> fieldsOfScience) { this._fieldsOfScience = fieldsOfScience?.ToList(); return this; }
		public DatasetLocalQuery FieldsOfScience(String fieldsOfScience) { this._fieldsOfScience = fieldsOfScience.AsList(); return this; }
		public DatasetLocalQuery PublishedRange(RangeOf<DateOnly?> publishedRange) { this._publishedRange = publishedRange; return this; }
		public DatasetLocalQuery SizeRange(RangeOf<long?> sizeRange) { this._sizeRange = sizeRange; return this; }
		public DatasetLocalQuery MimeType(String mimeType) { this._mimeType = mimeType; return this; }
		public DatasetLocalQuery Authorize(AuthorizationFlags flags) { this._authorize = flags; return this; }
		public DatasetLocalQuery EnableTracking() { base.NoTracking = false; return this; }
		public DatasetLocalQuery DisableTracking() { base.NoTracking = true; return this; }
		public DatasetLocalQuery AsDistinct() { base.Distinct = true; return this; }
		public DatasetLocalQuery AsNotDistinct() { base.Distinct = false; return this; }

		protected override bool IsFalseQuery()
		{
			return this.IsEmpty(this._ids) || this.IsEmpty(this._excludedIds) || this.IsEmpty(this._collectionIds) || this.IsEmpty(this._fieldsOfScience);
		}

		public async Task<DataManagement.Data.Dataset> Find(Guid id, Boolean tracked = true)
		{
			if (tracked) return await this._dbContext.Datasets.FindAsync(id);
			else return await this._dbContext.Datasets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		protected override IQueryable<DataManagement.Data.Dataset> Queryable()
		{
			IQueryable<DataManagement.Data.Dataset> query = this._dbContext.Datasets.AsQueryable();
			return query;
		}

		protected override async Task<IQueryable<DataManagement.Data.Dataset>> ApplyAuthzAsync(IQueryable<DataManagement.Data.Dataset> query)
		{
			if (this._authorize.HasFlag(AuthorizationFlags.None)) return query;
			if (this._authorize.HasFlag(AuthorizationFlags.Permission) && await this._authorizationContentResolver.HasPermission(Permission.BrowseDataset)) return query;
			List<IQueryable<Guid>> predicates = new List<IQueryable<Guid>>();
			if (this._authorize.HasFlag(AuthorizationFlags.Context))
			{
				HashSet<Guid> permittedDatasetIds = new HashSet<Guid>();
				List<Guid> affiliatedDatasetIds = await this._authorizationContentResolver.AffiliatedDatasetIds();
				if (affiliatedDatasetIds != null) permittedDatasetIds.AddRange(affiliatedDatasetIds);

				List<String> affiliatedDatasetGroups = await this._authorizationContentResolver.AffiliatedDatasetGroupCodes();
				if (affiliatedDatasetGroups.Count > 0)
				{
					List<Guid> groupDatasetIds = await this._dbContext.DatasetCollections.Where(x => affiliatedDatasetGroups.Contains(x.Collection.Code)).Select(x => x.DatasetId).Distinct().ToListAsync();
					if (groupDatasetIds != null) permittedDatasetIds.AddRange(groupDatasetIds);
				}

				if (permittedDatasetIds != null && permittedDatasetIds.Count > 0)
				{
					predicates.Add(System.Linq.Queryable.Where(this._dbContext.Datasets, x =>
												permittedDatasetIds.Contains(x.Id))
											.Select(x => x.Id));
				}
			}
			//AuthorizationFlags.Owner not applicable
			if (predicates.Count == 0) return query.Where(x => false);

			IQueryable<Guid> union = this.AsUnion(predicates);
			return query.Where(x => union.Contains(x.Id));
		}

		protected override Task<IQueryable<DataManagement.Data.Dataset>> ApplyFiltersAsync(IQueryable<DataManagement.Data.Dataset> query)
		{
			if (this._ids != null) query = query.Where(x => this._ids.Contains(x.Id));
			if (this._excludedIds != null) query = query.Where(x => !this._excludedIds.Contains(x.Id));
			if (!String.IsNullOrEmpty(this._like)) query = query.Where(x => EF.Functions.ILike(x.Name, this._like));
			if (!String.IsNullOrEmpty(this._license)) query = query.Where(x => EF.Functions.ILike(x.License, this._license));
			if (!String.IsNullOrEmpty(this._mimeType)) query = query.Where(x => EF.Functions.ILike(x.MimeType, this._mimeType));
			if (this._fieldsOfScience != null) query = query.Where(x => this._fieldsOfScience.Contains(x.FieldOfScience));
			if (this._publishedRange != null)
			{
				if (this._publishedRange.Start.HasValue) 
				{
					DateTime rangeStart = this._publishedRange.Start.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
					query = query.Where(x => x.DatePublishedRaw != null && rangeStart <= x.DatePublishedRaw);
				}
				if (this._publishedRange.End.HasValue)
				{
					DateTime rangeEnd = this._publishedRange.End.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
					query = query.Where(x => x.DatePublishedRaw != null && rangeEnd >= x.DatePublishedRaw);
				}
			}
			if (this._sizeRange != null)
			{
				if (this._sizeRange.Start.HasValue)
				{
					long rangeStart = this._sizeRange.Start.Value;
					query = query.Where(x => x.Size != null && rangeStart <= x.Size);
				}
				if (this._sizeRange.End.HasValue)
				{
					long rangeEnd = this._sizeRange.End.Value;
					query = query.Where(x => x.Size != null && rangeEnd >= x.Size);
				}
			}
			if (this._collectionIds != null) query = query.Where(x => x.Collections.Any(y => this._collectionIds.Contains(y.CollectionId)));

			return Task.FromResult(query);
		}

		protected override IOrderedQueryable<DataManagement.Data.Dataset> OrderClause(IQueryable<DataManagement.Data.Dataset> query, Cite.Tools.Data.Query.OrderingFieldResolver item)
		{
			IOrderedQueryable<DataManagement.Data.Dataset> orderedQuery = null;
			if (this.IsOrdered(query)) orderedQuery = query as IOrderedQueryable<DataManagement.Data.Dataset>;

			if (item.Match(nameof(DataManagement.Model.Dataset.Id))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Id);
			else if (item.Match(nameof(DataManagement.Model.Dataset.Code))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Code);
			else if (item.Match(nameof(DataManagement.Model.Dataset.Name))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Name);
			else if (item.Match(nameof(DataManagement.Model.Dataset.License))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.License);
			else if (item.Match(nameof(DataManagement.Model.Dataset.MimeType))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.MimeType);
			else if (item.Match(nameof(DataManagement.Model.Dataset.Size))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Size);
			else if (item.Match(nameof(DataManagement.Model.Dataset.Version))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.Version);
			else if (item.Match(nameof(DataManagement.Model.Dataset.DatePublished))) orderedQuery = this.OrderOn(query, orderedQuery, item, x => x.DatePublishedRaw);
			else return null;

			return orderedQuery;
		}

		protected override List<String> FieldNamesOf(IEnumerable<Cite.Tools.Data.Query.FieldResolver> items)
		{
			HashSet<String> projectionFields = new HashSet<String>();
			foreach (Cite.Tools.Data.Query.FieldResolver item in items)
			{
				if (item.Match(nameof(DataManagement.Model.Dataset.Id))) projectionFields.Add(nameof(DataManagement.Data.Dataset.Id));
				else if (item.Match(nameof(DataManagement.Model.Dataset.Code))) projectionFields.Add(nameof(DataManagement.Data.Dataset.Code));
				else if (item.Match(nameof(DataManagement.Model.Dataset.Name))) projectionFields.Add(nameof(DataManagement.Data.Dataset.Name));
				else if (item.Match(nameof(DataManagement.Model.Dataset.Description))) projectionFields.Add(nameof(DataManagement.Data.Dataset.Description));
				else if (item.Match(nameof(DataManagement.Model.Dataset.License))) projectionFields.Add(nameof(DataManagement.Data.Dataset.License));
				else if (item.Match(nameof(DataManagement.Model.Dataset.Url))) projectionFields.Add(nameof(DataManagement.Data.Dataset.Url));
				else if (item.Match(nameof(DataManagement.Model.Dataset.Version))) projectionFields.Add(nameof(DataManagement.Data.Dataset.Version));
				else if (item.Match(nameof(DataManagement.Model.Dataset.Headline))) projectionFields.Add(nameof(DataManagement.Data.Dataset.Headline));
				else if (item.Match(nameof(DataManagement.Model.Dataset.Keywords))) projectionFields.Add(nameof(DataManagement.Data.Dataset.Keywords));
				else if (item.Match(nameof(DataManagement.Model.Dataset.FieldOfScience))) projectionFields.Add(nameof(DataManagement.Data.Dataset.FieldOfScience));
				else if (item.Match(nameof(DataManagement.Model.Dataset.Language))) projectionFields.Add(nameof(DataManagement.Data.Dataset.Language));
				else if (item.Match(nameof(DataManagement.Model.Dataset.Country))) projectionFields.Add(nameof(DataManagement.Data.Dataset.Country));
				else if (item.Match(nameof(DataManagement.Model.Dataset.DatePublished))) projectionFields.Add(nameof(DataManagement.Data.Dataset.DatePublishedRaw));
				else if (item.Match(nameof(DataManagement.Model.Dataset.Size))) projectionFields.Add(nameof(DataManagement.Data.Dataset.Size));
				else if (item.Match(nameof(DataManagement.Model.Dataset.MimeType))) projectionFields.Add(nameof(DataManagement.Data.Dataset.MimeType));
				else if (item.Match(nameof(DataManagement.Model.Dataset.ProfileRaw))) projectionFields.Add(nameof(DataManagement.Data.Dataset.Profile));
			}
			return projectionFields.ToList();
		}

		public async Task<List<DataManagement.Model.Dataset>> CollectAsyncAsModels()
		{
			List<DataManagement.Data.Dataset> datas = await this.CollectAsync();
			List<DataManagement.Model.Dataset> models = datas.Select(x => DatasetLocalQuery.ToModel(x)).ToList();
			return models;
		}

		public async Task<DataManagement.Model.Dataset> FirstAsyncAsModel()
		{
			DataManagement.Data.Dataset datas = await this.FirstAsync();
			DataManagement.Model.Dataset models = DatasetLocalQuery.ToModel(datas);
			return models;
		}

		private static DataManagement.Model.Dataset ToModel(DataManagement.Data.Dataset data)
		{
			if(data == null) return null;
			return new DataManagement.Model.Dataset()
			{
				Id = data.Id,
				Code = data.Code,
				Name = data.Name,
				Description = data.Description,
				License = data.License,
				MimeType = data.MimeType,
				Size = data.Size,
				Url = data.Url,
				Version = data.Version,
				Headline = data.Headline,
				Keywords = data.Keywords.ParseCsv(),
				FieldOfScience = data.FieldOfScience.ParseCsv(),
				Language = data.Language.ParseCsv(),
				Country = data.Country.ParseCsv(),
				DatePublished = data.DatePublished,
				ProfileRaw = data.Profile
			};
		}
	}
}
