using Microsoft.Extensions.DependencyInjection;

namespace DataGEMS.Gateway.App.Service.UserFavorite
{
	public static class Extensions
	{
		public static IServiceCollection AddUserFavoriteServices(this IServiceCollection services)
		{
			services.AddScoped<IUserFavoriteService, UserFavoriteService>();

			return services;
		}
	}
}
