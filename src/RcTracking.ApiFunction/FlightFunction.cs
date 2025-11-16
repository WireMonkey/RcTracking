using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RcTracking.ApiFunction.Interface;
using RcTracking.Shared.Model;

namespace RcTracking.ApiFunction;

public class FlightFunction
{
    private readonly ILogger<FlightFunction> _logger;
    private readonly IFlightService _flightService;

    public FlightFunction(ILogger<FlightFunction> logger, IFlightService flightService)
    {
        _logger = logger;
        _flightService = flightService;
    }

    [Function("Flight")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete")] HttpRequest req)
    {

        var verb = req.Method.ToLower();
        _logger.LogInformation("C# HTTP trigger function processed a request. Verb: {Verb}.", verb);
        return verb switch
        {
            "get" => await Get(req),
            "post" => await Post(req),
            "put" => await Put(req),
            "delete" => await Delete(req),
            _ => new StatusCodeResult(StatusCodes.Status405MethodNotAllowed)
        };
    }

    private async Task<IActionResult> Post(HttpRequest req)
    {
        var flight = await req.ReadFromJsonAsync<FlightModel>();
        if(flight == null)
        {
            return new BadRequestObjectResult("Please pass a flight in the request body");
        }

        var result = await _flightService.CreateFlightAsync(flight);
        return new OkObjectResult(result);
    }

    private async Task<IActionResult> Get(HttpRequest req)
    {
        var id = req.Query["id"].ToString();
        var result = string.IsNullOrWhiteSpace(id) ? new OkObjectResult(await _flightService.GetFlightAsync())
            : new OkObjectResult(await _flightService.GetFlightAsync(Guid.Parse(id)));
        return result;
    }

    private async Task<IActionResult> Put(HttpRequest req)
    {
        var flight = await req.ReadFromJsonAsync<FlightModel>();
        if (flight == null || flight?.Id == null || flight.Id == Guid.Empty)
        {
            return new BadRequestObjectResult("Please pass a valid flight model in the request body");
        }
        var result = await _flightService.UpdateFlightAsync(flight);
        return new OkObjectResult(result);
    }

    private async Task<IActionResult> Delete(HttpRequest req)
    {
        var id = req.Query["id"].ToString();
        if (string.IsNullOrWhiteSpace(id))
        {
            return new BadRequestObjectResult("Please pass a flight id in the query string");
        }
        await _flightService.DeleteFlightAsync(Guid.Parse(id));
        return new OkResult();
    }
}