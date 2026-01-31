using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RcTracking.ApiFunction.Interface;
using RcTracking.Shared.Model;

namespace RcTracking.ApiFunction;

public class PlaneFunction
{
    private readonly ILogger<PlaneFunction> _logger;
    private readonly IPlaneService _planeService;

    public PlaneFunction(ILogger<PlaneFunction> logger, IPlaneService planeService)
    {
        _logger = logger;
        _planeService = planeService;
    }

    [Function("Plane")]
    public async Task<IActionResult> Plane([HttpTrigger(AuthorizationLevel.Function, "get", "post", "put", "delete")] HttpRequest req)
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

    private async Task<IActionResult> Get(HttpRequest req)
    {
        var id = req.Query["id"].ToString();
        _logger.LogInformation("Get processing with, Id: {Id}", id);

        try
        {
            var result = string.IsNullOrWhiteSpace(id) ? new OkObjectResult(await _planeService.GetPlanesAsync())
                : new OkObjectResult(await _planeService.GetPlaneAsync(Guid.Parse(id)));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception: {ex} Id: {Id}", ex, id);
            return new BadRequestObjectResult(ex.ToString());
        }
    }

    private async Task<IActionResult> Post(HttpRequest req)
    {
        var body = await req.ReadFromJsonAsync<PlaneModel>();
        if (body == null || body.Id != Guid.Empty || string.IsNullOrWhiteSpace(body.Name))
        {
            return new BadRequestObjectResult("Please pass a valid body");
        }

        try
        {
            var result = await _planeService.CreatePlaneAsync(body);
            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception: {ex} Body: {Body}", ex, body);
            return new BadRequestObjectResult(ex.ToString());
        }
    }

    private async Task<IActionResult> Put(HttpRequest req)
    {
        var body = await req.ReadFromJsonAsync<PlaneModel>();
        if (body == null || body.Id == Guid.Empty || string.IsNullOrWhiteSpace(body.Name))
        {
            return new BadRequestObjectResult("Please pass a valid body");
        }

        _logger.LogInformation("Put processing with, Id: {Id}.", body.Id);
        try
        {
            return new OkObjectResult(await _planeService.UpdatePlaneAsync(body.Id, body));
        }
        catch (KeyNotFoundException)
        {
            return new BadRequestObjectResult("Please pass a valid key");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception: {ex} Body: {Body}", ex, body);
            return new BadRequestObjectResult(ex.ToString());
        }
    }

    private async Task<IActionResult> Delete(HttpRequest req)
    {
        var id = req.Query["id"].ToString();
        _logger.LogInformation("Delete processing with, Id: {Id}", id);
        if (string.IsNullOrWhiteSpace(id))
        {
            return new BadRequestObjectResult("Please pass a valid id");
        }
        try
        {
            await _planeService.DeletePlaneAsync(Guid.Parse(id));
            return new OkResult();
        }
        catch (KeyNotFoundException)
        {
            return new BadRequestObjectResult("Please pass a valid key");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception: {ex}", ex);
            return new BadRequestObjectResult(ex.ToString());
        }
    }
}