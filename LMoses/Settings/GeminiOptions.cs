namespace LMoses.Settings;

public sealed class GeminiOptions
{
    /// <summary>
    /// Google AI Studio Gemini API key. Prefer setting via environment variable GEMINI_API_KEY.
    /// </summary>
    public string ApiKey { get; set; } = "Api";


    /// <summary>
    /// Model name, e.g. gemini-2.5-flash.
    /// </summary>
    public string Model { get; set; } = "gemini-2.5-flash";

    /// <summary>
    /// Base endpoint for the Gemini API.
    /// </summary>
    public string EndpointBase { get; set; } = "https://generativelanguage.googleapis.com";
}


