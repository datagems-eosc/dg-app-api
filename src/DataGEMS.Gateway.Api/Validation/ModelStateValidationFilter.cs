using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using DataGEMS.Gateway.App.Exception;

namespace DataGEMS.Gateway.Api.Validation
{
	public class ModelStateValidationFilter : TypeFilterAttribute
	{
		public ModelStateValidationFilter(params String[] sensitiveKeys) : base(typeof(ModelStateValidationFilterImpl))
		{
			Arguments = new Object[] { sensitiveKeys };
		}

		private class ModelStateValidationFilterImpl : IAsyncActionFilter, IOrderedFilter
		{
			public int Order { get; set; }
			private readonly String[] _sensitiveKeys;

			public ModelStateValidationFilterImpl(String[] sensitiveKeys)
			{
				this._sensitiveKeys = sensitiveKeys;
			}

			public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
			{
				if (context.ModelState.IsValid)
				{
					var resultContext = await next();
					return;
				}

				List<KeyValuePair<string, List<string>>> errors = context.ModelState
					.Select(x => new KeyValuePair<string, List<string>>(
						x.Key, 
						(this._sensitiveKeys == null || !this._sensitiveKeys.Contains(x.Key)) ? x.Value.Errors.Select(y => y.ErrorMessage).ToList() : null)).ToList();

				throw new DGValidationException("unsucessful model binding", errors);
			}
		}
	}
}
