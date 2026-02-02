using e_parliament.Interface;
using Microsoft.EntityFrameworkCore;
using Events.DATA;
using Events.Helpers;
using Events.Helpers.Hosted;
using Events.Repository;
using Events.Services;
using Events.Helpers.OneSignal;
using Events.Services.Payment;

namespace Events.Extensions
{
    public static class ApplicationServicesExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<DataContext>(options => options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
            services.AddAutoMapper(typeof(UserMappingProfile).Assembly);
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
            services.AddScoped<IArticleServices, ArticleService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<ISeatIoService, SeatIoService>();
            services.AddScoped<AuthorizeActionFilter>();
            // services.AddScoped<PermissionSeeder>();
            services.AddScoped<IChartService , ChartService>();
            services.AddScoped<IEventService , EventService>();
            services.AddScoped<ICategoryService , CategoryService>();
            services.AddScoped<IBookService , BookService>();
            services.AddScoped<IFileService , FileService>();
            services.AddScoped<ITicketTemplateService , TicketTemplateService>();
            services.AddScoped<ITicketService , TicketService>();
            services.AddScoped<IPointOfSaleService , PointOfSaleService>();
            services.AddScoped<IEventFavoriteService , EventFavoriteService>();
            services.AddScoped<IStatisticService , StatisticService>();
            
            // Payment Gateway Services
            services.AddHttpClient<AmwalPaymentGateway>();
            services.AddScoped<IPaymentGateway, AmwalPaymentGateway>();
            services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
            
            services.AddScoped<ITagService , TagService>();
            services.AddScoped<INotificationService , NotificationService>();
            services.AddScoped<ISupportMessageService ,SupportMessageService>();
            services.AddHostedService<BookCleanupService>();
            services.AddHostedService<NotificationsMiddleware>();
            services.AddHostedService<BookTimeoutNotification>();
            // seed data from permission seeder service
            
            // var serviceProvider = services.BuildServiceProvider();
            // var permissionSeeder = serviceProvider.GetService<PermissionSeeder>();
            // permissionSeeder.SeedPermissions();

            return services;
        }
    }
}