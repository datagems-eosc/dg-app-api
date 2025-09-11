using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cite.Tools.Common.Extensions;
using Cite.Tools.Data.Query;
using Cite.Tools.Json;
using Cite.Tools.Logging.Extensions;
using DataGEMS.Gateway.App.Common;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using DataGEMS.Gateway.App.LogTracking;
using DataGEMS.Gateway.App.Model;
using DataGEMS.Gateway.App.Model.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace DataGEMS.Gateway.App.Query
{
	public class WorkflowExecutionQuery : Cite.Tools.Data.Query.IQuery
	{
		private String _id { get; set; }
		private RangeOf<DateOnly?> _queuedAt { get; set; }
		private RangeOf<DateOnly?> _startDate { get; set; }
		private RangeOf<DateOnly?> _endDate { get; set; }
		private RangeOf<DateOnly?> _runAfter {  get; set; }
		private List<WorkflowRunType> _runType { get; set; }
		private List<WorkflowRunState> _state { get; set; }
		private String _triggeredBy { get; set; }
		private List<String?> _listDagIds { get; set; }

		public Paging Page { get; set; }
		public Ordering Order { get; set; }

		private readonly IHttpClientFactory _httpClientFactory;
		private readonly Service.Airflow.AirflowConfig _config;
		private readonly ILogger<WorkflowExecutionQuery> _logger;
		private readonly ErrorThesaurus _errors;
		private readonly LogCorrelationScope _logCorrelationScope;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly Service.Airflow.IAirflowAccessTokenService _airflowAccessTokenService;

		public WorkflowExecutionQuery(
			IHttpClientFactory httpClientFactory,
			Service.Airflow.AirflowConfig config,
			ILogger<WorkflowExecutionQuery> logger,
			JsonHandlingService jsonHandlingService,
			LogCorrelationScope logCorrelationScope,
			Service.Airflow.IAirflowAccessTokenService airflowAccessTokenService,
			ErrorThesaurus errors)
		{
			this._httpClientFactory = httpClientFactory;
			this._config = config;
			this._logger = logger;
			this._errors = errors;
			this._logCorrelationScope = logCorrelationScope;
			this._jsonHandlingService = jsonHandlingService;
			this._airflowAccessTokenService = airflowAccessTokenService;
		}

		public WorkflowExecutionQuery Id(string id) { this._id = id; return this; }
		public WorkflowExecutionQuery TriggeredBy(string triggeredBy) { this._triggeredBy = triggeredBy; return this; }
		public WorkflowExecutionQuery State(IEnumerable<WorkflowRunState> state) { this._state = state?.ToList(); return this; }
		public WorkflowExecutionQuery State(WorkflowRunState state){ this._state = state.AsList(); return this; }
		public WorkflowExecutionQuery RunType(WorkflowRunType runType) { this._runType = runType.AsList(); return this; }
		public WorkflowExecutionQuery RunType(IEnumerable<WorkflowRunType> runType) { this._runType = runType?.ToList(); return this; }
		public WorkflowExecutionQuery QueuedAt(RangeOf<DateOnly?> queuedAt) { this._queuedAt = queuedAt; return this; }
		public WorkflowExecutionQuery RunAfter(RangeOf<DateOnly?> runAfter) {  this._runAfter = runAfter; return this; }
		public WorkflowExecutionQuery StartDate(RangeOf<DateOnly?> startDate) { this._startDate = startDate; return this; }
		public WorkflowExecutionQuery EndDate(RangeOf<DateOnly?> endDate) { this._endDate = endDate; return this; }
		public WorkflowExecutionQuery ListDagIds(List<String?> listDagIds) { this._listDagIds = listDagIds; return this; }

		protected bool IsFalseQuery()
		{
			return this._state.IsNotNullButEmpty();
		}

		public async Task<Service.Airflow.Model.AirflowDagExecution> ByIdAsync()
		{
			if (String.IsNullOrEmpty(this._id)) return null;

			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.DagExecutionListEndpoint.Replace("{id}", this._id)}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				Service.Airflow.Model.AirflowDagExecution model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowDagExecution>(content);
				return model;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, "problem converting response {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}
		}


		public async Task<Service.Airflow.Model.AirflowDagExecution> BatchByIdAsync()
		{
			if (String.IsNullOrEmpty(this._id)) return null;

			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.DagExecutionBatchListEndpoint.Replace("{id}", this._id)}");
			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			String content = await this.SendRequest(request);
			try
			{
				Service.Airflow.Model.AirflowDagExecution model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowDagExecution>(content);
				return model;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, "problem converting response {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}
		}

		//List Cool
		public async Task<List<Service.Airflow.Model.AirflowDagExecution>> CollectAsync(string dagid)
		{
			Service.Airflow.Model.AirflowDagListExecution model = await this.CollectBaseAsync(dagid,false);
			return model?.DagRuns ?? Enumerable.Empty<Service.Airflow.Model.AirflowDagExecution>().ToList();
		}

		public async Task<int> CountAsync(string dagid)
		{
			Service.Airflow.Model.AirflowDagListExecution model = await this.CollectBaseAsync(dagid,true);
			return model?.TotalEntries ?? 0;
		}


		//Batch Collect
		public async Task<List<Service.Airflow.Model.AirflowDagExecution>> CollectBatchAsync()
		{
			Service.Airflow.Model.AirflowDagListExecution model = await this.CollectBatchBaseAsync(false);
			return model?.DagRuns ?? Enumerable.Empty<Service.Airflow.Model.AirflowDagExecution>().ToList();
		}

		public async Task<int> CountBatchAsync()
		{
			Service.Airflow.Model.AirflowDagListExecution model = await this.CollectBatchBaseAsync(true);
			return model?.TotalEntries ?? 0;
		}

		private async Task<Service.Airflow.Model.AirflowDagListExecution> CollectBatchBaseAsync(Boolean useInCount)
		{
			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			HttpRequestMessage batchrequest = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.DagExecutionBatchListEndpoint}");

			batchrequest.Headers.Add(HeaderNames.Accept, "application/json");
			batchrequest.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			Service.Airflow.Model.AirflowBatchExecutionRequest requestModel = new Service.Airflow.Model.AirflowBatchExecutionRequest
			{
					DagIds = new List<string> { "hello_world", "123456" }
			};

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{this._config.BaseUrl}{this._config.DagExecutionListEndpoint}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(requestModel), Encoding.UTF8, "application/json")
			};


			String content = await this.SendRequest(batchrequest);
			try
			{
				Service.Airflow.Model.AirflowDagListExecution model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowDagListExecution>(content);
				return model;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, "problem converting response {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}
		}


		// List
		private async Task<Service.Airflow.Model.AirflowDagListExecution> CollectBaseAsync(string dagId ,Boolean useInCount)
		{
			String token = await this._airflowAccessTokenService.GetAirflowAccessTokenAsync();
			if (token == null) throw new DGApplicationException(this._errors.TokenExchange.Code, this._errors.TokenExchange.Message);

			Service.Airflow.Model.AirflowListExecutionRequest requestModel = new Service.Airflow.Model.AirflowListExecutionRequest
			{
				DagId = dagId
			};

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.DagExecutionListEndpoint.Replace("{id}", dagId)}")
			{
				Content = new StringContent(this._jsonHandlingService.ToJson(requestModel), Encoding.UTF8, "application/json")
			};

			request.Headers.Add(HeaderNames.Accept, "application/json");
			request.Headers.Add(HeaderNames.Authorization, $"Bearer {token}");

			QueryString qs = new QueryString();

			if (!string.IsNullOrEmpty(this._triggeredBy))  qs = qs.Add("triggered_by", this._triggeredBy);

			if (this._startDate != null)
			{
				if (this._startDate.Start.HasValue)
				{
					DateTime rangeStart = this._startDate.Start.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
					qs = qs.Add("start_date_gte", rangeStart.ToString("o", CultureInfo.InvariantCulture));
				}
				if (this._startDate.End.HasValue)
				{
					DateTime rangeEnd = this._startDate.End.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
					qs = qs.Add("start_date_lte", rangeEnd.ToString("o", CultureInfo.InvariantCulture));
				}
			}
			if (this._endDate != null)
			{
				if (this._endDate.Start.HasValue)
				{
					DateTime rangeStart = this._endDate.Start.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
					qs = qs.Add("end_date_gte", rangeStart.ToString("o", CultureInfo.InvariantCulture));
				}
				if (this._endDate.End.HasValue)
				{
					DateTime rangeEnd = this._endDate.End.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
					qs = qs.Add("end_date_lte", rangeEnd.ToString("o", CultureInfo.InvariantCulture));
				}
			}
			if (this._queuedAt != null)
			{
				if (this._queuedAt.Start.HasValue)
				{
					DateTime rangeStart = this._queuedAt.Start.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
					qs = qs.Add("start_date_gte", rangeStart.ToString("o", CultureInfo.InvariantCulture));
				}
				if (this._queuedAt.End.HasValue)
				{
					DateTime rangeEnd = this._queuedAt.End.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
					qs = qs.Add("start_date_lte", rangeEnd.ToString("o", CultureInfo.InvariantCulture));
				}
			}
			if (this._runAfter != null)
			{
				if (this._runAfter.Start.HasValue)
				{
					DateTime rangeStart = this._runAfter.Start.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
					qs = qs.Add("run_after_gte", rangeStart.ToString("o", CultureInfo.InvariantCulture));
				}
				if (this._runAfter.End.HasValue)
				{
					DateTime rangeEnd = this._runAfter.End.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
					qs = qs.Add("run_after_lte", rangeEnd.ToString("o", CultureInfo.InvariantCulture));
				}
			}

			if (this._runType != null ) this._runType.ForEach(x => qs = qs.Add("run_type", x.ToString()));
			if (this._state != null) this._state.ForEach(x => qs = qs.Add("state", x.ToString()));

			String orderBy = this.ApplyOrdering();
			if (!String.IsNullOrEmpty(orderBy) && !useInCount) qs = qs.Add("order_by", orderBy);

			if (useInCount)
			{
				qs = qs.Add("offset", 0.ToString());
				qs = qs.Add("limit", 1.ToString());
			}
			else if (this.Page != null && !this.Page.IsEmpty)
			{
				if (this.Page.Offset >= 0) qs = qs.Add("offset", this.Page.Offset.ToString());
				if (this.Page.Size > 0) qs = qs.Add("limit", this.Page.Size.ToString());
			}

			String content = await this.SendRequest(request);
			try
			{
				Service.Airflow.Model.AirflowDagListExecution model = this._jsonHandlingService.FromJson<Service.Airflow.Model.AirflowDagListExecution>(content);
				return model;
			}
			catch (System.Exception ex)
			{
				this._logger.Error(ex, "problem converting response {content}", content);
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, null, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}
		}

		private async Task<String> SendRequest(HttpRequestMessage request)
		{
			HttpResponseMessage response = null;
			try { response = await this._httpClientFactory.CreateClient().SendAsync(request); }
			catch (System.Exception ex)
			{
				this._logger.Error(ex, $"could not complete the request. response was {response?.StatusCode}");
				throw new DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId);
			}

			try { response.EnsureSuccessStatusCode(); }
			catch (System.Exception ex)
			{
				String errorPayload = null;
				try { errorPayload = await response.Content.ReadAsStringAsync(); } catch (System.Exception) { }
				this._logger.Error(ex, "non successful response. StatusCode was {statusCode} and Payload {errorPayload}", response?.StatusCode, errorPayload);
				Boolean includeErrorPayload = response != null && response.StatusCode == System.Net.HttpStatusCode.BadRequest;
				throw new Exception.DGUnderpinningException(this._errors.UnderpinningService.Code, this._errors.UnderpinningService.Message, (int?)response?.StatusCode, UnderpinningServiceType.Workflow, this._logCorrelationScope.CorrelationId, includeErrorPayload ? errorPayload : null);
			}
			String content = await response.Content.ReadAsStringAsync();
			return content;
		}

		private String ApplyOrdering()
		{
			if (this.Order == null || this.Order.IsEmpty) return null;

			foreach (OrderingFieldResolver field in this.Order.Items.Select(x => new OrderingFieldResolver(x)).ToList())
			{
				if (field.Match(nameof(App.Model.WorkflowExecution.Id))) return "dag_id";
				else if (field.Match(nameof(App.Model.WorkflowExecution.Start))) return "start_date";
				else if (field.Match(nameof(App.Model.WorkflowExecution.End))) return "end_date";
				else if (field.Match(nameof(App.Model.WorkflowExecution.TriggeredBy))) return "triggered_by";
				else if (field.Match(nameof(App.Model.WorkflowExecution.QueuedAt))) return "queued_At";
				else if (field.Match(nameof(App.Model.WorkflowExecution.State))) return "state";
				else if (field.Match(nameof(App.Model.WorkflowExecution.RunType))) return "run_type";
			}
			return null;
		}
	}
}
//QueryString qs = new QueryString();

//if (!string.IsNullOrEmpty(this._triggeredBy)) qs = qs.Add("triggered_by", this._triggeredBy);

//if (this._listDagIds != null && this._listDagIds.Any())
//{
//	qs = qs.Add("listDagIds", string.Join(",", this._listDagIds.Where(x => x != null)));
//}

//if (this._startDate != null)
//{
//	if (this._startDate.Start.HasValue)
//	{
//		DateTime rangeStart = this._startDate.Start.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
//		qs = qs.Add("start_date_gte", rangeStart.ToString("o", CultureInfo.InvariantCulture));
//	}
//	if (this._startDate.End.HasValue)
//	{
//		DateTime rangeEnd = this._startDate.End.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
//		qs = qs.Add("start_date_lte", rangeEnd.ToString("o", CultureInfo.InvariantCulture));
//	}
//}
//if (this._endDate != null)
//{
//	if (this._endDate.Start.HasValue)
//	{
//		DateTime rangeStart = this._endDate.Start.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
//		qs = qs.Add("end_date_gte", rangeStart.ToString("o", CultureInfo.InvariantCulture));
//	}
//	if (this._endDate.End.HasValue)
//	{
//		DateTime rangeEnd = this._endDate.End.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
//		qs = qs.Add("end_date_lte", rangeEnd.ToString("o", CultureInfo.InvariantCulture));
//	}
//}
//if (this._queuedAt != null)
//{
//	if (this._queuedAt.Start.HasValue)
//	{
//		DateTime rangeStart = this._queuedAt.Start.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
//		qs = qs.Add("start_date_gte", rangeStart.ToString("o", CultureInfo.InvariantCulture));
//	}
//	if (this._queuedAt.End.HasValue)
//	{
//		DateTime rangeEnd = this._queuedAt.End.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
//		qs = qs.Add("start_date_lte", rangeEnd.ToString("o", CultureInfo.InvariantCulture));
//	}
//}
//if (this._runAfter != null)
//{
//	if (this._runAfter.Start.HasValue)
//	{
//		DateTime rangeStart = this._runAfter.Start.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
//		qs = qs.Add("run_after_gte", rangeStart.ToString("o", CultureInfo.InvariantCulture));
//	}
//	if (this._runAfter.End.HasValue)
//	{
//		DateTime rangeEnd = this._runAfter.End.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
//		qs = qs.Add("run_after_lte", rangeEnd.ToString("o", CultureInfo.InvariantCulture));
//	}
//}

//if (this._runType != null) this._runType.ForEach(x => qs = qs.Add("run_type", x.ToString()));
//if (this._state != null) this._state.ForEach(x => qs = qs.Add("state", x.ToString()));

//String orderBy = this.ApplyOrdering();
//if (!String.IsNullOrEmpty(orderBy) && !useInCount) qs = qs.Add("order_by", orderBy);

//if (useInCount)
//{
//	qs = qs.Add("offset", 0.ToString());
//	qs = qs.Add("limit", 1.ToString());
//}
//else if (this.Page != null && !this.Page.IsEmpty)
//{
//	if (this.Page.Offset >= 0) qs = qs.Add("offset", this.Page.Offset.ToString());
//	if (this.Page.Size > 0) qs = qs.Add("limit", this.Page.Size.ToString());
//}