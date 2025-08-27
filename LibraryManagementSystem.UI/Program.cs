using LibraryManagementSystem.BLL.Services;
using LibraryManagementSystem.BLL.Validation;
using LibraryManagementSystem.DAL.Caching;
using LibraryManagementSystem.DAL.Data;
using LibraryManagementSystem.DAL.Repositories;
using LibraryManagementSystem.DAL.UnitOfWork;
using LibraryManagementSystem.UI.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LibraryManagementSystem.BLL.Middleware;
using LibraryManagementSystem.UI.HealthChecks;

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
            // إضافة خدمات Session
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "LibraryManagement.Session";
            });

            // إضافة Razor Pages
            services.AddRazorPages(options =>
            {
                options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
            });

            // إضافة Controllers للـ API
            services.AddControllers();

            // إضافة خدمات التخزين المؤقت
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1000; // حد أقصى 1000 عنصر
                options.CompactionPercentage = 0.25; // ضغط 25% عند الوصول للحد الأقصى
            });
            services.AddMemoryCache(); // تمكين IMemoryCache
            services.AddScoped<ICacheService, MemoryCacheService>();

            // إضافة خدمات قاعدة البيانات
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=(localdb)\\mssqllocaldb;Database=LibraryManagementSystem;Trusted_Connection=true;MultipleActiveResultSets=true";

            services.AddScoped<IDatabaseConnectionFactory>(provider =>
                new DatabaseConnectionFactory(connectionString));

            // إعدادات JWT
            var jwtSection = configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSection);
            var jwtSettings = jwtSection.Get<JwtSettings>();

            // إضافة المستودعات
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBorrowingRepository, BorrowingRepository>();

            // إضافة وحدة العمل
            services.AddScoped<IUnitOfWork>(provider =>
            {
                var connectionFactory = provider.GetRequiredService<IDatabaseConnectionFactory>();
                var logger = provider.GetRequiredService<ILogger<UnitOfWork>>();
                var cacheService = provider.GetRequiredService<ICacheService>();
                return new UnitOfWork(connectionFactory, logger, cacheService, provider);
            });

            // إضافة خدمات منطق الأعمال
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IBorrowingService, BorrowingService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();

            // إضافة خدمات التحقق من قواعد الأعمال
            services.AddScoped<IBusinessRuleValidator, BusinessRuleValidator>();

            // إضافة خدمات JWT
            services.AddScoped<IJwtService, JwtService>();

            // إضافة Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // إضافة إعدادات المكتبة
            services.Configure<LibrarySettings>(configuration.GetSection("LibrarySettings"));

            // إضافة خدمات التسجيل
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            // إضافة خدمات HTTP Context
            services.AddHttpContextAccessor();

            // إضافة خدمات التحقق من صحة النموذج
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });

            // إضافة معالجة الأخطاء العامة
            services.AddGlobalErrorHandling();

            // إضافة معالجة أخطاء قاعدة البيانات
            services.AddDatabaseErrorHandling(connectionString);
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
                var connectionFactory = scope.ServiceProvider.GetRequiredService<IDatabaseConnectionFactory>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                logger.LogInformation("بدء تهيئة قاعدة البيانات - Starting database initialization");

                // تشغيل التهيئة بشكل متزامن عند بدء التطبيق
                connectionFactory.InitializeDatabaseAsync().GetAwaiter().GetResult();

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
            // تكوين معالجة الأخطاء العامة
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            // تكوين معالجة الأخطاء حسب البيئة
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
            app.UseHttpsRedirection();

            // الملفات الثابتة
            app.UseStaticFiles();

            // التوجيه
            app.UseRouting();

            // الجلسة
            app.UseSession();

            // التفويض
            app.UseAuthentication(); // يجب أن يكون قبل UseAuthorization
            app.UseAuthorization();

            // تكوين Razor Pages
            app.MapRazorPages();

            // تكوين Controllers للـ API
            app.MapControllers();

            // إضافة مراقبة الصحة
            app.UseHealthMonitoring();

            // الصفحة الافتراضية
            app.MapGet("/", () => Results.Redirect("/Auth/Login"));
        }
    }
}
