using FloodDamageAssessment.Models;
using FloodDamageAssessment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FloodDamageAssessment.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssessmentsController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly IAssessmentService _service;
    private readonly IConfiguration _config;
    private readonly ILogger<AssessmentsController> _logger;

    public AssessmentsController(IAssessmentService service, IConfiguration config, ILogger<AssessmentsController> logger)
    {
        _service = service;
        _config = config;
        _logger = logger;
    }

    private static bool TryValidate(Assessment dto, out List<string> errors)
    {
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), results, validateAllProperties: true);
        errors = results.Select(r => r.ErrorMessage ?? "Invalid value").ToList();
        return isValid;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAll()
    {
        var deleted = await _service.DeleteAllAsync();
        return Ok(new { ok = true, deleted });
    }

    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Post()
    {
        try
        {
            var payload = Request.Form["payload"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(payload)) return BadRequest("Missing payload");
            var dto = JsonSerializer.Deserialize<Assessment>(payload, JsonOptions);
            if (dto == null || string.IsNullOrWhiteSpace(dto.Id)) return BadRequest("Payload must include a valid id");
            if (!TryValidate(dto, out var validationErrors)) return BadRequest(new { ok = false, errors = validationErrors });

            var uploads = _config.GetValue<string>("Uploads:Path") ?? "uploads";
            var uploadsDirectory = Path.Combine(AppContext.BaseDirectory, uploads);
            Directory.CreateDirectory(uploadsDirectory);
            var photoUrls = new List<string>();
            foreach (var file in Request.Form.Files)
            {
                var fileName = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "-" + Path.GetFileName(file.FileName);
                var savePath = Path.Combine(uploadsDirectory, fileName);
                await using var stream = System.IO.File.Create(savePath);
                await file.CopyToAsync(stream);
                photoUrls.Add($"{Request.Scheme}://{Request.Host.Value}/uploads/{fileName}");
            }

            dto.PhotoUrlsJson = JsonSerializer.Serialize(photoUrls);
            await _service.UpsertAsync(dto);
            return Ok(new { ok = true, photoUrls });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Assessment upload failed");
            return Problem("Assessment upload failed", statusCode: 500);
        }
    }

    [HttpPost("batch")]
    public async Task<IActionResult> Batch([FromBody] JsonElement body)
    {
        if (!body.TryGetProperty("items", out var items)) return BadRequest("Missing items");

        var saved = 0;
        var errors = new List<object>();
        var index = 0;
        foreach (var item in items.EnumerateArray())
        {
            try
            {
                var dto = item.Deserialize<Assessment>(JsonOptions);
                if (dto == null || string.IsNullOrWhiteSpace(dto.Id))
                {
                    errors.Add(new { index, reason = "Missing or invalid id" });
                    continue;
                }
                if (!TryValidate(dto, out var validationErrors))
                {
                    errors.Add(new { index, id = dto.Id, reason = string.Join("; ", validationErrors) });
                    continue;
                }
                await _service.UpsertAsync(dto);
                saved++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Batch item {Index} failed to save", index);
                errors.Add(new { index, reason = ex.Message });
            }
            finally
            {
                index++;
            }
        }
        return Ok(new { ok = true, saved, failed = errors.Count, errors });
    }
}
