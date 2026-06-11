using AspireCafe.Menu.API.Managers.Business;
using AspireCafe.Menu.API.Managers.Data;
using AspireCafe.Menu.API.Managers.DataContext;
using AspireCafe.Menu.API.Managers.Facades;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Aspire-managed EF Core + SQL Server connection (named "MenuDb" from AppHost)
builder.AddSqlServerDbContext<MenuDbContext>("MenuDb");

builder.Services.AddScoped<IMenuDataManager, MenuDataManager>();
builder.Services.AddScoped<IMenuBusinessManager, MenuBusinessManager>();
builder.Services.AddScoped<IMenuFacade, MenuFacade>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for the Angular POS frontend
const string PosCors = "PosCors";
builder.Services.AddCors(o => o.AddPolicy(PosCors, p =>
    p.WithOrigins("http://localhost:4200", "https://localhost:4200")
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();

app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MenuDbContext>();
    await MenuDbContext.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(PosCors);
app.UseAuthorization();
app.MapControllers();

app.Run();
