using Aviation.Maintenance.Grpc.Services;
using Aviation.Maintenance.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Подключаем DbContext + доменные сервисы
builder.Services.AddMaintenanceInfrastructure(builder.Configuration);

// Регистрируем gRPC
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<WorkOrderGrpcService>();

// Просто для проверки через браузер
app.MapGet("/", () => "Aviation.Maintenance.Grpc is running (gRPC).");

app.Run();
