using Microsoft.Extensions.DependencyInjection;
using Web.DataAccess.CQS.Queries.Items;
using Web.DataAccess.CQS.Queries.Tokens;
using Web.Services.Abstractions;
using Web.Services.Implementations;

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

        public static IServiceCollection RegisterRefreshTokenByIdMediatr(this IServiceCollection services)
        {
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(GetRefreshTokenByIdQuery).Assembly);
            });

            return services;
        }

        public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IArticleService, ArticleService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAdminService, AdminService>();

            return services;
        }
    }
}
