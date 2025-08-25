using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.DAL.Caching;
using LibraryManagementSystem.DAL.Data;
using LibraryManagementSystem.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.UI
{
    /// <summary>
    /// نقطة دخول التطبيق الرئيسية
    /// Main application entry point
    /// </summary>
    public class Program
    {
        /// <summary>
        /// الدالة الرئيسية لبدء التطبيق
        /// Main function to start the application
        /// </summary>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // إضافة الخدمات إلى الحاوية
            // Add services to the container
            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            // تهيئة قاعدة البيانات
            // Initialize database
            InitializeDatabase(app);

            // تكوين pipeline طلبات HTTP
            // Configure the HTTP request pipeline
            ConfigurePipeline(app);

            app.Run();
        }

        /// <summary>
        /// تكوين الخدمات
        /// Configure services
        /// </summary>
        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // إضافة Razor Pages
            // Add Razor Pages
            services.AddRazorPages(options =>
            {
                options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
            });

            // إضافة خدمات التخزين المؤقت
            // Add caching services
            services.AddMemoryCache();
            services.AddScoped<ICacheService, MemoryCacheService>();

            // إضافة خدمات قاعدة البيانات
            // Add database services
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=.;Database=LibraryManagementSystem;Trusted_Connection=true;TrustServerCertificate=True;MultipleActiveResultSets=true";

            services.AddScoped<IDatabaseConnectionFactory>(provider =>
                new DatabaseConnectionFactory(connectionString));

            // إضافة خدمة تهيئة قاعدة البيانات
            // Add database initialization service
            services.AddScoped<DatabaseInitializer>(provider =>
                new DatabaseInitializer(connectionString, provider.GetRequiredService<ILogger<DatabaseInitializer>>()));

            // إضافة المستودعات
            // Add repositories
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBorrowingRepository, BorrowingRepository>();

            // إضافة خدمات منطق الأعمال
            // Add business logic services
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IBorrowingService, BorrowingService>();

            // إضافة إعدادات المكتبة
            // Add library settings
            services.Configure<LibrarySettings>(configuration.GetSection("LibrarySettings"));

            // إضافة خدمات التسجيل
            // Add logging services
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            // إضافة خدمات HTTP Context
            // Add HTTP Context services
            services.AddHttpContextAccessor();

            // إضافة خدمات التحقق من صحة النموذج
            // Add model validation services
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });
        }

        /// <summary>
        /// تهيئة قاعدة البيانات
        /// Initialize database
        /// </summary>
        private static void InitializeDatabase(WebApplication app)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var databaseInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                logger.LogInformation("بدء تهيئة قاعدة البيانات - Starting database initialization");

                // تشغيل تهيئة قاعدة البيانات بشكل متزامن عند بدء التطبيق
                // Run database initialization synchronously at application startup
                databaseInitializer.InitializeDatabaseAsync().GetAwaiter().GetResult();

                logger.LogInformation("تم إكمال تهيئة قاعدة البيانات بنجاح - Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "فشل في تهيئة قاعدة البيانات - Failed to initialize database");
                throw;
            }
        }

        /// <summary>
        /// تكوين pipeline المعالجة
        /// Configure processing pipeline
        /// </summary>
        private static void ConfigurePipeline(WebApplication app)
        {
            // تكوين معالجة الأخطاء
            // Configure error handling
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            // إعادة توجيه HTTPS
            // HTTPS redirection
            app.UseHttpsRedirection();

            // الملفات الثابتة
            // Static files
            app.UseStaticFiles();

            // التوجيه
            // Routing
            app.UseRouting();

            // التفويض
            // Authorization
            app.UseAuthorization();

            // تكوين Razor Pages
            // Configure Razor Pages
            app.MapRazorPages();

            // الصفحة الافتراضية
            // Default page
            app.MapGet("/", () => Results.Redirect("/Index"));
        }
    }
}
