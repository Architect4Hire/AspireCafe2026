using AspireCafe.Orders.API.Managers.Business;
using AspireCafe.Orders.API.Managers.Data;
using AspireCafe.Orders.API.Managers.DataContext;
using AspireCafe.Orders.API.Managers.Facades;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSqlServerDbContext<OrdersDbContext>("OrdersDb");

builder.Services.AddScoped<IOrderDataManager, OrderDataManager>();
builder.Services.AddScoped<IOrderBusinessManager, OrderBusinessManager>();
builder.Services.AddScoped<IOrderFacade, OrderFacade>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string PosCors = "PosCors";
builder.Services.AddCors(o => o.AddPolicy(PosCors, p =>
    p.WithOrigins("http://localhost:4200", "https://localhost:4200")
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();

app.MapDefaultEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await OrdersDbContext.EnsureCreatedAsync(db);
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
