namespace BlogTest
{
    public static class MiddlewareProvider
    {
        public static void AddMiddleware(this IApplicationBuilder builder)
        {
            builder.UseDefaultFiles();
            builder.UseStaticFiles();
            builder.UseAuthentication();
            builder.UseAuthorization();
        }
    }
}
