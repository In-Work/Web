using Microsoft.EntityFrameworkCore;
using Web.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Web.Infrastructure;
using Serilog;
using Serilog.Events;

namespace Web.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.File("log.log")
                .CreateBootstrapLogger();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllersWithViews();

            builder.Services.RegisterApplicationServices();
            builder.Services.RegisterRefreshTokenByIdMediatr();

            builder.Services.AddSerilog((services, lc) => lc
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console(LogEventLevel.Error)
                .WriteTo.File("log.log"));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/User/Login";
                });

            builder.Services.AddAuthorization(); 

            var app = builder.Build();

            app.UseSerilogRequestLogging();
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}

#region JwtBearer
// using System.Text;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.IdentityModel.Tokens;
// using Web.DataAccess.CQS.Queries.Tokens;

// var configuration = builder.Configuration;
// var jwtIssuer = configuration.GetSection("jwtToken:Issuer").Get<string>();
// var jwtAudience = configuration.GetSection("jwtToken:Audience").Get<string>();
// var jwtSecretKey = configuration.GetSection("jwtToken:Secret").Get<string>();

// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
// .AddJwtBearer(opt =>
// {
//     opt.TokenValidationParameters = new TokenValidationParameters()
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,
//         ValidIssuer = jwtIssuer,
//         ValidAudience = jwtAudience,
//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey!))
//     };
// });
#endregion