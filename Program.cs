using Microsoft.EntityFrameworkCore;
using RememberAll.src.Data;
using RememberAll.src.Repositories;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITodoListRepository, TodoListRepository>();
builder.Services.AddScoped<ITodoItemRepository, TodoItemRepository>();
builder.Services.AddScoped<IListAccessRepository, ListAccessRepository>();
builder.Services.AddScoped<IInviteRepository, InviteRepository>();

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=app.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Host the Swagger UI at the application's root ("/")
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

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

app.Run();
