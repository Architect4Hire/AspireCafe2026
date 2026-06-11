var builder = DistributedApplication.CreateBuilder(args);

// ---------- SQL Server (one container, three databases) ----------
var sqlPassword = builder.AddParameter("sql-password", secret: true);

var sqlServer = builder.AddSqlServer("sql", password: sqlPassword)
                       .WithDataVolume("aspirecafe-sql-data")
                       .WithLifetime(ContainerLifetime.Persistent);

var menuDb = sqlServer.AddDatabase("MenuDb");
var ordersDb = sqlServer.AddDatabase("OrdersDb");
var paymentsDb = sqlServer.AddDatabase("PaymentsDb");

// ---------- Microservices ----------
var menuApi = builder.AddProject<Projects.AspireCafe_Menu_API>("menu-api")
                     .WithReference(menuDb)
                     .WaitFor(menuDb)
                     .WithExternalHttpEndpoints();

var ordersApi = builder.AddProject<Projects.AspireCafe_Orders_API>("orders-api")
                       .WithReference(ordersDb)
                       .WaitFor(ordersDb)
                       .WithExternalHttpEndpoints();

var paymentsApi = builder.AddProject<Projects.AspireCafe_Payments_API>("payments-api")
                         .WithReference(paymentsDb)
                         .WaitFor(paymentsDb)
                         .WithExternalHttpEndpoints();

// ---------- Angular 20 POS frontend ----------
builder.AddNpmApp("pos-web", "../AspireCafe.POS", "start")
       .WithReference(menuApi)
       .WithReference(ordersApi)
       .WithReference(paymentsApi)
       //.WithHttpEndpoint(port: 4200, env: "PORT")
       .WithEnvironment("API_MENU", menuApi.GetEndpoint("https"))
       .WithEnvironment("API_ORDERS", ordersApi.GetEndpoint("https"))
       .WithEnvironment("API_PAYMENTS", paymentsApi.GetEndpoint("https"))
       .WithExternalHttpEndpoints()
       .PublishAsDockerFile();

builder.Build().Run();
