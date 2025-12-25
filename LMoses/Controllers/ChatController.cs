using LMoses.Models;
using LMoses.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LMoses.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ChatController : ControllerBase
{
    private readonly SlidePdfStore _slides;
    private readonly GeminiClient _gemini;
    private readonly ILogger<ChatController> _logger;

    public ChatController(SlidePdfStore slides, GeminiClient gemini, ILogger<ChatController> logger)
    {
        _slides = slides;
        _gemini = gemini;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Ask([FromBody] ChatRequest request, CancellationToken ct)
    {
        var question = request?.Question?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(question))
        {
            return BadRequest(new { error = "Question is required." });
        }

        try
        {
            var selected = _slides.GetBestPdfsForQuestion(question);
            if (selected.Count == 0)
            {
                return Ok(new ChatResponse
                {
                    Answer = "No slide PDFs were found on the server. Please ensure the PDFs are available under the Slides folder.",
                    Sources = new List<string>()
                });
            }

            var answer = await _gemini.GenerateGroundedAnswerAsync(question, selected, ct);
            return Ok(new ChatResponse
            {
                Answer = answer,
                Sources = selected.Select(s => s.FileName).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat request failed.");
            return StatusCode(500, new
            {
                error = ex.Message
            });
        }
    }
}


