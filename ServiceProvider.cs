using Microsoft.EntityFrameworkCore;
namespace BlogTest
{
    public static class ServiceProvider
    {
        public static void AddServices(this IServiceCollection services, string connection)
        {
            services.AddAuthentication("Cookies").AddCookie(options => options.LoginPath = "/login");
            services.AddAuthorization();
            services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));
        }
    }
}
