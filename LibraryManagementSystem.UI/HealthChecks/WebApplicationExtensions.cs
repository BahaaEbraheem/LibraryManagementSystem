namespace LibraryManagementSystem.UI.HealthChecks
{
    public static class WebApplicationExtensions
    {
        /// <summary>
        /// استخدام مراقبة الصحة
        /// Use health monitoring
        /// </summary>
        public static WebApplication UseHealthMonitoring(this WebApplication app)
        {
            // إضافة نقاط فحص الصحة
            // Add health check endpoints
            app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(x => new
                        {
                            name = x.Key,
                            status = x.Value.Status.ToString(),
                            exception = x.Value.Exception?.Message,
                            duration = x.Value.Duration.ToString()
                        }),
                        duration = report.TotalDuration.ToString()
                    };
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
                }
            });

            return app;
        }
    }
}
