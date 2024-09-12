using Microsoft.Extensions.DependencyInjection;
using Web.DataAccess.CQS.Queries.Items;

namespace Web.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterItemMediatr(this IServiceCollection services)
        {
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(GetItemsByCategoryIdQuery).Assembly);
            });

            return services;
        }
    }
}
