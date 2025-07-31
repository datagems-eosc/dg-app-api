using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DataGEMS.Gateway.App.Model.Builder;
using DataGEMS.Gateway.App.Model;
using Npgsql.Internal.Postgres;
using Cite.Tools.FieldSet;
using DataGEMS.Gateway.App.ErrorCode;
using DataGEMS.Gateway.App.Exception;
using Cite.Tools.Data.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Cite.Tools.Json;
using Microsoft.Net.Http.Headers;
using DataGEMS.Gateway.App.Authorization;
using DataGEMS.Gateway.App.LogTracking;
using DataGEMS.Gateway.App.Service.Discovery.Model;

// exw na ftiaksw to model kai to builder gia to implemenation kai epishs prepei na kanw exception handling edw pera gia auta poy mporei na epistrepsei 

namespace DataGEMS.Gateway.App.Service.Airflow
{
	public class AirflowService : IAirflowService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<AirflowService> _logger;
		private readonly AirflowBuilder _builder;
		private readonly ErrorThesaurus _errors;
		private readonly BuilderFactory _builderFactory;
		private readonly JsonHandlingService _jsonHandlingService;
		private readonly AirflowConfig _config;

		public AirflowService(
			IHttpClientFactory httpClientFactory,
			ILogger<AirflowService> logger,
			ErrorThesaurus errors,
			AirflowBuilder builder,
			BuilderFactory builderFactory,
			JsonHandlingService jsonHandlingService,
			AirflowConfig config
			)
		{
			this._httpClientFactory = httpClientFactory;
			this._logger = logger;
			this._builder = builder;
			this._errors = errors;
			this._builderFactory = builderFactory;
			this._jsonHandlingService = jsonHandlingService;
			this._config = config;
		}

		public async Task<List<DataGEMS.Gateway.App.Model.Airflow>> GetDagRunsAsync(AirflowInfo request, FieldSet fieldset)
		{
			//  basic auth
			var byteArray = Encoding.ASCII.GetBytes($"{this._config.username}:{this._config.password}");
			var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

			
			var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{this._config.BaseUrl}{this._config.DagListEndpoint}");
			httpRequest.Headers.Authorization = authHeader;
			httpRequest.Headers.Add(HeaderNames.Accept, "application/json");


			// send the http request
			string content = await SendRequest(httpRequest);

			try
			{
				//  json handling
				Model.AirflowResponse rawResponse = this._jsonHandlingService.FromJson<Model.AirflowResponse>(content);
				
				return await this._builderFactory.Builder<App.Model.Builder.AirflowBuilder>()
					.Authorize(AuthorizationFlags.Any)
					.Build(fieldset, rawResponse?.Dags);
			}
			catch (System.Exception ex)
			{
				_logger.LogError(ex, "failed to parse response from airflow: {content}", content);
				throw new DGUnderpinningException(_errors.UnderpinningService.Code, _errors.UnderpinningService.Message);
			}
		}
	
		private async Task<string> SendRequest(HttpRequestMessage request)
		{
			HttpResponseMessage response = null;
			try
			{
				response = await _httpClientFactory.CreateClient().SendAsync(request);
				response.EnsureSuccessStatusCode();
				string content = await response.Content.ReadAsStringAsync();
				return content;
			}
			catch (System.Exception ex)
			{
				_logger.LogError(ex, "could not complete request. response was {StatusCode}", response?.StatusCode);
				throw new DGUnderpinningException(_errors.UnderpinningService.Code, _errors.UnderpinningService.Message);
			}
		}

	}
}

// akuro prepei na kanw extend ton builder opws to exw edw pera     public class CrossDatasetDiscoveryBuilder : Builder<CrossDatasetDiscovery, Service.Discovery.Model.CrossDatasetDiscoveryResult>
// giati etsi tha mporesei o airflow builder na kanei conversion sto type toy builder.ibuilder




