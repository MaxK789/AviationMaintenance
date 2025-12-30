using Aviation.Maintenance.Grpc.Interceptors;
using Aviation.Maintenance.Grpc.Services;
using Aviation.Maintenance.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["Security:ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey))
    throw new InvalidOperationException("Security:ApiKey is missing in Aviation.Maintenance.Grpc");

// Підключаємо DbContext + доменні сервіси
builder.Services.AddMaintenanceInfrastructure(builder.Configuration);

// Реєструємо gRPC
builder.Services.AddGrpc(o =>
{
    o.Interceptors.Add<ApiKeyServerInterceptor>();
    o.Interceptors.Add<GrpcExceptionInterceptor>();
});
builder.Services.AddGrpcReflection();
builder.Services.AddGrpcHealthChecks();
builder.Services.AddSingleton<ApiKeyServerInterceptor>();
builder.Services.AddSingleton<GrpcExceptionInterceptor>();

var app = builder.Build();

app.MapGrpcService<WorkOrderGrpcService>();
app.MapGrpcHealthChecksService();
app.MapGrpcReflectionService();

// Просто для перевірки через браузер
app.MapGet("/", () => "Aviation.Maintenance.Grpc is running (gRPC).");

app.Run();
