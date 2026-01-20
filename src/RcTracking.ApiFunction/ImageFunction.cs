using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RcTracking.ApiFunction.Interface;
using System.Drawing;

namespace RcTracking.ApiFunction;

public class ImageFunction
{
    private readonly ILogger<ImageFunction> _logger;
    private readonly IImageService _imageService;

    public ImageFunction(ILogger<ImageFunction> logger, IImageService imageService)
    {
        _logger = logger;
        _imageService = imageService;
    }

    [Function("Image")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", "put", "delete")] HttpRequest req)
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
        var data = await ParseForm(req);

        var result = await _imageService.AddImageAsync(data.planeId, data.image);
        return new OkObjectResult(result);
    }

    private async Task<IActionResult> Get(HttpRequest req)
    {
        var id = req.Query["id"].ToString();
        var result = string.IsNullOrWhiteSpace(id)
            ? new OkObjectResult(await _imageService.GetImagesAsync())
            : new OkObjectResult(await _imageService.GetImageAsync(Guid.Parse(id)));
        return result;
    }

    private async Task<IActionResult> Put(HttpRequest req)
    {
        var data = await ParseForm(req);
        var result = await _imageService.UpdateImageAsync(data.planeId, data.image);
        return new OkObjectResult(result);
    }

    private async Task<IActionResult> Delete(HttpRequest req)
    {
        var id = req.Query["id"].ToString();
        if (string.IsNullOrWhiteSpace(id))
        {
            return new BadRequestObjectResult("Please pass an id in the query string");
        }
        await _imageService.DeleteImageAsync(Guid.Parse(id));
        return new OkResult();
    }

    private async Task<(Guid planeId, Image image)> ParseForm(HttpRequest req)
    {
        var form = req.ReadFormAsync().Result;
        string id = form["planeId"];
        if (!Guid.TryParse(id, out Guid planeId))
        {
            throw new ArgumentException("Invalid or missing planeId");
        }
        var file = form.Files.FirstOrDefault();
        if (file == null)
        {
            throw new ArgumentException("No file uploaded");
        }
        Image image;
        try
        {
            image = Image.FromStream(file.OpenReadStream());
        }
        catch
        {
            throw new ArgumentException("Uploaded file is not a valid image");
        }
        return (planeId, image);
    }
}