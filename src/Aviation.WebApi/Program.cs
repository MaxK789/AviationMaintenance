using Aviation.Maintenance.Grpc;
using Aviation.Maintenance.Infrastructure.Extensions;
using Aviation.WebApi.Errors;
using Aviation.WebApi.GraphQL;
using Aviation.WebApi.GraphQL.Loaders;
using Aviation.WebApi.GraphQL.Resolvers;
using Aviation.WebApi.Authentication;
using Aviation.WebApi.Hubs;
using Aviation.WebApi.Grpc;
using Aviation.WebApi.Options;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
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

builder.Services
    .AddAuthentication(ApiKeyAuthenticationHandler.Scheme)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.Scheme, _ => { });

builder.Services.AddAuthorization();

builder.Services.Configure<GrpcOptions>(builder.Configuration.GetSection("Grpc"));

var apiKey = builder.Configuration["Security:ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey))
    throw new InvalidOperationException("Security:ApiKey is not configured");

builder.Services.AddSingleton(new ApiKeyClientInterceptor(apiKey));

builder.Services.AddGrpcClient<WorkOrderService.WorkOrderServiceClient>((sp, o) =>
{
    var grpc = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GrpcOptions>>().Value;

    if (string.IsNullOrWhiteSpace(grpc.WorkOrdersUrl))
        throw new InvalidOperationException("Grpc:WorkOrdersUrl is not configured");

    o.Address = new Uri(grpc.WorkOrdersUrl);
})
.AddInterceptor<ApiKeyClientInterceptor>();

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
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Name = ApiKeyAuthenticationHandler.HeaderName,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter API key"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var ex = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

        if (ex is null)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return;
        }

        ProblemDetails pd;

        if (ex is AppException appEx)
        {
            context.Response.StatusCode = appEx.StatusCode;

            pd = new ProblemDetails
            {
                Status = appEx.StatusCode,
                Title = appEx.Code,
                Detail = appEx.Message,
                Instance = context.Request.Path
            };

            pd.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;
            pd.Extensions["code"] = appEx.Code;
        }
        else if (ex is RpcException rpc)
        {
            var (status, title) = GrpcHttpStatusMapper.Map(rpc.StatusCode);
            context.Response.StatusCode = status;

            pd = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = app.Environment.IsDevelopment() ? rpc.Status.Detail : "Upstream service error",
                Instance = context.Request.Path
            };

            pd.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;
            pd.Extensions["grpcStatus"] = rpc.StatusCode.ToString();
            pd.Extensions["code"] = "GRPC_ERROR";
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            pd = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "INTERNAL_ERROR",
                Detail = app.Environment.IsDevelopment() ? ex.ToString() : "Unexpected server error",
                Instance = context.Request.Path
            };

            pd.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;
            pd.Extensions["code"] = "INTERNAL_ERROR";
        }

        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(pd);
    });
});

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
    pd.Extensions["code"] = "HTTP_STATUS";

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization();

// Хаб для ЛР2
app.MapHub<MaintenanceHub>("/hubs/maintenance").RequireAuthorization();

app.MapGraphQL("/api/graphql").RequireAuthorization();

app.Run();
