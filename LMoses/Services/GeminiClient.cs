using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using LMoses.Settings;

namespace LMoses.Services;

public sealed class GeminiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    private readonly GeminiOptions _options;

    public GeminiClient(HttpClient http, IOptions<GeminiOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<string> GenerateGroundedAnswerAsync(
        string question,
        IReadOnlyList<SlidePdf> pdfs,
        CancellationToken ct)
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = _options.ApiKey;
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured. Set GEMINI_API_KEY env var or Gemini:ApiKey in appsettings.json.");
        }

        var parts = new List<object>
        {
            new
            {
                text = BuildPrompt(question, pdfs.Select(p => p.FileName).ToList())
            }
        };

        foreach (var pdf in pdfs)
        {
            var bytes = await File.ReadAllBytesAsync(pdf.FullPath, ct);
            parts.Add(new
            {
                // Gemini REST expects camelCase: inlineData + mimeType
                inlineData = new
                {
                    mimeType = "application/pdf",
                    data = Convert.ToBase64String(bytes)
                }
            });
        }

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                maxOutputTokens = 900
            }
        };

        var endpointBase = _options.EndpointBase.TrimEnd('/');
        var model = _options.Model;
        var url = $"{endpointBase}/v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}";

        using var resp = await _http.PostAsJsonAsync(url, payload, JsonOptions, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gemini API call failed ({(int)resp.StatusCode}): {body}");
        }

        // Minimal parse: candidates[0].content.parts[0].text
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
        {
            return "I couldn't generate an answer from the slides.";
        }

        var text = candidates[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return string.IsNullOrWhiteSpace(text) ? "I couldn't generate an answer from the slides." : text!;
    }

    private static string BuildPrompt(string question, List<string> sources)
    {
        var sourcesLine = sources.Count == 0 ? "No PDFs were attached." : string.Join(", ", sources);

        return
            "You are LMoses, a study assistant. Answer the student's question using ONLY the attached slide PDFs. " +
            "If the slides do not contain the answer, say you don't know from the provided slides.\n\n" +
            $"Attached slide PDFs: {sourcesLine}\n\n" +
            "Requirements:\n" +
            "- Explain clearly for a student.\n" +
            "- Prefer bullet points for steps/definitions.\n" +
            "- At the end, include a line exactly like: Sources: <comma-separated PDF filenames you used>\n\n" +
            $"Question: {question}";
    }
}


