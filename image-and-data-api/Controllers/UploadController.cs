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

            // Create a JSON object for the form data
            var formData = new
            {
                Dropdown = dropdown,
                TextInput = textInput,
                FreeText = freeText
            };

            // Serialize the form data to JSON
            var jsonData = System.Text.Json.JsonSerializer.Serialize(formData);

            // Upload the JSON data as a separate blob
            var jsonBlobName = $"{file.FileName}_metadata.json";
            var jsonBlobClient = new BlobClient(_blobConnectionString, _containerName, jsonBlobName);
            using (var jsonStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonData)))
            {
                await jsonBlobClient.UploadAsync(jsonStream, true);
            }

            return Ok(new { Message = "File uploaded successfully!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
