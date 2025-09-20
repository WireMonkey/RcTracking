using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RcTracking.ApiFunction.Context;
using RcTracking.ApiFunction.Interface;
using RcTracking.ApiFunction.Service;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.AddAzureCosmosClient(connectionName: "rc-tracking-cosmos-db");
builder.AddCosmosDbContext<PlaneContext>("rc-tracking-cosmos-db", "RcTrackingDb");

builder.Services.AddScoped<IPlaneService, PlaneService>();

builder.Build().Run();