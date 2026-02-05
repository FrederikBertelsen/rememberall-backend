using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using RememberAll.src.Data;
using RememberAll.src.Repositories;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services;
using RememberAll.src.Services.Interfaces;
using RememberAll.src.Entities;
using Microsoft.OpenApi;
using RememberAll.src.Middleware;

namespace RememberAll;

public partial class Program
{
    private static void Main(string[] args) => CreateWebApplication(args).Run();

    public static WebApplication CreateWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        builder.Services.AddScoped<ITodoListRepository, TodoListRepository>();
        builder.Services.AddScoped<ITodoListService, TodoListService>();

        builder.Services.AddScoped<ITodoItemRepository, TodoItemRepository>();
        builder.Services.AddScoped<ITodoItemService, TodoItemService>();

        builder.Services.AddScoped<IListAccessRepository, ListAccessRepository>();
        builder.Services.AddScoped<IListAccessService, ListAccessService>();

        builder.Services.AddScoped<IInviteRepository, InviteRepository>();
        builder.Services.AddScoped<IInviteService, InviteService>();

        // Auth services
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Health checks
        builder.Services.AddHealthChecks();

        builder.Services.AddControllers();


        // Authentication (cookie-based)
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

        // Allow SvelteKit dev origin to send cookies
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Svelte", policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                      .AllowCredentials()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });


        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=app.db"));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            // Host the Swagger UI at the application's root ("/")
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "RememberAll API V1");
                options.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection();

        app.UseCors("Svelte");

        app.UseMiddleware<GlobalExceptionMiddleware>();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapHealthChecks("/api/health");
        app.MapControllers();

        // Ensure database exist and apply any pending migrations on startup
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            try
            {
                db.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "An error occurred while initialising the database.");
                throw;
            }
        }

        return app;
    }
}