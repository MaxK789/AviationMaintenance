using System.Text.Json.Serialization;
using Aviation.Maintenance.Grpc;
using Aviation.Maintenance.Infrastructure.Extensions;
using Aviation.WebApi.GraphQL;
using Aviation.WebApi.GraphQL.Loaders;
using Aviation.WebApi.GraphQL.Resolvers;
using Aviation.WebApi.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Подключаем нашу инфраструктуру (DbContext + сервисы)
builder.Services.AddMaintenanceInfrastructure(builder.Configuration);

// Добавляем контроллеры и делаем enum'ы строками в JSON
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// SignalR
builder.Services.AddSignalR();

builder.Services.AddGrpcClient<WorkOrderService.WorkOrderServiceClient>(o =>
{
    // ВАЖНО: сюда подставь адрес, который пишет консоль при запуске Aviation.Maintenance.Grpc
    // пример:
    // o.Address = new Uri("https://localhost:7120");
    o.Address = new Uri("http://localhost:5004");
});

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<WorkOrderResolvers>()
    .AddTypeExtension<AircraftResolvers>()
    .AddDataLoader<AircraftByIdDataLoader>();

// Swagger (удобно для теста ЛР1)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// В dev включаем Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Хаб для ЛР2
app.MapHub<MaintenanceHub>("/hubs/maintenance");

app.MapGraphQL("/api/graphql");

app.Run();
