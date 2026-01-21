using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace RcTracking.ApiFunction;

public class Health
{
    private readonly ILogger<Health> _logger;
    private readonly CosmosClient _cosmosClient;

    public Health(ILogger<Health> logger, CosmosClient client)
    {
        _logger = logger;
        _cosmosClient = client;
    }

    [Function("Health")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "option","get","post")] HttpRequest req)
    {
        _logger.LogInformation("Testing connection to cosmos db");

        var db = _cosmosClient.GetDatabase("RcTrackingDb");
        var flightsC = db.GetContainer("flights");
        var planesC = db.GetContainer("planes");
        var imagesC = db.GetContainer("images");

        return new OkObjectResult("Healthy");
    }
}