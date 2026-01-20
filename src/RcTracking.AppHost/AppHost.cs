using Aspire.Hosting.Azure;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var sharedMi = builder.AddAzureUserAssignedIdentity("rc-tracking-identity");

#pragma warning disable ASPIRECOSMOSDB001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var cosmos = builder.AddAzureCosmosDB("rc-tracking-cosmos-db")
    .RunAsPreviewEmulator(emulator =>
    {
        emulator.WithDataVolume();
        emulator.WithDataExplorer();
    });
#pragma warning restore ASPIRECOSMOSDB001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var db = cosmos.AddCosmosDatabase("RcTrackingDb");
var flights = db.AddContainer("flights", "/id");
var planes = db.AddContainer("planes", "/id");
var images = db.AddContainer("images", "/id");

var functions = builder.AddAzureFunctionsProject<RcTracking_ApiFunction>("rc-tracking-function")
    .WithAzureUserAssignedIdentity(sharedMi)
    .WaitFor(cosmos)
    .WithReference(cosmos);

var gateway = builder.AddYarp("gateway")
    .WithReference(functions)
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("/api/{**catch-all}", functions);
    })
    .WithHostPort(8080)
    .WithAzureUserAssignedIdentity(sharedMi)
    .WaitFor(functions)
    .WithExternalHttpEndpoints();

builder.AddProject<RcTracking_UI>("rc-tracking-ui")
    .WithAzureUserAssignedIdentity(sharedMi)
    .WaitFor(gateway);

builder.Build().Run();
