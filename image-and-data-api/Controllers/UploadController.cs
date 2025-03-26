using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class UploadController : ControllerBase
{
    private readonly string _blobConnectionString;
    private readonly string _containerName;

    public UploadController(IConfiguration configuration)
    {
        _blobConnectionString = configuration["ConnectionString"];
        _containerName = configuration["ContainerName"];
    }

    [HttpPost(Name = "upload")]
    public async Task<IActionResult> Upload([FromForm] IFormCollection form)
    {
        try
        {
            // Access form fields
            var dropdown = form["dropdown"].ToString();
            var textInput = form["textInput"].ToString();
            var freeText = form["freeText"].ToString();

            // Access the file
            var file = form.Files["file"];
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Upload to Azure Blob
            var blobClient = new BlobClient(_blobConnectionString, _containerName, file.FileName);
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return Ok(new { Message = "File uploaded successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
