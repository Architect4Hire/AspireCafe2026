using AspireCafe.Payments.API.Managers.Business;
using AspireCafe.Payments.API.Managers.Data;
using AspireCafe.Payments.API.Managers.DataContext;
using AspireCafe.Payments.API.Managers.Facades;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSqlServerDbContext<PaymentsDbContext>("PaymentsDb");

builder.Services.AddScoped<IPaymentDataManager, PaymentDataManager>();
builder.Services.AddScoped<IPaymentBusinessManager, PaymentBusinessManager>();
builder.Services.AddScoped<IPaymentFacade, PaymentFacade>();

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
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    await PaymentsDbContext.EnsureCreatedAsync(db);
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
