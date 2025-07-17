using Cite.Tools.Data.Query;
using Cite.Tools.Validation;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.Common.Validation;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Query;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace DataGEMS.Gateway.Api.Model.Lookup
{
	public class DatasetLookup : Cite.Tools.Data.Query.Lookup
	{
		[SwaggerSchema(description:"Limit lookup to items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> Ids { get; set; }
		[SwaggerSchema(description: "Exclude from the lookup items with specific ids. If set, the list of ids must not be empty")]
		public List<Guid> ExcludedIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items belonging to the specific collection ids. If set, the list of ids must not be empty")]
		public List<Guid> CollectionIds { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose name matches the pattern")]
		public String Like { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose license matches the provided value")]
		public String License { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose mime type matches the provided value")]
		public String MimeType { get; set; }
		[SwaggerSchema(description: "Limit lookup to items belonging to the specific fields of science. If set, the list of values must not be empty")]
		public List<String> FieldsOfScience { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose published date is within the provided bounds. Any of the bounds can be left unset")]
		public RangeOf<DateOnly?> PublishedRange { get; set; }
		[SwaggerSchema(description: "Limit lookup to items whose size is within the provided bounds. Any of the bounds can be left unset")]
		public RangeOf<long?> SizeRange { get; set; }

		public DatasetLocalQuery Enrich(QueryFactory factory)
		{
			DatasetLocalQuery query = factory.Query<DatasetLocalQuery>();

			if (this.Ids != null) query.Ids(this.Ids);
			if (this.ExcludedIds != null) query.ExcludedIds(this.ExcludedIds);
			if (this.CollectionIds != null) query.CollectionIds(this.CollectionIds);
			if (!String.IsNullOrEmpty(this.Like)) query.Like(this.Like);
			if (!String.IsNullOrEmpty(this.License)) query.License(this.License);
			if (!String.IsNullOrEmpty(this.MimeType)) query.MimeType(this.MimeType);
			if (this.FieldsOfScience != null) query.FieldsOfScience(this.FieldsOfScience);
			if (this.PublishedRange != null) query.PublishedRange(this.PublishedRange);
			if (this.SizeRange != null) query.SizeRange(this.SizeRange);

			this.EnrichCommon(query);

			return query;
		}

		public class QueryValidator : BaseValidator<DatasetLookup>
		{
			public QueryValidator(
				IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> localizer,
				ValidatorFactory validatorFactory,
				ILogger<QueryValidator> logger,
				ErrorThesaurus errors) : base(validatorFactory, logger, errors)
			{
				this._localizer = localizer;
			}

			private readonly IStringLocalizer<DataGEMS.Gateway.Resources.MySharedResources> _localizer;

			protected override IEnumerable<ISpecification> Specifications(DatasetLookup item)
			{
				return new ISpecification[]{
					//ids must be null or not empty
					this.Spec()
						.Must(() => !item.Ids.IsNotNullButEmpty())
						.FailOn(nameof(DatasetLookup.Ids)).FailWith(this._localizer["validation_setButEmpty", nameof(DatasetLookup.Ids)]),
					//excludedIds must be null or not empty
					this.Spec()
						.Must(() => !item.ExcludedIds.IsNotNullButEmpty())
						.FailOn(nameof(DatasetLookup.ExcludedIds)).FailWith(this._localizer["validation_setButEmpty", nameof(DatasetLookup.ExcludedIds)]),
					//datasetIds must be null or not empty
					this.Spec()
						.Must(() => !item.CollectionIds.IsNotNullButEmpty())
						.FailOn(nameof(DatasetLookup.CollectionIds)).FailWith(this._localizer["validation_setButEmpty", nameof(DatasetLookup.CollectionIds)]),
					//paging without ordering not supported
					this.Spec()
						.If(()=> item.Page != null && !item.Page.IsEmpty)
						.Must(() => !item.Order.IsEmpty)
						.FailOn(nameof(DatasetLookup.Page)).FailWith(this._localizer["validation_pagingWithoutOrdering"]),
				};
			}
		}
	}
}
