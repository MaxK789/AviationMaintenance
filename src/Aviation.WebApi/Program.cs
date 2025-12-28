using Aviation.Maintenance.Grpc;
using Aviation.Maintenance.Infrastructure.Extensions;
using Aviation.WebApi.Errors;
using Aviation.WebApi.GraphQL;
using Aviation.WebApi.GraphQL.Loaders;
using Aviation.WebApi.GraphQL.Resolvers;
using Aviation.WebApi.Hubs;
using Aviation.WebApi.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json.Serialization;

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

builder.Services.Configure<GrpcOptions>(builder.Configuration.GetSection("Grpc"));

builder.Services.AddGrpcClient<WorkOrderService.WorkOrderServiceClient>((sp, o) =>
{
    var grpc = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GrpcOptions>>().Value;

    if (string.IsNullOrWhiteSpace(grpc.WorkOrdersUrl))
        throw new InvalidOperationException("Grpc:WorkOrdersUrl is not configured");

    o.Address = new Uri(grpc.WorkOrdersUrl);
});

builder.Services.AddProblemDetails(o =>
{
    o.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddExceptionHandler<GrpcExceptionHandler>();
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Чтобы 400 по валидации тоже было в формате ProblemDetails (и с traceId)
builder.Services.Configure<ApiBehaviorOptions>(o =>
{
    o.InvalidModelStateResponseFactory = actionContext =>
    {
        var problem = new ValidationProblemDetails(actionContext.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "VALIDATION_ERROR",
            Instance = actionContext.HttpContext.Request.Path
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? actionContext.HttpContext.TraceIdentifier;
        return new BadRequestObjectResult(problem)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<WorkOrderResolvers>()
    .AddTypeExtension<AircraftResolvers>()
    .AddDataLoader<AircraftByIdDataLoader>()
    .AddErrorFilter<GraphQLErrorFilter>()
    .ModifyRequestOptions(o =>
    {
        o.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    });

// Swagger (удобно для теста ЛР1)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

// ProblemDetails даже для “обычных” 404/401/403 без exception
app.UseStatusCodePages(async statusCtx =>
{
    var http = statusCtx.HttpContext;

    // Если уже есть body — не трогаем
    if (http.Response.HasStarted) return;

    var status = http.Response.StatusCode;
    if (status < 400) return;

    var pd = new ProblemDetails
    {
        Status = status,
        Title = ReasonPhrases.GetReasonPhrase(status),
        Instance = http.Request.Path
    };

    pd.Extensions["traceId"] = Activity.Current?.Id ?? http.TraceIdentifier;

    http.Response.ContentType = "application/problem+json";
    await http.Response.WriteAsJsonAsync(pd);
});

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
